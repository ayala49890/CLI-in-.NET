using System;
using System.CommandLine;
using System.IO;
using System.Reflection.PortableExecutable;

var bundleOption1 = new Option<string>("--language", "languages to include") { IsRequired = true };
var bundleOption2 = new Option<FileInfo>("--output", "file path and name") { IsRequired = true };
var bundleOption3 = new Option<bool>("--note", "write the sourse code as a comment in the bundle file");
var bundleOption4 = new Option<bool>("--sort", "the order of copying the code files by extension");
var bundleOption5 = new Option<bool>("--remove-empty-lines", "remove empty lines in the code");
var bundleOption6 = new Option<string>("--author", "the creator`s name of the file will appear");
var bundleCommand = new Command("bundle", "bundle code files to a single file");


bundleOption1.AddAlias("-l");
bundleOption2.AddAlias("-o");
bundleOption3.AddAlias("-n");
bundleOption4.AddAlias("-s");
bundleOption5.AddAlias("-r");
bundleOption6.AddAlias("-a");

bundleCommand.AddOption(bundleOption1);
bundleCommand.AddOption(bundleOption2);
bundleCommand.AddOption(bundleOption3);
bundleCommand.AddOption(bundleOption4);
bundleCommand.AddOption(bundleOption5);
bundleCommand.AddOption(bundleOption6);

string currentPath = Directory.GetCurrentDirectory();
List<string> files = Directory.GetFiles(currentPath, "", SearchOption.AllDirectories).Where(file => !file.Contains("bin") && !file.Contains("Debug") && !file.Contains("node_modules") && !file.Contains(".git") && !file.Contains(".vscode")).ToList();



string[] langs = { "html", "phyton", "c#", "c", "c++", "java", "asembler", "sql" };
string[] end = { ".html", ".py", ".cs", ".c", ".cpp", ".java", ".asm", ".sql" };

bundleCommand.SetHandler(async (language, output, note, sort, remove, author) =>
{
    while (!ValidPath(output.FullName))
    {
        Error(output.FullName + "is not a valid path");
        return;
    }
    if (File.Exists(output.FullName))
    {
        Warning(output.FullName + " was exists, do you want override this file y/n");
        var answer = Console.ReadLine();
        if (answer == "n" || answer == "N")
            return;
    }
    string[] s = GetLangs(language, end, langs);
    files = files.Where(f => s.Contains(Path.GetExtension(f))).ToList();
 
    if (sort)
        files = files.OrderBy(e => Path.GetExtension(e)).ToList();
    else
        files.Sort();
    await WriteToFile(output, files, note, author, remove);
}, bundleOption1, bundleOption2, bundleOption3, bundleOption4, bundleOption5, bundleOption6);
static string[] GetLangs(string lan, string[] allEnd, string[] allLangs)
{
    if (lan.Contains("all"))
        return allEnd;
    string[] languages = lan.Split(' ');
    for (int i = 0; i < languages.Length; i++)
        if (allLangs.Contains(languages[i]))
            languages[i] = allEnd[Array.IndexOf(allLangs, languages[i])];
        else Error($"Language {languages[i]} is not recognized, It is ignored");
    return languages.Where(l => l[0] == '.').ToArray();
}
static void Error(string Error)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: " + Error);
    Console.ForegroundColor = ConsoleColor.White;

    return;
}
static void Warning(string Warning)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Warning: " + Warning);
    Console.ForegroundColor = ConsoleColor.White;
}
static async Task WriteToFile(FileInfo path, List<string> files, bool note, string author, bool remove)
{
    try
    {
        using (StreamWriter file = new StreamWriter(path.FullName, true))
        {
            if (author != null)
            {
                await file.WriteLineAsync($"/* Author: {author}*/");
            }


            foreach (var f in files)
            {
                await file.WriteLineAsync("***");
                if (note)
                {
                    string p = Path.GetRelativePath(path.FullName, f);
                    string sourceInfo = $"/* Source: {Path.GetFileName(path.ToString())} - {p}*/";
                    await file.WriteLineAsync(sourceInfo);
                }

                using (var codeFile = new StreamReader(f))
                {
                    while (!codeFile.EndOfStream)
                    {
                        string line = codeFile.ReadLine();
                        if (!remove || line.Length > 0)
                            file.WriteLine(line);
                    }
                    codeFile.Close();
                }
            }
        }
        Console.WriteLine("File was created");
    }
    catch (DirectoryNotFoundException)
    {
        Error("");
    }

}
static bool ValidPath(string name)
{
    if (name != null)
        if (name.Contains('*') || name.Contains('?') || name.Contains('>') || name.Contains('<') || name.Contains('|') || name.Contains('&'))
        {
            Warning("file name can`t include the following characters: * ? \" < > | / ");
            return false;
        }

    return true;
}
var creatersp = new Command("create-rsp");
creatersp.SetHandler((output) =>
{
    string filePath, lang, author = "";
    bool note = false, remove = false, sort = false;
    char res;
    Console.WriteLine("Write the response file's name");
    string fileName = Console.ReadLine();
    if (fileName == "")
    {
        Warning("You didn`t write name to file, the default name you got is: response.rsp");
        fileName = "response";
    }

    StreamWriter file = new StreamWriter($"{fileName}.rsp");
    Console.WriteLine("Enter file name (you need to add all path)");
    filePath = Console.ReadLine();
    while (filePath.Length == 0)
    {
        Warning("It's required!  Enter file path (you need to add all path)");
        filePath = Console.ReadLine();
    }
    while (!ValidPath(filePath))
    {
        Warning(filePath + "is not a valid path. Enter a valid file name (you need to add all path)");
        filePath = Console.ReadLine();
    }
    Console.Write("Write any languege from this list: (you can choose 'all')");
    for (int i = 0; i < langs.Length; i++)
    {
        Console.Write(langs[i] + ' ');
    }
    Console.WriteLine("");
    lang = Console.ReadLine();
    while (lang == "")
    {
        Error("Languages is required, choose valid language/s");
        lang = Console.ReadLine();
    }
    Console.WriteLine("Do you want to sort by extension? y/n");
    res = char.Parse(Console.ReadLine());
    if (res == 'y' || res == 'Y')
        sort = true;
    Console.WriteLine("Do you want to write the source file? (y/n)");
    res = char.Parse(Console.ReadLine());
    if (res == 'y' || res == 'Y')
        note = true;
    Console.WriteLine("Do you want to write author name? (y/n)");
    res = char.Parse(Console.ReadLine());
    if (res == 'y' || res == 'Y')
    {
        Console.WriteLine("What is author name?");
        author = Console.ReadLine();
    }
    Console.WriteLine("Do you want to delete empty lines? (y/n)");
    res = char.Parse(Console.ReadLine());
    if (res == 'y' || res == 'Y')
        remove = true;
    file.Write("bundle");
    file.Write(" -o "+ fileName);
    file.Write($" -l \"{lang}\" ");
    if (author != "")
        file.Write(" -a " + author);
    if (sort)
        file.Write(" -s ");
    if (note)
        file.Write(" -n ");
    if (remove)
        file.Write(" -r ");
    file.Close();
    Console.WriteLine($"Response file added succesfully called {fileName}.rsp, now you can use it. write:fib @{fileName}.rsp");

});
var rootCommand = new RootCommand("Root");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(creatersp); 
await rootCommand.InvokeAsync(args);
