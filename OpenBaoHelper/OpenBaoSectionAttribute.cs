namespace OpenBaoHelper;

[AttributeUsage(AttributeTargets.Class)]
public sealed class OpenBaoSectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
