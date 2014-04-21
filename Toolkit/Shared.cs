using System;
using System.IO;
using System.Net;
using RegawMOD.Android;

namespace Toolkit
{
    public static class Shared
    {
        // Shared Classes
        public static AndroidController AndroidController;
        public static Toolkit Toolkit;
        public static StartDialog StartDialog;
        public static UpdateDialog UpdateDialog;
        public static Device Device;

        // Download links
        public const string VersionsUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2WGZjMzNacTRBX1U";
        public const string ToolkitUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2MUJvakZxWTYyRlE";
        public const string TwrpTf700TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2R01waksxTVQ0dEE";
        public const string TwrpTf300TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2SDZmQXNkSEg1T00";
        public const string TwrpHammerheadUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2UGRsRkdsR0NuNkE";
        public const string CwmTf700TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2VU80UE9EQzFmSk0";
        public const string CwmTf300TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2U1Z0NktRWFFOcE0";
        public const string CwmHammerheadUrl ="https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2LThhLXdTQUhIY2c";
        public const string PhilzTf700TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2R1IxODNQVDFpbFU";
        public const string PhilzTf300TUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2N0RMNW1OYVdmc1U";
        public const string PhilzHammerheadUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2NFJuS3NoQ3hhQ0k";
        public const string UnlockToolUrl = "https://docs.google.com/uc?export=download&id=0B54vSUgF4EB2ZVppR1RHQTY4RUk";
        
        // Android version of the latest Asus firmware
        public const string MinAndroidVersion = "4.2.1";

        // The property in the build.prop which indicates the code name of the device
        public const string CodeNameProperty = "ro.build.product";

        // The property in the build.prop which indicates the device name
        public const string DeviceNameProperty = "ro.product.model";

        // The property in the build.prop which indicates the android version
        public const string AndroidVersionProperty = "ro.build.version.release";

        // List of supported devices
        public static string[] ValidDevices = { "tf700t", "tf300t", "me301t", "hammerhead" };

        // The index of the selected device of all connected devices
        public static int DeviceIndex;

        // The name of the selected device
        public static string DeviceName;

        // The code name of the selected device
        public static string CodeName;

        // The Android version of the selected device
        public static string AndroidVersion;

        // The serial number of the selected device
        public static string SerialNumber;

        // Shows if the selected device is already rooted
        public static bool IsRooted;

        // Used to dispose the AndroidController only when needed
        public static bool IsDisposeable = true;

        // Shows outdated version
        public static bool IsOutdated = false;

        // Log an info in the logBox in the toolkit form and show it to the user
        public static void Log(string text)
        {
            if (Toolkit != null)
                Toolkit.Log(text);
        }

        // Log an error in the logBox in the toolkit form and show it to the user
        public static void LogError(string text)
        {
            if (Toolkit != null)
                Toolkit.LogError(text);
        }

        // Enable or disable all buttons of the toolkit form (e. g. while flashing)
        public static void ToggleButtons(bool value)
        {
            if (Toolkit != null)
                Toolkit.ToggleButtons(value);
        }

        // Use the wait cursor to show running background processes
        public static void WaitCursor(bool value)
        {
            if (Toolkit != null)
                Toolkit.WaitCursor(value);
            if (StartDialog != null)
                StartDialog.WaitCursor(value);
        }

        // Change progress of the progressBar
        public static void ProgressBarValue(int progress)
        {
            if(Toolkit != null)
                Toolkit.ProgressBarValue(progress);
            if (UpdateDialog != null)
                UpdateDialog.ProgressBarValue(progress);
        }

        // Change text of the progressLabel
        public static void ProgressLabelText(string text)
        {
            if (Toolkit != null)
                Toolkit.ProgressLabelText(text);
            if (UpdateDialog != null)
                UpdateDialog.ProgressLabelText(text);
        }

        // This method is called when a download progress has changed
        public static void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressBarValue(e.ProgressPercentage);
            double bytesIn = Math.Round((Convert.ToDouble(e.BytesReceived) / 1024), 0);
            double totalBytes = Math.Round((Convert.ToDouble(e.TotalBytesToReceive) / 1024), 0);
            ProgressLabelText("Downloaded " + bytesIn + " KB of " + totalBytes + " KB");
        }

        // Getting the direct download link from Google Drive 
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
                LogError(e.Message);
                return null;
            }
        }

        // Free used resources
        public static void CleanUp()
        {
            if (File.Exists("versions.txt") && IsDisposeable)
                File.Delete("versions.txt");
            if(IsDisposeable)
                AndroidController.Dispose();
        }
    }
}
