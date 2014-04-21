using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using RegawMOD.Android;

namespace Toolkit
{
    public class Unlock
    {
        public Unlock()
        {
            Shared.ToggleButtons(false);
            Shared.WaitCursor(true);

            if (Shared.CodeName == "hammerhead")
            {
                Shared.ProgressBarValue(0);

                Shared.ProgressLabelText("Rebooting into bootloader ...");
                Shared.Device.RebootBootloader();
                Shared.ProgressBarValue(20);

                Shared.ProgressLabelText("Requesting unlock ...");
                Shared.Log(Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(Shared.Device, "oem unlock")));
                Shared.ProgressBarValue(80);

                MessageBox.Show("Now please follow the instructions on your device.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                Shared.Log("Unlock requested!");
                Shared.ProgressBarValue(100);

                Shared.ToggleButtons(true);
                Shared.WaitCursor(false);
            }
            else
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += Shared.ProgressChanged;
                    webClient.DownloadFileCompleted += DownloadCompleted;

                    Shared.ProgressLabelText("Getting direct download link ...");
                    string downloadUrl = Shared.ResolveUrl(Shared.UnlockToolUrl);

                    if (downloadUrl != null)
                    {
                        webClient.DownloadFileAsync(new Uri(downloadUrl), @"AsusUnlock.apk");
                    }
                    else
                    {
                        Shared.ProgressBarValue(100);
                        Shared.LogError("Redirection failed!");
                        Shared.ToggleButtons(true);
                        Shared.WaitCursor(false);
                    }
                }
            }
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Shared.ProgressLabelText("Installing Asus Unlock Tool v8 ...");
            Shared.ProgressBarValue(10);

            string output = Adb.ExecuteAdbCommand(Adb.FormAdbCommand(Shared.Device, "install AsusUnlock.apk"), true);
            File.Delete("AsusUnlock.apk");

            Shared.ProgressLabelText(String.Empty);
            Shared.Log(output);

            if (!output.Contains("Failure"))
            {
                Shared.Log("Installation finished!");

                MessageBox.Show("Now you can start the unlock app on your device.\r\n" +
                                "Please read the information carefully and follow the instructions there.\r\n" +
                                "When your device is unlocked you can flash a recovery.", "Info",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                Shared.LogError("Installation failed!");
            }
            Shared.ProgressBarValue(100);
            Shared.ToggleButtons(true);
            Shared.WaitCursor(false);
        }
    }
}
