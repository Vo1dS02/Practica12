namespace Arch.ConsoleApp;

public readonly record struct PropertyData(
    string Name,
    PropertyType Type,
    PropertyAccess Access);

public enum PropertyType
{
    Int,
    Real,
    Bool,
    String
}

public enum PropertyAccess
{
    ReadOnly,
    ReadWrite
}