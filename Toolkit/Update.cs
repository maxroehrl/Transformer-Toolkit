using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace Toolkit
{
    public class Update
    {
        public Update()
        {
            // Delete old versions
            if (File.Exists("Transformer Toolkit.exe.old"))
                File.Delete("Transformer Toolkit.exe.old");

            // Check if internet is working
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFileCompleted += DownloadComplete;

                    // Get direct download link
                    string downloadUrl = Shared.ResolveUrl(Shared.VersionsUrl);

                    if (downloadUrl != null)
                        webClient.DownloadFileAsync(new Uri(downloadUrl), @"versions.txt");
                }
            }
            else
            {
                MessageBox.Show("This toolkit needs a working internet connection to work properly!", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                // Hide versions.txt
                File.SetAttributes("versions.txt", FileAttributes.Hidden);

                using (StreamReader versions = new StreamReader("versions.txt"))
                {
                    string line;
                    while ((line = versions.ReadLine()) != null)
                    {
                        if (line.StartsWith("toolkit:"))
                        {
                            string onlineVersion = line.Replace("toolkit:", String.Empty);

                            // Check if online version is higher than current version
                            if (Convert.ToInt32(onlineVersion.Replace(".", String.Empty)) >
                                Convert.ToInt32(Application.ProductVersion.Replace(".", String.Empty)))
                                new Thread(() => Application.Run(new UpdateDialog(onlineVersion))).Start();
                            else
                                Shared.StartDialog.Unhide();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Fetching versions failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Shared.StartDialog.Unhide();
            }
        }
    }
}

