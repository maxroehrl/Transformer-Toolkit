/*
 * Transformer Toolkit.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using RegawMOD.Android;
using Toolkit.Properties;

namespace Toolkit
{
    public partial class Toolkit : Form
    {
        public Toolkit()
        {
            InitializeComponent();
            Shared.Toolkit = this;
            Icon = Resources.Icon;
        }

        #region Event listener

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
                using (var versions = new StreamReader("versions.txt"))
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
                Shared.LogError("Fetching recovery versions failed!");
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

            var t = new Thread(() => new Recovery("custom"));
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
            if (modeBox.SelectedText == "ADB")
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(Shared.Device, "reboot").WithTimeout(1000));
            else
            {
                Fastboot.ExecuteFastbootCommandNoReturn(
                    Fastboot.FormFastbootCommand(Shared.Device, "reboot").WithTimeout(1000));
            }
        }

        private void rebootRecoveryButton_Click(object sender, EventArgs e)
        {
            Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(Shared.Device, "reboot recovery").WithTimeout(1000));
        }

        private void rebootBootloaderButton_Click(object sender, EventArgs e)
        {
            if (modeBox.SelectedText == "ADB")
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbCommand(Shared.Device, "reboot bootloader").WithTimeout(1000));
            else
            {
                Fastboot.ExecuteFastbootCommandNoReturn(
                    Fastboot.FormFastbootCommand(Shared.Device, "reboot-bootloader").WithTimeout(1000));
            }
        }

        private void logcatButton_Click(object sender, EventArgs e)
        {
            WaitCursor(true);
            ToggleButtons(false);

            // Delete old logcat
            if (File.Exists("Logcat.txt"))
                File.Delete("Logcat.txt");

            ProgressLabelText("Saving logcat ...");
            string unixLog = Adb.ExecuteAdbCommand(Adb.FormAdbCommand(Shared.Device, "logcat -d"), true);

            // Convert UNIX text file to DOS text file
            string log = Regex.Replace(unixLog, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);

            // Save logcat
            File.WriteAllText("Logcat.txt", log);

            // Open logcat
            try
            {
                Process.Start("Logcat.txt");
            }
            catch (Exception)
            {
                LogError("Saving Logcat.txt failed!");
            }

            ProgressLabelText(String.Empty);
            WaitCursor(false);
            ToggleButtons(true);
        }

        private void screenshotButton_Click(object sender, EventArgs e)
        {
            WaitCursor(true);
            ToggleButtons(false);

            // Show screenshot
            try
            {
                ProgressLabelText("Saving screenshot ...");
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbShellCommand(Shared.Device, false,
                    "screencap -p /sdcard/screen.png"));
                string fileName = "Screenshot " + DateTime.Now.ToString("F").Replace(":", "-") + ".png";
                Shared.Device.PullFile("/sdcard/screen.png", fileName);
                Adb.ExecuteAdbCommandNoReturn(Adb.FormAdbShellCommand(Shared.Device, false, "rm /sdcard/screen.png"));
                Process.Start(fileName);
            }
            catch (Exception)
            {
                LogError("Saving screenshot failed!");
            }

            ProgressLabelText(String.Empty);
            WaitCursor(false);
            ToggleButtons(true);
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

        #endregion

        #region Public methods

        /// <summary>
        ///     Enable or disable all buttons of this form
        /// </summary>
        /// <param name="value">True to enable and false to disable all buttons</param>
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
                logcatButton.Enabled = value;
                screenshotButton.Enabled = value;
            }));
        }

        /// <summary>
        ///     Log an information in the logBox in this form
        /// </summary>
        /// <param name="text">The printed text</param>
        public void Log(string text)
        {
            Invoke(new MethodInvoker(() => logBox.AppendText(text + Environment.NewLine)));
        }

        /// <summary>
        ///     Log an error in the logBox in this form
        /// </summary>
        /// <param name="text">The printed text</param>
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

        /// <summary>
        ///     Use the wait cursor to show running background processes
        /// </summary>
        /// <param name="value">True to enable and false to disable the waiting cursor</param>
        public void WaitCursor(bool value)
        {
            UseWaitCursor = value;
        }

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