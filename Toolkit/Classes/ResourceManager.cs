/*
 * ResourceManager.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.IO;
using System.Reflection;

namespace Toolkit
{
    class ResourceManager
    {
        public static readonly string TempPath = Path.GetTempPath() + "\\Transformer Toolkit\\";
        public static bool IsDisposeable = true;

        public static void Dispose()
        {
            if (!IsDisposeable) return;
            Adb.KillAllAdbProcesses();
            if (File.Exists(NetManager.VersionFile))
                File.Delete(NetManager.VersionFile);
        }

        public static void ExtractAdbBinaries()
        {
            if (!Directory.Exists(TempPath))
                Directory.CreateDirectory(TempPath);

            string[] adbExecuteables = { "adb.exe", "AdbWinApi.dll", "AdbWinUsbApi.dll", "fastboot.exe" };
            try
            {
                foreach (string fileName in adbExecuteables)
                {
                    using (Stream s = Assembly.GetCallingAssembly().GetManifestResourceStream("Toolkit.Resources.Adb." + fileName))
                    using (BinaryReader r = new BinaryReader(s))
                    using (FileStream fs = new FileStream(TempPath + fileName, FileMode.OpenOrCreate))
                    using (BinaryWriter w = new BinaryWriter(fs))
                        w.Write(r.ReadBytes((int) s.Length));
                }
            }
            catch (Exception)
            {
                Adb.KillAllAdbProcesses();
            }
        }
    }
}
