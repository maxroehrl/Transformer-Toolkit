/*
 * Driver Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Toolkit.Properties;

namespace Toolkit
{
    public partial class DriverDialog : Form
    {
        private readonly string _driverPackagePath = ResourceManager.TempPath + "Drivers.zip";
        private readonly string _extractedDriverPath = ResourceManager.TempPath + "Drivers\\";

        public DriverDialog()
        {
            InitializeComponent();
            Icon = Resources.Icon;
        }

        private void DriverDialog_Load(object sender, EventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender1, e1) =>
            {
                if (File.Exists(_driverPackagePath))
                    File.Delete(_driverPackagePath);
                if (Directory.Exists(_extractedDriverPath))
                    Directory.Delete(_extractedDriverPath, true);

                ProgressLabelText("Downloading driver ...");
                NetManager.DownloadFileAsync(NetManager.GetDriverUrl(), _driverPackagePath, ProgressLabelText, ExtractAndInstallDrivers);
            };
            bw.RunWorkerAsync();
        }

        private void ExtractAndInstallDrivers()
        {
            ProgressLabelText("Extracting driver ...");
            System.IO.Compression.ZipFile.ExtractToDirectory(_driverPackagePath, _extractedDriverPath);

            ProgressLabelText("Installing driver ...");
            InstallHinfSection(IntPtr.Zero, IntPtr.Zero, _extractedDriverPath + "android_winusb.inf", 0);

            File.Delete(_driverPackagePath);
            Directory.Delete(_extractedDriverPath, true);
            MessageBox.Show("Drivers are installed now. Please reconnect your device.", "Information",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            Invoke(new MethodInvoker(Close));
        }

        private void ProgressLabelText(string text)
        {
            Invoke(new MethodInvoker(() => progressLabel.Text = text));
        }

        /// <summary>
        ///     Install an .inf based driver
        /// </summary>
        /// <param name="hwnd">Must be IntPtr.Zero</param>
        /// <param name="moduleHandle">Must be IntPtr.Zero</param>
        /// <param name="cmdLineBuffer">Path to the .inf file</param>
        /// <param name="nCmdShow">Must be 0</param>
        [DllImport("Setupapi.dll", EntryPoint = "InstallHinfSection", CallingConvention = CallingConvention.StdCall)]
        private static extern void InstallHinfSection(
            [In] IntPtr hwnd,
            [In] IntPtr moduleHandle,
            [In, MarshalAs(UnmanagedType.LPWStr)] string cmdLineBuffer,
            int nCmdShow);
    }
}