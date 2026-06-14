// See https://aka.ms/new-console-template for more information

using Arch.ConsoleApp;

var file = FileHelper.GetExistingFile();

var lines = File.ReadLines(file.FullName);
var props = lines
    .Where(CsvParser.IsValidLine)
    .Select(CsvParser.ParseProperty);
    
var className = Path.GetFileNameWithoutExtension(file.Name);
var src = ClassGenerator.GenerateClass(className, props);

Console.WriteLine(src);
