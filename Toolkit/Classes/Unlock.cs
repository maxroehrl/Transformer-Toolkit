/*
 * Unlock.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Toolkit
{
    public class Unlock
    {
        private readonly string _unlockerPath = ResourceManager.TempPath + "Unlocker.apk";

        public Unlock(Toolkit toolkit, Device device)
        {
            toolkit.ToggleButtons(false);
            toolkit.ShowLoadingSpinner(true);

            if (device.GetCodeName() == "hammerhead" || device.GetCodeName() == "hammerheadcaf")
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
                using (WebClient webClient = new WebClient())
                {
                    int counter = 200;
                    webClient.DownloadProgressChanged += (sender, e) =>
                    {
                        if (counter++%500 != 0) return;
                        double bytesIn = Math.Round((Convert.ToDouble(e.BytesReceived)/1024), 0);
                        toolkit.Log($"Downloading: {bytesIn} KB");
                    };
                    webClient.DownloadFileCompleted += (sender, e) =>
                    {
                        toolkit.Log("Installing Asus Unlock Tool v8 ...");
                        string output = Adb.ExecuteAdbCommand($"install \"{_unlockerPath}\"", device);
                        File.Delete(_unlockerPath);
                        toolkit.Log(output);

                        if (!output.Contains("Failure"))
                        {
                            toolkit.Log("Starting unlock app ...");
                            Adb.ExecuteAdbShellCommand("am start -n com.asus.unlock/com.asus.unlock.EulaActivity", device);
                            toolkit.Log("Installation finished!");

                            toolkit.ShowMessageBox("Now you can start the unlock app on your device.\r\n" +
                                                   "Please read the information carefully and follow the instructions there.\r\n" +
                                                   "When your device is unlocked you can flash a recovery.");

                            DialogResult dialogResult =
                                toolkit.ShowMessageBoxYesNo("After you unlocked your device you should generate \r\n" +
                                                            "your nvflash blobs to make it unbrickable.\r\n" +
                                                            "Would you like to visit the website of AndroidRoot.Mobi?");
                            if (dialogResult == DialogResult.Yes)
                                Process.Start("https://www.androidroot.mobi/pages/guides/tegra3-guide-nvflash-jellybean/");
                        }
                        else
                            toolkit.LogError("Installation failed!");
                    };
                    toolkit.Log("Downloading Asus Unlock Tool v8 ...");
                    webClient.DownloadFileAsync(NetManager.GetUnlockerUrl(), _unlockerPath);
                }
            }
            toolkit.ShowLoadingSpinner(false);
            toolkit.ToggleButtons(true);
        }
    }
}