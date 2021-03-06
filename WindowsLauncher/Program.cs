﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TechnicLauncher
{
    static class Program
    {
        public const string LaucherFile = "technic-launcher.jar";
        public static string AppPath;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            AppPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            AppPath = Path.Combine(AppPath, ".techniclauncher");
            if (!Directory.Exists(AppPath))
                Directory.CreateDirectory(AppPath);

            Application.Run(new Form1());
        }

        private static String GetJavaInstallationPath()
        {
            const string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
            using (var baseKey = Registry.LocalMachine.OpenSubKey(javaKey))
            {
                if (baseKey != null)
                {
                    String currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                    {
                        if (homeKey != null)
                        {
                            var home = homeKey.GetValue("JavaHome");
                            return home.ToString();
                        }
                    }
                }
            }
            return null;
        }

        private static String LocateJavaFromPath()
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (path == null)
                return null;
            var folders = path.Split(';');
            foreach (var folder in folders)
            {
                if (folder.ToLowerInvariant().Contains("system32"))
                    continue;
                var javaPath = Path.Combine(folder, "java.exe");
                if (File.Exists(javaPath))
                {
                    return folder;
                }
            }
            return null;
        }

        private static String LocateJavaPath()
        {
            var path = Environment.GetEnvironmentVariable("JAVA_HOME");
            if (path == null)
                return null;
            var folders = path.Split(';');
            foreach (var folder in folders)
            {
                var javaPath = Path.Combine(Path.Combine(folder, "bin"), "java.exe");
                if (File.Exists(javaPath))
                {
                    return folder;
                }
            }
            return null;
        }

        private static String LocateJavaFromProgramFiles()
        {
            var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            var path = Path.Combine(programFiles, "Java");
            if (!Directory.Exists(path))
            {
                if (path.Contains("(x86)"))
                {
                    path = path.Replace(" (x86)", "");
                    if (!Directory.Exists(path))
                        return null;
                }
                else
                {
                    return null;
                }
            }
            var folders = new List<string>(Directory.GetDirectories(path));
            if (folders.Count <= 0)
            {
                path = path.Replace("Program Files", "Program Files (x86)");
                if (!Directory.Exists(path))
                    return null;
                folders.AddRange(Directory.GetDirectories(path));
            }
            folders.Reverse();
            foreach (var folder in folders)
            {
                var javaPath = Path.Combine(Path.Combine(folder, "bin"), "java.exe");
                if (File.Exists(javaPath))
                {
                    return folder;
                }
            }
            return null;
        }

        public static void RunLauncher(String launcherPath)
        {
            var java = GetJavaInstallationPath() ?? 
                LocateJavaFromPath() ??
                LocateJavaPath() ??
                LocateJavaFromProgramFiles();
            if (java == null)
            {
                MessageBox.Show(@"Can't find java directory.");
            }
            else
            {   
                var info = new ProcessStartInfo
                               {
                                   CreateNoWindow = true,
                                   WorkingDirectory = Application.StartupPath,
                                   FileName = Path.Combine(java, "bin\\java.exe"),
                                   Arguments = String.Format("-jar {0}", launcherPath),
                                   UseShellExecute = false
                               };
                Process.Start(info);
            }
            Application.Exit();
        }

    }
}
