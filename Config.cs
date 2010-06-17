using System;
using System.Configuration;
using System.IO;
using System.Linq;
using AutoVersioner.Helpers;

namespace AutoVersioner
{
    // todo #1. also parse csproj files and validate those using appropriate schemas
    // todo #2. also verify that all files mentioned in csproj lie under auto-detected dir

    internal class Config
    {
        public String ProjectFile { get; private set; }
        public String ProjectDir { get; private set; }
        public String TemplateFile { get; private set; }
        public String DestinationFile { get; private set; }
        public bool IsVerbose { get; private set; }

        public Config(String[] args)
        {
            if (args.Count() == 1 && args[0] == "/?")
            {
                Banners.About();
                throw new ConfigurationErrorsException(String.Empty);
            }
            else
            {
                if (args.LastOrDefault() == "/verbose")
                {
                    IsVerbose = true;
                    args = args.Take(args.Count() - 1).ToArray();
                }

                if (args.Count() != 3)
                {
                    throw new ConfigurationErrorsException("Expected three arguments as explained below.");
                }
                else
                {
                    ProjectFile = Path.GetFullPath(args[0]);
                    if (IsVerbose) Console.WriteLine("Resolved %ProjectFile as {0}.", ProjectFile.ToTrace());
                    if (!File.Exists(ProjectFile))
                    {
                        throw new ConfigurationErrorsException(String.Format(
                            "%ProjectFile (specified as \"{0}\") does not exist.", ProjectFile.ToTrace()));
                    }

                    ProjectDir = Path.GetDirectoryName(ProjectFile);
                    if (IsVerbose) Console.WriteLine("Resolved %ProjectDir as {0}.", ProjectDir.ToTrace());

                    TemplateFile = Path.GetFullPath(args[1]);
                    if (IsVerbose) Console.WriteLine("Resolved %TemplateFile as {0}.", TemplateFile.ToTrace());
                    if (!File.Exists(TemplateFile))
                    {
                        throw new ConfigurationErrorsException(String.Format(
                            "%TemplateFile (specified as \"{0}\") does not exist.", TemplateFile.ToTrace()));
                    }

                    DestinationFile = Path.GetFullPath(args[2]);
                    if (IsVerbose) Console.WriteLine("Resolved %DestinationFile as {0}.", DestinationFile.ToTrace());
                    if (!File.Exists(DestinationFile))
                    {
                        if (IsVerbose) Console.WriteLine("Destination file doesn't exist at {0}. Creating it...", DestinationFile.ToTrace());

                        var destinationDir = Path.GetDirectoryName(DestinationFile);
                        if (!Directory.Exists(destinationDir))
                        {
                            if (IsVerbose) Console.WriteLine("Destination file directory doesn't exist at {0}. Creating it...", destinationDir.ToTrace());
                            Directory.CreateDirectory(destinationDir);
                            if (IsVerbose) Console.WriteLine("Destination file directory successfully created at {0}.", destinationDir.ToTrace());
                        }

                        File.WriteAllText(DestinationFile, String.Empty);
                        if (IsVerbose) Console.WriteLine("Destination file successfully created at {0}.", DestinationFile.ToTrace());
                    }
                }
            }
        }
    }
}
