/*
 * Recovery.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using RegawMOD.Android;

namespace Toolkit
{
    public class Recovery
    {
        /// <summary>
        ///     Download and and flash a recovery over fastboot
        /// </summary>
        /// <param name="recovery">The name of the desired recovery</param>
        public Recovery(string recovery)
        {
            Shared.ToggleButtons(false);
            Shared.WaitCursor(true);

            if (recovery == "custom")
            {
                // Choose a recovery image
                var openFileDialog = new OpenFileDialog
                {
                    // Only allow .blob and .img files
                    Filter = "Image files (*.img)|*.img|Blob files (*.blob)|*.blob",
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                    FlashRecovery("\"" + openFileDialog.FileName + "\"");
                else
                {
                    Shared.LogError("No file selected!");
                    Shared.ToggleButtons(true);
                    Shared.WaitCursor(false);
                }
            }
            else
            {
                // Delete old file
                if (File.Exists("recovery.img"))
                    File.Delete("recovery.img");

                // Download selected recovery.img
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += DownloadCompleted;
                    webClient.DownloadProgressChanged += Shared.ProgressChanged;

                    string url = String.Empty;

                    // TF300T and ME301T use the same recoveries
                    if (recovery == "twrp" && Shared.CodeName == Shared.ValidDevices[0])
                        url = Shared.TwrpTf700TUrl;
                    else if (recovery == "twrp" && Shared.CodeName == "tf300t")
                        url = Shared.TwrpTf300TUrl;
                    else if (recovery == "twrp" && Shared.CodeName == "me301t")
                        url = Shared.TwrpTf300TUrl;
                    else if (recovery == "twrp" && Shared.CodeName == "hammerhead")
                        url = Shared.TwrpHammerheadUrl;
                    else if (recovery == "cwm" && Shared.CodeName == "tf700t")
                        url = Shared.CwmTf700TUrl;
                    else if (recovery == "cwm" && Shared.CodeName == "tf300t")
                        url = Shared.CwmTf300TUrl;
                    else if (recovery == "cwm" && Shared.CodeName == "me301t")
                        url = Shared.CwmTf300TUrl;
                    else if (recovery == "cwm" && Shared.CodeName == "hammerhead")
                        url = Shared.CwmHammerheadUrl;
                    else if (recovery == "philz" && Shared.CodeName == "tf700t")
                        url = Shared.PhilzTf700TUrl;
                    else if (recovery == "philz" && Shared.CodeName == "tf300t")
                        url = Shared.PhilzTf300TUrl;
                    else if (recovery == "philz" && Shared.CodeName == "me301t")
                        url = Shared.PhilzTf300TUrl;
                    else if (recovery == "philz" && Shared.CodeName == "hammerhead")
                        url = Shared.PhilzHammerheadUrl;

                    if (url != String.Empty)
                    {
                        Shared.ProgressLabelText("Getting direct download link ...");
                        string downloadUrl = Shared.ResolveUrl(url);

                        if (downloadUrl != null)
                        {
                            Shared.Log("Starting download of " + recovery + "-" + Shared.CodeName + ".img");
                            webClient.DownloadFileAsync(new Uri(downloadUrl), @"recovery.img");
                        }
                        else
                        {
                            Shared.LogError("Redirection failed!");
                            Shared.ToggleButtons(true);
                            Shared.WaitCursor(false);
                        }
                    }
                    else
                    {
                        Shared.LogError("No suiting recovery file found!");
                        Shared.ToggleButtons(true);
                        Shared.WaitCursor(false);
                    }
                }
            }
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                Shared.Log("Download complete");
                FlashRecovery("recovery.img");
            }
            else
                Shared.LogError(e.Error.Message + Environment.NewLine + "Download failed!");
        }

        private void FlashRecovery(string path)
        {
            Shared.ProgressBarValue(0);

            Shared.ProgressLabelText("Rebooting into bootloader ...");
            Shared.Device.RebootBootloader();
            Shared.ProgressBarValue(20);

            Shared.ProgressLabelText("Flashing recovery ...");
            string output =
                Fastboot.ExecuteFastbootCommand(Fastboot.FormFastbootCommand(Shared.Device, "flash recovery", path));
            Shared.Log(output);
            Shared.ProgressBarValue(60);

            if (output.Contains("FAILED"))
                Shared.LogError("Flashing recovery with fastboot failed!");
            else
                Shared.Log("Recovery successfully flashed!");

            Shared.ProgressLabelText("Rebooting ...");
            Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(Shared.Device, "reboot"));
            Shared.ProgressBarValue(80);

            Shared.ProgressLabelText("Cleaning up ...");
            if (File.Exists("recovery.img"))
                File.Delete("recovery.img");
            Shared.ProgressBarValue(100);

            Shared.ProgressLabelText(String.Empty);
            Shared.ToggleButtons(true);
            Shared.WaitCursor(false);
        }
    }
}