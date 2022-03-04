using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CognitiveABM.FCM;
using CognitiveABM.QLearning;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Core.ModelContainer.Entities;
using Mars.Core.SimulationManager.Entities;
using Mars.Core.SimulationStarter;

public class ABM
{

    public ModelDescription modelDescription;

    public static int QlearningTotalFittness = 0;


    public ABM(ModelDescription modelDescription)
    {
        this.modelDescription = modelDescription;
    }


    public void RunSimulation(string terrainFileName, string[] args)
    {
        FileUtils.ChangeTerrainFilePath(terrainFileName);
    }

    //---FCM---//
    // public List<float> Test(FCM fcm, int generations, string[] args)
    // {
    //     var startTime = DateTime.Now;
    //
    //     for (int generation = 0; generation < generations; generation++)
    //     {
    //         Console.WriteLine("\nTest generation: {0} of {1}", generation, generations);
    //
    //         LoggerFactory.SetLogLevel(LogLevel.Warning);
    //         LoggerFactory.DeactivateConsoleLogging();
    //
    //
    //         SimulationStarter task = SimulationStarter.Start(this.modelDescription, args);
    //
    //         Stopwatch stopWatch = new Stopwatch();
    //         stopWatch.Start();
    //
    //         SimulationWorkflowState loopResults = task.Run();
    //
    //         if (loopResults.IsFinished)
    //         {
    //             stopWatch.Stop();
    //             // Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");
    //
    //             stopWatch.Restart();
    //             var values = fcm.Run(false, 200, true);
    //             stopWatch.Stop();
    //             // Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");
    //
    //             GC.Collect();
    //
    //             return values;
    //         }
    //     }
    //     return null;
    // }
    //
    // public void Train(FCM fcm, int generations, float targetFitness, Boolean saveGenomes, string[] args)
    // {
    //     var startTime = DateTime.Now;
    //     for (int generation = 0; generation < generations; generation++)
    //     {
    //         Console.WriteLine("\nGeneration: {0} of {1}", generation, generations);
    //
    //         LoggerFactory.SetLogLevel(LogLevel.Warning);
    //         LoggerFactory.DeactivateConsoleLogging();
    //
    //         SimulationStarter task = SimulationStarter.Start(this.modelDescription, args);
    //
    //         Stopwatch stopWatch = new Stopwatch();
    //         stopWatch.Start();
    //
    //         SimulationWorkflowState loopResults = task.Run();
    //
    //         if (loopResults.IsFinished)
    //         {
    //             stopWatch.Stop();
    //             Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");
    //
    //             stopWatch.Restart();
    //             fcm.Run(true, targetFitness, saveGenomes);
    //             stopWatch.Stop();
    //
    //             Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");
    //
    //             GC.Collect();
    //         }
    //     }
    //
    //     string filename = FileUtils.CreateTimestampedFilename("Genomes", DateTime.Now, ".csv");
    //     fcm.WriteGenomes(filename);
    //     fcm.WriteGenomes("genomes.csv");
    // }

    //---QLearning---//
    public List<float> Test(int generations, int steps, string fitnessColumnName, string fitnessFileName, string[] args)
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

                //stopWatch.Restart();
                //var values = fcm.Run(false, 200, true);
                //stopWatch.Stop();
                // Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");
                List<float> agentFitness = Fitness(steps, fitnessColumnName, fitnessFileName);

                var avg = agentFitness.Average();
                var max = agentFitness.Max();

                Console.WriteLine("Average fitness: {0:F2}, Max fitness: {1:F2}", avg, max);

                GC.Collect();
                return agentFitness;
            }
        }
        return null;
    }

    // public void Train(int generations, int steps, string fitnessColumnName, string fitnessFileName, string[] args)
    // {
    //     var startTime = DateTime.Now;
    //     for (int generation = 0; generation < generations; generation++)
    //     {
    //         Console.WriteLine("\nGeneration: {0} of {1}", generation, generations);
    //
    //         LoggerFactory.SetLogLevel(LogLevel.Warning);
    //         LoggerFactory.DeactivateConsoleLogging();
    //
    //         SimulationStarter task = SimulationStarter.Start(this.modelDescription, args);
    //
    //         Stopwatch stopWatch = new Stopwatch();
    //         stopWatch.Start();
    //
    //         SimulationWorkflowState loopResults = task.Run();
    //
    //         if (loopResults.IsFinished)
    //         {
    //             stopWatch.Stop();
    //             Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");
    //
    //             List<float> agentFitness = Fitness(steps, fitnessColumnName, fitnessFileName);
    //
    //             var avg = agentFitness.Average();
    //             var max = agentFitness.Max();
    //
    //             Console.WriteLine("Average fitness: {0:F2}, Max fitness: {1:F2}", avg, max);
    //
    //             GC.Collect();
    //         }
    //     }
    //
    // }//end train

    public List<float> Fitness(int steps, string fitnessColumnName, string fitnessFileName)
    {
        var fitnessValues = new List<float>();

        using (var reader = new StreamReader(fitnessFileName))
        {

            var header = reader.ReadLine();
            List<string> headerValues = new List<string>(header.Split(','));
            int indexOfFitnessValues = headerValues.FindIndex((str) => str == fitnessColumnName);
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var values = line.Split(',');
                if (Convert.ToInt32(values[0]) <= steps)
                {
                    fitnessValues.Add(float.Parse(values[indexOfFitnessValues]));
                }
            }
        }

        return fitnessValues;
    }//end Fitness
}
