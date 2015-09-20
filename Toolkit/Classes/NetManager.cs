/*
 * Update.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Toolkit
{
    public static class NetManager
    {
        public const string GoogleDriveStaticUrlPrefix = "https://docs.google.com/uc?export=download&id=";
        public const string VersionsUrl = GoogleDriveStaticUrlPrefix + "0B54vSUgF4EB2Qmx4VXFrSWdGRmM";
        public const string OldAppFileName = "Transformer Toolkit.exe.old";
        public static bool IsOutdated;
        public static readonly string VersionFile = ResourceManager.TempPath + "versions.xml";
        private static StartDialog _startDialog;

        public static void CheckForUpdate(StartDialog startDialog)
        {
            _startDialog = startDialog;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (sender, args) =>
            {
                if (File.Exists(OldAppFileName))
                    File.Delete(OldAppFileName);
                if (File.Exists(VersionFile))
                    File.Delete(VersionFile);

                // Check if internet is working
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    _startDialog.ShowErrorMessageBox("This toolkit needs a working internet connection to work properly!");
                    Application.Exit();
                }
                else
                {
                    using (WebClient webClient = new WebClient())
                    {
                        webClient.DownloadFile(new Uri(ResolveGoogleDriveStaticUrl(VersionsUrl)), VersionFile);
                        string onlineVersion = GetFirstValue("toolkit", "version");
                        // Check if online version is higher than current version
                        if (Convert.ToInt32(onlineVersion.Replace(".", string.Empty)) <=
                            Convert.ToInt32(Application.ProductVersion.Replace(".", string.Empty)))
                            return;
                        _startDialog.ShowUpdateDialog(onlineVersion);
                        _startDialog.VersionIsOutdated();
                        IsOutdated = true;
                    }
                }
            };
            bw.RunWorkerAsync();
        }

        public static string GetTwrpVersion(Device device)
        {
            return GetTwrpValueForDevice(device, "version");
        }

        public static Uri GetTwrpUrl(Device device)
        {
            return new Uri(GetTwrpValueForDevice(device, "url"));
        }

        public static Uri GetDriverUrl()
        {
            return new Uri(GetFirstValue("drivers", "url"));
        }

        public static Uri GetUnlockerUrl()
        {
            return new Uri(GetFirstValue("unlocker", "url"));
        }

        public static Uri GetToolkitUrl()
        {
            return new Uri(GetFirstValue("toolkit", "url"));
        }

        private static string GetFirstValue(string type, string tag)
        {
            XDocument xml = XDocument.Load(VersionFile);
            IEnumerable<string> query = from c in xml.Root.Descendants(type)
                select c.Element(tag).Value;
            string output = query.FirstOrDefault();
            return tag == "version" ? output : ResolveGoogleDriveStaticUrl(output.Insert(0, GoogleDriveStaticUrlPrefix));
        }

        private static string GetTwrpValueForDevice(Device device, string tag)
        {
            XDocument xml = XDocument.Load(VersionFile);
            IEnumerable<string> query = from c in xml.Root.Descendants("twrp")
                                        where c.Attribute("device").Value == device.GetCodeName()
                                        select c.Element(tag).Value;
            string output = query.FirstOrDefault();
            return tag == "version" ? output : ResolveGoogleDriveStaticUrl(output.Insert(0, GoogleDriveStaticUrlPrefix));
        }

        private static string ResolveGoogleDriveStaticUrl(string url)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
                webRequest.AllowAutoRedirect = false;
                HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse();
                return (response.Headers.Get("Location"));
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}