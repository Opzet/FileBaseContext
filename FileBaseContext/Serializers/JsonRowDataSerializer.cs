using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;
using System.Text.Json;

namespace FileBaseContext.Serializers;

public class JsonRowDataSerializer : IRowDataSerializer
{
    private readonly IEntityType _entityType;
    private readonly object _keyValueFactory;
    private readonly int[] _keyColumns;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonRowDataSerializer(IEntityType entityType, object keyValueFactory)
    {
        _entityType = entityType;
        _keyValueFactory = keyValueFactory;
        _keyColumns = CreateKeyColumnsLookup(entityType);
        _jsonOptions = CreateJsonOptions(entityType);
    }

    static int[] CreateKeyColumnsLookup(IEntityType entityType)
    {
        var columnNames = entityType.GetProperties()
            .Select(p => p.GetColumnName())
            .ToArray();

        var primaryKey = entityType.FindPrimaryKey();

        return primaryKey.Properties
            .Select(p => Array.IndexOf(columnNames, p.GetColumnName()))
            .ToArray();
    }

    public string FileExtension => ".json";

    public void Deserialize<TKey> (Stream stream, Dictionary<TKey, object[]> result)
    {
        Debug.Assert (stream.Position==0);
        if (stream.Length==0)
        {
            return;
        }

        var rowsData = JsonSerializer.Deserialize<List<JsonRowData>> (stream, _jsonOptions);
        if (rowsData==null)
        {
            return;
        }

        var keyValueFactory = (IPrincipalKeyValueFactory<TKey>)_keyValueFactory;
        foreach (var rowData in rowsData)
        {
            var keyValues = new object[_keyColumns.Length];
            var columnValues = rowData.ColumnValues;

            for (int i = 0; i<keyValues.Length; i++)
                keyValues[i]=columnValues[_keyColumns[i]];

            TKey key = (TKey)keyValueFactory.CreateFromKeyValues(keyValues);

            if (!result.ContainsKey (key))
            {
                result.Add (key, columnValues);
            }
            else
            {
                var keyValuesString = string.Join (", ", keyValues.Select (k => k?.ToString ()??"null"));
                var columnValuesString = string.Join (", ", columnValues.Select (c => c?.ToString ()??"null"));
                var rowDataString = $"JsonRowData {{ ColumnValues = [{columnValuesString}] }}";

                if (Debugger.IsAttached)
                {
                    Debug.WriteLine ($"<SCHEMA ERROR> Deserialize<TKey>(Stream... Duplicate pKey : Column [{columnValuesString}] Data '{rowDataString}' Duplicate pKey name [{keyValuesString}] ?");
                }
                else
                {
                    throw new InvalidOperationException ($"<SCHEMA ERROR> Deserialize<TKey>(Stream... Duplicate pKey : Column [{columnValuesString}] Data '{rowDataString}' Duplicate pKey name [{keyValuesString}] ?");
                }
            }
        }
    }


    public void Serialize<TKey>(Stream stream, IReadOnlyDictionary<TKey, object[]> source)
    {
        var rowsData = new List<JsonRowData>(source.Count);
        foreach (var columnValues in source.Values)
            rowsData.Add(new(columnValues));

        Debug.Assert(stream.Length == 0);
        JsonSerializer.Serialize(stream, rowsData, _jsonOptions);
    }

    internal static JsonSerializerOptions CreateJsonOptions(IEntityType entityType)
    {
        return CreateJsonOptions(JsonColumnInfo.FromEntityType(entityType));
    }

    internal static JsonSerializerOptions CreateJsonOptions(IEnumerable<JsonColumnInfo> columns)
    {
        return new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            WriteIndented = true,
            Converters =
            {
                new JsonRowDataConverter(columns),
            },
        };
    }
}
