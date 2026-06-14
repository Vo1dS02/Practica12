using System.Diagnostics;

namespace Arch.ConsoleApp;

public static class ClassGenerator
{
    public static string ToCamelCase(string s) =>
        char.ToLower(s[0]) + s[1..];

    public static string GenerateProperty(PropertyData prop)
    {
        var accessor = prop.Access switch
        {
            PropertyAccess.ReadOnly => "{ get; }",
            PropertyAccess.ReadWrite => "{ get; set; }",
            _ => throw new UnreachableException()
        };
        var type = prop.Type switch
        {
            PropertyType.Real => "double",
            PropertyType.Bool => "bool",
            PropertyType.Int => "int",
            PropertyType.String => "string",
            _ => throw new UnreachableException()
        };
        return $"\tpublic {type} {prop.Name} {accessor}";
    }
    
    public static string GenerateClass(string className, IEnumerable<PropertyData> props) =>
        $$"""
        public class {{className}}
        {
        {{string.Join('\n', props.Select(GenerateProperty))}}
        }
        """;
}