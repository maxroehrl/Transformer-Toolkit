/*
 * Update Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;
using Toolkit.Properties;

namespace Toolkit
{
    public partial class UpdateDialog : Form
    {
        public UpdateDialog(string onlineVersion)
        {
            InitializeComponent();
            Icon = Resources.Icon;
            versionLabel.Text += onlineVersion;
        }

        private void yesButton_Click(object sender, EventArgs e)
        {
            yesButton.Hide();
            noButton.Hide();
            label1.Hide();
            loadingSpinner.Show();
            progressLabel.Show();

            progressLabel.Text = "Renaming old toolkit ...";
            File.Move(Process.GetCurrentProcess().MainModule.FileName, NetManager.OldAppFileName);

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += (sender1, e1) =>
                {
                    double bytesIn = Math.Round((Convert.ToDouble(e1.BytesReceived) / 1024), 0);
                    progressLabel.Text = $"Downloading: {bytesIn} KB";
                };
                webClient.DownloadFileCompleted += RestartToolkit;

                progressLabel.Text = "Starting download ...";
                webClient.DownloadFileAsync(NetManager.GetToolkitUrl(), "Transformer Toolkit.exe");
            }
        }

        private void noButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void RestartToolkit(object sender, AsyncCompletedEventArgs args)
        {
            if (args.Error == null)
            {
                progressLabel.Text = "Download completed";
                MessageBox.Show("The toolkit will restart now!", "Update completed", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                Application.Restart();
            }
            else
                MessageBox.Show("Updating toolkit failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}