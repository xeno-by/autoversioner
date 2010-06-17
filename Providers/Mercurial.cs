using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AutoVersioner.Helpers;
using AutoVersioner.Tokens;

namespace AutoVersioner.Providers
{
    internal static class Mercurial
    {
        public static bool Autodetect(Config cfg)
        {
            var tree = cfg.ProjectDir.DirTree();
            return tree.Any(dir => dir.GetDirectories(".hg").Count() > 0);
        }

        private static IEnumerable<DirectoryInfo> DirTree(this String dirName)
        {
            for (var dir = new DirectoryInfo(dirName); dir != null; dir = dir.Parent)
                yield return dir;
        }

        public static ResolvedTokens ResolveTokens(Config cfg)
        {
            var hgexe = AutodetectPathToHgExe(cfg);
            if (hgexe == null) return null;

            var psi = new ProcessStartInfo(hgexe);
            psi.Arguments = String.Format("log");
            psi.WorkingDirectory = cfg.ProjectDir;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            if (cfg.IsVerbose) Console.WriteLine("Launching \"hg.exe log\"...");
            var p = Process.Start(psi);
            var stdout = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            if (cfg.IsVerbose) Console.WriteLine("\"hg.exe log\" has completed and produced the following output:");
            if (cfg.IsVerbose) Console.WriteLine(stdout.ToTrace());

            if (cfg.IsVerbose) Console.WriteLine("Detecting the changeset in the output of hg.exe...");
            var lines = stdout.Split('\n').Select(ln => ln.Trim()).ToArray();
            var first = lines.FirstOrDefault();
            if (first == null)
            {
                if (cfg.IsVerbose) Console.WriteLine("Detection failed: output is empty.");
                return null;
            }
            var match = Regex.Match(first, @"^changeset:.*:(?<changeset>.*)$");
            if (!match.Success)
            {
                if (cfg.IsVerbose) Console.WriteLine("Detection failed: first line of the output didn't match the \"^changeset:.*:(?<changeset>.*)$\" regex.");
                return null;
            }

            var changeset = match.Result("${changeset}");
            if (cfg.IsVerbose) Console.WriteLine("Changeset is successfully detected as: {0}", changeset.ToTrace());

            var tokens = new ResolvedTokens(cfg);
            tokens.Codebase = cfg.ProjectDir;
            tokens.Revision = changeset;
            return tokens;
        }

        private static String AutodetectPathToHgExe(Config cfg)
        {
            var var_path = Environment.GetEnvironmentVariable("PATH");
            if (cfg.IsVerbose) Console.WriteLine("Detecting path to hg.exe...");
            if (cfg.IsVerbose) Console.WriteLine("PATH is {0}...", var_path.ToTrace());

            var paths = var_path.Split(Path.PathSeparator).Select(s => s.Trim()).ToArray();
            foreach (var path in paths)
            {
                var dir = path;
                if (!dir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    dir += Path.DirectorySeparatorChar.ToString();
                var hgexe = dir + "hg.exe";

                if (cfg.IsVerbose) Console.WriteLine("Checking at {0}...", hgexe.ToTrace());
                if (File.Exists(hgexe))
                {
                    if (cfg.IsVerbose) Console.WriteLine("Successfully found hg.exe at {0}...", hgexe.ToTrace());
                    return hgexe;
                }
            }

            Console.WriteLine("Cannot find hg.exe in PATH, which is:" + (paths.Count() > 0 ? "" : "<empty>"));
            foreach (var path in paths) Console.WriteLine(path.ToTrace());
            Console.WriteLine();
            Console.WriteLine("Please, install TortoiseHg (btw it's free) ");
            Console.WriteLine("and then register path to hg.exe in the PATH environment variable");
            Console.WriteLine("(hg.exe usually resides under \"%INSTALL_DIR%\\hg.exe\")");
            return null;
        }
    }
}