using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
namespace Helios.Relay.CsvExport
{
    public class CsvExportField
    {
        public CsvExportField(string name, string section, string key)
        {
            Name = name;
            Source = section;
            Key = key;
        }

        public readonly string Name;
        public readonly string Source;
        public readonly string Key;
    }
}
