/*
 * Start Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using RegawMOD.Android;

namespace Toolkit
{
    /// <summary>
    /// A dialog where you can select a device from all connected devices
    /// </summary>
    public partial class StartDialog : Form
    {
        public StartDialog()
        {
            InitializeComponent();
            Shared.StartDialog = this;
            Icon = Properties.Resources.Icon;
        }

        #region Event listener

        private void StartDialog_Load(object sender, EventArgs e)
        {
            // Show version in title bar
            Text += Application.ProductVersion;

            // Initialize AndroidController and check for devices
            new Thread(() =>
            {
                Shared.AndroidController = AndroidController.Instance;
                Invoke(new MethodInvoker(RefreshConnectedDevices));
            }).Start();

            // Check for toolkit updates
            new Thread(() => new Update()).Start();
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

            // Only get informations about the selcted device if the clicked item gets checked
            if (e.CurrentValue == CheckState.Unchecked)
            {
                new Thread(() =>
                {
                    // Getting the index of the selected item which is 
                    // the index of the device in the AndroidController.ConnectedDevices
                    Invoke(new MethodInvoker(() => Shared.DeviceIndex = ConnectedDevicesListBox.SelectedIndex));

                    // Getting informations about this device if the index is valid
                    Shared.UpdateInformations();

                    // Check if the device is supported
                    Invoke(new MethodInvoker(() =>
                    {
                        if (!Shared.ValidDevices.Contains(Shared.CodeName))
                        {
                            MessageBox.Show("Please only connect the TF700T, TF300T, ME301T or the Nexus 5 with enabled USB debugging.",
                                Shared.DeviceName + " is not supported",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            RefreshConnectedDevices();
                        }
                        // Check if the android version is outdated
                        else if (Convert.ToInt32(Shared.AndroidVersion.Replace(".", String.Empty)) <
                                 Convert.ToInt32(Shared.MinAndroidVersion.Replace(".", String.Empty)))
                        {
                            DialogResult dialogResult = MessageBox.Show("Please update to the latest firmware from Asus.\r\n" +
                                "Would you like to visit the download site?",
                                "Android version " + Shared.AndroidVersion + " is outdated",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                            if (dialogResult == DialogResult.Yes)
                                Process.Start("http://www.asus.com/Tablets_Mobile/ASUS_Transformer_Pad_TF700T/HelpDesk_Download/");

                            RefreshConnectedDevices();
                        }
                        else
                        {
                            // We can start over now
                            startButton.Enabled = true;
                        }
                    }));
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

        private void driverButton_Click(object sender, EventArgs e)
        {
            new Thread(() => Application.Run(new DriverDialog())).Start();
            driverButton.Enabled = false;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Refresh the list of connected devices
        /// </summary>
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
                            MessageBox.Show("The informations about the connected device could not be fetched. " +
                                            "Follow the instructions to setup USB debugging.",
                                "Getting informations failed",
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

        /// <summary>
        /// Use the wait cursor to show running background processes
        /// </summary>
        /// <param name="value">True to enable and false to disable the waiting cursor</param>
        public void WaitCursor(bool value)
        {
            UseWaitCursor = value;
        }

        /// <summary>
        /// Show outdated version in titlebar
        /// </summary>
        public void VersionIsOutdated()
        {
            Invoke(new MethodInvoker(() => Text += " (Update available)"));
        }

        /// <summary>
        /// Hide the startDialog form
        /// </summary>
        public void Unhide()
        {
            Invoke(new MethodInvoker(() =>
            {
                WindowState = FormWindowState.Normal;
                ShowInTaskbar = true;
            }));
        }

        #endregion
    }
}
