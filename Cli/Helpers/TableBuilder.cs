using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Spectre.Console;

namespace Cli.Helpers
{
    public class TableBuilder<T> where T: notnull
    {
        public Table Table { get; }
        private IEnumerable<T> _data { get; set; }

        public TableBuilder(IEnumerable<T> tableData)
        {
            _data = tableData;
            Table = new Table();

            var dataType = typeof(T);
            PropertyInfo[] properties = dataType.GetProperties();

            foreach (var propertyName in properties.Select(property => property.Name))
            {
                Table.AddColumn(propertyName);
            }

            foreach (var rowData in _data)
            {
                var rowValues = properties
                    .Select(property => property.GetValue(rowData)?.ToString() ?? string.Empty)
                    .ToArray();
                Table.AddRow(rowValues);
            }

        }

    }
}