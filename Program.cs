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
    private Dictionary<string, string> filePair = [];
    private readonly string filePairPath = "jsons/filepair.json";
    private readonly string folderOrgPath = "jsons/setfolderorg.json";
    private readonly string movedFilePairsPath = "jsons/movedfilepairs.json";
    private readonly JsonSerializerOptions jsonOptions = new () { WriteIndented = true };
    private Dictionary<string, string> movedFilePairs = []; //storage for file source to new directory when moved (startorg)
    public FileOrganizer() 
    {
        Init();
    }

    public void Init()
    {
        LoadJson<Dictionary<string, string>>(filePairPath);
        LoadJson<string>(folderOrgPath);
        LoadJson<Dictionary<string, string>>(movedFilePairsPath);

        filePair = CopyJsonContent<Dictionary<string, string>>(filePairPath);
        folderDirectory = CopyJsonContent<string>(folderOrgPath);
        movedFilePairs = CopyJsonContent<Dictionary<string, string>>(movedFilePairsPath);

        Console.WriteLine("**********super file organizer****************");
        Console.WriteLine("To see all possible commands enter help");

        void LoadJson<T>(string jsonDir)
        {
            //if json file does not exist, created it and initialize its new default type;
            if (!File.Exists(jsonDir))
            {
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

        T CopyJsonContent<T>(string filePath)
        {
            string jsonTextContent = File.ReadAllText(filePath);
            var jsonData = JsonSerializer.Deserialize<T>(jsonTextContent);
            if (jsonData == null)
            {
                Console.WriteLine($"json file {filePath} for file pair does not exist");
                return default(T);
            }
            return jsonData;
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
            case "undo":
                UndoOrg();
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

        movedFilePairs.Clear();
        for (int i = 0; i < filesInDir.Length; i++)
        {
            findFile(filesInDir[i]);
        }

        void findFile(string filePath)
        {
            bool movedFile = false;
            Console.WriteLine("name to look for     --     found file-path     ->     new directory for file");
            foreach (KeyValuePair<string, string> file in filePair) 
            {
                int index = filePath.IndexOf(folderDirectory);
                string fileName = (index < 0)
                    ? filePath
                    : filePath.Remove(index, folderDirectory.Length);

                if (fileName.IndexOf(file.Key, 0, fileName.Length) != -1)
                {
                    movedFile = true;
                    string newDirectory = file.Value + fileName;
                    Console.WriteLine($"{file.Key} -- {filePath} ->  {newDirectory}");
                    File.Move(filePath, newDirectory);
                    if (movedFilePairs.TryGetValue(filePath, out string _))
                    {
                        continue;
                    }
                    movedFilePairs.Add(filePath, newDirectory);
                }
            }

            if (!movedFile)
            {
                Console.WriteLine("no files found");
            }
            UpdateJsons();
        }
    }

    public void UndoOrg()
    {
        Console.WriteLine("files undo");
        foreach (KeyValuePair<string, string> filePair in movedFilePairs)
        {
            Console.WriteLine(filePair.Key + " -> " + filePair.Value);
            File.Move(filePair.Value, filePair.Key);
        }
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
        string jsonMovedFP = JsonSerializer.Serialize(movedFilePairs, jsonOptions);
        File.WriteAllText(movedFilePairsPath, jsonMovedFP);
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

    public static void Help()
    {
        Console.WriteLine("COMMANDS:");
        Console.WriteLine("  set filename [name-to-look-for] to [folder-directory]");
        Console.WriteLine("  --to set a name to automatically move to a folder-director");
        Console.WriteLine("  set orgdir [folder-directory]");
        Console.WriteLine("  --to set a folder-directory to organize, where the files will be taken from]");
    }
}
