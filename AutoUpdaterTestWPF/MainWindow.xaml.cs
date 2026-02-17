using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AutoUpdaterTestWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        UpdateInfoEventArgs _args = null;


        public MainWindow()
        {
            InitializeComponent();
            Assembly assembly = Assembly.GetEntryAssembly();
            LabelVersion.Content = $"Current Version : {assembly.GetName().Version}";
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
            this.Closing += MainWindow_Closing;


            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdater_ParseUpdateInfoEvent;

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
            if (_args != null)
                AutoUpdater.Update(_args);

        }

        private void ButtonCheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            AutoUpdater.Synchronous = true;
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            var token = "eyJhbGciOiJSUzI1NiIsInR5cCI6ImF0K2p3dCJ9.eyJuYmYiOjE3NzEzMDY0OTYsImV4cCI6MTc3MTMwNjc5NiwiaXNzIjoiaHR0cHM6Ly9hcGktcWEtY2lzLXRyYWRpbmcubG9naWNpZWwtc2VydmljZXMuY29tIiwiYXVkIjpbImFncmVlbWVudC1hcGkiLCJhcmNoaXZlcy1hcGkiLCJWdHJhZGVyIl0sImNsaWVudF9pZCI6IlZ0cmFkZXIiLCJzdWIiOiJhOGIxNWQ3Yi03ZThjLTRmNzItOGU1Yi1kNGJmZGQzNzE3YzQiLCJhdXRoX3RpbWUiOjE3NzEzMDU0NjAsImlkcCI6ImxvY2FsIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiYWxpdnNAc2hhcmtsYXNlcnMuY29tIiwiQXNwTmV0LklkZW50aXR5LlNlY3VyaXR5U3RhbXAiOiJBR0k0T0ZXSUdKRVBIUUpUSFhDWU5XSjNGVDNaWEtVTCIsImJvb3RoX2lkIjoiTFNMIiwibWF4X2h1Yl9jb25uZWN0aW9ucyI6IjQiLCJpZGxlX3RpbWVvdXQiOiIzNjAwIiwicm9sZSI6IkFkbWluIiwicm9sZV9wZXJtaXNzaW9uIjoie30iLCJpc0FkbWluUm9sZSI6InRydWUiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJhbGl2cyIsIm5hbWUiOiJhbGl2cyIsImVtYWlsIjoiYWxpdnNAc2hhcmtsYXNlcnMuY29tIiwiZW1haWxfdmVyaWZpZWQiOiJGYWxzZSIsImZpcnN0TmFtZSI6ImFsaSIsImxhc3ROYW1lIjoiaGFpZGVyIiwiVXNlcklkIjoiYThiMTVkN2ItN2U4Yy00ZjcyLThlNWItZDRiZmRkMzcxN2M0IiwiVkFOIjoiODUwMzQ5MTkiLCJpYXQiOjE3NzEzMDU0NjAsInNjb3BlIjpbImFncmVlbWVudC1hcGkiLCJhcmNoaXZlcy1hcGkiLCJWdHJhZGVyIiwib2ZmbGluZV9hY2Nlc3MiXSwiYW1yIjpbInB3ZCJdfQ.Y8gxazFeYoCtLPKZhxs-7q_r88XiR88RGbWRGS4LBYw50zvYZ5NGKcX3vN83xDU79mp0LwVDNtmaNs_jPUM42rMSHUeTK8TfGg9vJWbZfgT3fQvNdmLM-zKuIaAvlPnNzgp4aDmE7vsGG9lxIbg80G2U2zyXEG5QqLxszVAuTnw";

            CustomAuthentication customAuthentication = new CustomAuthentication($"Bearer {token}");
            AutoUpdater.BasicAuthXML = customAuthentication;
            AutoUpdater.Start("http://10.0.10.82:8001/api/Version/GetVersion?UserName=alivs&Booth=LSL");

        }

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {

                if (args.IsUpdateAvailable)
                {
                    _args = args;
                    MessageBoxResult dialogResult = MessageBoxResult.No;
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
                            Task.Factory.StartNew(() => AutoUpdater.DownloadSilently(args));
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

        private void AutoUpdater_ParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {

            dynamic obj = JsonConvert.DeserializeObject(args.RemoteData);

            if (obj != null)
            {
                args.UpdateInfo = new UpdateInfoEventArgs
                {
                    CurrentVersion = obj.version,
                    //ChangelogURL = json.changelog,
                    DownloadURL = obj.url,
                    Mandatory = new Mandatory
                    {
                        Value = obj.mandatory.value,
                        UpdateMode = obj.mandatory.mode,
                        MinimumVersion = obj.mandatory.minVersion
                    },
                    //CheckSum = new CheckSum
                    //{
                    //    Value = json.checksum.value,
                    //    HashingAlgorithm = json.checksum.hashingAlgorithm
                    //}
                };

            }
        }

    }
}