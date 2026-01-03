using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Persistence.LocalCache;
using Castle.Core.Logging;
using DiveLogExporter.Model;
using DiveLogModels;
using Shearwater;

namespace DiveLogExporter.Exporter
{
    public class ShearwaterDiveLogExporter : IDiveLogExporter
    {
        public string Name => "Shearwater";

        public IReadOnlyList<string> SupportedExtensions { get; } = new List<string> { ".db" };

        private static readonly int MaxTankCount = 4;

        public bool CanHandle(string inputPath)
        {
            return File.Exists(inputPath) && SupportedExtensions.Contains(Path.GetExtension(inputPath), StringComparer.OrdinalIgnoreCase);
        }

        public List<GeneralDiveLog> Export(string inputPath)
        {
            var shearwaterDataService = new DataService(inputPath);
            var shearwaterDiveLogs = shearwaterDataService.GetDiveLogsWithRaw();
            Console.WriteLine($"[{Name}] Found {shearwaterDiveLogs.Count} dives in database");

            var res = new List<GeneralDiveLog>();

            foreach (var shearwaterDiveLog in shearwaterDiveLogs)
            {
                var shearwaterDiveLogSamples = shearwaterDataService.GetDiveLogRecordsWithRaw(shearwaterDiveLog.DiveID);
                res.Add(ExportSingleDiveLog(shearwaterDiveLog, shearwaterDiveLogSamples));
            }

            return res;
        }


        private GeneralDiveLog ExportSingleDiveLog(DiveLog shearwaterDiveLog, List<DiveLogSample> shearwaterDiveLogSamples)
        {
            return new GeneralDiveLog
            {
                Summary = ExportSingleDiveLogSummary(shearwaterDiveLog),
                Samples = ExportSingleDiveLogSampls(shearwaterDiveLog, shearwaterDiveLogSamples),
                Tanks = ExportSingleDiveLogTanks(shearwaterDiveLog),
            };
        }

        private GeneralDiveLogSummary ExportSingleDiveLogSummary(DiveLog shearwaterDiveLog)
        {
            var header = shearwaterDiveLog.DiveLogHeader;
            var footer = shearwaterDiveLog.DiveLogFooter;
            var details = shearwaterDiveLog.DiveLogDetails;
            var interpretedLog = shearwaterDiveLog.InterpretedLogData;

            var res = new GeneralDiveLogSummary
            {
                // Summary Info
                Number = ShearwaterUtilsWrapper.GetDiveNumber(shearwaterDiveLog),
                Mode = ShearwaterUtilsWrapper.GetDiveMode(shearwaterDiveLog),
                StartDate = details.DiveDate.ToString(),
                EndDate = details.DiveDate.AddSeconds(footer.DiveTimeInSeconds).ToString(),
                DurationInSeconds = footer.DiveTimeInSeconds,
                Buddy = details.Buddy.Value,
                Location = details.Location.Value,
                Site = details.Site.Value,
                Note = details.Notes.Value,

                // Environment Info
                DepthInMetersMax = footer.MaxDiveDepth,
                DepthInMetersAvg = interpretedLog.AverageDepth,
                TemperatureInCelsiusMax = interpretedLog.MaxTemp,
                TemperatureInCelsiusMin = interpretedLog.MinTemp,
                TemperatureInCelsiusAvg = interpretedLog.AverageTemp,
                SurfacePressureInMillibarPreDive = header.SurfacePressure,
                SurfacePressureInMillibarPostDive = footer.SurfacePressure,
                SurfaceIntervalInSeconds = (int)TimeSpan.FromMinutes(header.SurfaceTime).TotalSeconds,
                Salinity = header.Salinity,
                SalinityType = ShearwaterUtilsWrapper.GetSalinityType(shearwaterDiveLog),

                // Computer Info
                ComputerModel = ShearwaterUtilsWrapper.GetComputerName(shearwaterDiveLog),
                ComputerSerialNumber = ShearwaterUtilsWrapper.GetComputerSerialNumber(shearwaterDiveLog),
                ComputerFirmwareVersion = (int)header.FirmwareVersion,
                BatteryType = ShearwaterUtilsWrapper.GetComputerBatteryType(shearwaterDiveLog),
                BatteryVoltagePreDive = header.InternalBatteryVoltage,
                BatteryVoltagePostDive = footer.InternalBatteryVoltage,
                SampleRateInMs = header.SampleRateMs,
                DataFormat = $"{interpretedLog.DiveLogDataFormat}-{ShearwaterUtilsWrapper.GetDiveLogVersion(shearwaterDiveLog)}-{shearwaterDiveLog.DbVersion}",
            };

            if (!ShearwaterUtilsWrapper.IsFreeDive(shearwaterDiveLog))
            {
                // Optional Deco Info
                res.DecoModel = ShearwaterUtilsWrapper.GetDecoModel(shearwaterDiveLog);
                res.GradientFactorLow = header.GradientFactorLow;
                res.GradientFactorHigh = header.GradientFactorHigh;
                res.GradientFactor99Max = interpretedLog.PeakEndGF99;
                res.CNSPercentPreDive = header.CnsPercent;
                res.CNSPercentPostDive = footer.CnsPercent;
            }

            return res;
        }

