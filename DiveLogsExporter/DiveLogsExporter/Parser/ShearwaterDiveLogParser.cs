using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Persistence.LocalCache;
using DiveLogsExporter.Model;
using DiveLogModels;

namespace DiveLogsExporter.Parser
{
    public class ShearwaterDiveLogParser : IDiveLogParser
    {
        public string Name => "Shearwater";

        public IReadOnlyList<string> SupportedExtensions { get; } = new List<string> { ".db" };

        private static readonly int MaxTankCount = 4;

        public bool CanHandle(string inputPath)
        {
            return File.Exists(inputPath) && SupportedExtensions.Contains(Path.GetExtension(inputPath), StringComparer.OrdinalIgnoreCase);
        }

        public List<GeneralDiveLog> Parse(string inputPath)
        {
            var shearwaterDataService = new DataService(inputPath);
            var shearwaterDiveLogs = shearwaterDataService.GetDiveLogsWithRaw();
            Console.WriteLine($"[{Name}] Found {shearwaterDiveLogs.Count} dives");

            var res = new List<GeneralDiveLog>();

            foreach (var shearwaterDiveLog in shearwaterDiveLogs)
            {
                var shearwaterDiveLogSamples = shearwaterDataService.GetDiveLogRecordsWithRaw(shearwaterDiveLog.DiveID);
                res.Add(ParseSingleDiveLog(shearwaterDiveLog, shearwaterDiveLogSamples));
            }

            return res;
        }


        private GeneralDiveLog ParseSingleDiveLog(DiveLog shearwaterDiveLog, List<DiveLogSample> shearwaterDiveLogSamples)
        {
            return new GeneralDiveLog
            {
                Summary = ParseSingleDiveLogSummary(shearwaterDiveLog),
                Samples = ParseSingleDiveLogSamples(shearwaterDiveLog, shearwaterDiveLogSamples),
                Tanks = ParseSingleDiveLogTanks(shearwaterDiveLog),
            };
        }

        private GeneralDiveLogSummary ParseSingleDiveLogSummary(DiveLog shearwaterDiveLog)
        {
            var header = shearwaterDiveLog.DiveLogHeader;
            var footer = shearwaterDiveLog.DiveLogFooter;
            var details = shearwaterDiveLog.DiveLogDetails;
            var interpretedLog = shearwaterDiveLog.InterpretedLogData;

            var timezoneInfo = Shearwater.ShearwaterUtils.GetTimezoneFromLocation(details.Location.Value);
            var startDate = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(details.DiveDate, DateTimeKind.Unspecified), timezoneInfo);

            var res = new GeneralDiveLogSummary
            {
                // Summary Info
                Number = Shearwater.ShearwaterUtils.GetDiveNumber(shearwaterDiveLog),
                Mode = Shearwater.ShearwaterUtils.GetDiveMode(shearwaterDiveLog),
                StartDate = startDate,
                EndDate = startDate.AddSeconds(footer.DiveTimeInSeconds),
                DurationInSeconds = footer.DiveTimeInSeconds,
                Diver = Shearwater.ShearwaterUtils.GetDiver(shearwaterDiveLog) ?? "Jovery",
                Buddy = details.Buddy.Value,
                Location = details.Location.Value,
                Site = details.Site.Value,
                Note = Shearwater.ShearwaterUtils.GetNotes(shearwaterDiveLog),

                // Environment Info
                DepthInMetersMax = footer.MaxDiveDepth,
                DepthInMetersAvg = interpretedLog.AverageDepth,
                //HeartRateMax = new Random().Next(120, 180),
                //HeartRateMin = new Random().Next(40, 60),
                //HeartRateAvg = new Random().Next(60, 120),
                TemperatureInCelsiusMax = interpretedLog.MaxTemp,
                TemperatureInCelsiusMin = interpretedLog.MinTemp,
                TemperatureInCelsiusAvg = interpretedLog.AverageTemp,
                SurfacePressureInMillibarPreDive = header.SurfacePressure,
                SurfacePressureInMillibarPostDive = footer.SurfacePressure,
                SurfaceIntervalInSeconds = (int)TimeSpan.FromMinutes(header.SurfaceTime).TotalSeconds,
                WaterDenisity = header.Salinity,
                WaterType = Shearwater.ShearwaterUtils.GetSalinityType(shearwaterDiveLog),

                // Computer Info
                ComputerModel = Shearwater.ShearwaterUtils.GetComputerName(shearwaterDiveLog),
                ComputerSerialNumber = Shearwater.ShearwaterUtils.GetComputerSerialNumber(shearwaterDiveLog),
                ComputerFirmwareVersion = header.FirmwareVersion.ToString(),
                BatteryType = Shearwater.ShearwaterUtils.GetComputerBatteryType(shearwaterDiveLog),
                BatteryVoltagePreDive = header.InternalBatteryVoltage,
                BatteryVoltagePostDive = footer.InternalBatteryVoltage,
                SampleRateInMs = header.SampleRateMs,
                DataFormat = $"{interpretedLog.DiveLogDataFormat}-{Shearwater.ShearwaterUtils.GetDiveLogVersion(shearwaterDiveLog)}-{shearwaterDiveLog.DbVersion}",
            };

