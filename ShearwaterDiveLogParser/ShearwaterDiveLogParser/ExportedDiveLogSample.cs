namespace ShearwaterDiveLogExporter
{
    internal class ExportedDiveLogSample
    {
        public int Number { get; set; }

        public int ElapsedTimeInSeconds { get; set; }

        public object Depth { get; set; }

        public int TimeToSurfaceInMinutes { get; set; }

        public int TimeToSurfaceInMinutesAtPlusFive { get; set; }

        public int NoDecoLimit { get; set; }

        public int CNS { get; set; }

        public double GasDensity { get; set; }

        public int GradientFactor99 { get; set; }

        public float PPO2 { get; set; }

        public float PPN2 { get; set; }

        public float PPHE { get; set; }

        public string TankPressureInBar { get; set; }

        public string SAC { get; set; }

        public int Temperature { get; set; }

        public float BatteryVoltage { get; set; }

        public string GasTimeRemainingInMinutes { get; set; }
    }
}