        private List<GeneralDiveLogSample> ExportSingleDiveLogSampls(DiveLog shearwaterDiveLog, List<DiveLogSample> shearwaterDiveLogSamples)
        {
            var res = new List<GeneralDiveLogSample>();

            foreach (var shearwaterDiveLogSample in shearwaterDiveLogSamples)
            {
                if (shearwaterDiveLogSample.RawBytes != null)
                {
                    continue;
                }

                var sample = new GeneralDiveLogSample
                {
                    Number = ShearwaterUtilsWrapper.GetDiveNumber(shearwaterDiveLog),
                    ElapsedTimeInSeconds = (int)shearwaterDiveLogSample.TimeSinceStartInSeconds,
                    Depth = ShearwaterUtilsWrapper.GetDepthInMeters(shearwaterDiveLog, shearwaterDiveLogSample),
                    Temperature = shearwaterDiveLogSample.WaterTemperature,
                    BatteryVoltage = shearwaterDiveLogSample.BatteryVoltage,
                };

                if (!ShearwaterUtilsWrapper.IsFreeDive(shearwaterDiveLog))
                {

                    sample.TimeToSurfaceInMinutes = shearwaterDiveLogSample.TimeToSurface;
                    sample.TimeToSurfaceInMinutesAtPlusFive = shearwaterDiveLogSample.AtPlusFive;
                    sample.NoDecoLimit = shearwaterDiveLogSample.CurrentNoDecoLimit;
                    sample.CNS = shearwaterDiveLogSample.CentralNervousSystemPercentage;
                    sample.GasDensity = ShearwaterUtilsWrapper.GetGasDensityInGPerL(shearwaterDiveLog, shearwaterDiveLogSample);
                    sample.GradientFactor99 = shearwaterDiveLogSample.Gf99;
                    sample.PPO2 = shearwaterDiveLogSample.AveragePPO2;
                    (sample.PPN2, sample.PPHe) = ShearwaterUtilsWrapper.GetGasPartialPressureInAta(shearwaterDiveLog, shearwaterDiveLogSample);
                    sample.Tank1PressureInBar = ShearwaterUtilsWrapper.GetTankPressureInBar(shearwaterDiveLogSample, 0);
                    sample.Tank2PressureInBar = ShearwaterUtilsWrapper.GetTankPressureInBar(shearwaterDiveLogSample, 1);
                    sample.Tank3PressureInBar = ShearwaterUtilsWrapper.GetTankPressureInBar(shearwaterDiveLogSample, 2);
                    sample.Tank4PressureInBar = ShearwaterUtilsWrapper.GetTankPressureInBar(shearwaterDiveLogSample, 3);
                    sample.SurfaceAirConsumptionInBar = ShearwaterUtilsWrapper.GetSurfaceAirConsumptionInBar(shearwaterDiveLogSample);
                    sample.GasTimeRemainingInMinutes = ShearwaterUtilsWrapper.GetGasTimeRemainingInMinutes(shearwaterDiveLogSample);
                }

                res.Add(sample);
            }

            return res.Any() ? res : null;
        }

        private List<GeneralDiveLogTankInformation> ExportSingleDiveLogTanks(DiveLog shearwaterDiveLog)
        {
            var res = new List<GeneralDiveLogTankInformation>();
            if (!ShearwaterUtilsWrapper.IsFreeDive(shearwaterDiveLog))
            {
                for (int i = 0; i < MaxTankCount; ++i)
                {
                    var tankInfo = ShearwaterUtilsWrapper.GetTankInformation(shearwaterDiveLog, i);

                    if (tankInfo != null)
                    {
                        res.Add(tankInfo);
                    }
                }
            }

            return res.Any() ? res : null;
        }
    }
}
