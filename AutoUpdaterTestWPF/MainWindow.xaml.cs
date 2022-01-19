using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using AutoUpdaterDotNET;

namespace AutoUpdaterTestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UpdateInfoEventArgs _args=null;
        public MainWindow()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetEntryAssembly();
            LabelVersion.Content = $"Current Version : {assembly.GetName().Version}";
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
            this.Closing += MainWindow_Closing;
            //AutoUpdater.LetUserSelectRemindLater = true;
            //AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            //AutoUpdater.RemindLaterAt = 1;
            //AutoUpdater.ReportErrors = true;
            //DispatcherTimer timer = new DispatcherTimer {Interval = TimeSpan.FromMinutes(2)};
            //timer.Tick += delegate { AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTestWPF.xml"); };
            //timer.Start();
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_args !=null)
                AutoUpdater.Update(_args);
            
        }

        private void ButtonCheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            //AutoUpdater.UpdateMode = Mode.Forced;
            AutoUpdater.Synchronous = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            //AutoUpdater.Start("http://localhost:10001/updates/AutoUpdaterTest-1.xml");
            AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTestWPF.xml");
            
        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                
                if (args.IsUpdateAvailable)
                {
                    _args = args;
                    MessageBoxResult dialogResult= MessageBoxResult.No;
                    if (args.Mandatory.Value)
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                    }
                    //else
                    //{
                    //    dialogResult =
                    //        MessageBox.Show(
                    //            $@"There is new version {args.CurrentVersion} available. You are using version {
                    //                    args.InstalledVersion
                    //                }. Do you want to update the application now?", @"Update Available",
                    //            MessageBoxButton.YesNo,
                    //            MessageBoxImage.Information);
                    //}

                    // Uncomment the following line if you want to show standard update dialog instead.
                    // AutoUpdater.ShowUpdateForm(args);

                    if (dialogResult.Equals(MessageBoxResult.Yes) || dialogResult.Equals(MessageBoxResult.OK))
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                Environment.Exit(-1);
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                    else if (dialogResult.Equals(MessageBoxResult.No) || dialogResult.Equals(MessageBoxResult.None))
                    {
                        try
                        {
                            Task.Factory.StartNew(()=>AutoUpdater.DownloadSilently(args));
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                //oSignalEvent.Set();
            }
            else
            {
                if (args.Error is System.Net.WebException)
                {
                    MessageBox.Show(
                        @"There is a problem reaching update server. Please check your internet connection and try again later.",
                        @"Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                //oSignalEvent.Set();
            }
        }
    }
}