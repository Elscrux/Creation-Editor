using System.IO.Abstractions;
using Noggog;
namespace CreationEditor.Settings;

public interface ISettingPathProvider {
    public FilePath Path { get; }

    public FilePath GetFullPath(ISetting setting);
}

public sealed class SettingPathProvider : ISettingPathProvider {
    private readonly IFileSystem _fileSystem;
    
    public SettingPathProvider(
        IFileSystem fileSystem) {
        _fileSystem = fileSystem;
    }
    
    public FilePath Path => "Settings";
    
    public FilePath GetFullPath(ISetting setting) {
        return new FilePath(_fileSystem.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path,  $"{setting.Name}.json"));
    }
}