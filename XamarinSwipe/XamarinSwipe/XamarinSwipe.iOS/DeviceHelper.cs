using System;
using System.Net;
using CoreFoundation;
using Foundation;
using UIKit;
using Xamarin.Forms;
using XamarinSwipe.DependencyInterfaces;
using XamarinSwipe.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceHelper))]
namespace XamarinSwipe.iOS
{
    public class DeviceHelper : IDeviceHelper
    {
        public DeviceHelper() { }

        public IDiagnosticData GetDiagnosticData()
        {
            var diagData = new DiagnosticData
            {
                Platform = "iOS",
                OsVersion = NSProcessInfo.ProcessInfo.OperatingSystemVersionString,
                DeviceType = (Device.Idiom == TargetIdiom.Tablet ? "Tablet" : "Phone"),
                AppVersion = NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString()
            };

            //get the memoery
            double residentSize;
            double totalMemory;

            MachMemoryHelper.ReportMemory(out residentSize, out totalMemory);
            diagData.AddProperty("Total Memory",
                (NSProcessInfo.ProcessInfo.PhysicalMemory/1024/1024).ToString("N0") + " MB");
            var freeMemory = totalMemory - residentSize;
            diagData.AddProperty("Free Memory", (freeMemory).ToString("0.##") + " MB");

            //cell and wifi
            string wifi;
            string cell;

            var ss = new SocketDefs();
            ss.GetMacInfo(out wifi, out cell);

            diagData.AddProperty("Cellular Stutus", !string.IsNullOrWhiteSpace(cell) ? "Yes" : "No");
            diagData.AddProperty("Wifi Status", !string.IsNullOrWhiteSpace(wifi) ? "Yes" : "No");

            return diagData;
        }

        public string GetDeviceId()
        {
            var key = NSBundle.MainBundle.InfoDictionary["CFBundleIdentifier"] + "_DeviceID";
            var userDefaults = NSUserDefaults.StandardUserDefaults;
            var deviceId = userDefaults.StringForKey(key) ?? Guid.NewGuid().ToString();

            userDefaults.SetString(deviceId, key);

            return UIDevice.CurrentDevice.IdentifierForVendor.AsString();           
        }      
	}
}