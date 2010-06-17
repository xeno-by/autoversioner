using System;
using System.Collections.Generic;
using System.Linq;
using AutoVersioner.Helpers;

namespace AutoVersioner.Tokens
{
    internal class ResolvedTokens : BaseDictionary<String, String>
    {
        public String Codebase { get; set; }
        public String Revision { get; set; }
        public String Codehash { get; private set; }
        public String Timestamp { get; private set; }
        public String Userhash { get; private set; }
        public String Machinehash { get; private set; }

        public ResolvedTokens(Config cfg)
        {
            // todo. also compute hash of the entire codebase
            Codehash = "$CODEHASH$";
            Timestamp = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
            Userhash = (Environment.UserDomainName + "\\\\" + Environment.UserName).Md5Hash();
            Machinehash = Environment.MachineName.Md5Hash();
        }

        private Dictionary<String, String> TokenMap()
        {
            var map = new Dictionary<String, String>();
            foreach (var pi in GetType().GetProperties())
            {
                if (pi.PropertyType != typeof(String)) continue;
                if (pi.GetIndexParameters().Count() > 0) continue;
                var value = (String)pi.GetValue(this, null);

                var regular = String.Format("${0}$", pi.Name.ToUpper());
                var escaped = String.Format("$\"{0}\"$", pi.Name.ToUpper());
                map.Add(regular, value);
                map.Add(escaped, value.ToCSharpString().Unquote());
            }

            return map;
        }

        #region Dictionary<String, String> boilerplate

        public override int Count
        {
            get { return TokenMap().Count; }
        }

        public override IEnumerator<KeyValuePair<String, String>> GetEnumerator()
        {
            return TokenMap().GetEnumerator();
        }

        public override bool ContainsKey(String key)
        {
            return TokenMap().ContainsKey(key);
        }

        public override bool TryGetValue(String key, out String value)
        {
            return TokenMap().TryGetValue(key, out value);
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override void Add(String key, String value)
        {
            throw new NotSupportedException();
        }

        public override bool Remove(String key)
        {
            throw new NotSupportedException();
        }

        public override void Clear()
        {
            throw new NotSupportedException();
        }

        protected override void SetValue(String key, String value)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
