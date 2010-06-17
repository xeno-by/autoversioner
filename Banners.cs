using System;

namespace AutoVersioner
{
    internal class Banners
    {
        public static void About()
        {
            Console.Write(String.Format(
                "[AutoVersioner {0}]: a free tool capable of auto-replacing certain   " +
                "tokens in source files with values based on: codebase characteristics, current  "+
                "date/time, logged on user and current machine.", 
                typeof(Program).Assembly.GetName().Version));
        }

        public static void Help()
        {
            Console.WriteLine("Syntax: AutoVersioner %ProjectFile %TemplateFile %DestinationFile [/verbose]");
            Console.WriteLine("Parameters:");
            Console.WriteLine("    %ProjectFile       Csproj that describes the project");
            Console.WriteLine("    %TemplateFile      Path to the template file (e.g. AssemblyInfo.template)");
            Console.WriteLine("    %DestinationFile   Path to the resulting file (e.g. AssemblyInfo.cs)");
            Console.WriteLine("    /verbose           Optional parameter useful for debugging purposes");
            Console.WriteLine();
            Console.WriteLine("Supported tokens:");
            Console.WriteLine("    $CODEBASE$         Path to the repository that contains %ProjectDir");
            Console.WriteLine("    $REVISION$         Current revision of the working copy");
            Console.WriteLine("    $CODEHASH$         Hash of codebase (might be used to distinguish ");
            Console.WriteLine("                       logically different versions of the project built");
            Console.WriteLine("                       in-between commits during regular development cycle)");
            Console.WriteLine("    $TIMESTAMP$        Current date/time formatted as dd-mm-yyyy hh:MM:ss");
            Console.WriteLine("    $USERHASH$         Hash of currently logged in user");
            Console.WriteLine("    $MACHINEHASH$      Hash of current machine characteristics");
        }
    }
}
