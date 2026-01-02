using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiveLogExporter.Exporter
{
    public class GarminDiveLogExporter : IDiveLogExporter
    {
        public string Name => "Garmin Dive Log Exporter";

        public IReadOnlyList<string> SupportedExtensions => new List<string> { ".fit" };

        public bool CanHandle(string inputPath)
        {
            return File.Exists(inputPath) && SupportedExtensions.Contains(Path.GetExtension(inputPath), StringComparer.OrdinalIgnoreCase);
        }

        public List<ExportedDiveLog> Export(string inputPath)
        {
            throw new NotImplementedException();
        }
    }
}
