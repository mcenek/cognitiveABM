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
using System.Text.RegularExpressions;

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
    public List<float> Test(int generations, int steps, string fitnessColumnName, string fitnessFileName, string terrianFilePath, string[] args)
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
                //List<float> agentFitness = Fitness(steps, fitnessColumnName, fitnessFileName);
                List<float> agentFitness = QLearning.fitness;
                var avg = agentFitness.Average();
                var max = agentFitness.Max();

                Console.WriteLine("Average fitness: {0:F2}, Max fitness: {1:F2}", avg, max);
                exportInfo(QLearning.patchDict, QLearning.animalIDHolder, QLearning.fitDict, terrianFilePath);
                GC.Collect();
                return agentFitness;
            }
        }
        return null;
    }



    /**
     * @param values: list of string arrays containg values to print
     * @param terrianFilePath: String of full pathway to selected terrian File
     * @description: prints all values of the values parameter into a csv file named after the terrian
     */
    public void exportInfo(Dictionary<int,List<float[]>> patchDict, List<int> animalIdList, Dictionary<int, List<float>> fitDict, string terrianFilePath){
          string fileName = "./output/" + Path.GetFileNameWithoutExtension(terrianFilePath) + "_exportInfo.csv";
          var w = new StreamWriter(path: fileName);

          //write headers to csv
          string[] headers = new string[18];
          headers[0] = "AnimalID";
          headers[1] = "TickNum";
          for(int k = 2; k < headers.Length-3; k++){
            headers[k] = "LandscapePatch " + (k-2).ToString();
          }
          headers[11] = "Current Elevation";
          headers[12] = "Next Elevation";
          headers[13] = "Fitness Gained";
          headers[14] = "Average Fitness";
          headers[15] = "Total Fitness";
          headers[16] = "X Pos";
          headers[17] = "Y Pos";
          w.Write(String.Join(",", headers) + "\n");

          //write data to csv
          List<float[]> patchList = new List<float[]>();
          List<float> fitList = new List<float>();
          List<float> currentFit = new List<float>();
          foreach (int id in animalIdList){
            patchList = patchDict[id];
            fitList = fitDict[id];
            foreach (float[] array in patchList){
               w.Write(String.Join(",", array) + "\n");

            }
          }
          w.Close();
    }//end exportInfo

}
