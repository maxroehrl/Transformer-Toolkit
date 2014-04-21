using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows.Forms;

namespace Toolkit
{
    public partial class UpdateDialog : Form
    {
        private readonly string _newVersion;

        public UpdateDialog(string onlineVersion)
        {
            InitializeComponent();
            Shared.UpdateDialog = this;
            Icon = Properties.Resources.Icon;
            _newVersion = onlineVersion;
        }

        private void UpdateDialog_Load(object sender, EventArgs e)
        {
            // Show outdated version in titlebar
            Shared.IsOutdated = true;
            Shared.StartDialog.VersionIsOutdated();

            // Add new version to label
            versionLabel.Text += _newVersion;
        }

        private void yesButton_Click(object sender, EventArgs e)
        {
            // Change element of the form
            yesButton.Hide();
            noButton.Hide();
            progressBar.Show();
            progressLabel.Show();

            ProgressLabelText("Renaming old toolkit ...");
            File.Move(Process.GetCurrentProcess().MainModule.FileName, "Transformer Toolkit.exe.old");

            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += Shared.ProgressChanged;
                webClient.DownloadFileCompleted += RestartToolkit;

                ProgressLabelText("Getting dircet download link ...");
                string downloadUrl = Shared.ResolveUrl(Shared.ToolkitUrl);

                if (downloadUrl != null)
                    webClient.DownloadFileAsync(new Uri(downloadUrl), @"Transformer Toolkit.exe");
            }
        }

        private void noButton_Click(object sender, EventArgs e)
        {
            // Show startDialog
            Shared.StartDialog.Unhide();
            Close();
        }

        public void RestartToolkit(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ProgressLabelText("Download completed");
                MessageBox.Show("The toolkit will restart now!", "Update completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
            }
            else
            {
                MessageBox.Show("Updating toolkit failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Change the text of the progress label
        public void ProgressLabelText(string text)
        {
            Invoke((new MethodInvoker(() => progressLabel.Text = text)));
        }

        // Change progress of the progressBar
        public void ProgressBarValue(int progress)
        {
            Invoke(new MethodInvoker(() => progressBar.Value = progress));
        }
    }
}
