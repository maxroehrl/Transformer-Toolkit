/*
 * ResourceManager.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.IO;
using System.Reflection;

namespace Toolkit
{
    internal static class ResourceManager
    {
        public static readonly string TempPath = Path.GetTempPath() + "\\Transformer Toolkit\\";
        public static readonly string[] AdbExecuteables = { "adb.exe", "AdbWinApi.dll", "AdbWinUsbApi.dll", "fastboot.exe" };
        public static bool IsDisposeable = true;

        public static void Dispose()
        {
            if (!IsDisposeable)
                return;
            ProcessManager.KillAllAdbProcesses();
            if (File.Exists(NetManager.ManifestFile))
                File.Delete(NetManager.ManifestFile);
        }

        public static void ExtractAdbBinaries()
        {
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);
            try
            {
                foreach (var fileName in AdbExecuteables)
                    using (var s = Assembly.GetCallingAssembly().GetManifestResourceStream("Toolkit.Resources.Adb." + fileName))
                        if (s != null)
                            using (var r = new BinaryReader(s))
                            using (var fs = new FileStream(TempPath + fileName, FileMode.OpenOrCreate))
                            using (var w = new BinaryWriter(fs))
                                w.Write(r.ReadBytes((int) s.Length));
            }
            catch (Exception)
            {
                ProcessManager.KillAllAdbProcesses();
            }
        }
    }
}
