using HillClimberExample;
using Mars.Core.ModelContainer.Entities;

public static class Program
{
    public const string OUTPUT_FILENAME = "Animal.csv";
    public const string FITNESS_COLUMNNAME = "BioEnergy";
    public const int STEPS = 150;

    public static void Main(string[] args)
    {
        var testGenmoes = FileUtils.ReadGenomesFromFile(".\\output\\genomes.csv");
        HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME);

        ModelDescription description = new ModelDescription();
        description.AddLayer<Terrain>();
        description.AddAgent<Animal, Terrain>();

        ABM abm = new ABM(modelDescription: description);

        abm.Train(fcm, 30, 200, true, args);

        abm.Train(fcm, 30, 200, true, args);

        abm.Train(fcm, 30, 200, true, args);

        abm.Train(fcm, 30, 200, true, args);

        testGenmoes = FileUtils.ReadGenomesFromFile(".\\output\\genomes.csv");
        fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, testGenmoes);
        abm.Test(fcm, 2, args);
    }
}
