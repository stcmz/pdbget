using pdbget.Helpers;

namespace pdbget.PdbFormat;

public static class RecordTypeExtensions
{
    public static RecordType ParseRecordType(this string s)
    {
        return EnumByMatchParser<RecordType>.Parse(s);
    }

    public static bool TryParseRecordType(this string s, out RecordType value)
    {
        return EnumByMatchParser<RecordType>.TryParse(s, out value);
    }
}
