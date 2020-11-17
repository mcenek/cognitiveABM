using System.Collections.Generic;
using HillClimberExample;
using Mars.Core.ModelContainer.Entities;

public static class Program
{
    public const string OUTPUT_FILENAME = "Animal.csv";
    public const string FITNESS_COLUMNNAME = "BioEnergy";
    public const int STEPS = 150;

    public static void Main(string[] args)
    {

        var terrainFilePaths = new string[] { "./layers/grid.csv", "./layers/gradient.csv" };
        var fitnessVals = new List<List<float>>();

        foreach (string terrainFilePath in terrainFilePaths)
        {
            FileUtils.ChangeTerrainFilePath(terrainFilePath);

            var testGenmoes = FileUtils.ReadGenomesFromFile(".\\output\\genomes.csv");
            HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME);

            ModelDescription description = GetModelDescription();
            ABM abm = new ABM(modelDescription: description);

            abm.Train(fcm, 30, 200, true, args);

            testGenmoes = FileUtils.ReadGenomesFromFile(".\\output\\genomes.csv");
            fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, testGenmoes);
            fitnessVals.Add(abm.Test(fcm, 2, args));
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
