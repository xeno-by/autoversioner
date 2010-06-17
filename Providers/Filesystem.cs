using AutoVersioner.Tokens;

namespace AutoVersioner.Providers
{
    internal class Filesystem
    {
        public static bool Autodetect(Config cfg)
        {
            return true;
        }

        public static ResolvedTokens ResolveTokens(Config cfg)
        {
            var tokens = new ResolvedTokens(cfg);
            tokens.Codebase = cfg.ProjectDir;
            tokens.Revision = "N/A";
            return tokens;
        }
    }
}