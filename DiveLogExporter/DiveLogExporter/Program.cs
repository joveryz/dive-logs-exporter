using System;
using System.IO;
using System.Text;
using Assets.Scripts.Persistence.LocalCache;
using DiveLogExporter.Exporter;
using DiveLogModels;

namespace DiveLogExporter
{
    internal class Program
    {
        static int Main(string[] args)
        {
            // 1st arg: path to shearwater db
            // 2nd arg: path to output csv files
            if (args.Length != 2)
            {
                Console.WriteLine("Usage: DiveLogExporter <path to shearwater db> <output directory>");
                return 1;
            }

            var summaryCsvString = new StringBuilder();
            var tankCsvString = new StringBuilder();
            var samplesCsvString = new StringBuilder();

            summaryCsvString.AppendLine(new ExportedDiveLogSummary().ToCsvHeader());
            tankCsvString.AppendLine(new ExportedDiveLogTank().ToCsvHeader());
            samplesCsvString.AppendLine(new ExportedDiveLogSample().ToCsvHeader());

            var inputPath = args[0];
            var factory = new ExporterFactory();
            var exporter = factory.GetExporter(inputPath);
            if (exporter == null)
            {
                Console.WriteLine("Cannot find an exporter for the given input file.");
                return 1;
            }

            exporter.Export(args[0]).ForEach(diveLog =>
            {
                summaryCsvString.AppendLine(diveLog.Summary.ToCsvRow());
                tankCsvString.AppendLine(diveLog.Tank.ToCsvRow());
                samplesCsvString.AppendLine(diveLog.Samples.ToCsvRows());
            });

            var destDir = args[1];
            if (!destDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                destDir += Path.DirectorySeparatorChar;
            }
            Console.WriteLine($"Writing export files to {destDir}...");
            File.WriteAllText(Path.Combine(destDir, "shearwater-export-summary.csv"), summaryCsvString.ToString());
            File.WriteAllText(Path.Combine(destDir, "shearwater-export-tanks.csv"), tankCsvString.ToString());
            File.WriteAllText(Path.Combine(destDir, "shearwater-export-samples.csv"), samplesCsvString.ToString());
            Console.WriteLine("Export complete.");

            return 0;
        }
    }
}
