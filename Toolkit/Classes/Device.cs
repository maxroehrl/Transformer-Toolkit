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
        public static readonly string[] SupportedDevices = { "tf700t", "tf300t", "me301t", "hammerhead", "hammerheadcaf" };

        private readonly string _serialNumber;
        private readonly string _deviceName;
        private readonly string _codeName;
        private readonly string _androidVersion;
        private readonly bool _supported;
        private readonly bool _outdatedFirmware;
        private readonly bool _rooted;

        public Device(string serialNumber, StartDialog startDialog)
        {
            _serialNumber = serialNumber;
            _deviceName = Adb.GetBuildProperty("ro.product.model", this);
            if (string.IsNullOrEmpty(_deviceName))
            {
                startDialog.ShowErrorMessageBox($"The informations about the connected device with the serial number \"{_serialNumber}\" " +
                    "could not be fetched. Please follow the instructions to setup USB debugging.");
            }
            else
            {
                _codeName = Adb.GetBuildProperty("ro.build.product", this);
                _androidVersion = Adb.GetBuildProperty("ro.build.version.release", this);
                _supported = SupportedDevices.Contains(_codeName);
                _outdatedFirmware = (Convert.ToInt32(_androidVersion.Replace(".", string.Empty)) <
                                     Convert.ToInt32(MinAndroidVersion.Replace(".", string.Empty)));
                string suOutput = Adb.ExecuteAdbShellCommand("su -v", this);
                _rooted = !(suOutput.Contains("not found") || suOutput.Contains("permission denied"));
            }
        }

        public string GetSerialNumber()
        {
            return _serialNumber;
        }

        public string GetDeviceName()
        {
            return _deviceName;
        }

        public string GetCodeName()
        {
            return _codeName;
        }

        public string GetAndroidVersion()
        {
            return _androidVersion;
        }

        public bool IsSupported()
        {
            return _supported;
        }

        public bool IsOutdatedFirmware()
        {
            return _outdatedFirmware;
        }

        public bool IsRooted()
        {
            return _rooted;
        }
    }
}
