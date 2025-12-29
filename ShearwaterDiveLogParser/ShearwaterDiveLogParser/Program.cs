using Assets.Scripts.Persistence.LocalCache;
using Shearwater;

namespace ShearwaterDiveLogExporter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataService dataService = new DataService("D:\\shearwater-example.db");
            LocalCache localCache = new LocalCache(dataService);
            string[] allIds = dataService.GetAllIds();
            var logs = dataService.GetDiveLogsWithRaw();

            var id = allIds[23];
            var log = dataService.GetDiveLog(id);
            var samples = dataService.GetDiveLogRecordsWithRaw(id);

            DiveLogSummary summary = new DiveLogSummary(log);

            object[] para = new object[2];
            para[0] = log;
            para[1] = samples.ToArray();
            ShearwaterXMLExporterMod exporter = new ShearwaterXMLExporterMod();
            exporter.ExportDive(para, "D:\\123333");
        }
    }
}
