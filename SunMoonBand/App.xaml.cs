using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using SunMoonBand.Pages;
//using SunMoonBand.Theme;
using SunMoonBand.Utilities;
using SunMoonBandCommon;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
/*


namespace SunMoon
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Если стек навигации не восстанавливается для перехода к первой странице,
                    // настройка новой страницы путем передачи необходимой информации в качестве параметра
                    // параметр
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Обеспечение активности текущего окна
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }
}
*/

// ====================

/*
*  Copyright © 2015 Russell Libby
*/


namespace SunMoonBand
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : INotifyPropertyChanged
    {
        #region Private fields

        private readonly ObservableCollectionEx<SunMoonObservation> _observations;
        private TransitionCollection _transitions;

        private bool _syncing = true;
        private bool _paired;
        private bool _tileAdded;

        #endregion

        #region Private methods

        /// <summary>
        /// Determines if we can sync to and/or remove the band tile.
        /// </summary>
        /// <returns>True if connected to a band tile and not currently syncing.</returns>
        private bool CanSyncOrRemove()
        {
            return (_paired && _tileAdded && !_syncing);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;

            if (rootFrame == null) throw new ArgumentException(@"RootFrame is null.");

            rootFrame.ContentTransitions = _transitions ?? new TransitionCollection { new NavigationThemeTransition() };
            rootFrame.Navigated -= RootFrame_FirstNavigated;
        }

        #endregion

        #region Protected methods

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            //if (System.Diagnostics.Debugger.IsAttached) DebugSettings.EnableFrameRateCounter = true;
#endif

            var rootFrame = Window.Current.Content as Frame;

            if (!((e.PreviousExecutionState == ApplicationExecutionState.Running) || (e.PreviousExecutionState == ApplicationExecutionState.Suspended)))
            {
                /* Add persistent loading here */
            }

            if (rootFrame == null)
            {
                rootFrame = new Frame { CacheSize = 1 };
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                if (rootFrame.ContentTransitions != null)
                {
                    _transitions = new TransitionCollection();

                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        _transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += RootFrame_FirstNavigated;

                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments)) throw new Exception("Failed to create initial page");
            }

            Window.Current.Activate();
        }

        /// <summary>
        /// Handle window creation by setting our own custom theme.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            //ThemeManager.SetThemeColor((Color)Resources["ThemeColor"]);

            base.OnWindowCreated(args);
        }

        #endregion

        #region Constuctor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            _observations = new ObservableCollectionEx<SunMoonObservation>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Loads the new observations into our observable collection.
        /// </summary>
        /// <param name="collection">The collection to load.</param>
        public void LoadObservations(ICollection<SunMoonObservation> collection)
        {
            if (collection == null) return;

            var index = 0;

            foreach (var item in collection)
            {
                if (index >= _observations.Count)
                {
                    _observations.Add(item);
                }
                else
                {
                    var existing = _observations[index];

                    existing.Title = item.Title;
                    existing.SecondaryTitle = item.SecondaryTitle;
                    existing.Content = item.Content;
                    existing.IconIndex = item.IconIndex;
                }

                index++;
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Observable collection of user selected stations.
        /// </summary>
        public ObservableCollection<SunMoonObservation> Observations
        {
            get { return _observations; }
        }

        /// <summary>
        /// True if a tile can be added, otherwise false.
        /// </summary>
        public bool CanAddTile
        {
            get { return (_paired && !_tileAdded && !_syncing); }
        }

        /// <summary>
        /// True if a tile can be removed, otherwise false.
        /// </summary>
        public bool CanRemoveTile
        {
            get { return CanSyncOrRemove(); }
        }

        /// <summary>
        /// True if we can sync items with the band, otherwise false.
        /// </summary>
        public bool CanSync
        {
            get { return CanSyncOrRemove(); }
        }

        /// <summary>
        /// Returns the menu description based on alternate site value.
        /// </summary>
        public string SiteDescription
        {
            get { return UseAlternateSource ? "use primary site" : "use secondary site"; }
        }

        /// <summary>
        /// True if the alternate azure site should be used.
        /// </summary>
        public bool UseAlternateSource
        {
            get
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                return (localSettings.Values["UseAlternateSource"] != null) && (bool)localSettings.Values["UseAlternateSource"];
            }
            set
            {
                var localSettings = ApplicationData.Current.LocalSettings;

                localSettings.Values["UseAlternateSource"] = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("UseAlternateSource"));
                PropertyChanged(this, new PropertyChangedEventArgs("SiteDescription"));
            }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsPaired
        {
            get { return _paired; }
            set
            {
                _paired = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsPaired"));
            }
        }

        /// <summary>
        /// True if a band is paired, otherwise false.
        /// </summary>
        public bool IsTileAdded
        {
            get { return _tileAdded; }
            set
            {
                _tileAdded = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsTileAdded"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanAddTile"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanRemoveTile"));
            }
        }

        /// <summary>
        /// Returns the visibility state for the progress bar when syncing.
        /// </summary>
        public Visibility SyncVisibility
        {
            get { return (_syncing ? Visibility.Visible : Visibility.Collapsed); }
        }

        /// <summary>
        /// Returns true if we are already syncing with the band.
        /// </summary>
        public bool IsSyncing
        {
            get { return _syncing; }
            set
            {
                _syncing = value;

                if (PropertyChanged == null) return;

                PropertyChanged(this, new PropertyChangedEventArgs("IsSyncing"));
                PropertyChanged(this, new PropertyChangedEventArgs("SyncVisibility"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanSync"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanAddTile"));
                PropertyChanged(this, new PropertyChangedEventArgs("CanRemoveTile"));
            }
        }

        /// <summary>
        /// True if the sync was initiated by the background task, otherwise false.
        /// </summary>
        public bool BackgroundSync { get; set; }

        /// <summary>
        /// The event that is fired when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Returns the current instance of the application.
        /// </summary>
        public static new App Current
        {
            get { return (App)Application.Current; }
        }

        #endregion
    }
}

