using System.Text.Json;

namespace EasyCaster.Alarm;

public class IOHelpers
{
    public static void EnsureDirectoryExists(string path)
    {
        if (!System.IO.Directory.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
    }

    public static T ReadJson<T>(string path)
    {
        var stringContent =System.IO.File.ReadAllText(path);
        return JsonSerializer.Deserialize<T>(stringContent);
    }

    public static void WriteJson(string path, object value)
    {
        var stringContent = JsonSerializer.Serialize(value);
        System.IO.File.WriteAllText(path, stringContent);
    }
}
