using System.Collections.Generic;
using HillClimberExample;
using CognitiveABM.QLearning;
using Mars.Core.ModelContainer.Entities;

public static class Program
{
    public const string OUTPUT_FILENAME = "Animal.csv";
    public const string FITNESS_COLUMNNAME = "BioEnergy";
    public const int STEPS = 250;

    public static void Main(string[] args)
    {
        //var terrainFilePaths = new string[] { "./layers/landscape.csv" };
        var terrainFilePaths = new string[] { "./layers/landscape.csv", "./layers/gradient.csv", "./layers/grid.csv" };
        var fitnessVals = new List<List<float>>();

        foreach (string terrainFilePath in terrainFilePaths)
        {
            FileUtils.ChangeTerrainFilePath(terrainFilePath);

            QLearning.usePerfectQMap = 1;
            List<List<float>> trainGenomes = null;
            if (terrainFilePath != terrainFilePaths[0]) {
                trainGenomes = FileUtils.ReadGenomesFromFile(".\\output\\genomes.csv");
            }
            //HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);
            ABM abm = new ABM(modelDescription: GetModelDescription());
            abm.Train(10, terrainFilePath, args);
            QLearning.usePerfectQMap = 0;
            fitnessVals.Add(abm.Test(1, STEPS, FITNESS_COLUMNNAME,OUTPUT_FILENAME,terrainFilePath, args));
            //abm.Train(fcm, 10, 200, true, args);

            // var genomes = FileUtils.ReadGenomesFromFile(".\\output\\good.csv");
            //fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);
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
