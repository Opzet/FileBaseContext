using FileBaseContext.Serializers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Text;

namespace FileBaseContext.Storage;

public class FileBaseContextFileManager : IFileBaseContextFileManager
{
    private readonly IFileSystem _fileSystem;
    private string _databasename = "";
    private string _location;

    public FileBaseContextFileManager(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public string GetFileName(IEntityType _entityType, IRowDataSerializer serializer)
    {
        string name = _entityType.GetTableName().GetValidFileName();

        string path = string.IsNullOrEmpty (_location)
            ? _fileSystem.Path.Combine (AppDomain.CurrentDomain.BaseDirectory, _databasename)
            : Path.Combine (_location, _databasename);

        // Ensure the directory exists
        if (!_fileSystem.Directory.Exists(path.ToLower()))
        {
            Debug.WriteLine ($"<NOT EXISTS> No Database Path = '{path}'");
            _fileSystem.Directory.CreateDirectory(path);
            Debug.WriteLine ($"Created ok..");

        }

        return _fileSystem.Path.Combine(path, name + serializer.FileExtension);
    }


    public void Init(IFileBaseContextScopedOptions options)
    {
        _databasename = options.DatabaseName;
        _location = options.Location;
    }
    public Dictionary<TKey, object[]> Load<TKey> (IEntityType _entityType, IRowDataSerializer serializer)
    {
        var rows = new Dictionary<TKey, object[]> ();
        string path = "";
        try
        {
            path=GetFileName (_entityType, serializer);
            using var stream = _fileSystem.File.OpenRead (path);
            serializer.Deserialize (stream, rows);
        }
        catch (FileNotFoundException ex)
        {
            Debug.WriteLine ($"Load > File not found: {path}. FileNotFoundException: {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Debug.WriteLine ($"Load > Directory not found: {path}. DirectoryNotFoundException: {ex.Message}");
        }
        catch (IOException ex)
        {
            Debug.WriteLine ($"Load > IO error while accessing {path}. IOException: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine ($"Load > Deserialization InvalidOperationException error in {path}. Exception: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine ($"Load > Deserialize > An error occurred while loading data from {path}. Exception: {ex.Message}");
        }

        return rows;
    }

    public void Save<TKey>(IEntityType _entityType, Dictionary<TKey, object[]> objectsMap, IRowDataSerializer serializer)
    {
        string path = GetFileName(_entityType, serializer);
        using var stream = _fileSystem.File.Create(path);
        serializer.Serialize(stream, objectsMap);
    }
}