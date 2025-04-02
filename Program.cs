// See https://aka.ms/new-console-template for more information
using System.Text;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        FileOrganizer fileOrganizer = new();
        while (true)
        {
            fileOrganizer.Loop();
        }
    }
}

class FileOrganizer
{
    private string? folderDirectory = "";
    private Dictionary<string, string> filePair = new();
    private string filePairPath = "filepair.json";
    private string folderOrgPath = "setfolderorg.json";
    private JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    public FileOrganizer() 
    {
        Init();
    }

    public void Init()
    {
        LoadJson<Dictionary<string, string>>(filePairPath);
        LoadJson<string>(folderOrgPath);

        // copy json content for filepair
        string fpairJsonContent = File.ReadAllText(filePairPath);
        var jsonFilePair = JsonSerializer.Deserialize<Dictionary<string, string>>(fpairJsonContent);
        if (jsonFilePair == null)
        {
            Console.WriteLine($"json file {filePairPath} for file pair does not exist");
            return;
        }
        filePair = jsonFilePair;

        // copy json content for orgdir or folderorg
        string forgJsonContent = File.ReadAllText(folderOrgPath);
        var jsonFolderOrg = JsonSerializer.Deserialize<string>(forgJsonContent);
        if (jsonFolderOrg == null)
        {
            Console.WriteLine($"json {folderOrgPath} does not exist");
            return;
        }
        folderDirectory = jsonFolderOrg;

        Console.WriteLine("**********super file organizer****************");
        Console.WriteLine("To see all possible commands enter help");

        void LoadJson<T>(string jsonDir)
        {
            if (!File.Exists(jsonDir))
            {
                Type type = typeof(T);
                Console.WriteLine($"File not found, creating new file named {jsonDir}");
                T newEmpty;
                if (typeof(T) == typeof(string))
                {
                    newEmpty = (T)(object)"";
                }
                else 
                    newEmpty = Activator.CreateInstance<T>();
                var newDir = JsonSerializer.Serialize(newEmpty, jsonOptions);
                using (FileStream fs = File.Create(jsonDir))
                { 
                    Byte[] info = new UTF8Encoding(true).GetBytes(newDir);
                    fs.Write(info);
                }
            }
        }
    }

    public void Loop()
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
            case "remove":
                RemoveFilePair(commands);
                break;
            case "start":
                StartOrg();
                break;
            default:
                Error();
                break;
        }
    }

    public void StartOrg()
    {
        if (folderDirectory == null)
        {
            Console.WriteLine("folder directory is not set");
            return;
        }

        if (filePair.Count == 0)
        {
            Console.WriteLine("There is no set file to organize");
            return;
        }

        string[] filesInDir = Directory.GetFiles(folderDirectory);

        for (int i = 0; i < filesInDir.Length; i++)
        {
            findFile(filesInDir[i]);
        }

        void findFile(string fileName)
        {
            foreach (KeyValuePair<string, string> file in filePair) 
            {
                int index = fileName.IndexOf(folderDirectory);
                string cleanPath = (index < 0)
                    ? fileName
                    : fileName.Remove(index, folderDirectory.Length);
                //Console.WriteLine($"{fileName}");
                if (fileName.IndexOf(file.Key, 0, fileName.Length) != -1)
                {
                    Console.WriteLine($"{file.Key} -> {fileName}");
                    //Console.WriteLine(file.Value + cleanPath);
                    File.Move(fileName, file.Value + cleanPath);
                }
            }
        }
    }

    public void Help()
    {
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  set filename [name-to-look-for] to [folder-directory]");
        Console.WriteLine("  --to set a name to automatically move to a folder-director");
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
            string fileName = command[2];
            string folderDir = command[3];

            //check if folderDirectory exists
            if (!Directory.Exists(folderDir))
            {
                Console.WriteLine($"folder path {folderDir} does not exist");
                return;
            }

            if (!filePair.TryAdd(fileName, folderDir))
            {
                Console.WriteLine("happens");
                filePair[fileName] = folderDir;
            }

            UpdateJsons();
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

            UpdateJsons();
        }
    }

    public void RemoveFilePair(string[] command)
    {
        try 
        {
            filePair.Remove(command[1]);
        }
        catch (Exception e) 
        {
            if (command.Length != 2)
            {
                Console.WriteLine("Remove has 1 parameter filename");
                return;
            }
            Console.WriteLine("filename is not added to filepair");
            Console.WriteLine(e.ToString());
        }
        UpdateJsons();
    }

    public void UpdateJsons()
    {
        string jsonFilePair = JsonSerializer.Serialize(filePair, jsonOptions);
        File.WriteAllText(filePairPath, jsonFilePair);
        string jsonFolderOrg = JsonSerializer.Serialize(folderDirectory, jsonOptions);
        File.WriteAllText(folderOrgPath, jsonFolderOrg);
    }

    public void DisplayConfig(string[] command)
    {
        if (command.Length == 2)
        {
            //checks if input clear as parameter
            ClearConfig(command[1]);
            return;
        }

        if (folderDirectory != "" || folderDirectory != null)
            Console.WriteLine($"Directory of folder to organize: {folderDirectory}");
        else
            Console.WriteLine("Directory of folder to organize is not set");

        if (filePair.Count == 0)
        {
            Console.WriteLine("FileNames and redirectories not set");
            return;
        }
        Console.WriteLine("FileNames and redirectories: ");
        foreach (KeyValuePair<string, string> file in filePair)
        {
            Console.WriteLine(file.Key + " -> " + file.Value);
        }
    }

    public void ClearConfig(string parameter)
    {
        if (parameter != "clear")
        {
            Console.WriteLine("INVALID INPUT: config has 1 overload: config clear");
            return;
        }
        filePair.Clear();
        folderDirectory = "";
        UpdateJsons();
    }
}
// 1. set filename [name-to-look-for] to [orgdir]
// 2. display-config
