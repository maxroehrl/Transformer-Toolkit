/*
 * Unlock.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Toolkit
{
    public class Unlock
    {
        private readonly Toolkit _toolkit;
        private readonly Device _device;
        private readonly string _unlockerPath = ResourceManager.TempPath + "Unlocker.apk";

        public Unlock(Toolkit toolkit, Device device)
        {
            _toolkit = toolkit;
            _device = device;

            toolkit.ToggleButtons(false);
            toolkit.ShowLoadingSpinner(true);

            if (device.CodeName == "hammerhead" || device.CodeName == "hammerheadcaf")
            {
                toolkit.Log("Rebooting into bootloader ...");
                Adb.ExecuteAdbCommand("reboot bootloader", device);

                toolkit.Log("Requesting unlock ...");
                toolkit.Log(Adb.ExecuteFastbootCommand("oem unlock", device));

                toolkit.ShowMessageBox("Now please follow the instructions on your device.");

                toolkit.Log("Unlock requested!");
            }
            else
            {
                toolkit.Log("Downloading Asus Unlock Tool v8 ...");
                NetManager.DownloadFileAsync(NetManager.GetUnlockerUrl(), _unlockerPath, toolkit.Log, InstallUnlocker);
            }
            toolkit.ShowLoadingSpinner(false);
            toolkit.ToggleButtons(true);
        }

        private void InstallUnlocker()
        {
            _toolkit.Log("Installing Asus Unlock Tool v8 ...");
            string output = Adb.ExecuteAdbCommand($"install \"{_unlockerPath}\"", _device);
            File.Delete(_unlockerPath);
            _toolkit.Log(output);

            if (output.Contains("Failure"))
            {
                _toolkit.LogError("Installation failed!");
                return;
            }
            _toolkit.Log("Starting unlock app ...");
            Adb.ExecuteAdbShellCommand("am start -n com.asus.unlock/com.asus.unlock.EulaActivity", _device);
            _toolkit.Log("Installation finished!");

            _toolkit.ShowMessageBox("Now you can start the unlock app on your device.\r\n" +
                                    "Please read the information carefully and follow the instructions there.\r\n" +
                                    "When your device is unlocked you can flash a recovery.");

            DialogResult dialogResult =
                _toolkit.ShowMessageBoxYesNo("After you unlocked your device you should generate \r\n" +
                                            "your nvflash blobs to make it unbrickable.\r\n" +
                                            "Would you like to visit the website of AndroidRoot.Mobi?");
            if (dialogResult == DialogResult.Yes)
                Process.Start("https://www.androidroot.mobi/pages/guides/tegra3-guide-nvflash-jellybean/");
        }
    }
}