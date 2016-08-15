using System;
using System.IO;
using System.Net;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Webkit;
using Xamarin.Forms;
using XamarinSwipe.DependencyInterfaces;
using XamarinSwipe.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceHelper))]
namespace XamarinSwipe.Droid
{
    public class DeviceHelper : IDeviceHelper
    {
        public DeviceHelper() { }

        public IDiagnosticData GetDiagnosticData()
        {
            var diagData = new DiagnosticData
            {
                Platform = "Android",
                OsVersion = Build.VERSION.Release + "(" + Build.VERSION.Sdk + ")",
                DeviceType = (Device.Idiom == TargetIdiom.Tablet ? "Tablet" : "Phone"),
                AppVersion = Forms.Context.PackageManager.GetPackageInfo(Forms.Context.PackageName, 0).VersionName

            };

            try
            {


                //additional properties 
                diagData.AddProperty("Manufacturer", Build.Manufacturer);
                diagData.AddProperty("Model", Build.Model);
                diagData.AddProperty("Cpu", Build.CpuAbi);

                //memory
                using (
                    var actManager =
                        (ActivityManager)Forms.Context.GetSystemService(Context.ActivityService))
                using (var memInfo = new ActivityManager.MemoryInfo())
                {
                    actManager.GetMemoryInfo(memInfo);
                    diagData.AddProperty("Total Memory", (memInfo.TotalMem / 1024 / 1024).ToString("N0") + " MB");
                    diagData.AddProperty("Free Memory", (Convert.ToDouble(memInfo.AvailMem) / 1024f / 1024f).ToString("N2") + " MB");
                }

                //airplane mode
                var airplaneMode =
                    Settings.System.GetInt(Forms.Context.ContentResolver, Settings.Global.AirplaneModeOn, 0) != 0
                        ? "On"
                        : "Off";
                diagData.AddProperty("Airplane Mode", airplaneMode);

                //wifi and mobile status
                using (
                    var connMgr =
                        (ConnectivityManager)Forms.Context.GetSystemService(Context.ConnectivityService))
                {
                    var wifiStat = "Off";
                    var mobileStat = "Off";

                    if (connMgr != null && airplaneMode != "On")
                    {
                        foreach (var networkInfo in connMgr.GetAllNetworkInfo()) //todo this is depricated
                        {
                            switch (networkInfo.Type)
                            {
                                case ConnectivityType.Wifi:
                                    if (networkInfo.IsConnected)
                                        wifiStat = "Connected";
                                    break;
                                case ConnectivityType.Mobile:
                                    if (networkInfo.IsConnected)
                                    {
                                        mobileStat = networkInfo.IsRoaming ? "Roaming" : "Connected";
                                    }
                                    break;
                            }
                        }
                        if (connMgr.ActiveNetworkInfo != null)
                        {
                            switch (connMgr.ActiveNetworkInfo.Type)
                            {
                                case ConnectivityType.Wifi:
                                    wifiStat = "Active " + wifiStat;
                                    break;
                                case ConnectivityType.Mobile:
                                    mobileStat = "Active " + mobileStat;
                                    break;
                            }
                        }
                    }

                    diagData.AddProperty("Cellular Status", mobileStat);
                    diagData.AddProperty("Wifi Status", wifiStat);
                }
            }
            catch
            {

                //debug something, whatever this is demo code.
            }

            return diagData;
        }
    }
}