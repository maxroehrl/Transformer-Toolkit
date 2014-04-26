/*
 * Shared.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using RegawMOD.Android;

namespace Toolkit
{
    /// <summary>
    /// Contains globally shared information and methods
    /// </summary>
    public static class Shared
    {
        #region AndroidLib

        /// <summary>
        /// Manages all connected devices
        /// </summary>
        public static AndroidController AndroidController;

        /// <summary>
        /// The in the <see cref="StartDialog"/> selected device
        /// </summary>
        public static Device Device;

        #endregion

        #region Forms
        public static Toolkit Toolkit;
        public static StartDialog StartDialog;
        public static UpdateDialog UpdateDialog;
        public static DriverDialog DriverDialog;
        #endregion
        
        #region Constants

        #region Download Urls
        public const string VersionsUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2WGZjMzNacTRBX1U";
        public const string ToolkitUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2MUJvakZxWTYyRlE";
        public const string DriverUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2WUxsd1VmNDhTSWs";
        public const string TwrpTf700TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2R01waksxTVQ0dEE";
        public const string TwrpTf300TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2SDZmQXNkSEg1T00";
        public const string TwrpHammerheadUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2UGRsRkdsR0NuNkE";
        public const string CwmTf700TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2VU80UE9EQzFmSk0";
        public const string CwmTf300TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2U1Z0NktRWFFOcE0";
        public const string CwmHammerheadUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2LThhLXdTQUhIY2c";
        public const string PhilzTf700TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2R1IxODNQVDFpbFU";
        public const string PhilzTf300TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2N0RMNW1OYVdmc1U";
        public const string PhilzHammerheadUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2NFJuS3NoQ3hhQ0k";
        public const string UnlockToolUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2ZVppR1RHQTY4RUk";
        #endregion

        /// <summary>
        /// Android version of the latest Asus firmware for TF700T/TF300T/ME301T
        /// </summary>
        public const string MinAndroidVersion = "4.2.1";
        
        /// <summary>
        /// Property which contains the code name of a device
        /// </summary>
        public const string CodeNameProperty = "ro.build.product";

        /// <summary>
        /// Property which contains the name of a device
        /// </summary>
        public const string DeviceNameProperty = "ro.product.model";

        /// <summary>
        /// Property which contains the android version of a device
        /// </summary>
        public const string AndroidVersionProperty = "ro.build.version.release";

        /// <summary>
        /// List of supported devices
        /// </summary>
        public static readonly string[] ValidDevices = { "tf700t", "tf300t", "me301t", "hammerhead" };

        #endregion

        #region Variables

        /// <summary>
        /// The index of the selected device in the AndroidController.ConnectedDevices array
        /// </summary>
        public static int DeviceIndex;

        /// <summary>
        /// The name of the selected device
        /// </summary>
        public static string DeviceName;

        /// <summary>
        /// The code name of the selected device
        /// </summary>
        public static string CodeName;

        /// <summary>
        /// The Android version of the selected device
        /// </summary>
        public static string AndroidVersion;

        /// <summary>
        /// The serial number of the selected device
        /// </summary>
        public static string SerialNumber;

        /// <summary>
        /// Shows if the selected device is already rooted
        /// </summary>
        public static bool IsRooted;

        /// <summary>
        /// Used to dispose the AndroidController only when needed
        /// </summary>
        public static bool IsDisposeable = true;

        /// <summary>
        /// Used to show an outdated version of the toolkit in the titlebar
        /// </summary>
        public static bool IsOutdated = false;

        #endregion
        
        #region Methods

        /// <summary>
        /// Refreshes the informations about the selected device
        /// </summary>
        public static void UpdateInformations()
        {
            SerialNumber = AndroidController.ConnectedDevices[DeviceIndex];
            Device = AndroidController.GetConnectedDevice(SerialNumber);
            DeviceName = Device.BuildProp.GetProp(DeviceNameProperty);
            CodeName = Device.BuildProp.GetProp(CodeNameProperty).ToLower();
            AndroidVersion = Device.BuildProp.GetProp(AndroidVersionProperty);
            IsRooted = Device.HasRoot;
        }

        /// <summary>
        /// Log an information in the logBox in the toolkit form
        /// </summary>
        /// <param name="text">The printed text</param>
        public static void Log(string text)
        {
            if (Toolkit != null)
                Toolkit.Log(text);
        }

        /// <summary>
        /// Log an error in the logBox in the toolkit form with red color
        /// </summary>
        /// <param name="text">The printed text</param>
        public static void LogError(string text)
        {
            if (Toolkit != null)
                Toolkit.LogError(text);
        }

        /// <summary>
        /// Enable or disable all buttons of the toolkit form
        /// </summary>
        /// <param name="value">True to enable and false to disable all buttons</param>
        public static void ToggleButtons(bool value)
        {
            if (Toolkit != null)
                Toolkit.ToggleButtons(value);
        }

        /// <summary>
        /// Use the wait cursor to show running background processes
        /// </summary>
        /// <param name="value">True to enable and false to disable the waiting cursor</param>
        public static void WaitCursor(bool value)
        {
            if (Toolkit != null)
                Toolkit.WaitCursor(value);
            if (StartDialog != null)
                StartDialog.WaitCursor(value);
        }

        /// <summary>
        /// Change the progress of the progressBar
        /// </summary>
        /// <param name="progress">Value from 0 to 100</param>
        public static void ProgressBarValue(int progress)
        {
            if(Toolkit != null)
                Toolkit.ProgressBarValue(progress);
            if (UpdateDialog != null)
                UpdateDialog.ProgressBarValue(progress);
            if (DriverDialog != null)
                DriverDialog.ProgressBarValue(progress);
        }

        /// <summary>
        /// Change the text of the progressLabel
        /// </summary>
        /// <param name="text">Displayed text</param>
        public static void ProgressLabelText(string text)
        {
            if (Toolkit != null)
                Toolkit.ProgressLabelText(text);
            if (UpdateDialog != null)
                UpdateDialog.ProgressLabelText(text);
            if (DriverDialog != null)
                DriverDialog.ProgressLabelText(text);
        }

        /// <summary>
        /// Handles download progress changes and displays them
        /// </summary>
        /// <param name="sender">Sender object</param>
        /// <param name="e">DownloadProgressChanged event args</param>
        public static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBarValue(e.ProgressPercentage);
            double bytesIn = Math.Round((Convert.ToDouble(e.BytesReceived) / 1024), 0);
            double totalBytes = Math.Round((Convert.ToDouble(e.TotalBytesToReceive) / 1024), 0);
            ProgressLabelText("Downloaded " + bytesIn + " KB of " + totalBytes + " KB");
        }

        /// <summary>
        /// Returns the direct download link from a file on Google Drive 
        /// </summary>
        /// <param name="url">Static download link of a file</param>
        /// <returns>Direct download link of a file</returns>
        public static string ResolveUrl(string url)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest) WebRequest.Create(url);
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse) webRequest.GetResponse();
                return response.Headers.Get("Location");
            }
            catch (Exception e)
            {
                if (Toolkit != null)
                    LogError(e.Message);
                else
                    MessageBox.Show(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Free resources used by the AndroidController and delete versions.txt if needed
        /// </summary>
        public static void CleanUp()
        {
            if (File.Exists("versions.txt") && IsDisposeable)
                File.Delete("versions.txt");
            if(IsDisposeable && AndroidController != null)
                AndroidController.Dispose();
        }

        #endregion

        #region Extern Methods

        /// <summary>
        /// Install an .inf based driver
        /// </summary>
        /// <param name="hwnd">Must be IntPtr.Zero</param>
        /// <param name="moduleHandle">Must be IntPtr.Zero</param>
        /// <param name="cmdLineBuffer">Path to the .inf file</param>
        /// <param name="nCmdShow">Must be 0</param>
        [DllImport("Setupapi.dll", EntryPoint = "InstallHinfSection", CallingConvention = CallingConvention.StdCall)]
        public static extern void InstallHinfSection(
            [In] IntPtr hwnd,
            [In] IntPtr moduleHandle,
            [In, MarshalAs(UnmanagedType.LPWStr)] string cmdLineBuffer,
            int nCmdShow);

        #endregion
    }
}
