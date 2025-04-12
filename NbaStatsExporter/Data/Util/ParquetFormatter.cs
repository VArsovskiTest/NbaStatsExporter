using System.Reflection;
using ParquetSharp.Schema;
using ParquetSharp;

namespace NbaStatsExporter.Data.ExporterUtil
{
    public class ParquetFormatter
    {
        // Convert the data to Parquet format and write to a stream
        public void ExportDataToParquet<T>(List<T> data, string parquetFilePath, string seasonFolder)
        {
            using (var stream = new FileStream(parquetFilePath, FileMode.Create, FileAccess.Write))
            {
                WriteDataToParquet(data, stream);
            }
        }

        // Write the data to Parquet format using ParquetSharp
        private void WriteDataToParquet<T>(List<T> data, Stream stream)
        {
            var schemaFields = new List<Column>();

            // Generate the schema based on properties of T
            foreach (var property in typeof(T).GetProperties())
            {
                schemaFields.Add(CreateColumn(property));
            }

            // Create the schema group for the Parquet file (a "root" group in this case)
            var schemaNode = new GroupNode("root", Repetition.Required, (IReadOnlyList<Node>)schemaFields);

            // Create the Parquet file writer
            using (var writer = new ParquetFileWriter(stream: stream, writerProperties: WriterProperties.GetDefaultWriterProperties(), schema: schemaNode))
            {
                using (var rowGroupWriter = writer.AppendRowGroup())
                {
                    // Write data for each property in the class
                    foreach (var property in typeof(T).GetProperties())
                    {
                        // Get a column writer for the property
                        var columnWriter = rowGroupWriter.NextColumn();

                        // Retrieve the values for the current property from the data list
                        var values = data.Select(item => property.GetValue(item)).ToList();

                        //// Apply the appropriate visitor depending on the type of the column
                        //var visitor = new LogicalColumnWriterVisitor<String>(columnWriter, values.Cast<String>());
                        //columnWriter.Apply(visitor);  // Apply visitor for double type

                        // Add more type checks as needed (e.g., for float, decimal, etc.)
                    }
                }
            }
        }

        // Helper method to create the appropriate Column for a property
        private Column CreateColumn(PropertyInfo property)
        {
            // For example, handle different types like string, int, etc.
            if (property.PropertyType == typeof(string))
            {
                return new Column(typeof(string), property.Name, LogicalType.String());
            }
            else if (property.PropertyType == typeof(int))
            {
                return new Column(typeof(int), property.Name, LogicalType.Int(32, true));
            }
            else if (property.PropertyType == typeof(double))
            {
                return new Column(typeof(double), property.Name, LogicalType.Decimal(10, 5));
            }
            else if (property.PropertyType == typeof(float))
            {
                return new Column(typeof(float), property.Name, LogicalType.Decimal(7, 2));
            }
            else if (property.PropertyType == typeof(DateTime))
            {
                return new Column(typeof(DateTime), property.Name, LogicalType.Timestamp(true, TimeUnit.Millis));
            }

            // Add more type checks as needed (e.g., long, bool, etc.)

            throw new NotSupportedException($"Unsupported type: {property.PropertyType}");
        }
    }
}
