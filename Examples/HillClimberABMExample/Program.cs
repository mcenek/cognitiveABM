using System;
using System.Collections.Generic;
using HillClimberExample;
using CognitiveABM.QLearning;
using Mars.Core.ModelContainer.Entities;
using TerrainGenerator;
using RewardGenerator;

public static class Program
{
    public const string OUTPUT_FILENAME = "Animal.csv";
    public const string FITNESS_COLUMNNAME = "BioEnergy";
    public static string[] terrainFilePaths;
    public static string terrainFilePath;
    public const int STEPS = 250;

    public static void Main(string[] args)
    {
        // Dynamic Generation of Terrain 
        TerrainGenerator.TerrainGenerator.Main(args);
        // Dynamic Reward Generation
        RewardGenerator.RewardGenerator.Main(args);
        
        terrainFilePaths = new string[] { "./layers/landscape.csv", "./layers/moatGauss.csv", "./layers/landscapeInvert.csv" }; // ./layers/grid.csv
        var fitnessVals = new List<List<float>>();
        var random = new Random();
        int numTrain;

        foreach (string filePath in terrainFilePaths)
        {
            terrainFilePath = filePath;
            FileUtils.ChangeTerrainFilePath(terrainFilePath);


            QLearning.usePerfectQMap = 0;
            List<List<float>> trainGenomes = null;
            if (terrainFilePath != terrainFilePaths[0]) { // if not landscape, then take from train
                trainGenomes = FileUtils.ReadGenomesFromFile("./output/genomes.csv");
            }
            HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);
            ABM abm = new ABM(modelDescription: GetModelDescription());
            
            // randomized amount of trained agents for each terrain [50-100]
            numTrain = random.Next(50,101);
            abm.Train(fcm, numTrain, 0, true, terrainFilePath, args);

            fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);

            fitnessVals.Add(abm.Test(fcm, 1, terrainFilePath, args));
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
