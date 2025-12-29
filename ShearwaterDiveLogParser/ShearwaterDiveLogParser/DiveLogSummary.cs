using Assets.Scripts.DiveLogs.Utils.DiveLogSampleUtils;
using Assets.Scripts.DiveLogs.Utils.DiveLogUtils;
using Assets.Scripts.Persistence.LocalCache.Schema;
using Assets.Scripts.Persistence.LocalCache.Schema.shearwaterDesktop;
using Assets.Scripts.Utility;
using CoreParserUtilities;
using DiveLogModels;
using ExtendedCoreParserUtilities;
using ShearwaterDiveLogExporter.Subsurface;
using ShearwaterUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace ShearwaterDiveLogExporter
{
    internal class DiveLogSummary
    {
        public int Number { get; set; }

        public int GradientFactorLow { get; set; }

        public int GradientFactorHigh { get; set; }

        public int SurfaceIntervalInMinutes { get; set; }


        public double CNSPercentPreDive { get; set; }

        public double CNSPercentPostDive { get; set; }

        public bool O2SensorStatusPreDive { get; set; }

        public bool O2SensorStatusPostDive { get; set; }

        public string DecoModel { get; set; }


        public int SensorDisplay { get; set; }

        public int SurfacePressureInMBarPreDive { get; set; }

        public int SurfacePressureInMBarPostDive { get; set; }

        public double PPO2SetpointLowPreDive { get; set; }

        public double PPO2SetpointHighPreDive { get; set; }

        public double PPO2SetpointLowPostDive { get; set; }

        public double PPO2SetpointHighPostDive { get; set; }

        public int VpmbConservatism { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public List<bool> ErrorHistory { get; set; }

        public double MaxDepthInMeters { get; set; }

        public int DurationInSeconds { get; set; }








        // Computer Info
        public string ComputerModel { get; set; }

        public string ComputerSerialNumber { get; set; }

        public int ComputerFirmwareVersion { get; set; }

        public string ComputerDataFormat { get; set; }

        public int ComputerLogVersion { get; set; }

        public int ComputerDatabaseVersion { get; set; }

        // Computer Battery Info
        public string ComputerBatteryType { get; set; }

        public double ComputerBatteryVoltagePreDive { get; set; }

        public double ComputerBatteryVoltagePostDive { get; set; }



        public int Features { get; set; }

        public DiveLogSummary(DiveLogModels.DiveLog shearwaterDiveLog)
        {
            DiveLogHeader diveLogHeader = shearwaterDiveLog.DiveLogHeader;
            DiveLogFooter diveLogFooter = shearwaterDiveLog.DiveLogFooter;
            FinalLog finalLog = shearwaterDiveLog.FinalLog;
            TimeSpan timeSpan = default(DateTime).FromUnixTimeStamp(shearwaterDiveLog.DiveLogFooter.Timestamp) - default(DateTime).FromUnixTimeStamp(shearwaterDiveLog.DiveLogHeader.Timestamp);

            Number = int.Parse(DiveLogMetaDataResolver.GetDiveNumber(shearwaterDiveLog));
            GradientFactorLow = diveLogHeader.GradientFactorLow;
            GradientFactorHigh = diveLogHeader.GradientFactorHigh;
            SurfaceIntervalInMinutes = diveLogHeader.SurfaceTime;
            CNSPercentPreDive = diveLogHeader.CnsPercent;
            O2SensorStatusPreDive = diveLogHeader.O2Sensor1Status;
            DecoModel = DecoModelUtil.DecoModelString(diveLogHeader.DecoModel);
            SensorDisplay = diveLogHeader.SensorDisplay;
            SurfacePressureInMBarPreDive = diveLogHeader.SurfacePressure;
            PPO2SetpointLowPreDive = diveLogHeader.LowPPO2Setpoint;
            PPO2SetpointHighPreDive = diveLogHeader.HighPPO2Setpoint;
            VpmbConservatism = diveLogHeader.VpmbConservatism;
            StartDate = shearwaterDiveLog.DiveLogDetails.DiveDate.ToString();
            CNSPercentPostDive = diveLogFooter.CnsPercent;
            EndDate = (shearwaterDiveLog.DiveLogDetails.DiveDate + timeSpan).ToString();
            PPO2SetpointHighPostDive = diveLogFooter.HighPPO2Setpoint;
            PPO2SetpointLowPostDive = diveLogFooter.LowPPO2Setpoint;
            O2SensorStatusPostDive = diveLogFooter.O2Sensor1Status;
            SurfacePressureInMBarPostDive = diveLogFooter.SurfacePressure;
            ErrorHistory = diveLogFooter.ErrorFlags0;
            MaxDepthInMeters = diveLogFooter.MaxDiveDepth;
            DurationInSeconds = diveLogFooter.DiveTimeInSeconds;


            ComputerModel = DiveLogProductUtil.FriendlyProductName(finalLog);
            ComputerSerialNumber = DiveLogSerialNumberUtil.GetSerialNumberToHex(shearwaterDiveLog);
            ComputerFirmwareVersion = (int)diveLogHeader.FirmwareVersion;
            ComputerDataFormat = shearwaterDiveLog.InterpretedLogData.DiveLogDataFormat;
            ComputerLogVersion = DiveLogMetaDataResolver.GetLogVersion(shearwaterDiveLog);
            ComputerDatabaseVersion = (int)shearwaterDiveLog.DbVersion;

            ComputerBatteryType = DiveLogBatteryUtil.GetBatteryType(shearwaterDiveLog.DiveLogHeader);
            ComputerBatteryVoltagePreDive = diveLogHeader.InternalBatteryVoltage;
            ComputerBatteryVoltagePostDive = diveLogFooter.InternalBatteryVoltage;

            Mode = DiveLogModeUtils.GetModeName(diveLogHeader.DisplayMode, diveLogHeader.OCRecSubMode, DiveLogMetaDataResolver.GetLogVersion(shearwaterDiveLog));

            Features = finalLog.Features;
        }
    }
}