            if (!Shearwater.ShearwaterUtils.IsFreeDive(shearwaterDiveLog))
            {
                // Optional Deco Info
                res.DecoModel = Shearwater.ShearwaterUtils.GetDecoModel(shearwaterDiveLog);
                res.GradientFactorLow = header.GradientFactorLow;
                res.GradientFactorHigh = header.GradientFactorHigh;
                res.GradientFactorSurfaceEnd = interpretedLog.PeakEndGF99;
                res.CentralNervousSystemPercentPreDive = header.CnsPercent;
                res.CentralNervousSystemPercentPostDive = footer.CnsPercent;
            }

            return res;
        }

        private List<GeneralDiveLogSample> ParseSingleDiveLogSamples(DiveLog shearwaterDiveLog, List<DiveLogSample> shearwaterDiveLogSamples)
        {
            var res = new List<GeneralDiveLogSample>();
            //var random = new Random();

            foreach (var shearwaterDiveLogSample in shearwaterDiveLogSamples)
            {
                if (shearwaterDiveLogSample.RawBytes != null)
                {
                    continue;
                }

                var sample = new GeneralDiveLogSample
                {
                    Number = Shearwater.ShearwaterUtils.GetDiveNumber(shearwaterDiveLog),
                    ElapsedTimeInSeconds = (int)shearwaterDiveLogSample.TimeSinceStartInSeconds,
                    Depth = Shearwater.ShearwaterUtils.GetDepthInMeters(shearwaterDiveLog, shearwaterDiveLogSample),
                    Temperature = shearwaterDiveLogSample.WaterTemperature,
                    //HeartRate = random.Next(60, 120),
                    BatteryVoltage = shearwaterDiveLogSample.BatteryVoltage,
                };

                if (!Shearwater.ShearwaterUtils.IsFreeDive(shearwaterDiveLog))
                {

                    sample.TimeToSurfaceInMinutes = shearwaterDiveLogSample.TimeToSurface;
                    sample.TimeToSurfaceInMinutesAtPlusFive = shearwaterDiveLogSample.AtPlusFive;
                    sample.NoDecoLimit = shearwaterDiveLogSample.CurrentNoDecoLimit;
                    sample.CentralNervousSystemPercent = shearwaterDiveLogSample.CentralNervousSystemPercentage;
                    sample.GasDensity = Shearwater.ShearwaterUtils.GetGasDensityInGPerL(shearwaterDiveLog, shearwaterDiveLogSample);
                    sample.GradientFactor99 = shearwaterDiveLogSample.Gf99;
                    sample.PPO2 = shearwaterDiveLogSample.AveragePPO2;
                    (sample.PPN2, sample.PPHe) = Shearwater.ShearwaterUtils.GetGasPartialPressureInAta(shearwaterDiveLog, shearwaterDiveLogSample);
                    sample.Tank1PressureInBar = Shearwater.ShearwaterUtils.GetTankPressureInBar(shearwaterDiveLogSample, 0);
                    sample.Tank2PressureInBar = Shearwater.ShearwaterUtils.GetTankPressureInBar(shearwaterDiveLogSample, 1);
                    sample.Tank3PressureInBar = Shearwater.ShearwaterUtils.GetTankPressureInBar(shearwaterDiveLogSample, 2);
                    sample.Tank4PressureInBar = Shearwater.ShearwaterUtils.GetTankPressureInBar(shearwaterDiveLogSample, 3);
                    sample.SurfaceAirConsumptionInBar = Shearwater.ShearwaterUtils.GetSurfaceAirConsumptionInBar(shearwaterDiveLogSample);
                    sample.GasTimeRemainingInMinutes = Shearwater.ShearwaterUtils.GetGasTimeRemainingInMinutes(shearwaterDiveLogSample);
                }

                res.Add(sample);
            }

            return res.Any() ? res : null;
        }

        private List<GeneralDiveLogTankInformation> ParseSingleDiveLogTanks(DiveLog shearwaterDiveLog)
        {
            var res = new List<GeneralDiveLogTankInformation>();
            if (!Shearwater.ShearwaterUtils.IsFreeDive(shearwaterDiveLog))
            {
                for (int i = 0; i < MaxTankCount; ++i)
                {
                    var tankInfo = Shearwater.ShearwaterUtils.GetTankInformation(shearwaterDiveLog, i);

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
