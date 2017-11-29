using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;


namespace FileManager
{
    public class FileManager
    {
        private string _currentPath;
        private readonly FileSystem _fileSystem;
        private readonly Dictionary<string, Tuple<string, Delegate>> _commands;

        public FileManager(FileSystem fileSystem)
        {
            this._currentPath = "/";
            this._fileSystem = fileSystem;
            this._commands = new Dictionary<string, Tuple<string, Delegate>>();
            _commands.Add("ls", new Tuple<string, Delegate>("Display the contents of the current directory, or a given path.", new Func<string[], bool>(CommandLs)));
            _commands.Add("exit", new Tuple<string, Delegate>("Exits the application.", new Func<string[], bool>(CommandExit)));
            _commands.Add("cd", new Tuple<string, Delegate>("Changes the directory to the given path.", new Func<string[], bool>(CommandCd)));
            _commands.Add("help", new Tuple<string, Delegate>("Displays this help message.", new Func<string[], bool>(CommandHelp)));
            _commands.Add("del", new Tuple<string, Delegate>("Deletes the file or directory given in the path.", new Func<string[], bool>(CommandDel)));
            _commands.Add("ren", new Tuple<string, Delegate>("Renames the file or directory given in the path with the new name.", new Func<string[], bool>(CommandRen)));
            _commands.Add("cat", new Tuple<string, Delegate>("Display the contents of a file", new Func<string[], bool>(CommandCat)));
        }

        public void StartConsole()
        {
            //Simpler than it looks. Forever display the current path and expect a command.
            //A command name is mapped to the dictionary above. If it matches, run it. If it returns false, break the loop and exit.
            //It uses some magic to store pointers to functions in the dictionary to avoid those nasty switch statements.
            //Add new commands by adding any function to the commands dictionary above.
            //The function must be public, return a bool, and take in a string[] with the arguments.
            Console.WriteLine("FileManager Console Started With FileSystem.");
            while (true)
            {
                Console.Write($"{_currentPath}> ");
                var line = Console.ReadLine();
                var args = CommandLineToArgs(line);
                var invoke = new object[]{args};
                if (_commands.ContainsKey(args[0].ToLower()))
                {
                    if (!(bool) _commands[args[0]].Item2.DynamicInvoke(invoke))
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

        private string ConvertToAbsolutePath(string path)
        {
            var trimmed = path.TrimEnd('/');
            var preppedPath = "";
            var fullPath = new List<string>();
            if (!trimmed.StartsWith("/") && _currentPath != "/")
            {
                //This is not an absolute path
                //Lets prepend the current path
                if (_currentPath.Split('/').Length > 1)
                {
                    fullPath.AddRange(_currentPath.Split('/').Skip(1));
                }
            }
            var splitPath = trimmed.Split('/');
            foreach (var item in splitPath)
            {
                switch (item)
                {
                    case "":
                        continue;
                    case "..":
                        fullPath.RemoveAt(fullPath.Count - 1);
                        continue;
                    case ".":
                        continue;
                }
                fullPath.Add(item);
            }
            return "/" + string.Join("/", fullPath.ToArray());
        }

        private bool CommandHelp(string[] args)
        {
            foreach (var command in _commands)
            {
                Console.WriteLine($"{command.Key}\t{command.Value.Item1}");
            }
            return true;
        }

        private bool CommandDel(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Path to be deleted not included.");
            }
            _fileSystem.DeleteObject(ConvertToAbsolutePath(args[1]));
            return true;
        }

        private bool CommandRen(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Path to be deleted not included.");
            }
            _fileSystem.RenameObject(ConvertToAbsolutePath(args[1]), args[2]);
            return true;
        }

        private bool CommandCd(string[] args)
        {
            if (args.Length == 1)
            {
                return true;
            }
            else
            {
                var path = ConvertToAbsolutePath(args[1]);
                _fileSystem.GetDirectoryContents(path);
                _currentPath = path;
            }
            return true;
        }

        private bool CommandLs(string[] args)
        {
            if (args.Length == 1)
            {
                //There were no arguments provided.
                Console.WriteLine(ParseDirectory(_fileSystem.GetDirectoryContents(_currentPath)));
                return true;
            }
            else
            {
                Console.WriteLine(ParseDirectory(_fileSystem.GetDirectoryContents(ConvertToAbsolutePath(args[1]))));
            }
            return true;
        }

        private bool CommandCat(string[] args)
        {
            if (args.Length == 1)
            {
                return true;
            }
            string path = ConvertToAbsolutePath(args[1]);
            File f = _fileSystem.GetFile(path);
            byte[] data = f.FileData;
            Console.WriteLine($"Displaying file {f.Filename}");
            for (int i = 0; i < data.Length; i++)
            {
                Console.Write($"{(char)data[i]}");
            }
            return false;
        }

        private bool CommandExit(string[] args)
        {
            Console.WriteLine("Goodbye.");
            return false;
        }

        private static string ParseDirectory(DirectoryTable table)
        {
            return table.Rows.Aggregate("", (current, row) => current + $"{row.GetString()}\n");
        }

        //I didn't feel like parsing out arguments to commands myself on the command line. So I used some black magic.
        //This uses the Windows API that parses commands in Windows to create the arguments for a program. Guaranteed to work beautifully.
        //Unfortunately does not work on Linux since it relies on the Windows shell being present.
        [DllImport("shell32.dll", SetLastError = true)]
        static extern IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string lpCmdLine, out int pNumArgs);

        private static string[] CommandLineToArgs(string commandLine)
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