using pdbget.Helpers;

namespace pdbget;

public static class AminoAcidExtensions
{
    public static AminoAcid ParseAminoAcid(this string s)
    {
        return EnumByMatchParser<AminoAcid>.Parse(s);
    }

    public static bool TryParseAminoAcid(this string s, out AminoAcid value)
    {
        return EnumByMatchParser<AminoAcid>.TryParse(s, out value);
    }

    public static string GetName(this AminoAcid value)
    {
        return EnumAnnotationHelper<AminoAcid>.GetAttribute<NameAttribute>(value).Name;
    }

    public static char GetCode(this AminoAcid value)
    {
        return EnumAnnotationHelper<AminoAcid>.GetAttribute<NameAttribute>(value).InternalName![0];
    }
}
