using System;
using System.Text;

namespace XmlComplex
{
    public static class EncodingResolver
    {
        public static Encoding Resolve(string value)
        {
            var lower = value.ToLower();
            var hasBe = lower.Contains("be");
            var hasBom = lower.Contains("bom") && !lower.Contains("nobom");

            if (value.StartsWith("utf-8", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("utf8", StringComparison.CurrentCultureIgnoreCase))
                return new UTF8Encoding(hasBom);

            if (value.StartsWith("utf-32", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("utf32", StringComparison.CurrentCultureIgnoreCase))
                return new UTF32Encoding(hasBe, hasBom);

            if (value.StartsWith("utf-16", StringComparison.CurrentCultureIgnoreCase) || value.StartsWith("utf16", StringComparison.CurrentCultureIgnoreCase))
                return new UnicodeEncoding(hasBe, hasBom);

            return Encoding.GetEncoding(value);
        }
    }
}
