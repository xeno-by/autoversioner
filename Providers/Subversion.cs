using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AutoVersioner.Tokens;
using AutoVersioner.Helpers;

namespace AutoVersioner.Providers
{
    internal static class Subversion
    {
        public static bool Autodetect(Config cfg)
        {
            var tree = cfg.ProjectDir.DirTree();
            return tree.Any(dir => dir.GetDirectories(".svn").Count() > 0);
        }

        private static IEnumerable<DirectoryInfo> DirTree(this String dirName)
        {
            for (var dir = new DirectoryInfo(dirName); dir != null; dir = dir.Parent)
                yield return dir;
        }

        public static ResolvedTokens ResolveTokens(Config cfg)
        {
            var subwcrev = AutodetectPathToSubWCRev(cfg);
            if (subwcrev == null) return null;

            var subwcrevTokens = new Dictionary<String, String>();
            var probeIn = @".\probe.in";
            var probeOut = @".\probe.out";
            try
            {
                var tokenNames = new List<String>();
                tokenNames.Add("$WCREV$");
                tokenNames.Add("$WCURL$");
                File.WriteAllText(probeIn, String.Join(Environment.NewLine, tokenNames.ToArray()));

                if (cfg.IsVerbose) Console.WriteLine("Launching SubWCRev.exe for a synthetic file that contains the following text:");
                if (cfg.IsVerbose) Console.WriteLine(File.ReadAllText(probeIn.ToTrace()));
                var psi = new ProcessStartInfo(subwcrev);
                psi.Arguments = String.Format("\"{0}\" \"{1}\" \"{2}\"",
                    cfg.ProjectDir, probeIn, probeOut);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                Process.Start(psi).WaitForExit();
                if (!File.Exists(probeOut))
                {
                    Console.WriteLine("SubWCRev has failed due to an unknown reason.");
                    return null;
                }
                else
                {
                    if (cfg.IsVerbose) Console.WriteLine("SubWCRev.exe has completed and produced the following output:");
                    if (cfg.IsVerbose) Console.WriteLine(File.ReadAllText(probeOut.ToTrace()));

                    var tokenValues = File.ReadAllLines(probeOut);
                    for (var i = 0; i < tokenNames.Count(); ++i)
                    {
                        subwcrevTokens.Add(tokenNames[i], tokenValues[i]);
                    }
                }
            }
            finally
            {
                File.Delete(probeIn);
                File.Delete(probeOut);
            }

            var codebase = subwcrevTokens["$WCURL$"].Replace(@"\", "/");
            var revision = subwcrevTokens["$WCREV$"];
            if (cfg.IsVerbose) Console.WriteLine("Codebase is successfully detected as: {0}", codebase.ToTrace());
            if (cfg.IsVerbose) Console.WriteLine("Revision is successfully detected as: {0}", revision.ToTrace());

            var tokens = new ResolvedTokens(cfg);
            tokens.Codebase = codebase;
            tokens.Revision = revision;
            return tokens;
        }

        private static String AutodetectPathToSubWCRev(Config cfg)
        {
            if (cfg.IsVerbose) Console.WriteLine("Detecting path to SubWCRev.exe...");
            if (cfg.IsVerbose) Console.WriteLine("PATH is {0}...", Environment.GetEnvironmentVariable("PATH"));

            var paths = Environment.GetEnvironmentVariable("PATH").Split(Path.PathSeparator).Select(s => s.Trim()).ToArray();
            foreach (var path in paths)
            {
                var dir = path;
                if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    dir += Path.DirectorySeparatorChar.ToString();
                var subwcrev = dir + "SubWCRev.exe";

                if (cfg.IsVerbose) Console.WriteLine("Checking at {0}...", subwcrev.ToTrace());
                if (File.Exists(subwcrev))
                {
                    if (cfg.IsVerbose) Console.WriteLine("Successfully found SubWCRev.exe at {0}...", subwcrev.ToTrace());
                    return subwcrev;
                }
            }

            Console.WriteLine("Cannot find hg.exe in PATH, which is:" + (paths.Count() > 0 ? "" : "<empty>"));
            foreach (var path in paths) Console.WriteLine(path.ToTrace());
            Console.WriteLine();
            Console.WriteLine("Please, install TortoiseSVN (btw it's free) ");
            Console.WriteLine("and then register path to SubWCRev.exe in the PATH environment variable");
            Console.WriteLine("(SubWCRev.exe usually resides under \"%INSTALL_DIR%\\bin\\SubWCRev.exe\")");
            return null;
        }
    }
}
