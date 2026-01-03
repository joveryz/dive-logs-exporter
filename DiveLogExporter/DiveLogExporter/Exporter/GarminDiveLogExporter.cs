using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Persistence.LocalCache;
using DiveLogExporter.Model;
using DiveLogModels;
using Dynastream.Fit;
using OxyPlot;
using Shearwater;
using static Assets.Scripts.Sync.AppSyncService;

namespace DiveLogExporter.Exporter
{
    public class GarminDiveLogExporter : IDiveLogExporter
    {
        public string Name => "Garmin";

        public IReadOnlyList<string> SupportedExtensions => new List<string> { ".fit" };

        public bool CanHandle(string inputPath)
        {
            return System.IO.File.Exists(inputPath) && SupportedExtensions.Contains(Path.GetExtension(inputPath), StringComparer.OrdinalIgnoreCase);
        }

        public List<GeneralDiveLog> Export(string inputPath)
        {
            var decodeDemo = new Decode();
            var fitListener = new FitListener();
            decodeDemo.MesgEvent += fitListener.OnMesg;
            decodeDemo.Read(new FileStream(inputPath, FileMode.Open));
            return ExportDiveLogs(fitListener.FitMessages);
        }

        private List<GeneralDiveLog> ExportDiveLogs(FitMessages garminDiveLogs)
        {
            var res = new List<GeneralDiveLog>();
            var summaries = new List<GeneralDiveLogSummary>();

            var garminSession = garminDiveLogs.SessionMesgs.First();

            for (int i = 0; i < garminDiveLogs.LapMesgs.Count; ++i)
            {
                var garminLap = garminDiveLogs.LapMesgs[i];
                var garminDiveSummary = garminDiveLogs.DiveSummaryMesgs[i + 1];

                summaries.Add(new GeneralDiveLogSummary
                {
                    // Summary Info
                    Number = (int)garminDiveSummary.GetReferenceIndex(),
                    Mode = garminLap.GetSubSport().ToString(),
                    StartDate = new Dynastream.Fit.DateTime(garminLap.GetStartTime().GetTimeStamp(), -2).ToString(),
                    EndDate = new Dynastream.Fit.DateTime(garminLap.GetStartTime().GetTimeStamp(), garminDiveSummary.GetBottomTime().Value + 3).ToString(),
                    DurationInSeconds = (int)Math.Floor(garminDiveSummary.GetBottomTime().Value) + 5,
                    Buddy = "Unknown",
                    Location = "Unknown",
                    Site = "Unknown",
                    Note = "Unknown",

                    // Environment Info
                    DepthInMetersMax = garminDiveSummary.GetMaxDepth().Value,
                    DepthInMetersAvg = garminDiveSummary.GetAvgDepth().Value,
                    TemperatureInCelsiusMax = garminLap.GetMaxTemperature().Value,
                    TemperatureInCelsiusMin = garminLap.GetMinTemperature().Value,
                    TemperatureInCelsiusAvg = garminLap.GetAvgTemperature().Value,
                    SurfacePressureInMillibarPreDive = -1,
                    SurfacePressureInMillibarPostDive = -1,
                    SurfaceIntervalInSeconds = (int)garminDiveSummary.GetSurfaceInterval().GetValueOrDefault(0),
                    Salinity = -1,
                    SalinityType = "Unknown",

                    // Computer Info
                    //ComputerModel = ShearwaterUtilsWrapper.GetComputerName(garminDiveSummary),
                    //ComputerSerialNumber = ShearwaterUtilsWrapper.GetComputerSerialNumber(garminDiveSummary),
                    //ComputerFirmwareVersion = (int)header.FirmwareVersion,
                    //BatteryType = ShearwaterUtilsWrapper.GetComputerBatteryType(garminDiveSummary),
                    //BatteryVoltagePreDive = header.InternalBatteryVoltage,
                    //BatteryVoltagePostDive = footer.InternalBatteryVoltage,
                    //SampleRateInMs = header.SampleRateMs,
                    //DataFormat = $"{interpretedLog.DiveLogDataFormat}-{ShearwaterUtilsWrapper.GetDiveLogVersion(garminDiveSummary)}-{garminDiveSummary.DbVersion}",
                });

                res.Add(new GeneralDiveLog
                {
                    Summary = summaries.Last(),
                });
            }

            var samples = new List<GeneralDiveLogSample>();

            foreach (var garminRecord in garminDiveLogs.RecordMesgs)
            {
                samples.Add(new GeneralDiveLogSample
                {
                    Number = (int)garminRecord.GetTimestamp().GetTimeStamp(),
                    Depth = garminRecord.GetDepth(),
                    Temperature = (int)garminRecord.GetTemperature(),
                });
            }

            foreach (var sample in samples)
            {
                var currentTime = new Dynastream.Fit.DateTime((uint)sample.Number).GetDateTime();
                foreach (var diveLog in res)
                {
                    var diveStartTime = System.DateTime.Parse(diveLog.Summary.StartDate);
                    var diveEndTime = System.DateTime.Parse(diveLog.Summary.EndDate);
                    if (currentTime >= diveStartTime && currentTime <= diveEndTime)
                    {
                        sample.Number = diveLog.Summary.Number;
                        sample.ElapsedTimeInSeconds = (int)(currentTime - diveStartTime).TotalSeconds;
                        if (diveLog.Samples == null)
                        {
                            diveLog.Samples = new List<GeneralDiveLogSample>();
                        }
                        diveLog.Samples.Add(sample);
                    }
                }
            }

            return res;
        }
    }
}
