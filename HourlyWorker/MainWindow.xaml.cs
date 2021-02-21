using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Notifications.Wpf;
using System.ComponentModel;

namespace HourlyWorker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        /// <summary>
        /// List of all user work projects.
        /// </summary>
        public List<WorkProject> WorkProjects { get; private set; } = new List<WorkProject>();

        /// <summary>
        /// Backing field for the <see cref="ShowNotifications"/>.
        /// </summary>
        private bool showNotifications = true;
        /// <summary>
        /// Enable windows notifications?
        /// </summary>
        public bool ShowNotifications
        {
            get => showNotifications;
            set
            {
                if (showNotifications == value)
                    return;

                showNotifications = value;
                RaisePropertyChanged(nameof(ShowNotifications));
            }
        }

        /// <summary>
        /// Backing field for the <see cref="NightMode"/>.
        /// </summary>
        private bool nightMode = false;
        /// <summary>
        /// Is application in the night mode?
        /// </summary>
        public bool NightMode
        {
            get => nightMode;
            set
            {
                if (nightMode == value)
                    return;

                nightMode = value;
                RaisePropertyChanged(nameof(NightMode));
            }
        }

        /// <summary>
        /// Backing field for the <see cref="LookForInput"/>.
        /// </summary>
        private bool lookForInput = true;
        /// <summary>
        /// Detect if user is AFK?
        /// </summary>
        public bool LookForInput
        {
            get => lookForInput;
            set
            {
                if (lookForInput == value)
                    return;

                lookForInput = value;
                RaisePropertyChanged(nameof(LookForInput));
            }
        }

        /// <summary>
        /// Timer.
        /// </summary>
        private DispatcherTimer dispatcherTimer;
        /// <summary>
        /// Tray icon.
        /// </summary>
        private System.Windows.Forms.NotifyIcon icon;
        /// <summary>
        /// Manager for the windows notifications.
        /// </summary>
        private NotificationManager notificationManager;

        /// <summary>
        /// <see cref="dispatcherTimer"/> interval.
        /// </summary>
        public static readonly TimeSpan TimerInterval = new TimeSpan(0, 1, 0);

        ///<inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;
        ///<inheritdoc/>
        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        private System.Drawing.Point prevCursorPoint;
        private int samePosition = 0;

        /// <summary>
        /// Window initialization.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Init();
            InitNotifyIcon();
            KeyDown += new KeyEventHandler(MainWindow_KeyDown);
        }

        /// <summary>
        /// Reset AFK timer if any key is pressed.
        /// </summary>
        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            samePosition = 0;
        }

        /// <summary>
        /// Initialize tray icon with its events.
        /// </summary>
        private void InitNotifyIcon()
        {
            icon = new System.Windows.Forms.NotifyIcon();
            icon.Icon = Properties.Resources.Main;
            icon.Visible = true;
            icon.DoubleClick += (sender, e) =>
                {
                    ShowWindow();
                };
            icon.BalloonTipClosed += (sender, e) =>
                {
                    DisposeTrayIcon();
                };
            icon.MouseDown += new System.Windows.Forms.MouseEventHandler(TrayMouseDown);
        }

        /// <summary>
        /// Main logic initialization.
        /// </summary>
        private void Init()
        {
            //init notification manager
            notificationManager = new NotificationManager();

            //init timer
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(TimerTick);
            dispatcherTimer.Interval = TimerInterval;
            dispatcherTimer.Start();

            //load saved data
            SaveLoad.SaveData saveData = SaveLoad.Load();
            if (saveData != null)
            {
                ShowNotifications = saveData.ShowNotifications;
                LookForInput = saveData.LookForCursor;

                if (saveData.WorkProjects != null)
                    WorkProjects = saveData.WorkProjects;
            }
            
            //set data source in the datagrid
            ProjectsDataGrid.ItemsSource = WorkProjects;
        }

        /// <inheritdoc/>
        protected override void OnStateChanged(EventArgs e)
        {
            //hide window to the tray when minimized
            if (WindowState == WindowState.Minimized)
                Hide();

            base.OnStateChanged(e);
        }

        /// <summary>
        /// Timer tick. Updates all work projects.
        /// </summary>
        private void TimerTick(object sender, EventArgs e)
        {
            UpdateCursor();

            foreach(WorkProject wp in WorkProjects)
            {
                wp.Update();
            }
        }

        /// <summary>
        /// Update cursor previous position and check if it changed.
        /// </summary>
        private void UpdateCursor()
        {
            if (!lookForInput)
                return;

            bool working = WorkProjects.Where(x => x.Counting).ToArray().Length > 0;
            if (!working)
                return;

            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;

            if (point.X == prevCursorPoint.X && point.Y == prevCursorPoint.Y)
                samePosition++;

            if (samePosition >= 3)
            {
                foreach(WorkProject wp in WorkProjects)
                {
                    wp.Stop();
                }
                samePosition = 0;
            }

            prevCursorPoint = point;
        }

        /// <summary>
        /// Unhide window.
        /// </summary>
        private void ShowWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            Focus();
        }

        /// <summary>
        /// Dispose tray icon.
        /// </summary>
        private void DisposeTrayIcon()
        {
            if (icon != null)
            {
                icon.Visible = false;
                icon.Dispose();
            }
        }

        /// <summary>
        /// Open AddDialogue window to add new work project.
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            AddDialogue addDialogue = new AddDialogue();
            addDialogue.ShowDialog();
        }

        /// <summary>
        /// Save all data and dispose tray icon on exit.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            foreach (WorkProject wp in WorkProjects)
                wp.Stop();

            SaveLoad.Save(WorkProjects, ShowNotifications, LookForInput);

            DisposeTrayIcon();
        }

        /// <summary>
        /// Find root parent of the element.
        /// </summary>
        private T GetParent<T>(DependencyObject o) where T : DependencyObject
        {
            if (o == null || o is T)
                return (T)o;

            return GetParent<T>(VisualTreeHelper.GetParent(o));
        }

        /// <summary>
        /// Get work project from the datagrid row.
        /// </summary>
        private WorkProject GetWorkProject(object sender)
        {
            DataGridRow row = GetParent<DataGridRow>((Button)sender);
            return (WorkProject)row.Item;
        }

        /// <summary>
        /// Delete project. Asks user confirmation.
        /// </summary>
        private void DeleteProject(object sender, RoutedEventArgs e)
        {
            WorkProject wp = GetWorkProject(sender);
            MessageBoxResult message = MessageBox.Show(string.Format("Are you sure want to delete project \"{0}\"?", wp.Name), "Delete confirmation", MessageBoxButton.YesNo);
            if (message == MessageBoxResult.Yes)
            {
                RemoveProject(wp);
            }
        }

        /// <summary>
        /// Increments the work project timespan by an hour.
        /// </summary>
        private void Increment(object sender, RoutedEventArgs e)
        {
            WorkProject wp = GetWorkProject(sender);
            wp.TimeSpan.AddHours();
        }

        /// <summary>
        /// Decrements the work project timespan by an hour.
        /// </summary>
        private void Decrement(object sender, RoutedEventArgs e)
        {
            WorkProject wp = GetWorkProject(sender);
            wp.TimeSpan.AddHours(-1);
        }

        /// <summary>
        /// Start timer for the work project.
        /// </summary>
        private void InvokeProject(object sender, RoutedEventArgs e)
        {
            WorkProject wp = GetWorkProject(sender);
            if (wp.Counting)
                wp.Stop();
            else wp.Start();
        }

        /// <summary>
        /// Show context menu when user pressed RMB on the tray icon.
        /// </summary>
        private void TrayMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ContextMenu menu = (ContextMenu)FindResource("TrayMenu");
                menu.IsOpen = true;
            }
        }

        /// <summary>
        /// Stop all projects timers from the tray context menu.
        /// </summary>
        private void TrayStop(object sender, RoutedEventArgs e)
        {
            foreach(WorkProject wp in WorkProjects)
            {
                wp.Stop();
            }
        }

        /// <summary>
        /// Show window from the tray context menu.
        /// </summary>
        private void TrayOpen(object sender, RoutedEventArgs e)
        {
            ShowWindow();
        }

        /// <summary>
        /// Shutdown application from the tray context menu.
        /// </summary>
        private void TrayExit(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        /// <summary>
        /// Add new work project.
        /// </summary>
        /// <returns>true if the project successfully added.</returns>
        public bool AddProject(WorkProject workProject)
        {
            //show error if project with the same name already exists.
            if (!CheckProjectName(workProject.Name))
            {
                return false;
            }

            //add new project and update datagrid
            WorkProjects.Add(workProject);
            ProjectsDataGrid.Items.Refresh();
            return true;
        }

        /// <summary>
        /// Removes work project and updates the datagrid.
        /// </summary>
        public void RemoveProject(WorkProject workProject)
        {
            WorkProjects.Remove(workProject);
            ProjectsDataGrid.Items.Refresh();
        }

        /// <summary>
        /// Show windows notification.
        /// </summary>
        public void ShowNotification(NotificationContent notificationContent)
        {
            if (!ShowNotifications)
                return;

            notificationManager.Show(notificationContent, onClick: () => ShowWindow());
        }

        /// <summary>
        /// Check if project name is correct.
        /// </summary>
        /// <param name="showMessage">Show error message?</param>
        public bool CheckProjectName(string name, bool showMessage = true)
        {
            if (name.Length < 3)
            {
                if (showMessage)
                    MessageBox.Show("Project name is too short! Should be more than 2 symbols.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (name.Length > 20)
            {
                if (showMessage)
                    MessageBox.Show("Project name is too long! Should be less than 21 symbols.", "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (WorkProjects.FirstOrDefault(x => x.Name == name) != null)
            {
                if (showMessage)
                    MessageBox.Show(string.Format("Project with name \"{0}\" already exists!", name), "Invalid name", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            return true;
        }
    }
}
