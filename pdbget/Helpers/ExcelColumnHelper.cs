namespace pdbget.Helpers
{
    public static class ExcelColumnHelper
    {
        /// <summary>
        /// 1 => A, ... 26 => Z, 27 => AA, ...
        /// </summary>
        /// <param name="columnNum">1 based column number</param>
        /// <returns>A series of upper cased alphabetics</returns>
        public static string ToColumnRef(this uint columnNum)
        {
            string s = string.Empty;
            while (columnNum-- > 0)
            {
                s = (char)('A' + columnNum % 26) + s;
                columnNum /= 26;
            }
            return s;
        }

        /// <summary>
        /// 1 => A, ... 26 => Z, 27 => AA, ...
        /// </summary>
        /// <param name="columnNum">1 based column number</param>
        /// <returns>A series of upper cased alphabetics</returns>
        public static string ToColumnRef(this int columnNum)
        {
            string s = string.Empty;
            while (columnNum-- > 0)
            {
                s = (char)('A' + columnNum % 26) + s;
                columnNum /= 26;
            }
            return s;
        }

        /// <summary>
        /// 1 => A, ... 26 => Z, 27 => AA, ...
        /// </summary>
        /// <param name="columnNum">1 based column number</param>
        /// <param name="rowNum">1 based row number</param>
        /// <returns>A series of upper cased alphabetics</returns>
        public static string ToCellRef(this uint columnNum, uint rowNum)
        {
            return columnNum.ToColumnRef() + rowNum;
        }

        /// <summary>
        /// 1 => A, ... 26 => Z, 27 => AA, ...
        /// </summary>
        /// <param name="columnNum">1 based column number</param>
        /// <param name="rowNum">1 based row number</param>
        /// <returns>A series of upper cased alphabetics</returns>
        public static string ToCellRef(this int columnNum, int rowNum)
        {
            return columnNum.ToColumnRef() + rowNum;
        }

        /// <summary>
        /// Extract column number from a cell reference
        /// </summary>
        /// <param name="cellRef">A cell reference in the form of A1, B2, AA80</param>
        /// <returns>1 based column number</returns>
        public static uint ToColumnNumber(this string cellRef)
        {
            uint c = 0;
            for (int i = 0; i < cellRef.Length && char.IsUpper(cellRef[i]); i++)
                c = c * 26 + (uint)(cellRef[i] - 'A' + 1);
            return c;
        }

        /// <summary>
        /// Extract column and row number from a cell reference
        /// </summary>
        /// <param name="cellRef">A cell reference in the form of A1, B2, AA80</param>
        /// <returns>1 based column number</returns>
        public static (uint Col, uint Row) ToColumnRow(this string cellRef)
        {
            uint i = 0, c = 0, r = 0;
            for (; i < cellRef.Length && char.IsUpper(cellRef[(int)i]); i++)
                c = c * 26 + (uint)(cellRef[(int)i] - 'A' + 1);
            for (; i < cellRef.Length; i++)
                r = r * 10 + (uint)(cellRef[(int)i] - '0');
            return (c, r);
        }
    }
}
