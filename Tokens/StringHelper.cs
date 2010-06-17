using System;

namespace AutoVersioner.Tokens
{
    internal static class StringHelper
    {
        public static String Quote(this String s)
        {
            return "\"" + s + "\"";
        }

        public static String Unquote(this String s)
        {
            if (s.Length >= 2)
            {
                if (s[0] == '\'' && s[s.Length - 1] == '\'')
                {
                    return s.Substring(1, s.Length - 2);
                }

                if (s[0] == '\"' && s[s.Length - 1] == '\"')
                {
                    return s.Substring(1, s.Length - 2);
                }
            }

            return s;
        }
    }
}