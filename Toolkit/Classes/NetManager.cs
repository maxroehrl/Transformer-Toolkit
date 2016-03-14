/*
 * NetManager.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Toolkit
{
    internal static class NetManager
    {
        public const string GoogleDriveStaticUrlPrefix = "https://docs.google.com/uc?export=download&id=";
        public const string VersionsUrl = "0B54vSUgF4EB2Qmx4VXFrSWdGRmM";
        public const string OldAppFileName = "Transformer Toolkit.exe.old";
        public static bool IsOutdated;
        public static readonly string ManifestFile = ResourceManager.TempPath + "versions.xml";

        public static void CheckForUpdate(StartDialog startDialog)
        {
            var bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                if (File.Exists(OldAppFileName))
                    File.Delete(OldAppFileName);
                if (File.Exists(ManifestFile))
                    File.Delete(ManifestFile);

                // Check if internet is working
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    startDialog.ShowErrorMessageBox("This toolkit needs a working internet connection to work properly!");
                    Application.Exit();
                }

                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(ResolveGoogleDriveStaticUrl(VersionsUrl)), ManifestFile);
                    
                    // Check if online version is higher than current version
                    var onlineVersion = GetToolkitVersion();
                    if (Convert.ToInt32(onlineVersion.Replace(".", string.Empty))
                        <= Convert.ToInt32(Application.ProductVersion.Replace(".", string.Empty)))
                        return;
                    startDialog.ShowUpdateDialog(onlineVersion);
                    IsOutdated = true;
                }
            };
            bw.RunWorkerAsync();
        }

        public static void DownloadFileAsync(Uri address, string fileName, Action<string> downloadProgressChangedAction,
            Action downloadCompletedAction)
        {
            using (var webClient = new WebClient())
            {
                var counter = 200;
                webClient.DownloadProgressChanged += (sender, e) =>
                {
                    if (counter++ % 500 != 0)
                        return;
                    var bytesIn = Math.Round(Convert.ToDouble(e.BytesReceived) / 1024, 0);
                    downloadProgressChangedAction($"Downloading: {bytesIn} KB");
                };
                webClient.DownloadFileCompleted += (sender, e) => downloadCompletedAction();
                webClient.DownloadFileAsync(address, fileName);
            }
        }

        public static string GetTwrpVersion(Device device)
        {
            return ResolveFromManifest(Recovery.Twrp, "version", device);
        }

        public static Uri GetTwrpUrl(Device device)
        {
            return new Uri(ResolveFromManifest(Recovery.Twrp, "url", device));
        }

        public static Uri GetDriverUrl()
        {
            return new Uri(ResolveFromManifest("drivers", "url"));
        }

        public static Uri GetUnlockerUrl()
        {
            return new Uri(ResolveFromManifest("unlocker", "url"));
        }

        public static string GetToolkitVersion()
        {
            return ResolveFromManifest("toolkit", "version");
        }

        public static Uri GetToolkitUrl()
        {
            return new Uri(ResolveFromManifest("toolkit", "url"));
        }

        private static string ResolveFromManifest(string type, string tag, Device device = null)
        {
            var xml = XDocument.Load(ManifestFile);
            var descendants = xml.Root?.Descendants(type);
            // Select the twrp version according to the device
            if (type.Equals(Recovery.Twrp))
                descendants = descendants?.Where(desc => desc.Attribute("device").Value == device?.CodeName);
            var output = descendants?.First().Element(tag)?.Value;
            return tag.Equals("url") ? ResolveGoogleDriveStaticUrl(output) : output;
        }

        private static string ResolveGoogleDriveStaticUrl(string url)
        {
            try
            {
                var webRequest = (HttpWebRequest) WebRequest.Create(GoogleDriveStaticUrlPrefix + url);
                webRequest.AllowAutoRedirect = false;
                var response = (HttpWebResponse) webRequest.GetResponse();
                return response.Headers.Get("Location");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}