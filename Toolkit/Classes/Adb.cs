/*
 * Adb.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.Collections.Generic;
using System.IO;

namespace Toolkit
{
    internal static class Adb
    {
        private static readonly object Lock = new object();
        private static readonly string AdbExe = ResourceManager.TempPath + "adb.exe";
        
        public static string ExecuteCommand(string command, Device device = null)
        {
            lock (Lock)
                return ProcessManager.RunProcess(AdbExe, device == null ? command : $"-s {device.SerialNumber} {command}");
        }

        public static string ExecuteShellCommand(string command, Device device, bool root = false)
        {
            lock (Lock)
                return ProcessManager.RunProcess(AdbExe, root ? $"-s {device.SerialNumber} shell \"su -c \"{command}\"\""
                                                              : $"-s {device.SerialNumber} shell \"{command}\"");
        }

        public static string GetBuildProperty(string key, Device device)
        {
            return ExecuteCommand($"shell \"getprop {key}\"", device);
        }

        public static List<string> GetConnectedAdbDevicesSerialList()
        {
            var devicesSerialList = new List<string>();
            var devicesCommandOutput = ExecuteCommand("devices");

            // If the output is smaller than 29 there are no connected devices
            if (devicesCommandOutput.Length <= 29)
                return devicesSerialList;

            using (var s = new StringReader(devicesCommandOutput))
            {
                while (s.Peek() != -1)
                {
                    var line = s.ReadLine();
                    // Skip certain lines which represent no device serial
                    if (line == null || line.StartsWith("List") || line.StartsWith("\r\n") 
                        || line.StartsWith("*") || line.Trim() == "" || line.IndexOf('\t') == -1)
                        continue;
                    devicesSerialList.Add(line.Substring(0, line.IndexOf('\t')));
                }
            }
            return devicesSerialList;
        }
    }
}
