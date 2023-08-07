using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CognitiveABM.FCM;
using CognitiveABM.QLearning;
using CognitiveABM.QLearningABMAdditional;
using Mars.Common.Logging;
using Mars.Common.Logging.Enums;
using Mars.Core.ModelContainer.Entities;
using Mars.Core.SimulationManager.Entities;
using Mars.Core.SimulationStarter;
using System.Text.RegularExpressions;
using CognitiveABM.agentInformationHolder;

public class ABM
{

    public ModelDescription modelDescription;

    public static int QlearningTotalFittness = 0;
    public QLearningABMAdditional QLABMA = new QLearningABMAdditional();
    public agentInfoHolder agentHolder = new agentInfoHolder();
    public static float GlobalTargetFitnes = -100.0f;
    public static int[] pickUpStat = new int[] {0,0};

    public ABM(ModelDescription modelDescription)
    {
        this.modelDescription = modelDescription;
    }


    public void RunSimulation(string terrainFileName, string[] args)
    {
        FileUtils.ChangeTerrainFilePath(terrainFileName);
    }

    //---Combination---//

    public List<float> Test(FCM fcm, int generations, string terrianFilePath, string[] args)
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
                var values = fcm.Run(false, GlobalTargetFitnes, true);
                stopWatch.Stop();
                // Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");

                // List<float> agentFitness = agentHolder.getFit();
                // var avg = agentFitness.Average();
                // var max = agentFitness.Max();

                //Console.WriteLine("Average fitness: {0:F2}, Max fitness: {1:F2}", avg, max);
                Console.WriteLine("Times agents picked up rewards: " + pickUpStat[0]);
                Console.WriteLine("Times agents failed to picked up rewards: " + pickUpStat[1]);

                QLABMA.exportInfo(terrianFilePath, agentHolder);

                GC.Collect();

                //return values;
            }
        }
        return null;
    }

    public void Train(FCM fcm, int generations, float targetFitness, Boolean saveGenomes, string terrianFilePath, string[] args)
    {
        GlobalTargetFitnes = targetFitness;
        var startTime = DateTime.Now;
        //make method to get lambda value i guess
        float[] lambdaArray = QLABMA.getLambda(generations);

        for (int generation = 0; generation < generations; generation++)
        {

            if(generation == 0){
              QLearning.useMap = false;
            }
            else{
              QLearning.useMap = true;
            }
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
              //  Environment.Exit(0);
                Console.WriteLine($"Simulation execution finished in {stopWatch.ElapsedMilliseconds / 1000:N2} seconds");

                //-----------------------------------------------------------------------------------------------//
                stopWatch.Restart();
                fcm.Run(true, GlobalTargetFitnes, saveGenomes);
                Console.WriteLine("New Target Fitness: " + GlobalTargetFitnes);
                stopWatch.Stop();

                Console.WriteLine($"FCM finished in {stopWatch.ElapsedMilliseconds / 100:N2} seconds");

                //-----------------------------------------------------------------------------------------------//

                Console.WriteLine("Times agents picked up rewards: " + pickUpStat[0]);
                Console.WriteLine("Times agents failed to picked up rewards: " + pickUpStat[1]);
                pickUpStat[0] = 0;
                pickUpStat[1] = 0;
                Dictionary<int, float> scoreValue = QLABMA.getAgentScore(lambdaArray[generation], agentHolder);
                QLABMA.updateQMap(scoreValue, agentHolder);
                Console.WriteLine("QMap has been updated");
                GC.Collect();
            }
        }
        //QLearning.usePerfectQMap = 0;

    }
}
