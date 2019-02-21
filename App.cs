using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Tml_FNA_Installer
{
    class App
    {
        private static readonly bool Is64Bitsystem = Environment.Is64BitOperatingSystem;
        private static readonly bool IsWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
        private static readonly bool IsLinux = Environment.OSVersion.Platform == PlatformID.Unix;
        private static readonly bool IsMacOS = Environment.OSVersion.Platform == PlatformID.MacOSX;

        private static readonly string CurrentRunningDirectory = Environment.CurrentDirectory;

        public void Run()
        {
            PrintPlatformDetectionStuff();
            if (IsWindows)
            {
                var installationPath = GetInstallationPath();
                BackupSteamFile(installationPath);
                CopyMainFolderContent(installationPath);
                HandleNativeDllCopy(installationPath);
                Console.WriteLine("Installation completed!");
                Console.Read();
            }
        }

        private string GetInstallationPath()
        {
            string installationPath = GetInstallationPathFromRegistry();
            if (installationPath == null)
            {
                throw new Exception(
                    "Registry key not found, make sure you have the game properly installed or proceed to a manual installation");
            }

            Console.WriteLine("Terraria installation directory : " + installationPath);
            return installationPath;
        }

        public string GetInstallationPathFromRegistry()
        {
            var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 105600");
            if (key == null)
            {
                throw new Exception("Registry key could not be found");
            }


            Console.WriteLine(key.ValueCount);
            string[] allSubKey = key.GetSubKeyNames();
            Console.WriteLine(allSubKey);
            return (string)key.GetValue("InstallLocation", null);
        }

        private void CopyMainFolderContent(string installationPath)
        {

            foreach (var path in Directory.EnumerateFiles(CurrentRunningDirectory))
            {
                if (path.Contains("Tml FNA Installer") || path.Contains(".txt"))
                {
                    continue;
                }
                
                string fileName = Path.GetFileName(path);
                Console.WriteLine("Currently copying : " + fileName);
                File.Copy(path, Path.Combine(installationPath, fileName), true);
            }
        }

        private void HandleNativeDllCopy(string installationPath)
        {
            string[] specificPlatformFilePath;

            if (Is64Bitsystem)
            {
                specificPlatformFilePath = Directory.EnumerateFiles(CurrentRunningDirectory + "/x64").ToArray();
            }
            else
            {
                specificPlatformFilePath = Directory.EnumerateFiles(CurrentRunningDirectory + "/x86").ToArray();
            }

            CopyNativeFile(installationPath, specificPlatformFilePath, Is64Bitsystem);
        }

        public void CopyNativeFile(string gamePath, string[] nativeFilePath, bool isx64)
        {
            gamePath += (Is64Bitsystem) ? "/x64" : "/x86";
            Console.WriteLine(gamePath);
            if (!Directory.Exists(gamePath))
            {
                Directory.CreateDirectory(gamePath);
            }

            foreach (var file in nativeFilePath)
            {
                string fileName = Path.GetFileName(file);
                Console.WriteLine("Copying Native dll : " + fileName);
                File.Copy(file, gamePath + "/" + fileName, true);
            }
        }

        public void PrintPlatformDetectionStuff()
        {
            string currentOS = "Current OS : ";
            if (IsWindows)
            {
                currentOS += "Windows";
            }

            if (IsLinux)
            {
                currentOS += "Linux";
            }

            if (IsMacOS)
            {
                currentOS += "Mac OS";
            }
            Console.WriteLine(currentOS);
            Console.WriteLine("Is OS 64bit : " + ((Is64Bitsystem) ? "yes" : "no"));
        }

        private void BackupSteamFile(string gamePath)
        {
            Console.WriteLine("Backing up steam folder...");
            string backupPath = gamePath + "/OriginalBackup";
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);

                foreach (var path in Directory.EnumerateFiles(gamePath))
                {
                    if (path.Contains("Tml FNA Installer") || path.Contains(".txt"))
                    {
                        continue;
                    }

                    string fileName = Path.GetFileName(path);
                    Console.WriteLine("Currently copying : " + fileName);
                    File.Copy(path, Path.Combine(backupPath, fileName), true);
                }
            }
        }
    }
}
