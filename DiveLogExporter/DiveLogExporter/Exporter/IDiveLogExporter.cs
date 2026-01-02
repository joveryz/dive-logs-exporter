using System.Collections.Generic;

namespace DiveLogExporter.Exporter
{
    public interface IDiveLogExporter
    {
        string Name { get; }

        IReadOnlyList<string> SupportedExtensions { get; }

        bool CanHandle(string inputPath);

        List<ExportedDiveLog> Export(string inputPath);
    }
}
