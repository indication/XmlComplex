using System;

namespace XmlComplex
{
    public static class NewLineResolver
    {
        public static string Resolve(string value)
        {
            NewLineKind kind;
            if (!Enum.TryParse(value, true, out kind))
                return null;

            switch (kind)
            {
                case NewLineKind.CR:
                    return "\r";
                case NewLineKind.LF:
                    return "\n";
                case NewLineKind.CRLF:
                    return "\r\n";
                default:
                    return null;
            }
        }
    }
}
