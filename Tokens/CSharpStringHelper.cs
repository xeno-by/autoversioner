using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoVersioner.Tokens
{
    // http://blogs.msdn.com/csharpfaq/archive/2004/03/12/88415.aspx
    // \' - single quote, needed for character literals
    // \" - double quote, needed for string literals
    // \\ - backslash
    // \0 - Unicode character 0
    // \a - Alert (character 7)
    // \b - Backspace (character 8)
    // \f - Form feed (character 12)
    // \n - New line (character 10)
    // \r - Carriage return (character 13)
    // \t - Horizontal tab (character 9)
    // \v - Vertical tab (character 11)
    // \uxxxx - Unicode escape sequence for character with hex value xxxx
    // \xn[n][n][n] - Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx)
    // \Uxxxxxxxx - Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates)

    public static class CSharpStringHelper
    {
        private static Dictionary<char, String> _specialChars = 
            new Dictionary<char, String>();

        static CSharpStringHelper()
        {
            _specialChars['\0'] = @"\0";
            _specialChars['\a'] = @"\a";
            _specialChars['\b'] = @"\b";
            _specialChars['\f'] = @"\f";
            _specialChars['\n'] = @"\n";
            _specialChars['\r'] = @"\r";
            _specialChars['\t'] = @"\t";
            _specialChars['\v'] = @"\v";
            _specialChars['\''] = "\\\'";
            _specialChars['\"'] = "\\\"";
            _specialChars['\\'] = @"\\";
        }

        public static String ToCSharpString(this String s)
        {
            var sb = new StringBuilder();
            sb.Append("\"").Append(EscapeCSharpString(s)).Append("\"");
            return sb.ToString();
        }

        private static String EscapeCSharpString(String s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (_specialChars.ContainsKey(c))
                {
                    sb.Append(_specialChars[c]);
                }
                else
                {
                    if (c < 0x0020 || c > 0x007f)
                    {
                        sb.Append(@"\x" + ((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            return sb.ToString();
        }

        public static String FromCSharpString(this String s)
        {
            try
            {
                var matchDoubleQuote = Regex.Match(s, "^\"(?<string>.*)\"$");
                return UnescapeCSharpString(matchDoubleQuote.Result("${string}"));
            }
            catch (Exception e)
            {
                throw new NotSupportedException(String.Format(
                    "Deserialization error: Unexpected string literal format: {0}", s), e);
            }
        }

        private static String UnescapeCSharpString(String s)
        {
            var sb = new StringBuilder();

            var i = 0;
            while(i < s.Length)
            {
                if (s[i] == '\\')
                {
                    var next = s.Length == i+1 ? null : (char?)s[i+1];
                    if (next == '\0' || next == 'a' || next == 'b' || next == 'f' || next == 'n' ||
                        next == 'r' || next == 't' || next == 'v' || next == '\"' || next == '\\')
                    {
                        sb.Append(_specialChars
                          .Where(kvp => kvp.Value == "" + s[i] + s[i+1])
                          .Single());
                        i += 2;
                    }
                    else if (next == 'x' || next == 'u' || next == 'U')
                    {
                        // \uxxxx - Unicode escape sequence for character with hex value xxxx
                        // \xn[n][n][n] - Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx)
                        // \Uxxxxxxxx - Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates)
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotSupportedException(String.Format(
                            "'{0}' is an invalid escape sequence", "" + s[i] + next));
                    }
                }
                else if (s[i] == '\"' || s[i] == '\n' || s[i] == '\r')
                {
                    throw new NotSupportedException(String.Format(
                        "'{0}' cannot be a part of string literal", s[i]));
                }
                else
                {
                    sb.Append(s[i++]);
                }
            }

            return sb.ToString();
        }
    }
}