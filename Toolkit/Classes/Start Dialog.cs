﻿/*
 * Start Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Toolkit.Properties;

namespace Toolkit
{
    public partial class StartDialog : Form
    {
        private readonly List<Device> _connectedDevices = new List<Device>(); 
        private Device _selectedDevice;

        public StartDialog()
        {
            InitializeComponent();
            Icon = Resources.Icon;
        }

        private void StartDialog_Load(object sender, EventArgs e)
        {
            Text += Application.ProductVersion;
            NetManager.CheckForUpdate(this);
            RefreshConnectedDevices();
        }

        private void StartDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            ResourceManager.Dispose();
        }

        private void ConnectedDevicesListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var selectedItemIndex = ConnectedDevicesListBox.IndexFromPoint(e.Location);
            if (selectedItemIndex == ListBox.NoMatches)
                return;
            _selectedDevice = _connectedDevices[selectedItemIndex];

            if (!_selectedDevice.Supported)
            {
                MessageBox.Show("Please only connect the TF700T, TF300T, ME301T or the Nexus 5 with enabled USB debugging.",
                    _selectedDevice.DeviceName + " is not supported",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshConnectedDevices();
            }
            else if (_selectedDevice.OutdatedFirmware)
            {
                MessageBox.Show("Please update to the latest firmware from Asus.\r\n",
                        $"Android version {_selectedDevice.AndroidVersion} is outdated",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshConnectedDevices();
            }
            else
            {
                // We only load the Toolkit form and do not close the whole toolkit
                ResourceManager.IsDisposeable = false;
                new Thread(() => Application.Run(new Toolkit(_selectedDevice))).Start();
                Close();
            }
        }

        /// <summary>
        ///     Used to center the items in the list box and remove the higlight color of the selected item
        /// </summary>
        private void ConnectedDevicesListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index <= -1)
                return;
            var item = ConnectedDevicesListBox.Items[e.Index];
            var size = e.Graphics.MeasureString(item.ToString(), e.Font);
            e.DrawBackground();
            e.Graphics.FillRectangle(SystemBrushes.Control, e.Bounds);
            e.Graphics.DrawString(item.ToString(), e.Font, Brushes.Black, e.Bounds.Left + (e.Bounds.Width  - size.Width) / 2, e.Bounds.Top + (e.Bounds.Height - size.Height) / 2);
            e.DrawFocusRectangle();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            RefreshConnectedDevices();
        }

        private void driverButton_Click(object sender, EventArgs e)
        {
            new DriverDialog().ShowDialog();
            driverButton.Enabled = false;
        }

        private void RefreshConnectedDevices()
        {
            // Disable components until the worker has finished
            ConnectedDevicesListBox.Enabled = false;
            refreshButton.Enabled = false;

            // Reset form
            NoDevicesLabel.Hide();
            ConnectedDevicesListBox.Items.Clear();
            _connectedDevices.Clear();
            loadingSpinner.Show();

            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                // Add all devices to the connected devices list
                var serialList = Adb.GetConnectedAdbDevicesSerialList();
                foreach (var serial in serialList)
                    _connectedDevices.Add(new Device(serial, this));
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                // Check if there are connected devices
                if (_connectedDevices.Count == 0)
                {
                    NoDevicesLabel.Show();
                }
                else
                {
                    ConnectedDevicesListBox.Enabled = true;
                    foreach (var device in _connectedDevices)
                        ConnectedDevicesListBox.Items.Add($"{device.DeviceName} ({device.SerialNumber})");
                }
                loadingSpinner.Hide();
                refreshButton.Enabled = true;
            };
            bw.RunWorkerAsync();
        }

        public void ShowErrorMessageBox(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowUpdateDialog(string onlineVersion)
        {
            Invoke(new MethodInvoker(() =>
            {
                Text += " (Update available)";
                new UpdateDialog(onlineVersion).ShowDialog();
            }));
        }
    }
}