using System.Collections.Generic;
using DiveLogExporter.Model;

namespace DiveLogExporter.Exporter
{
    public interface IDiveLogExporter
    {
        string Name { get; }

        IReadOnlyList<string> SupportedExtensions { get; }

        bool CanHandle(string inputPath);

        List<GeneralDiveLog> Export(string inputPath);
    }
}
