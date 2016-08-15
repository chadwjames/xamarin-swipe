using System.Collections.Generic;
using Xamarin.Forms;
using XamarinSwipe.DependencyInterfaces;

namespace XamarinSwipe
{
    internal class DiagnosticsViewModel
    {
        public string Title { get; }

        public List<IDiagnosticEntry> DiagData { get; }
        public DiagnosticsViewModel()
        {
            var diagnosticData = DependencyService.Get<IDeviceHelper>().GetDiagnosticData();
            //todo configure the title and determine the is authenticated;
            Title = "System Diags";            

            DiagData = new List<IDiagnosticEntry>
            {
                new DiagnosticEntry("Platform", diagnosticData.Platform),
                new DiagnosticEntry("App Version", diagnosticData.AppVersion),
                new DiagnosticEntry("OS Version", diagnosticData.OsVersion),
                new DiagnosticEntry("Hardware Version", diagnosticData.DeviceType)           
            };
            foreach (var additionalProperty in diagnosticData.AdditionalProperties)
            {
                DiagData.Add(additionalProperty);
            }
        }        
    }
}
