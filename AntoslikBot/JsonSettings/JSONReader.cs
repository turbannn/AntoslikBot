using System.Text.Json;

namespace AntoslikBot.JsonSettings;

public class JSONReader
{
    public JSONStructure jSON { get; set; }

    public JSONReader()
    {
        jSON = ReadJsonStructure() ?? throw new NullReferenceException();
    }

    public JSONStructure? ReadJsonStructure()
    {
        Console.WriteLine("Enter json config path: ");
        string? filePath = Console.ReadLine();
        if (filePath == null || filePath == string.Empty)
        {
            Console.WriteLine("Error: Path not entered");
            return null;
        }

        FileStream fs;
        try
        {
            fs = new FileStream(filePath, FileMode.OpenOrCreate);
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine("File not found");
            Console.WriteLine(e.ToString());
            return null;
        }

        StreamReader sr = new StreamReader(fs);
        JSONStructure? jSON = new JSONStructure();

        string? jsonstr = sr.ReadToEnd();

        try
        {
            jSON = JsonSerializer.Deserialize<JSONStructure>(jsonstr);
        }
        catch (Exception ex)
        {
            Console.WriteLine("DESERIALIZATION EX COUGHT: ");
            Console.WriteLine(ex.ToString());
            return null;
        }

        fs.Close();
        fs.Dispose();

        sr.Close();
        sr.Dispose();
        return jSON;
    }
}
