namespace HighchartsCs
{
    public static class HighchartsHelpers
    {
        public static string StrEscape(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "''";
            }
            return "'" + value.Replace("'", "\'") + "'";
        }
    }
}