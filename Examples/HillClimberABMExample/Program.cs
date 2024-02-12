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
        TerrainGenerator.TerrainGenerator.Main(args);
        RewardGenerator.RewardGenerator.Main(args);
        
        // terrainFilePaths = new string[] { "./layers/moatGauss.csv" };
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
            //abm.Train(10, terrainFilePath, args);

            numTrain = random.Next(100,120);
            abm.Train(fcm, numTrain, 0, true, terrainFilePath, args);

            // QLearning.usePerfectQMap = 0;

            //  var genomes = FileUtils.ReadGenomesFromFile(".\\output\\good.csv");
            fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);

            fitnessVals.Add(abm.Test(fcm, 1, terrainFilePath, args));

            // fitnessVals.Add(abm.Test(1, STEPS, FITNESS_COLUMNNAME,OUTPUT_FILENAME,terrainFilePath, args));
            //abm.Train(fcm, 10, 200, true, args);

            //fitnessVals.Add(abm.Test(fcm, 1, args));
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
