using System.Collections.Generic;
using XamarinSwipe.DependencyInterfaces;

namespace XamarinSwipe
{
	public class DiagnosticData : IDiagnosticData
	{
		public string Platform { get; set; }
		public string AppVersion { get; set; }
		public string OsVersion { get; set; }
		public string DeviceType { get; set; }
	    public IList<IDiagnosticEntry> AdditionalProperties { get; }
	    public void AddProperty(string name, string value)
	    {
            AdditionalProperties.Add(new DiagnosticEntry(name,value));
        }

        public DiagnosticData ()
		{
			AdditionalProperties = new List<IDiagnosticEntry>();
		}
	}
}

