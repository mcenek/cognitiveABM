using System;
using System.Collections.Generic;
using System.Diagnostics;
using CognitiveABM.FCM;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Core.ModelContainer.Entities;
using Mars.Core.SimulationManager.Entities;
using Mars.Core.SimulationStarter;

public class ABM
{

    public ModelDescription modelDescription;

    public ABM(ModelDescription modelDescription)
    {
        this.modelDescription = modelDescription;
    }

    public void Test(FCM fcm, int generations, string[] args)
    {
        var startTime = DateTime.Now;

        for (int generation = 0; generation < generations; generation++)
        {
            Console.WriteLine("\nTest generation: {0} of {1}", generation, generations);

            LoggerFactory.SetLogLevel(LogLevel.Warning);
            LoggerFactory.DeactivateConsoleLogging();


            SimulationStarter task = SimulationStarter.Start(this.modelDescription, args);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            SimulationWorkflowState loopResults = task.Run();

            if (loopResults.IsFinished)
            {
                stopWatch.Stop();
                // Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");

                stopWatch.Restart();
                fcm.Run(false);
                stopWatch.Stop();
                // Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");

                GC.Collect();
            }
        }
    }

    public void Train(FCM fcm, int generations, float targetFitness, Boolean saveGenomes, string[] args)
    {
        var startTime = DateTime.Now;
        for (int generation = 0; generation < generations; generation++)
        {
            Console.WriteLine("\nGeneration: {0} of {1}", generation, generations);

            LoggerFactory.SetLogLevel(LogLevel.Warning);
            LoggerFactory.DeactivateConsoleLogging();

            SimulationStarter task = SimulationStarter.Start(this.modelDescription, args);

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            SimulationWorkflowState loopResults = task.Run();

            if (loopResults.IsFinished)
            {
                stopWatch.Stop();
                Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");

                stopWatch.Restart();
                fcm.Run(true, targetFitness, saveGenomes);
                stopWatch.Stop();

                Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");

                GC.Collect();
            }
        }

        string filename = FileUtils.CreateTimestampedFilename("Genomes", new DateTime(), ".csv");
        fcm.WriteGenomes(filename);
    }
}
