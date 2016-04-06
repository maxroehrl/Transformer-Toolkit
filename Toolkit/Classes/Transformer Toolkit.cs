/*
 * Transformer Toolkit.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
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

            DeviceNameLabel.Text += _device.DeviceName;
            CodeNameLabel.Text += _device.CodeName;
            AndroidVersionLabel.Text += _device.AndroidVersion;
            SerialNumberLabel.Text += _device.SerialNumber;
            RootedLabel.Text += _device.Rooted ? "Yes" : "No";

            if (File.Exists(NetManager.ManifestFile))
                twrpButton.Text += NetManager.GetTwrpVersion(_device);
            else
                InvokeLogError("Fetching recovery versions failed!");
        }

        private void Toolkit_FormClosing(object sender, FormClosingEventArgs e)
        {
            ResourceManager.Dispose();
        }

        private void unlockButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            var bw = new BackgroundWorker();
            bw.DoWork += (sender1, e1) => Unlock.RequestUnlock(this, _device);
            bw.RunWorkerAsync();
        }

        private void twrpButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            var bw = new BackgroundWorker();
            bw.DoWork += (sender1, e1) => Recovery.FetchRecovery(this, _device, Recovery.Twrp);
            bw.RunWorkerAsync();
        }

        private void customButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            MessageBox.Show("Remember if you select a wrong recovery image you can brick your device." +
                            "You have the full responsibility for any damage or fault caused by your decision.",
                "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

            // Thread must be started in SingleThreadedApartment because of the ChooseFileDialog
            var t = new Thread(() => Recovery.FetchRecovery(this, _device, Recovery.Custom));
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void modeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            rebootButton.Enabled = true;
            rebootRecoveryButton.Enabled = modeBox.SelectedIndex == 0;
            rebootBootloaderButton.Enabled = true;
        }

        private void rebootButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            if (modeBox.SelectedIndex == 0)
                Adb.ExecuteCommand("reboot", _device);
            else if (modeBox.SelectedIndex == 1)
                Fastboot.ExecuteCommand("reboot", _device);
            InvokeToggleButtons(true);
        }

        private void rebootRecoveryButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            if (modeBox.SelectedIndex == 0)
                Adb.ExecuteCommand("reboot recovery", _device);
            InvokeToggleButtons(true);
        }

        private void rebootBootloaderButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            if (modeBox.SelectedIndex == 0)
                Adb.ExecuteCommand("reboot bootloader", _device);
            else if (modeBox.SelectedIndex == 1)
                Fastboot.ExecuteCommand("reboot-bootloader", _device);
            InvokeToggleButtons(true);
        }

        private void logcatButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            // Thread must be started in SingleThreadedApartment because of the SaveFileDialog
            var t = new Thread(() =>
            {
                try
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = "Logcat.txt",
                        Filter = "Text files (*.txt) | *.txt"
                    };
                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        InvokeToggleButtons(true);
                        return;
                    }
                    InvokeLog("Saving logcat ...");
                    var logcat = Adb.ExecuteCommand("logcat -d", _device);
                    // Convert UNIX text file to DOS text file
                    logcat = Regex.Replace(logcat, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
                    File.WriteAllText(saveFileDialog.FileName, logcat);
                }
                catch (Exception)
                {
                    InvokeLogError("Saving Logcat.txt failed!");
                }
                InvokeToggleButtons(true);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        private void screenshotButton_Click(object sender, EventArgs e)
        {
            InvokeToggleButtons(false);
            // Thread must be started in SingleThreadedApartment because of the SaveFileDialog
            var t = new Thread(() =>
            {
                try
                {
                    var saveFileDialog = new SaveFileDialog
                    {
                        FileName = "Screenshot.png",
                        Filter = "Image files (*.png)|*.png"
                    };
                    if (saveFileDialog.ShowDialog() != DialogResult.OK)
                    {
                        InvokeToggleButtons(true);
                        return;
                    }
                    InvokeLog("Saving screenshot ...");
                    const string screenshot = "\"/sdcard/Screenshot.png\"";
                    Adb.ExecuteShellCommand($"screencap -p {screenshot}", _device);
                    Adb.ExecuteCommand($"pull {screenshot} \"{saveFileDialog.FileName}\"", _device);
                    Adb.ExecuteShellCommand($"rm {screenshot}", _device);
                }
                catch (Exception)
                {
                    InvokeLogError("Saving screenshot failed!");
                }
                InvokeToggleButtons(true);
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Start();
        }

        public void InvokeToggleButtons(bool value)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => ToogleButtons(value)));
            else
                ToogleButtons(value);
        }

        private void ToogleButtons(bool value)
        {
            twrpButton.Enabled = value;
            unlockButton.Enabled = value;
            customButton.Enabled = value;
            modeBox.Enabled = value;
            rebootButton.Enabled = value && modeBox.SelectedIndex >= 0;
            rebootRecoveryButton.Enabled = value && modeBox.SelectedIndex == 0;
            rebootBootloaderButton.Enabled = value && modeBox.SelectedIndex >= 0;
            logcatButton.Enabled = value;
            screenshotButton.Enabled = value;
        }

        public void InvokeLog(string text)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => logBox.AppendText(text + Environment.NewLine)));
            else
                logBox.AppendText(text + Environment.NewLine);
        }

        public void InvokeLogError(string text)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => LogError(text)));
            else
                LogError(text);
        }

        private void LogError(string text)
        {
            logBox.SelectionStart = logBox.TextLength;
            logBox.SelectionLength = 0;
            logBox.SelectionColor = Color.Red;
            logBox.AppendText(text + Environment.NewLine);
            logBox.SelectionColor = logBox.ForeColor;
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