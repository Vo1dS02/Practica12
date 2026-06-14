namespace Arch.ConsoleApp;

public static class CsvParser
{
    public static bool IsValidLine(string line)
    {
        if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        return line.Where(x => x is ';').Take(2).Count() >= 2;
    }
    
    public static PropertyData ParseProperty(string line)
    {
        var parts = line.Split(';');
        return new PropertyData(
            parts[0],
            Enum.Parse<PropertyType>(parts[1]),
            parts[2] switch
            {
                "RO" => PropertyAccess.ReadOnly,
                "RW" => PropertyAccess.ReadWrite,
                _ => throw new ArgumentException($"Unknown property type: {parts[1]}")
            });
    }
}