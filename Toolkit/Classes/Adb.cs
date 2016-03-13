/*
 * Adb.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace Toolkit
{
    class Adb
    {
        private static readonly object Lock = new object();
        private static readonly string AdbExe = ResourceManager.TempPath + "adb.exe";
        private static readonly string FastbootExe = ResourceManager.TempPath + "fastboot.exe";
        private const int Timeout = -1;

        public static string ExecuteAdbCommand(string command, Device device = null)
        {
            lock (Lock)
                return RunProcess(AdbExe, device == null ? command : $"-s {device.SerialNumber} {command}");
        }

        public static string ExecuteAdbShellCommand(string command, Device device, bool root = false)
        {
            lock (Lock)
                return RunProcess(AdbExe, root ? $"-s {device.SerialNumber} shell \"su -c \"{command}\"\""
                                               : $"-s {device.SerialNumber} shell \"{command}\"");
        }

        public static string ExecuteFastbootCommand(string command, Device device = null)
        {
            lock (Lock)
                return RunProcess(FastbootExe, device == null ? command : $"-s {device.SerialNumber} {command}");
        }

        public static List<string> GetConnectedAdbDevicesSerialList()
        {
            List<string> devicesSerialList = new List<string>();
            string devicesCommandOutput = ExecuteAdbCommand("devices");

            // If the output is smaller than 29 there are no connected devices
            if (devicesCommandOutput.Length <= 29)
                return devicesSerialList;

            using (StringReader s = new StringReader(devicesCommandOutput))
            {
                while (s.Peek() != -1)
                {
                    string line = s.ReadLine();
                    // Skip certain lines which represent no device serial
                    if (line.StartsWith("List") || line.StartsWith("\r\n") || line.StartsWith("*") || line.Trim() == "" || line.IndexOf('\t') == -1)
                        continue;
                    devicesSerialList.Add(line.Substring(0, line.IndexOf('\t')));
                }
            }
            return devicesSerialList;
        }

        public static string GetBuildProperty(string key, Device device)
        {
            return ExecuteAdbCommand($"shell \"getprop {key}\"", device);
        }

        public static void KillAllAdbProcesses()
        {
            foreach (Process p in Process.GetProcessesByName("adb"))
                p.Kill();
            foreach (Process p in Process.GetProcessesByName("fastboot"))
                p.Kill();
        }

        private static string RunProcess(string executable, string arguments)
        {
            using (Process p = new Process())
            {
                p.StartInfo.FileName = executable;
                p.StartInfo.Arguments = arguments;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                {
                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    p.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            output.AppendLine(e.Data);
                    };
                    p.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                            error.AppendLine(e.Data);
                    };

                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    
                    if (p.WaitForExit(Timeout) && outputWaitHandle.WaitOne(Timeout) && errorWaitHandle.WaitOne(Timeout))
                        return error.ToString().Trim().Length.Equals(0) ? output.ToString().Trim() : error.ToString().Trim();

                    return "TIMEOUT";
                }
            }
        }
    }
}
