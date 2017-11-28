using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;


namespace FileManager
{
    public class FileManager
    {
        private string CurrentPath;
        private FileSystem FileSystem;
        private Dictionary<string, Tuple<string, Delegate>> Commands;

        public FileManager(FileSystem fileSystem)
        {
            this.CurrentPath = "/";
            this.FileSystem = fileSystem;
            this.Commands = new Dictionary<string, Tuple<string, Delegate>>();
            Commands.Add("ls", new Tuple<string, Delegate>("Display the contents of the current directory, or a given path.", new Func<string[], bool>(CommandLS)));
            Commands.Add("exit", new Tuple<string, Delegate>("Exits the application.", new Func<string[], bool>(CommandExit)));
            Commands.Add("cd", new Tuple<string, Delegate>("Changes the directory to the given path.", new Func<string[], bool>(CommandCD)));
            Commands.Add("help", new Tuple<string, Delegate>("Displays this help message.", new Func<string[], bool>(CommandHelp)));
        }

        public void startConsole()
        {
            //Simpler than it looks. Forever display the current path and expect a command.
            //A command name is mapped to the dictionary above. If it matches, run it. If it returns false, break the loop and exit.
            //It uses some magic to store pointers to functions in the dictionary to avoid those nasty switch statements.
            //Add new commands by adding any function to the commands dictionary above.
            //The function must be public, return a bool, and take in a string[] with the arguments.
            Console.WriteLine("FileManager Console Started With FileSystem.");
            while (true)
            {
                Console.Write($"{CurrentPath}> ");
                var line = Console.ReadLine();
                String[] args = CommandLineToArgs(line);
                Object[] invoke = new object[]{args};
                if (Commands.ContainsKey(args[0].ToLower()))
                {
                    if (!(bool) Commands[args[0]].Item2.DynamicInvoke(invoke))
                {
                        break;
                    }
                }
                else
                {
                    Console.WriteLine("Command not found. Type \"help\" to see a list of commands.");
                }
            }
        }

        public string convertToAbsolutePath(string path)
        {
            string trimmed = path.TrimEnd('/');
            string preppedPath = "";
            List<string> fullPath = new List<string>();
            if (!trimmed.StartsWith("/"))
            {
                //This is not an absolute path
                //Lets prepend the current path
                fullPath.AddRange(CurrentPath.Split().Skip(1));
            }
            string[] splitPath = trimmed.Split('/');
            foreach (string item in splitPath)
            {
                if (item == String.Empty)
                {
                    continue;
                }
                if (item == "..")
                {
                    fullPath.RemoveAt(fullPath.Count - 1);
                    continue;
                }
                if (item == ".")
                {
                    continue;
                }
                fullPath.Add(item);
            }
            return "/" + string.Join("/", fullPath.ToArray());
        }

        public bool CommandHelp(string[] args)
        {
            foreach (var command in Commands)
            {
                Console.WriteLine($"{command.Key}\t{command.Value.Item1}");
            }
            return true;
        }
        
        public bool CommandCD(string[] args)
        {
            if (args.Length == 1)
            {
                return true;
            }
            else
            {
                string path = convertToAbsolutePath(args[1]);
                FileSystem.getDirectoryContents(path);
                CurrentPath = convertToAbsolutePath(path);
            }
            return true;
        }

        public bool CommandLS(string[] args)
        {
            if (args.Length == 1)
            {
                //There were no arguments provided.
                Console.WriteLine(parseDirectory(FileSystem.getDirectoryContents(CurrentPath)));
                return true;
            }
            else
            {
                Console.WriteLine(parseDirectory(FileSystem.getDirectoryContents(convertToAbsolutePath(args[1]))));
            }
            return true;
        }

        public bool CommandExit(string[] args)
        {
            Console.WriteLine("Goodbye.");
            return false;
        }

        public string parseDirectory(DirectoryTable table)
        {
            string output = "";
            foreach (var row in table.Rows)
            {
                output += $"{row.getString()}\n";
            }
            return output;
        }

        //I didn't feel like parsing out arguments to commands myself on the command line. So I used some black magic.
        //This uses the Windows API that parses commands in Windows to create the arguments for a program. Guaranteed to work beautifully.
        //Unfortunately does not work on Linux since it relies on the Windows shell being present.
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        public static string[] CommandLineToArgs(string commandLine)
        {
            int argc;
            var argv = CommandLineToArgvW(commandLine, out argc);
            if (argv == IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
                    args[i] = Marshal.PtrToStringUni(p);
                }

                return args;
            }
            finally
            {
                Marshal.FreeHGlobal(argv);
            }
        }
    }
}