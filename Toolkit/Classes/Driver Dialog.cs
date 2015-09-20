/*
 * Driver Dialog.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
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

                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += (sender2, e2) =>
                    {
                        double bytesIn = Math.Round((Convert.ToDouble(e2.BytesReceived)/1024), 0);
                        ProgressLabelText($"Downloading: {bytesIn} KB");
                    };
                    webClient.DownloadFileCompleted += (sender2, e2) =>
                    {
                        ProgressLabelText("Extracting driver ...");
                        System.IO.Compression.ZipFile.ExtractToDirectory(_driverPackagePath, _extractedDriverPath);

                        ProgressLabelText("Installing driver ...");
                        InstallHinfSection(IntPtr.Zero, IntPtr.Zero, _extractedDriverPath + "android_winusb.inf", 0);

                        File.Delete(_driverPackagePath);
                        Directory.Delete(_extractedDriverPath, true);
                        MessageBox.Show("Drivers are installed now. Please reconnect your device.", "Information",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseForm();
                    };
                    ProgressLabelText("Downloading driver ...");
                    webClient.DownloadFileAsync(NetManager.GetDriverUrl(), _driverPackagePath);
                }
            };
            bw.RunWorkerAsync();
        }

        private void ProgressLabelText(string text)
        {
            Invoke(new MethodInvoker(() => progressLabel.Text = text));
        }

        private void CloseForm()
        {
            Invoke(new MethodInvoker(Close));
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