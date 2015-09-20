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
using Toolkit.Properties;

namespace Toolkit
{
    public partial class Toolkit : Form
    {
        private readonly Device _device;

        public Toolkit(Device device)
        {
            _device = device;
            InitializeComponent();
            Icon = Resources.Icon;
        }

        private void Toolkit_Load(object sender, EventArgs e)
        {
            ResourceManager.IsDisposeable = true;
            Text += Application.ProductVersion;
            if (NetManager.IsOutdated)
                Text += " (Update available)";

            DeviceNameLabel.Text += _device.GetDeviceName();
            CodeNameLabel.Text += _device.GetCodeName();
            AndroidVersionLabel.Text += _device.GetAndroidVersion();
            SerialNumberLabel.Text += _device.GetSerialNumber();
            RootedLabel.Text += _device.IsRooted() ? "Yes" : "No";

            if (File.Exists(NetManager.VersionFile))
                twrpButton.Text += NetManager.GetTwrpVersion(_device);
            else
                LogError("Fetching recovery versions failed!");
        }

        private void Toolkit_FormClosing(object sender, FormClosingEventArgs e)
        {
            ResourceManager.Dispose();
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            new Thread(() => new Unlock(this, _device)).Start();
        }

        private void twrpButton_Click(object sender, EventArgs e)
        {
            new Thread(() => new Recovery(this, _device, "twrp")).Start();
        }

        private void customButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Remember if you select a wrong recovery image you can brick your device." +
                            "You have the full responsibility for any damage or fault caused by your decision.",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            Thread t = new Thread(() => new Recovery(this, _device, "custom"));
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
            ToggleButtons(false);
            loadingSpinner.Show();
            if (modeBox.SelectedText == "ADB")
                Adb.ExecuteAdbCommand("reboot", _device);
            else
                Adb.ExecuteFastbootCommand("reboot", _device);
            loadingSpinner.Hide();
            ToggleButtons(true);
        }

        private void rebootRecoveryButton_Click(object sender, EventArgs e)
        {
            ToggleButtons(false);
            loadingSpinner.Show();
            Adb.ExecuteAdbCommand("reboot recovery", _device);
            loadingSpinner.Hide();
            ToggleButtons(true);
        }

        private void rebootBootloaderButton_Click(object sender, EventArgs e)
        {
            ToggleButtons(false);
            loadingSpinner.Show();
            if (modeBox.SelectedText == "ADB")
                Adb.ExecuteAdbCommand("reboot bootloader", _device);
            else
                Adb.ExecuteFastbootCommand("reboot-bootloader", _device);
            loadingSpinner.Hide();
            ToggleButtons(true);
        }

        private void logcatButton_Click(object sender, EventArgs e)
        {
            ToggleButtons(false);
            loadingSpinner.Show();

            if (File.Exists("Logcat.txt"))
                File.Delete("Logcat.txt");

            Log("Saving logcat ...");
            try
            {
                string unixLog = Adb.ExecuteAdbCommand("logcat -d", _device);
                // Convert UNIX text file to DOS text file
                string log = Regex.Replace(unixLog, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
                File.WriteAllText("Logcat.txt", log);
                Process.Start("Logcat.txt");
            }
            catch (Exception)
            {
                LogError("Saving Logcat.txt failed!");
            }
            loadingSpinner.Hide();
            ToggleButtons(true);
        }

        private void screenshotButton_Click(object sender, EventArgs e)
        {
            ToggleButtons(false);
            loadingSpinner.Show();
            try
            {
                Log("Saving screenshot ...");
                Adb.ExecuteAdbShellCommand("screencap -p /sdcard/screen.png", _device);
                string fileName = "Screenshot " + DateTime.Now.ToString("F").Replace(":", "-") + ".png";
                Adb.ExecuteAdbCommand($"pull \"/sdcard/screen.png\" \"{fileName}\"", _device);
                Adb.ExecuteAdbShellCommand("rm /sdcard/screen.png", _device);
                Process.Start(fileName);
            }
            catch (Exception)
            {
                LogError("Saving screenshot failed!");
            }
            loadingSpinner.Hide();
            ToggleButtons(true);
        }

        public void ToggleButtons(bool value)
        {
            Invoke(new MethodInvoker(() =>
            {
                twrpButton.Enabled = value;
                unlockButton.Enabled = value;
                customButton.Enabled = value;
                modeBox.Enabled = value;
                if (value)
                {
                    // Enable the buttons accourding to the modeBox
                    modeBox_SelectedIndexChanged(new object(), new EventArgs());
                }
                else
                {
                    rebootButton.Enabled = false;
                    rebootRecoveryButton.Enabled = false;
                    rebootBootloaderButton.Enabled = false;
                }
                logcatButton.Enabled = value;
                screenshotButton.Enabled = value;
            }));
        }

        public void Log(string text)
        {
            Invoke(new MethodInvoker(() => logBox.AppendText(text + Environment.NewLine)));
        }

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

        public void ShowLoadingSpinner(bool value)
        {
            Invoke(value
                ? new MethodInvoker(() => loadingSpinner.Show())
                : (() => loadingSpinner.Hide()));
        }

        public void ShowMessageBox(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public DialogResult ShowMessageBoxYesNo(string message)
        {
            return MessageBox.Show(message, "Information", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        }
    }
}