/*
 * Recovery.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.IO;
using System.Windows.Forms;

namespace Toolkit
{
    internal static class Recovery
    {
        public const string Twrp = "twrp";
        public const string Custom = "custom";
        private static readonly string RecoveryPath = ResourceManager.TempPath + "recovery.img";
        private static Toolkit _toolkit;
        private static Device _device;

        public static void FetchRecovery(Toolkit toolkit, Device device, string recovery)
        {
            _toolkit = toolkit;
            _device = device;

            if (recovery.Equals(Custom))
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.img)|*.img|Blob files (*.blob)|*.blob",
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    FlashRecovery(openFileDialog.FileName);
                _toolkit.InvokeToggleButtons(true);
            }
            else if (recovery.Equals(Twrp))
            {
                if (File.Exists(RecoveryPath))
                    File.Delete(RecoveryPath);
                _toolkit.InvokeLog($"Starting download of twrp-{NetManager.GetTwrpVersion(device)}-{device.CodeName}.img ...");
                NetManager.DownloadFileAsync(NetManager.GetTwrpUrl(device), RecoveryPath, _toolkit.InvokeLog, FlashRecovery);
            }
        }

        private static void FlashRecovery()
        {
            FlashRecovery(RecoveryPath);
        }

        private static void FlashRecovery(string path)
        {
            _toolkit.InvokeLog("Rebooting into bootloader ...");
            Adb.ExecuteCommand("reboot bootloader", _device);

            _toolkit.InvokeLog("Flashing recovery ...");
            var output = Fastboot.ExecuteCommand($"flash recovery \"{path}\"", _device);
            if (File.Exists(RecoveryPath))
                File.Delete(RecoveryPath);
            _toolkit.InvokeLog(output);

            if (output.Contains("FAILED"))
                _toolkit.InvokeLogError("Flashing recovery with fastboot failed!");
            else
                _toolkit.InvokeLog("Recovery successfully flashed!");

            _toolkit.InvokeLog("Rebooting ...");
            Fastboot.ExecuteCommand("reboot", _device);
        }
    }
}