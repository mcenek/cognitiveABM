using System;
using System.Collections.Generic;
using HillClimberExample;
using CognitiveABM.QLearning;
using Mars.Core.ModelContainer.Entities;
using System.IO;
using System.Runtime.CompilerServices;

public static class Program
{
    public const string OUTPUT_FILENAME = "Animal.csv";
    public const string FITNESS_COLUMNNAME = "BioEnergy";
    public static string[] terrainFilePaths;
    public static string terrainFilePath;
    public const int STEPS = 250;


    public static void Main(string[] args)
    {
        TerrainGenerator.TerrainGenerator.Main(args);
        RewardGenerator.RewardGenerator.Main(args);
        File.WriteAllText("./layers/qMapGenerated8x8.csv", string.Empty);

        terrainFilePaths = new string[] { "./layers/landscape.csv", "./layers/moatGauss.csv", "./layers/grid.csv" };
        var fitnessVals = new List<List<float>>();
        var random = new Random();
        int numTrain;


        foreach (string filePath in terrainFilePaths)
        {
            terrainFilePath = filePath;
            FileUtils.ChangeTerrainFilePath(terrainFilePath);

            QLearning.usePerfectQMap = 0;
            List<List<float>> trainGenomes = null;
            if (terrainFilePath != terrainFilePaths[0]) {
                trainGenomes = FileUtils.ReadGenomesFromFile("./output/genomes.csv");
            }
            HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);
            ABM abm = new ABM(modelDescription: GetModelDescription());

            numTrain = random.Next(100,105);
            // Train
            abm.Train(fcm, numTrain, 0, true, terrainFilePath, args);

            // QLearning.usePerfectQMap = 0;
            //  var genomes = FileUtils.ReadGenomesFromFile(".\\output\\good.csv");
            fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);

            // Test
            fitnessVals.Add(abm.Test(fcm, 1, terrainFilePath, args));
        }

        // Upload data to HDFS
        bool uplaodtoHadoop = false;
        if (uplaodtoHadoop){
            HDFS.Upload.UploadToHDFS();
            Console.WriteLine("\nProgram finish.");
        }
    }

    private static ModelDescription GetModelDescription()
    {
        ModelDescription description = new ModelDescription();
        description.AddLayer<Terrain>();
        description.AddAgent<Animal, Terrain>();
        return description;
    }
}
