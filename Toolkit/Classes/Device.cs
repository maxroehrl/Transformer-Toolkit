/*
 * Device.cs - Developed by Max Röhrl for Transformer Toolkit
 */

using System;
using System.Linq;

namespace Toolkit
{
    public class Device
    {
        /// <summary>
        ///     Android version of the latest Asus firmware for TF700T/TF300T/ME301T
        /// </summary>
        private const string MinAndroidVersion = "4.2.1";

        /// <summary>
        ///     List of supported devices
        /// </summary>
        private static readonly string[] SupportedDevices = { "tf700t", "tf300t", "me301t", "hammerhead", "hammerheadcaf" };

        public string SerialNumber { get; }
        public string DeviceName { get; }
        public string CodeName { get; }
        public string AndroidVersion { get; }
        public bool Supported { get; }
        public bool OutdatedFirmware { get; }
        public bool Rooted { get; }

        public Device(string serialNumber, StartDialog startDialog)
        {
            SerialNumber = serialNumber;
            DeviceName = Adb.GetBuildProperty("ro.product.model", this);
            if (string.IsNullOrEmpty(DeviceName))
            {
                startDialog.ShowErrorMessageBox($"The informations about the connected device with the serial number \"{SerialNumber}\" "
                    + "could not be fetched. Please follow the instructions to setup USB debugging.");
            }
            else
            {
                CodeName = Adb.GetBuildProperty("ro.build.product", this);
                AndroidVersion = Adb.GetBuildProperty("ro.build.version.release", this);
                Supported = SupportedDevices.Contains(CodeName.ToLower());
                OutdatedFirmware = Convert.ToInt32(AndroidVersion.Replace(".", string.Empty))
                                 < Convert.ToInt32(MinAndroidVersion.Replace(".", string.Empty));
                string suOutput = Adb.ExecuteAdbShellCommand("su -v", this);
                Rooted = !(suOutput.Contains("not found") || suOutput.Contains("permission denied"));
            }
        }
    }
}
