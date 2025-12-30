using System;
using System.Collections.Generic;
using Assets.Scripts.DiveLogs.Utils.DiveLogUtils;
using Assets.Scripts.DiveLogs.Utils.Gases;
using Assets.Scripts.Utility;
using Assets.ShearwaterCloud.Modules.Graphs.DiveGraph.GraphAssembly.GraphDataAssembly.SeriesSampleAssemblers;
using CoreParserUtilities;
using DiveLogModels;
using ExtendedCoreParserUtilities;
using Shearwater;
using ShearwaterUtils;

namespace ShearwaterDiveLogExporter
{
    internal class ExportedDiveLog
    {
        public ExportedDiveLogSummary Summary { get; set; }

        public ExportedDiveLogTankProfile Profile { get; set; }

        public List<ExportedDiveLogSample> Samples { get; set; }

        public ExportedDiveLog(DiveLog shearwaterDiveLog, List<DiveLogSample> shearwaterDiveLogSamples)
        {
            var header = shearwaterDiveLog.DiveLogHeader;
            var footer = shearwaterDiveLog.DiveLogFooter;
            var details = shearwaterDiveLog.DiveLogDetails;
            var interpretedLog = shearwaterDiveLog.InterpretedLogData;
            var finalLog = shearwaterDiveLog.FinalLog;
            var tankProfileData = TankProfileSerializer.ConvertStringToTankProfileData(shearwaterDiveLog.DiveLogDetails.TankProfileData.Value);
            Summary = new ExportedDiveLogSummary
            {
                // Summary Info
                Number = int.Parse(DiveLogMetaDataResolver.GetDiveNumber(shearwaterDiveLog)),
                Mode = DiveLogModeUtils.GetModeName(header.Mode, header.OCRecSubMode, DiveLogMetaDataResolver.GetLogVersion(shearwaterDiveLog)),
                StartDate = shearwaterDiveLog.DiveLogDetails.DiveDate.ToString(),
                EndDate = (shearwaterDiveLog.DiveLogDetails.DiveDate + (default(DateTime).FromUnixTimeStamp(footer.Timestamp) - default(DateTime).FromUnixTimeStamp(header.Timestamp))).ToString(),
                DurationInSeconds = footer.DiveTimeInSeconds,
                DepthInMetersMax = footer.MaxDiveDepth,
                DepthInMetersAvg = interpretedLog.AverageDepth,
                Buddy = shearwaterDiveLog.DiveLogDetails.Buddy.Value,
                Location = shearwaterDiveLog.DiveLogDetails.Location.Value,
                Site = shearwaterDiveLog.DiveLogDetails.Site.Value,
                Note = shearwaterDiveLog.DiveLogDetails.Notes.Value,

                // Environment Info
                TemperatureInCelsiusMax = interpretedLog.MaxTemp,
                TemperatureInCelsiusMin = interpretedLog.MinTemp,
                TemperatureInCelsiusAvg = interpretedLog.AverageTemp,
                Salinity = DiveLogEnvironmentUtils.GetSalinityString(header.Salinity),
                SurfaceIntervalInSeconds = (int)TimeSpan.FromMinutes(header.SurfaceTime).TotalSeconds,
                SurfacePressureInMillibarPreDive = header.SurfacePressure,
                SurfacePressureInMillibarPostDive = footer.SurfacePressure,

                // Deco Info
                DecoModel = DecoModelUtil.DecoModelString(header.DecoModel),
                GradientFactorLow = header.GradientFactorLow,
                GradientFactorHigh = header.GradientFactorHigh,
                GradientFactor99Max = interpretedLog.PeakEndGF99,
                CNSPercentPreDive = header.CnsPercent,
                CNSPercentPostDive = footer.CnsPercent,

                // Computer Info
                ComputerModel = DiveLogProductUtilMod.FriendlyProductName(finalLog),
                ComputerSerialNumber = DiveLogSerialNumberUtil.GetSerialNumberToHex(shearwaterDiveLog),
                ComputerFirmwareVersion = (int)header.FirmwareVersion,
                BatteryType = DiveLogBatteryUtil.GetBatteryType(header),
                BatteryVoltagePreDive = header.InternalBatteryVoltage,
                BatteryVoltagePostDive = footer.InternalBatteryVoltage,
                SampleRateInMs = header.SampleRateMs,
                DataFormat = interpretedLog.DiveLogDataFormat,
                LogVersion = DiveLogMetaDataResolver.GetLogVersion(shearwaterDiveLog),
                DatabaseVersion = (int)shearwaterDiveLog.DbVersion,

                // Others
                O2SensorStatusPreDive = header.O2Sensor1Status,
                O2SensorStatusPostDive = footer.O2Sensor1Status,
                SensorDisplay = header.SensorDisplay,
                PPO2SetpointLowPreDive = header.LowPPO2Setpoint,
                PPO2SetpointLowPostDive = footer.LowPPO2Setpoint,
                PPO2SetpointHighPreDive = header.HighPPO2Setpoint,
                PPO2SetpointHighPostDive = footer.HighPPO2Setpoint,
                Features = finalLog.Features,
            };

            Samples = new List<ExportedDiveLogSample>();

            foreach (var shearwaterDiveLogSample in shearwaterDiveLogSamples)
            {
                if (shearwaterDiveLogSample.RawBytes != null)
                {
                    continue;
                }

                var absolutePressureInAta = GasUtil.GetAbsolutePressureATA((float)header.SurfacePressure, ConvertDepthToMeters(header, shearwaterDiveLogSample.Depth), false);
                var partialPressures = GasUtil.FindInertGasPartialPressures(shearwaterDiveLogSample.AveragePPO2, absolutePressureInAta, shearwaterDiveLogSample.FractionO2, shearwaterDiveLogSample.FractionHe);

                Samples.Add(new ExportedDiveLogSample
                {
                    Number = Summary.Number,
                    ElapsedTimeInSeconds = (int)shearwaterDiveLogSample.TimeSinceStartInSeconds,
                    Depth = ConvertDepthToMeters(header, shearwaterDiveLogSample.Depth),
                    TimeToSurfaceInMinutes = shearwaterDiveLogSample.TimeToSurface,
                    TimeToSurfaceInMinutesAtPlusFive = shearwaterDiveLogSample.AtPlusFive,
                    NoDecoLimit = shearwaterDiveLogSample.CurrentNoDecoLimit,
                    CNS = shearwaterDiveLogSample.CentralNervousSystemPercentage,
                    GasDensity = GraphSampleGasDensity.GasDensityFormulaOpenCircuit(shearwaterDiveLogSample, absolutePressureInAta),
                    GradientFactor99 = shearwaterDiveLogSample.Gf99,
                    PPO2 = shearwaterDiveLogSample.AveragePPO2,
                    PPN2 = partialPressures.ppN2ATA,
                    PPHE = partialPressures.ppHeATA,
                    TankPressureInBar = DiveLogGasMessageRetrieverMod.Get_Tank0_Message(shearwaterDiveLogSample),
                    SAC = DiveLogGasMessageRetrieverMod.Get_SAC_Message(shearwaterDiveLogSample),
                    Temperature = shearwaterDiveLogSample.WaterTemperature,
                    BatteryVoltage = shearwaterDiveLogSample.BatteryVoltage,
                    GasTimeRemainingInMinutes = DiveLogGasMessageRetrieverMod.Get_GasTime_Message(shearwaterDiveLogSample),
                });
            }
        }
        private static float ConvertDepthToMeters(DiveLogHeader diveLogHeader, float depth)
        {
            if (depth > 1000)
            {
                return UnitConverter.Convert_pressure_mBars_to_depth_m_f(depth, diveLogHeader.SurfacePressure, diveLogHeader.Salinity);
            }

            return depth;
        }
    }
}
