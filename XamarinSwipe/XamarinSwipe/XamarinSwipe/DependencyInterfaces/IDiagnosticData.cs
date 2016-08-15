using System.Collections.Generic;

namespace XamarinSwipe.DependencyInterfaces
{
    public interface IDiagnosticData
    {
        string Platform { get; set; }
        string AppVersion { get; set; }
        string OsVersion { get; set; }
        string DeviceType { get; set; }
        IList<IDiagnosticEntry> AdditionalProperties { get; }
        void AddProperty(string name, string value);        
    }
}