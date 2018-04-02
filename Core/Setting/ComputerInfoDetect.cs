﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


class ComputerInfoDetect
{
    public static String GetJavaInstallationPath()
    {
        if (Properties.Settings.Default["JavaPath"].ToString() == "Empty")
        {
            String javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
            {
                try
                {
                    String currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                        return homeKey.GetValue("JavaHome").ToString();
                }
                catch
                {
                    bool ok = false;

                    Process process = new Process();
                    try
                    {
                        string path = "";
                        process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.FileName = "cmd.exe";
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.RedirectStandardOutput = true;
                        process.StartInfo.RedirectStandardError = true;
                        process.StartInfo.Arguments = "/c \"" + "where java " + "\"";

                        process.OutputDataReceived += new DataReceivedEventHandler((s, e) =>
                        {
                            if (e.Data != null)
                            {
                                path = e.Data.ToString();
                            }
                        });
                        process.ErrorDataReceived += new DataReceivedEventHandler((s, e) =>
                        {
                            if (e.Data != null)
                            {
                                //Windows.DebugOutputConsole.singleton.Write(Pages.SplashScreen.singleton.manager.GetString("javaPathError"));
                            }
                        });

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        process.WaitForExit();

                        ok = (process.ExitCode == 0);
                        if (path == "")
                            return Properties.Settings.Default["JavaPath"].ToString();
                        else
                            return path;

                    }
                    catch
                    {
                        return Properties.Settings.Default["JavaPath"].ToString();
                    }

                }

            }
        }
        else
        {
            return Properties.Settings.Default["JavaPath"].ToString();
        }

    }

    public static int GetComputerArchitecture()
    {
        if (Environment.Is64BitOperatingSystem)
        {
            return 64;
        }
        else
        {
            return 32;
        }
    }

    public static bool isDirOK()
    {
        if (Path.GetDirectoryName(Assembly.GetEntryAssembly().Location).Contains(" "))
            return false;
        else
            return true;
    }

}