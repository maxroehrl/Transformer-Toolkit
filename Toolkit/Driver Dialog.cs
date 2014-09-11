/*
 * Driver Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Ionic.Zip;
using Toolkit.Properties;

namespace Toolkit
{
    /// <summary>
    ///     A dialog which shows the download and installation of a driver
    /// </summary>
    public partial class DriverDialog : Form
    {
        public DriverDialog()
        {
            InitializeComponent();
            Shared.DriverDialog = this;
            Icon = Resources.Icon;
        }

        #region Event listener

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            if (File.Exists("Drivers.zip"))
                File.Delete("Drivers.zip");
            if (Directory.Exists("Drivers"))
                Directory.Delete("Drivers", true);

            // Download Drivers.zip
            using (var webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += Shared.ProgressChanged;
                webClient.DownloadFileCompleted += DownloadComplete;

                ProgressLabelText("Getting direct download link ...");
                string downloadUrl = Shared.ResolveUrl(Shared.DriverUrl);

                if (downloadUrl != null)
                    webClient.DownloadFileAsync(new Uri(downloadUrl), @"Drivers.zip");
            }
        }

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                string path = Directory.GetCurrentDirectory() + @"\Drivers";

                using (var zipFile = new ZipFile("Drivers.zip"))
                {
                    ProgressLabelText("Extracting driver ...");
                    zipFile.ExtractAll(path);
                }
                ProgressLabelText("Installing driver ...");
                Shared.InstallHinfSection(IntPtr.Zero, IntPtr.Zero, path + @"\android_winusb.inf", 0);

                Hide();
                File.Delete("Drivers.zip");
                Directory.Delete("Drivers", true);
                MessageBox.Show("Drivers are installed now. Please reconnect your device.", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            }
            else
                MessageBox.Show("Download failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        #endregion

        #region Public methods

        /// <summary>
        ///     Change the text of the progressLabel
        /// </summary>
        /// <param name="text">Displayed text</param>
        public void ProgressLabelText(string text)
        {
            Invoke((new MethodInvoker(() => progressLabel.Text = text)));
        }

        /// <summary>
        ///     Change the progress of the progressBar
        /// </summary>
        /// <param name="progress">Value from 0 to 100</param>
        public void ProgressBarValue(int progress)
        {
            Invoke(new MethodInvoker(() => progressBar.Value = progress));
        }

        #endregion
    }
}