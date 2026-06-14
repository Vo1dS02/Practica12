namespace Arch.ConsoleApp;

public static class FileHelper
{
    public static FileInfo GetExistingFile()
    {
        while (true)
        {
            Console.WriteLine("Please enter the path to the file:");
            var path = Console.ReadLine();
            ArgumentNullException.ThrowIfNull(path);
            var file = new FileInfo(path);
            if (file.Exists)
            {
                return file;
            }
        }
    }
}