using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RegawMOD.Android;

namespace Toolkit
{
    public partial class StartDialog : Form
    {
        public StartDialog()
        {
            InitializeComponent();
            Shared.StartDialog = this;
            Icon = Properties.Resources.Icon;
        }

        private void StartDialog_Load(object sender, EventArgs e)
        {
            // Show version in title bar
            Text += Application.ProductVersion;

            // Initialize AndroidController
            new Thread(() => Shared.AndroidController = AndroidController.Instance).Start();
            // Check for updates
            new Thread(() => new Update()).Start();
            
            RefreshConnectedDevices();
        }

        private void StartDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            Shared.CleanUp();
        }

        private void ConnectedDevicesListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            Shared.WaitCursor(true);

            // Only allow one item being checked at a time
            for (int i = 0; i < ConnectedDevicesListBox.Items.Count; i++)
                if (i != e.Index)
                    ConnectedDevicesListBox.SetItemChecked(i, false);

            // Disable startButton while checking for support
            startButton.Enabled = false;

            // Only get infos about the selcted device if the clicked item gets checked
            if (e.CurrentValue == CheckState.Unchecked)
            {
                new Thread(() =>
                {
                    // Getting the index of the selected item which is 
                    // the index of the device in the AndroidController.ConnectedDevices
                    Invoke(new MethodInvoker(() => Shared.DeviceIndex = ConnectedDevicesListBox.SelectedIndex));

                    // Getting infos about this device if the index is valid
                    if (Shared.DeviceIndex >= 0)
                    {
                        Shared.SerialNumber = Shared.AndroidController.ConnectedDevices[Shared.DeviceIndex];
                        Shared.Device = Shared.AndroidController.GetConnectedDevice(Shared.SerialNumber);
                        Shared.DeviceName = Shared.Device.BuildProp.GetProp(Shared.DeviceNameProperty);
                        Shared.CodeName = Shared.Device.BuildProp.GetProp(Shared.CodeNameProperty).ToLower();
                        Shared.AndroidVersion = Shared.Device.BuildProp.GetProp(Shared.AndroidVersionProperty);
                        Shared.IsRooted = Shared.Device.HasRoot;
                    }
                    else
                    {
                        // A device is not responding and the user must allow ADB-Debugging
                        MessageBox.Show("The infos about the connected device could not be fetched. " +
                                        "Maybe your device is in recovery mode or you must allow " +
                                        "USB debugging first.",
                            "Getting infos failed",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    // Check if the device is supported
                    if (!Shared.ValidDevices.Contains(Shared.CodeName))
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            MessageBox.Show(
                                "Please only connect the TF700T, TF300T, or ME301T with enabled USB debugging.",
                                Shared.DeviceName + " is not supported",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            RefreshConnectedDevices();
                        }));
                    }
                    // Check if the android version is outdated
                    else if (Convert.ToInt32(Shared.AndroidVersion.Replace(".", String.Empty)) <
                             Convert.ToInt32(Shared.MinAndroidVersion.Replace(".", String.Empty)))
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            MessageBox.Show("Please update to the latest firmware from Asus.",
                                "Android version " + Shared.AndroidVersion + " is outdated",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            RefreshConnectedDevices();
                        }));
                    }
                    else
                    {
                        // We can start over now
                        Invoke((new MethodInvoker(() => startButton.Enabled = true)));
                    }
                    Shared.WaitCursor(false);
                }).Start();
            }
            else
            {
                Shared.WaitCursor(false);
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshConnectedDevices();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            new Thread(() => Application.Run(new Toolkit())).Start();

            // Do not dispose the AndroidController because we only load the toolkit
            // and do not close the toolkit
            Shared.IsDisposeable = false;

            // Close other forms
            if(Shared.UpdateDialog != null)
                Shared.UpdateDialog.Close();
            Close();
        }

        // Refresh the list of connected devices
        private void RefreshConnectedDevices()
        {
            Shared.WaitCursor(true);

            // Wait until the thread has finished
            ConnectedDevicesListBox.Enabled = false;
            refreshButton.Enabled = false;

            // Reset form
            NoDevicesLabel.Hide();
            ConnectedDevicesListBox.Items.Clear();
            startButton.Enabled = false;

            // Wait until AndroidController is ready
            while (Shared.AndroidController == null)
                Thread.Sleep(10);

            // Check if there are connected devices
            if (!Shared.AndroidController.HasConnectedDevices)
            {
                NoDevicesLabel.Show();
                refreshButton.Enabled = true;
                Shared.WaitCursor(false);
            }
            else
            {
                new Thread(() =>
                {
                    for (int i = 0; i < Shared.AndroidController.ConnectedDevices.Count; i++)
                    {
                        // Add all connected device names to the listbox
                        string serial = Shared.AndroidController.ConnectedDevices[i];
                        Device device = Shared.AndroidController.GetConnectedDevice(serial);
                        string deviceName = device.BuildProp.GetProp(Shared.DeviceNameProperty);
                        if (deviceName != null)
                            Invoke(new MethodInvoker(() => ConnectedDevicesListBox.Items.Add(deviceName + " (" + serial + ")")));
                        else
                            MessageBox.Show("The infos about the connected device could not be fetched. " +
                                            "Follow the instructions to setup USB debugging.",
                                "Getting infos failed",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    // Now the user can select a device
                    Invoke(new MethodInvoker(() =>
                    {
                        ConnectedDevicesListBox.Enabled = true;
                        refreshButton.Enabled = true;
                    }));
                    Shared.WaitCursor(false);
                }).Start();
            }
        }

        // Use the wait cursor to show running background processes
        public void WaitCursor(bool value)
        {
            UseWaitCursor = value;
        }

        // Show outdated version in titlebar
        public void VersionIsOutdated()
        {
            Invoke(new MethodInvoker(() => Text += " (Update available)"));
        }

        // Hide the startDialog
        public void Unhide()
        {
            Invoke(new MethodInvoker(() =>
            {
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            }));
        }
    }
}
