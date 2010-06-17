using System;

namespace AutoVersioner.Helpers
{
    internal static class Print
    {
        public static String ToTrace(this String s)
        {
            if (s == null) return "<null>";
            if (s == String.Empty) return "<empty>";
            return s;
        }
    }
}