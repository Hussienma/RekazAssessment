using SimpleDrive.Interfaces;

namespace SimpleDrive.Adapters;

public class PhysicalFileSystem : IFileSystem
{
    public bool FileExists(string path) => File.Exists(path);
    
    public bool DirectoryExists(string path) => Directory.Exists(path);
    
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);
    
    public string ReadAllText(string path) => File.ReadAllText(path);
    
    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);
}