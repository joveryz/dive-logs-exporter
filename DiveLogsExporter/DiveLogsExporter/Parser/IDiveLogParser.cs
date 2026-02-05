using System.Collections.Generic;
using DiveLogsExporter.Model;

namespace DiveLogsExporter.Parser
{
    public interface IDiveLogParser
    {
        string Name { get; }

        IReadOnlyList<string> SupportedExtensions { get; }

        bool CanHandle(string inputPath);

        List<GeneralDiveLog> Parse(string inputPath);
    }
}
