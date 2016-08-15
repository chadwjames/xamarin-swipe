using XamarinSwipe.DependencyInterfaces;

namespace XamarinSwipe
{
    public class DiagnosticEntry : IDiagnosticEntry
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public DiagnosticEntry(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}