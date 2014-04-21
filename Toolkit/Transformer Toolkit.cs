using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using RegawMOD.Android;

namespace Toolkit
{
    public partial class Toolkit : Form
    {
        public Toolkit()
        {
            InitializeComponent();
            Shared.Toolkit = this;
            Icon = Properties.Resources.Icon;
        }

        private void Toolkit_Load(object sender, EventArgs e)
        {
            // Do not show this label until it is used
            ProgressLabelText(String.Empty);

            // Now resources can be disposed when closing this from
            Shared.IsDisposeable = true;

            // Show version in title bar
            Text += Application.ProductVersion;

            // Show outdated version in titlebar
            if (Shared.IsOutdated)
                Text += " (Update available)";

            // Show device infos
            DeviceNameLabel.Text += Shared.DeviceName;
            CodeNameLabel.Text += Shared.CodeName;
            AndroidVersionLabel.Text += Shared.AndroidVersion;
            SerialNumberLabel.Text += Shared.SerialNumber;
            RootedLabel.Text += Shared.IsRooted;

            if (File.Exists("versions.txt"))
            {
                using (StreamReader versions = new StreamReader("versions.txt"))
                {
                    // Getting the recovery versions out of the versions.txt
                    string line;
                    while ((line = versions.ReadLine()) != null)
                    {
                        if (line.StartsWith("twrp-" + Shared.CodeName + ":"))
                            twrpButton.Text += line.Replace("twrp-" + Shared.CodeName + ":", String.Empty);
                        if (line.StartsWith("cwm-" + Shared.CodeName + ":"))
                            cwmButton.Text += line.Replace("cwm-" + Shared.CodeName + ":", String.Empty);
                        if (line.StartsWith("philz-" + Shared.CodeName + ":"))
                            philzButton.Text += line.Replace("philz-" + Shared.CodeName + ":", String.Empty);
                    }
                }
                File.Delete("versions.txt");
                Shared.Log("Fetching recovery versions completed");
            }
            else
            {
                Shared.LogError("Fetching recovery versions failed!");
            }
        }

        private void Toolkit_FormClosing(object sender, FormClosingEventArgs e)
        {
            Shared.CleanUp();
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            new Thread(() => new Unlock()).Start();
        }

        private void twrpButton_Click(object sender, EventArgs e)
        {
            new Thread(() => new Recovery("twrp")).Start();
        }

        private void cwmButton_Click(object sender, EventArgs e)
        {
            new Thread(() => new Recovery("cwm")).Start();
        }

        private void philzButton_Click(object sender, EventArgs e)
        {
            new Thread(() => new Recovery("philz")).Start();
        }

        private void customButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remember if you select a wrong recovery image you can brick your device." +
                            "You have the full responsibility for any damage or fault caused by your decision.",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            Thread t = new Thread(() => new Recovery("custom"));
            // Thread must be started in SingleThreadedApartment because of the choose file dialog
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void modeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (modeBox.SelectedIndex)
            {
                case 0: // ADB
                    rebootButton.Enabled = true;
                    rebootRecoveryButton.Enabled = true;
                    rebootBootloaderButton.Enabled = true;
                    break;
                case 1: // Fastboot cannot reboot into recovery
                    rebootButton.Enabled = true;
                    rebootRecoveryButton.Enabled = false;
                    rebootBootloaderButton.Enabled = true;
                    break;
            }
        }

        private void rebootButton_Click(object sender, EventArgs e)
        {
            if(modeBox.SelectedText == "ADB")
                Shared.Device.Reboot();
            else
                Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(Shared.Device, "reboot"));
        }

        private void rebootRecoveryButton_Click(object sender, EventArgs e)
        {
            Shared.Device.RebootRecovery();
        }

        private void rebootBootloaderButton_Click(object sender, EventArgs e)
        {
            if (modeBox.SelectedText == "ADB")
                Shared.Device.RebootBootloader();
            else
                Fastboot.ExecuteFastbootCommandNoReturn(Fastboot.FormFastbootCommand(Shared.Device, "reboot-bootloader"));
        }

        private void romButton_Click(object sender, EventArgs e)
        {
            switch (Shared.CodeName)
            {
                case "tf700t":
                    Process.Start("http://forum.xda-developers.com/transformer-tf700/development");
                    break;
                case "tf300t":
                case "me301t":
                    Process.Start("http://forum.xda-developers.com/transformer-tf300t/development");
                    break;
                case "hammerhead":
                    Process.Start("http://forum.xda-developers.com/google-nexus-5/development");
                    break;
                default:
                    Process.Start("http://forum.xda-developers.com");
                    break;
            }
        }

        // Enable or disable all buttons while doing background tasks
        public void ToggleButtons(bool value)
        {
            Invoke(new MethodInvoker(() =>
            {
                twrpButton.Enabled = value;
                cwmButton.Enabled = value;
                philzButton.Enabled = value;
                unlockButton.Enabled = value;
                customButton.Enabled = value;
                if (value)
                {
                    modeBox.Enabled = true;
                    modeBox_SelectedIndexChanged(new object(), new EventArgs());
                }
                else
                {
                    modeBox.Enabled = false;
                    rebootButton.Enabled = false;
                    rebootRecoveryButton.Enabled = false;
                    rebootBootloaderButton.Enabled = false;
                }
            }));
        }

        // Show info to the user
        public void Log(string text)
        {
            Invoke(new MethodInvoker(() => logBox.AppendText(text + Environment.NewLine)));
        }

        // Show error to the user
        public void LogError(string text)
        {
            Invoke(new MethodInvoker(() =>
            {
                logBox.SelectionStart = logBox.TextLength;
                logBox.SelectionLength = 0;
                logBox.SelectionColor = Color.Red;
                logBox.AppendText(text + Environment.NewLine);
                logBox.SelectionColor = logBox.ForeColor;
            }));
        }

        // Use the wait cursor to show running background processes
        public void WaitCursor(bool value)
        {
            UseWaitCursor = value;
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
