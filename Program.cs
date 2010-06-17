using System;
using System.Configuration;
using System.IO;
using AutoVersioner.Helpers;
using AutoVersioner.Providers;
using AutoVersioner.Tokens;

namespace AutoVersioner
{
    internal class Program
    {
        public static void Main(String[] args)
        {
            try
            {
                var config = new Config(args);

                Func<ResolvedTokens> resolveTokens = null;
                if (config.IsVerbose) Console.WriteLine("Detecting CVS repository at {0}...", config.ProjectDir.ToTrace());
                if (config.IsVerbose) Console.WriteLine("Detecting Subversion repository...");
                if (Subversion.Autodetect(config))
                {
                    if (config.IsVerbose) Console.WriteLine("Subversion repository detected.");
                    resolveTokens = () => Subversion.ResolveTokens(config);
                }
                else
                {
                    if (config.IsVerbose) Console.WriteLine("Failed to detect Subversion repository.");
                    if (config.IsVerbose) Console.WriteLine("Detecting Mercurial repository ...");
                    if (Mercurial.Autodetect(config))
                    {
                        if (config.IsVerbose) Console.WriteLine("Mercurial repository detected.");
                        resolveTokens = () => Mercurial.ResolveTokens(config);
                    }
                    else
                    {
                        if (config.IsVerbose) Console.WriteLine("No supported CVSes are detected.");
                        resolveTokens = () => Filesystem.ResolveTokens(config);
                    }
                }

                var resolvedTokens = resolveTokens();
                if (resolvedTokens != null)
                {
                    var template = File.ReadAllText(config.TemplateFile);
                    foreach (var token in resolvedTokens.Keys)
                    {
                        if (template.Contains(token))
                        {
                            template = template.Replace(token, resolvedTokens[token]);
                        }
                    }

                    File.WriteAllText(config.DestinationFile, template);
                }
            }
            catch (ConfigurationErrorsException cex)
            {
                if (cex.Message != null) Console.WriteLine(cex.Message);
                Console.WriteLine();
                Banners.Help();
            }
        }
    }
}
