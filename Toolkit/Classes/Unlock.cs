/*
 * Unlock.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Toolkit
{
    internal static class Unlock
    {
        private static readonly string UnlockerPath = ResourceManager.TempPath + "Unlocker.apk";
        private static Toolkit _toolkit;
        private static Device _device;

        public static void RequestUnlock(Toolkit toolkit, Device device)
        {
            _toolkit = toolkit;
            _device = device;

            switch (device.CodeName)
            {
                case "hammerhead":
                case "hammerheadcaf":
                    UnlockNexusBootloader();
                    break;
                case "tf700t":
                case "tf300t":
                case "me301t":
                    toolkit.InvokeLog("Downloading Asus Unlock Tool v8 ...");
                    NetManager.DownloadFileAsync(NetManager.GetUnlockerUrl(), UnlockerPath, _toolkit.InvokeLog, InstallUnlocker);
                    break;
                default:
                    toolkit.InvokeLogError("Unlocking your device is not possible with this tool");
                    toolkit.InvokeToggleButtons(true);
                    break;
            }
        }

        private static void UnlockNexusBootloader()
        {
            _toolkit.InvokeLog("Rebooting into bootloader ...");
            Adb.ExecuteCommand("reboot bootloader", _device);

            _toolkit.InvokeLog("Requesting unlock ...");
            _toolkit.InvokeLog(Fastboot.ExecuteCommand("oem unlock", _device));

            _toolkit.ShowMessageBox("Now please follow the instructions on your device.");

            _toolkit.InvokeLog("Unlock requested!");
            _toolkit.InvokeToggleButtons(true);
        }

        private static void InstallUnlocker()
        {
            _toolkit.InvokeLog("Installing Asus Unlock Tool v8 ...");
            var output = Adb.ExecuteCommand($"install \"{UnlockerPath}\"", _device);
            File.Delete(UnlockerPath);
            _toolkit.InvokeLog(output);

            if (output.Contains("Failure"))
            {
                _toolkit.InvokeLogError("Installation failed!");
                _toolkit.InvokeToggleButtons(true);
                return;
            }
            _toolkit.InvokeLog("Starting unlock app ...");
            Adb.ExecuteShellCommand("am start -n com.asus.unlock/com.asus.unlock.EulaActivity", _device);
            _toolkit.InvokeLog("Installation finished!");

            _toolkit.ShowMessageBox("Now you can start the unlock app on your device.\r\n" +
                                    "Please read the information carefully and follow the instructions there.\r\n" +
                                    "When your device is unlocked you can flash a recovery.");

            var dialogResult = _toolkit.ShowMessageBoxYesNo("After you unlocked your device you should generate \r\n" +
                                                            "your nvflash blobs to make it unbrickable.\r\n" +
                                                            "Would you like to visit the website of AndroidRoot.Mobi?");
            if (dialogResult == DialogResult.Yes)
                Process.Start("https://www.androidroot.mobi/pages/guides/tegra3-guide-nvflash-jellybean/");

            _toolkit.InvokeToggleButtons(true);
        }
    }
}