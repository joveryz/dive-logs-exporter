using System;
using System.Collections.Generic;
using DiveLogModels;
using Shearwater;

namespace DiveLogExporter
{
    public class ExportedDiveLog
    {
        public ExportedDiveLogSummary Summary { get; set; }

        public ExportedDiveLogTank Tank { get; set; }

        public List<ExportedDiveLogSample> Samples { get; set; }

    }
}
