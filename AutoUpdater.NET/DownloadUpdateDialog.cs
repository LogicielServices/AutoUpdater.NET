using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using AutoUpdaterDotNET.Properties;

namespace AutoUpdaterDotNET
{
    internal partial class DownloadUpdateDialog : Form
    {

        private MyWebClient _webClient;
        private string DownloadedFileName=null;
        private DateTime _startedAt;
        private DownloadManager downloader;
        private UpdateManager updater;
        private bool isDownloadSuccessfully;
        private bool isUpdateSuccessfully;
        public DownloadUpdateDialog(DownloadManager Downloader,UpdateManager updater)
        {
            InitializeComponent();
            this.updater = updater;
            //_args = args;
            downloader = Downloader;
            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.ForcedDownload)
            {
                ControlBox = false;
            }
        }

        private void DownloadUpdateDialogLoad(object sender, EventArgs e)
        {
            downloader.DownloadProgressChanged += OnDownloadProgressChanged;
            downloader._OnDownloadFileCompleted += Downloader__OnDownloadFileCompleted;
            downloader.DownloadFileAsync();
           


        }

        private void Updater__OnUpdateCompleted(string fileName, bool isUpdateSuccessfully)
        {
           
        }

        private void Downloader__OnDownloadFileCompleted(string FileName,object sender, EventArgs args, bool isDownloadSuccessfully)
        {
            this.isDownloadSuccessfully = isDownloadSuccessfully;
            DownloadedFileName = FileName;
            updater._OnUpdateCompleted += Updater__OnUpdateCompleted;
            if (DownloadedFileName != null)
                updater.Update(DownloadedFileName);

            DialogResult = DialogResult.Cancel;
            if (isDownloadSuccessfully)
            {
                DialogResult = DialogResult.OK;
            }
            FormClosing -= DownloadUpdateDialog_FormClosing;
            Close();
        }

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_startedAt == default(DateTime))
            {
                _startedAt = DateTime.Now;
            }
            else
            {
                var timeSpan = DateTime.Now - _startedAt;
                long totalSeconds = (long) timeSpan.TotalSeconds;
                if (totalSeconds > 0)
                {
                    var bytesPerSecond = e.BytesReceived / totalSeconds;
                    labelInformation.Text =
                        string.Format(Resources.DownloadSpeedMessage, BytesToString(bytesPerSecond));
                }
            }

            labelSize.Text = $@"{BytesToString(e.BytesReceived)} / {BytesToString(e.TotalBytesToReceive)}";
            progressBar.Value = e.ProgressPercentage;
        }
        private static string BytesToString(long byteCount)
        {
            string[] suf = {"B", "KB", "MB", "GB", "TB", "PB", "EB"};
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture)} {suf[place]}";
        }

        private void DownloadUpdateDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (AutoUpdater.Mandatory && AutoUpdater.UpdateMode == Mode.ForcedDownload)
            {
                if (ModifierKeys == Keys.Alt || ModifierKeys == Keys.F4)
                {
                    e.Cancel = true;
                    return;
                }
            }
            if (_webClient != null && _webClient.IsBusy)
            {
                _webClient.CancelAsync();
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}