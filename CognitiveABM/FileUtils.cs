
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public static class FileUtils
{
    public static List<List<float>> ReadGenomesFromFile(string path)
    {
        List<List<float>> genomes = new List<List<float>>();
        using (var reader = new StreamReader(path))
        {
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                var agentGenomes = new List<float>();
                foreach (var value in values)
                {
                    agentGenomes.Add(float.Parse(value));
                }
                genomes.Add(agentGenomes);
            }
        }
        return genomes;
    }

    public static void ChangeTerrainFilePath(string path)
    {
        JObject config;
        // read JSON directly from a file
        using (StreamReader file = File.OpenText(@"./config.json"))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            config = (JObject)JToken.ReadFrom(reader);
            config["layers"][0]["file"] = path;
        }

        File.WriteAllText(@"./config.json", config.ToString());
        Console.WriteLine("Changed terrain layer to: " + path);
    }

    public static void ChangeConfigProp(string prop, string value) {
        JObject config;
        // read JSON directly from a file
        using (StreamReader file = File.OpenText(@"./config.json"))
        using (JsonTextReader reader = new JsonTextReader(file))
        {
            config = (JObject)JToken.ReadFrom(reader);
            config[prop] = value;
        }

        File.WriteAllText(@"./config.json", config.ToString());
    }

    public static string CreateTimestampedFilename(string filename, DateTime time, string ext = ".txt")
    {
        return filename + "-" + time.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', ';') + ext;
    }
}
