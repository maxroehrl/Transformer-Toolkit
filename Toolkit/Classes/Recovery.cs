/*
 * Recovery.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.IO;
using System.Windows.Forms;

namespace Toolkit
{
    public class Recovery
    {
        private readonly Toolkit _toolkit;
        private readonly Device _device;
        private readonly string _recoveryPath = ResourceManager.TempPath + "recovery.img";

        public Recovery(Toolkit toolkit, Device device, string recovery)
        {
            _toolkit = toolkit;
            _device = device;

            _toolkit.ToggleButtons(false);
            _toolkit.ShowLoadingSpinner(true);

            if (recovery == "custom")
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.img)|*.img|Blob files (*.blob)|*.blob",
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    FlashRecovery(openFileDialog.FileName);
                else
                {
                    _toolkit.ToggleButtons(true);
                    _toolkit.ShowLoadingSpinner(false);
                }
            }
            else
            {
                if (File.Exists(_recoveryPath))
                    File.Delete(_recoveryPath);

                _toolkit.Log($"Starting download of twrp-{NetManager.GetTwrpVersion(_device)}-{_device.CodeName}.img ...");
                NetManager.DownloadFileAsync(NetManager.GetTwrpUrl(_device), _recoveryPath, _toolkit.Log, FlashRecovery);
            }
        }

        private void FlashRecovery()
        {
            FlashRecovery(_recoveryPath);
        }

        private void FlashRecovery(string path)
        {
            _toolkit.Log("Rebooting into bootloader ...");
            Adb.ExecuteAdbCommand("reboot bootloader", _device);

            _toolkit.Log("Flashing recovery ...");
            string output = Adb.ExecuteFastbootCommand($"flash recovery \"{path}\"", _device);
            if (File.Exists(path))
                File.Delete(path);
            _toolkit.Log(output);

            if (output.Contains("FAILED"))
                _toolkit.LogError("Flashing recovery with fastboot failed!");
            else
                _toolkit.Log("Recovery successfully flashed!");

            _toolkit.Log("Rebooting ...");
            Adb.ExecuteFastbootCommand("reboot", _device);
            
            _toolkit.ShowLoadingSpinner(false);
            _toolkit.ToggleButtons(true);
        }
    }
}