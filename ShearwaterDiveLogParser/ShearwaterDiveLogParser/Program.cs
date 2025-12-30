using System;
using System.IO;
using System.Text;
using Assets.Scripts.Persistence.LocalCache;
using ExtendedCoreParserUtilities;

namespace ShearwaterDiveLogExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // 1st arg: path to shearwater db
            // 2nd arg: path to output csv files
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: ShearwaterDiveLogExporter <path to shearwater db> <output directory>");
                return;
            }

            var shearwaterDataService = new DataService(args[0]);
            var shearwaterDiveLogs = shearwaterDataService.GetDiveLogsWithRaw();
            Console.WriteLine($"Found {shearwaterDiveLogs.Count} dives in Shearwater database.");

            var summaryCsvString = new StringBuilder();
            var samplesCsvString = new StringBuilder();

            summaryCsvString.AppendLine(new ExportedDiveLogSummary().ToCsvHeader());
            samplesCsvString.AppendLine(new ExportedDiveLogSample().ToCsvHeader());

            foreach (var shearwaterDiveLog in shearwaterDiveLogs)
            {
                var shearwaterDiveLogSamples = shearwaterDataService.GetDiveLogRecordsWithRaw(shearwaterDiveLog.DiveID);
                var exportedDiveLog = new ExportedDiveLog(shearwaterDiveLog, shearwaterDiveLogSamples);
                summaryCsvString.AppendLine(exportedDiveLog.Summary.ToCsvRow());
                samplesCsvString.AppendLine(exportedDiveLog.Samples.ToCsvRows());
                Console.WriteLine($"-----Exported dive #{DiveLogMetaDataResolver.GetDiveNumber(shearwaterDiveLog)} with {shearwaterDiveLogSamples.Count} samples");
            }

            File.WriteAllText(Path.Combine(args[1], "shearwater-export-summary.csv"), summaryCsvString.ToString());
            File.WriteAllText(Path.Combine(args[1], "shearwater-export-samples.csv"), samplesCsvString.ToString());
            Console.WriteLine("Export complete.");
        }
    }
}
