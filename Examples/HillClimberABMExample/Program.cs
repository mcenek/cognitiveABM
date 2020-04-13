using System;
using System.Diagnostics;
using System.IO;
using HillClimberExample;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Core.ModelContainer.Entities;
using Mars.Core.SimulationManager.Entities;
using Mars.Core.SimulationStarter;

public static class Program
{

    public static string OUTPUT_FILENAME = "Animal.csv";
    public static string FITNESS_COLUMNNAME = "BioEnergy";
    public static int STEPS = 2500;
    private static int maxGenerations = 100;
    private static DateTime startTime;


    public static void Main(string[] args)
    {

        startTime = DateTime.Now;

        HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME);

        string filename = CreateTimestampedFilename(filename: "FitnessValues", time: startTime, ext: ".csv");
        string path = ".\\output\\" + filename;
        var writer = new StreamWriter(path: path, append: true);



        for (int generation = 0; generation < maxGenerations; generation++)
        {

            Console.WriteLine("\nGeneration: {0} of {1}", generation, maxGenerations);

            LoggerFactory.SetLogLevel(LogLevel.Warning);
            LoggerFactory.DeactivateConsoleLogging();

            ModelDescription description = new ModelDescription();
            description.AddLayer<Terrain>();
            description.AddAgent<Animal, Terrain>();

            SimulationStarter task = SimulationStarter.Start(description, args);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            SimulationWorkflowState loopResults = task.Run();

            if (loopResults.IsFinished)
            {

                stopWatch.Stop();
                Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");

                stopWatch.Restart();
                var agentFitnessValues = fcm.Run();
                stopWatch.Stop();

                Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");


                foreach (double value in agentFitnessValues)
                {
                    writer.Write(value + ",");
                }
                writer.WriteLine();

                GC.Collect();
            }
        }
        writer.Close();
    }
    private static string CreateTimestampedFilename(string filename, DateTime time, string ext = ".txt")
    {
        return filename + time.ToString().Replace('/', '-').Replace(' ', '_').Replace(':', ';') + ext;
    }
}
