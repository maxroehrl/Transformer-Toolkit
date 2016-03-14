/*
 * Update Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Diagnostics;
using System.IO;
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

            var currentFileName = Process.GetCurrentProcess().MainModule.FileName;

            progressLabel.Text = "Renaming old toolkit ...";
            File.Move(currentFileName, NetManager.OldAppFileName);

            progressLabel.Text = "Starting download ...";
            NetManager.DownloadFileAsync(NetManager.GetToolkitUrl(), currentFileName, text => progressLabel.Text = text, RestartToolkit);
        }

        private void noButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private static void RestartToolkit()
        {
            MessageBox.Show("The toolkit will restart now!", "Update completed", MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            Application.Restart();
        }
    }
}