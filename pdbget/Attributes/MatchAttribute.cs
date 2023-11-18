using System;

namespace pdbget;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
public sealed class MatchAttribute : Attribute
{
    public string[] Patterns { get; }

    public MatchAttribute(params string[] patterns)
    {
        Patterns = patterns;
    }

    public bool CaseSensitive { get; set; } = true;
    public bool UseRegex { get; set; } = false;
}
