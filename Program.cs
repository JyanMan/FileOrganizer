// See https://aka.ms/new-console-template for more information
class Program
{
    static void Main(string[] args)
    {
        FileOrganizer fileOrganizer = new();
        while (true)
        {
            fileOrganizer.Init();
        }
    }
}

class FileOrganizer
{
    private string? folderDirectory = "";
    private Dictionary<string, string> filePair = new();
    public FileOrganizer()
    {
        Console.WriteLine("**********super file organizer****************");
        Console.WriteLine("To see all possible commands enter help");
    }

    public void Init()
    {
        Console.WriteLine("");
        Console.WriteLine("**************");
        Console.Write("input command: ");
        string? option = Console.ReadLine();
        string[] commands = (option != null) ? option.Split(' ') : [];
        Console.WriteLine("");
        if (commands.Length == 0)
        {
            Error();
            return;
        }

        switch (commands[0])
        {
            case "help":
                Help();
                break;
            case "set":
                Set(commands);
                break;
            case "config":
                DisplayConfig(commands);
                break;
            default:
                Error();
                break;
        }
    }

    public void Help()
    {
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  set filename [name-to-look-for] to [folder-directory]");
        Console.WriteLine("  --to set a name to automatically move to a folder-director]");
        Console.WriteLine("  set orgdir [folder-directory]");
        Console.WriteLine("  --to set a folder-directory to organize, where the files will be taken from]");
    }
    
    public void Error()
    {
        Console.WriteLine("INVALID INPUT");
        Help();
    }

    public void Set(string[] command)
    {
        if (command.Length < 3)
        {
            Help();
            return;
        }

        switch (command[1])
        {
            case "filename":
                setFileName(command);
                break;
            case "orgdir":
                setFolderDirectory(command);
                break;
            default:
                Error();
                break;
        }

        void setFileName(string[] command)
        {
            if (command.Length != 4)
            {
                Console.WriteLine("set filename requires 2 parameters: file-name, folder-directory");
                return;
            }
            string fileName = command[1];
            string folderDir = command[2];
            //check if folderDirectory exists
            if (!Directory.Exists(folderDir))
            {
                Console.WriteLine("folder path does not exist");
                return;
            }
            filePair.Add(fileName, folderDir);
        }

        void setFolderDirectory(string[] command)
        {
            if (command.Length != 3)
            {
                Console.WriteLine("set orgdir requires 1 parameter: folder-directory");
                return;
            }

            folderDirectory = command[2];

            if (!Directory.Exists(folderDirectory))
            {
                Console.WriteLine($"the path {folderDirectory} does not exist");
                return;
            }
            Console.WriteLine($"successfully set folder directory to: {folderDirectory}");
            // have a checker if directory exists
        }
    }

    public void DisplayConfig(string[] command)
    {
        if (folderDirectory != "" || folderDirectory != null)
            Console.WriteLine($"Directory of folder to organize: {folderDirectory}");
        else
            Console.WriteLine("Directory of folder to organize is not set");

        Console.Write("fileNames and redirectories ");
        if (filePair.Count == 0)
        {
            Console.WriteLine("not set");
            return;
        }
        Console.WriteLine("");
        foreach (KeyValuePair<string, string> file in filePair)
        {
            Console.WriteLine(file.Key + " -> " + file.Value);
        }
    }
}
// 1. set filename [name-to-look-for] to [orgdir]
// 2. display-config
