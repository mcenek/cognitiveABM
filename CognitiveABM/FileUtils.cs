
using System;
using System.Collections.Generic;
using System.IO;

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

    public static string CreateTimestampedFilename(string filename, DateTime time, string ext = ".txt")
    {
        return filename + "-" + time.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', ';') + ext;
    }
}