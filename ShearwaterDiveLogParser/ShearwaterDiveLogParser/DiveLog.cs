using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShearwaterDiveLogExporter
{
    internal class DiveLog
    {
        public DiveLogSummary Summary { get; set; }

        public List<DiveLogSample> Samples { get; set; }
    }
}
