using AutoUpdaterDotNET.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoUpdaterDotNET
{
    public class DownloadManager
    {
        private readonly UpdateInfoEventArgs _args;

        private string _tempFile;

        private MyWebClient _webClient;
        private Uri uri;

        private DateTime _startedAt;
        public event DownloadProgressChangedEventHandler DownloadProgressChanged
        {
            add { _webClient.DownloadProgressChanged += value; } 
            remove { _webClient.DownloadProgressChanged -= value; }
        }
        public delegate void OnDownloadFileCompleted(string fileName, object sender, EventArgs args,bool isDownloadSuccessfully);
        public event OnDownloadFileCompleted _OnDownloadFileCompleted;
        public DownloadManager(UpdateInfoEventArgs args)
        {
            _args = args;
            Init();
        }
        private void Init()
        {
            uri = new Uri(_args.DownloadURL);
            _webClient = AutoUpdater.GetWebClient(uri, AutoUpdater.BasicAuthDownload);
            if (string.IsNullOrEmpty(AutoUpdater.DownloadPath))
            {
                _tempFile = Path.GetTempFileName();
            }
            else
            {
                _tempFile = Path.Combine(AutoUpdater.DownloadPath, $"{Guid.NewGuid().ToString()}.tmp");
                if (!Directory.Exists(AutoUpdater.DownloadPath))
                {
                    Directory.CreateDirectory(AutoUpdater.DownloadPath);
                }
            }
            _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
        }
        public void DownloadFileAsync()
        {
            
            _webClient.DownloadFileAsync(uri, _tempFile);
            
        }

        //public void DownloadFile()
        //{
        //    _webClient.DownloadFile(uri, _tempFile);
        //    _webClient.DownloadFileCompleted += WebClientOnDownloadFileCompleted;
        //}
        private void WebClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs asyncCompletedEventArgs)
        {
            string tempPath = "";
            if (asyncCompletedEventArgs.Cancelled)
            {
                return;
            }

            try
            {
                if (asyncCompletedEventArgs.Error != null)
                {
                    throw asyncCompletedEventArgs.Error;
                }

                if (_args.CheckSum != null)
                {
                    CompareChecksum(_tempFile, _args.CheckSum);
                }

                ContentDisposition contentDisposition = null;
                if (_webClient.ResponseHeaders["Content-Disposition"] != null)
                {
                    contentDisposition = new ContentDisposition(_webClient.ResponseHeaders["Content-Disposition"]);
                }

                var fileName = string.IsNullOrEmpty(contentDisposition?.FileName)
                    ? Path.GetFileName(_webClient.ResponseUri.LocalPath)
                    : contentDisposition.FileName;

                 tempPath =
                    Path.Combine(
                        string.IsNullOrEmpty(AutoUpdater.DownloadPath) ? Path.GetTempPath() : AutoUpdater.DownloadPath,
                        fileName);

                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                File.Move(_tempFile, tempPath);

                //string installerArgs = null;
                //if (!string.IsNullOrEmpty(_args.InstallerArgs))
                //{
                //    installerArgs = _args.InstallerArgs.Replace("%path%",
                //        Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                //}

                //var processStartInfo = new ProcessStartInfo
                //{
                //    FileName = tempPath,
                //    UseShellExecute = true,
                //    Arguments = installerArgs
                //};

                //var extension = Path.GetExtension(tempPath);
                //if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                //{
                //    string installerPath = Path.Combine(Path.GetDirectoryName(tempPath), "ZipExtractor.exe");

                //    File.WriteAllBytes(installerPath, Resources.ZipExtractor);

                //    string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                //    string extractionPath = Path.GetDirectoryName(executablePath);

                //    if (!string.IsNullOrEmpty(AutoUpdater.InstallationPath) &&
                //        Directory.Exists(AutoUpdater.InstallationPath))
                //    {
                //        extractionPath = AutoUpdater.InstallationPath;
                //    }

                //    StringBuilder arguments =
                //        new StringBuilder($"\"{tempPath}\" \"{extractionPath}\" \"{executablePath}\"");
                //    string[] args = Environment.GetCommandLineArgs();
                //    for (int i = 1; i < args.Length; i++)
                //    {
                //        if (i.Equals(1))
                //        {
                //            arguments.Append(" \"");
                //        }

                //        arguments.Append(args[i]);
                //        arguments.Append(i.Equals(args.Length - 1) ? "\"" : " ");
                //    }

                //    processStartInfo = new ProcessStartInfo
                //    {
                //        FileName = installerPath,
                //        UseShellExecute = true,
                //        Arguments = arguments.ToString()
                //    };
                //}
                //else if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
                //{
                //    processStartInfo = new ProcessStartInfo
                //    {
                //        FileName = "msiexec",
                //        Arguments = $"/i \"{tempPath}\""
                //    };
                //    if (!string.IsNullOrEmpty(installerArgs))
                //    {
                //        processStartInfo.Arguments += " " + installerArgs;
                //    }
                //}

                //if (AutoUpdater.RunUpdateAsAdmin)
                //{
                //    processStartInfo.Verb = "runas";
                //}

                //try
                //{
                //    Process.Start(processStartInfo);
                //}
                //catch (Win32Exception exception)
                //{
                //    if (exception.NativeErrorCode == 1223)
                //    {
                //        _webClient = null;
                //    }
                //    else
                //    {
                //        throw;
                //    }
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.GetType().ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
                _webClient = null;
            }
            finally
            {
                if (_OnDownloadFileCompleted!=null)
                    _OnDownloadFileCompleted(tempPath, sender, asyncCompletedEventArgs, _webClient != null);
            }
        }

        private static void CompareChecksum(string fileName, CheckSum checksum)
        {
            using (var hashAlgorithm =
                HashAlgorithm.Create(
                    string.IsNullOrEmpty(checksum.HashingAlgorithm) ? "MD5" : checksum.HashingAlgorithm))
            {
                using (var stream = File.OpenRead(fileName))
                {
                    if (hashAlgorithm != null)
                    {
                        var hash = hashAlgorithm.ComputeHash(stream);
                        var fileChecksum = BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();

                        if (fileChecksum == checksum.Value.ToLower()) return;

                        throw new Exception(Resources.FileIntegrityCheckFailedMessage);
                    }

                    throw new Exception(Resources.HashAlgorithmNotSupportedMessage);
                }
            }
        }
    }
}
