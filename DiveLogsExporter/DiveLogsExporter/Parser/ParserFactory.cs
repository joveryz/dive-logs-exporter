using System.Collections.Generic;
using System.Linq;

namespace DiveLogsExporter.Parser
{
    public class ParserFactory
    {
        private readonly List<IDiveLogParser> _parsers = new List<IDiveLogParser>();

        public ParserFactory()
        {
            _parsers.Add(new ShearwaterDiveLogParser());
            _parsers.Add(new GarminDiveLogParser());
        }

        public IReadOnlyList<IDiveLogParser> GetAllParsers() => _parsers.AsReadOnly();

        public IDiveLogParser GetParser(string inputPath)
        {
            return _parsers.FirstOrDefault(e => e.CanHandle(inputPath));
        }

        public IEnumerable<string> GetSupportedExtensions()
        {
            return _parsers.SelectMany(e => e.SupportedExtensions).Distinct();
        }
    }
}
