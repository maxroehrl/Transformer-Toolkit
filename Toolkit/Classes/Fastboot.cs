namespace Toolkit
{
    internal static class Fastboot
    {
        private static readonly string FastbootExe = ResourceManager.TempPath + "fastboot.exe";
        private static readonly object Lock = new object();

        public static string ExecuteCommand(string command, Device device = null)
        {
            lock (Lock)
                return ProcessManager.RunProcess(FastbootExe, device == null ? command : $"-s {device.SerialNumber} {command}");
        }
    }
}
