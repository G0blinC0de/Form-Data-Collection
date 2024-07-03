using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace Helios.Relay.CsvExport
{
    public sealed class CsvExportRelay : Relay<CsvExportCacheEntry>
    {
        public CsvExportRelay(ICache cache, IConfiguration config) : base(cache)
        {
            _fields = config.GetSection("CSV-Export").GetSection("Fields").GetChildren()
                .Select(section => new CsvExportField(
                    section.GetSection("name").Get<string>(),
                    section.GetSection("source").Get<string>(),
                    section.GetSection("key").Get<string>()
                    )).ToArray();

            for (var i = 0; i < _fields.Length; i++)
            {
                var validation = ValidateField(_fields[i]);
                if (!validation.IsSuccess) throw new RelayException(validation.Message);
            }

            _entryFactory = new CsvExportCacheEntryFactory(_fields);

            _path = config.GetSection("CSV-Export").GetSection("Path").Get<string>();
            if (string.IsNullOrWhiteSpace(_path)) _path = "Logs/export.csv";
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            if (!File.Exists(_path)) WriteHeaders();
        }

        public override RelayService ServiceType => RelayService.CsvExport;

        private string _path;
        private CsvExportField[] _fields;

        protected override async Task HandleCacheEntry(CsvExportCacheEntry cacheEntry)
        {
            var builder = new StringBuilder();

            // Join the timestamp and fields
            builder.Append(cacheEntry.TimeCreated);
            foreach (var field in _fields) builder.Append($", {cacheEntry.Values[$"{field.Source}.{field.Key}"]}");
            builder.AppendLine();
            
            // Write the line
            await File.AppendAllTextAsync(_path, builder.ToString());
        }

        private ValidationResult ValidateField(CsvExportField field)
        {
            // Check that source has a valid value
            switch (field.Source)
            {
                case "Experience":
                case "Guest":
                case "File":
                    break;

                case "":
                case null:
                    return new ValidationResult(false, $"Missing source for field \"{field.Name}\"");

                default:
                    return new ValidationResult(false, $"Invalid source \"{field.Source}\" for field \"{field.Name}\"");
            }

            // Ensure some value for key
            if (string.IsNullOrWhiteSpace(field.Key)) return new ValidationResult(false, $"Missing key for field \"{field.Name}\"");

            // Valid
            return new ValidationResult(true, string.Empty);
        }

        private void WriteHeaders()
        {
            var builder = new StringBuilder();
            builder.Append("Timestamp, ");
            builder.AppendLine(string.Join(", ", _fields.Select(field => field.Name)));
            File.AppendAllText(_path, builder.ToString());
        }
    }
}
