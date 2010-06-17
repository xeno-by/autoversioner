using System;
using System.Security.Cryptography;
using System.Text;

namespace AutoVersioner.Helpers
{
    internal static class Md5
    {
        public static String Md5Hash(this String s)
        {
            var hasher = new MD5CryptoServiceProvider();
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(s));
            return Convert.ToBase64String(hash);
        }
    }
}