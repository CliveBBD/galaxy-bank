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
        private List<(string ColumnName, Func<T, string> ValueGetter)> _flattenedProperties;

        public TableBuilder(IEnumerable<T> tableData)
        {
            _data = tableData;
            Table = new Table();
            _flattenedProperties = new List<(string, Func<T, string>)>();

            FlattenProperties(typeof(T), "", (obj) => obj!, _flattenedProperties);

            foreach (var (columnName, _) in _flattenedProperties)
            {
                Table.AddColumn(columnName);
            }

            foreach (var item in _data)
            {
                var row = _flattenedProperties.Select(p => p.ValueGetter(item)).ToArray();
                Table.AddRow(row);
            }
        }

        private void FlattenProperties(Type type, string prefix, Func<object?, object?> parentObjectGetter, List<(string, Func<T, string>)> list)
        {
            foreach (var property in type.GetProperties())
            {
                var propertyName = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                Func<T, string> getter = (childObject) =>
                {
                    try
                    {
                        var parentValue = parentObjectGetter(childObject);
                        var value = property.GetValue(parentValue);
                        return value?.ToString() ?? string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                };

                if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    FlattenProperties(
                        property.PropertyType,
                        propertyName,
                        (childObject) => property.GetValue(parentObjectGetter(childObject)),
                        list
                    );
                }
                else
                {
                    list.Add((propertyName, getter));
                }
            }
        }


    }
}