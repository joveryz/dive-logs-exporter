using System;
using System.IO;
using System.Text;
using Assets.Scripts.Persistence.LocalCache;
using DiveLogExporter.Exporter;
using DiveLogExporter.Model;
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

            var allSummary = new StringBuilder();
            var allTanks = new StringBuilder();
            var allSamples = new StringBuilder();

            allSummary.AppendLine(new GeneralDiveLogSummary().ToCsvHeader());
            allTanks.AppendLine(new GeneralDiveLogTankInformation().ToCsvHeader());
            allSamples.AppendLine(new GeneralDiveLogSample().ToCsvHeader());

            var inputPath = args[0];
            var outputPath = args[1];
            var factory = new ExporterFactory();
            var exporter = factory.GetExporter(inputPath);
            if (exporter == null)
            {
                Console.WriteLine("Cannot find an exporter for the given input file.");
                return 1;
            }

            exporter.Export(inputPath).ForEach(diveLog =>
            {
                var summary = diveLog.Summary.ToCsvRow();
                var tanks = diveLog.Tanks.ToCsvRows();
                var samples = diveLog.Samples.ToCsvRows();

                if (!string.IsNullOrWhiteSpace(summary))
                {
                    allSummary.AppendLine(summary);
                }

                if (!string.IsNullOrWhiteSpace(tanks))
                {
                    allTanks.AppendLine(tanks);
                }

                if (!string.IsNullOrWhiteSpace(samples))
                {
                    allSamples.AppendLine(samples);
                }
            });

            if (!outputPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                outputPath += Path.DirectorySeparatorChar;
            }
            Console.WriteLine($"Writing export files to {outputPath}...");
            File.WriteAllText(Path.Combine(outputPath, "shearwater-export-summary.csv"), allSummary.ToString());
            File.WriteAllText(Path.Combine(outputPath, "shearwater-export-tanks.csv"), allTanks.ToString());
            File.WriteAllText(Path.Combine(outputPath, "shearwater-export-samples.csv"), allSamples.ToString());
            Console.WriteLine("Export complete.");

            return 0;
        }
    }
}
