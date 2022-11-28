using AutoUpdaterDotNET.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace AutoUpdaterDotNET
{
    public class UpdateManager
    {
        private readonly UpdateInfoEventArgs _args;
        private static ILog logger = LogManager.GetLogger("ZipExtractorLogger");

        public delegate void OnUpdateCompleted(string fileName, bool isUpdateSuccessfully);
        public event OnUpdateCompleted _OnUpdateCompleted;
        private bool isUpdateSuccessfyully = false;
        public UpdateManager(UpdateInfoEventArgs args)
        {
            logger.Info("Initializing Update Manager Object");
            _args = args;
        }
        
        public void Update(string tempPath)
        {
            logger.Info("Inside Update Method of Update Manager");
            string installerArgs = null;
            if (!string.IsNullOrEmpty(_args.InstallerArgs))
            {
                installerArgs = _args.InstallerArgs.Replace("%path%",
                    Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true,
                Arguments = installerArgs
            };
            logger.Info(processStartInfo.FileName);
            var extension = Path.GetExtension(tempPath);
            if (extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                string installerPath = Path.Combine(Path.GetDirectoryName(tempPath), "ZipExtractor.exe");

                File.WriteAllBytes(installerPath, Resources.ZipExtractor);

                string executablePath = Process.GetCurrentProcess().MainModule.FileName;
                string extractionPath = Path.GetDirectoryName(executablePath);

                if (!string.IsNullOrEmpty(AutoUpdater.InstallationPath) &&
                    Directory.Exists(AutoUpdater.InstallationPath))
                {
                    extractionPath = AutoUpdater.InstallationPath;
                }

                StringBuilder arguments =
                    new StringBuilder($"\"{tempPath}\" \"{extractionPath}\" \"{executablePath}\"");
                string[] args = Environment.GetCommandLineArgs();
                for (int i = 1; i < args.Length; i++)
                {
                    if (i.Equals(1))
                    {
                        arguments.Append(" \"");
                    }

                    arguments.Append(args[i]);
                    arguments.Append(i.Equals(args.Length - 1) ? "\"" : " ");
                }

                processStartInfo = new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,
                    Arguments = arguments.ToString()
                };
            }
            else if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
            {
                processStartInfo = new ProcessStartInfo
                {
                    FileName = "msiexec",
                    Arguments = $"/i \"{tempPath}\""
                };
                if (!string.IsNullOrEmpty(installerArgs))
                {
                    processStartInfo.Arguments += " " + installerArgs;
                }
            }

            if (AutoUpdater.RunUpdateAsAdmin)
            {
                processStartInfo.Verb = "runas";
            }

            try
            {
                logger.Info("Starting Process After Update Successfully True");
                isUpdateSuccessfyully = true;
                Process.Start(processStartInfo);
            }
            catch (Win32Exception exception)
            {logger.Error(exception.ToString());
                isUpdateSuccessfyully = false;
                if (exception.NativeErrorCode == 1223)
                {
                    //_webClient = null;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if(_OnUpdateCompleted != null)
                    _OnUpdateCompleted(tempPath, isUpdateSuccessfyully);
            }
        }
    }
}
