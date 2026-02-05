namespace DiveLogsExporter.Model
{
    public class GeneralDiveLogTankInformation
    {
        public int? Number { get; set; }

        public int? Index { get; set; }

        public bool? Enabled { get; set; }

        public string? TransmitterName { get; set; }

        public string? TransmitterSerialNumber { get; set; }

        public double? AverageDepthInMeters { get; set; }

        public int? GasO2Percent { get; set; }

        public int? GasHePercent { get; set; }

        public int? GasN2Percent { get; set; }
    }
}
