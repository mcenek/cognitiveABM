using System;
using System.Collections.Generic;
using CognitiveABM.QLearning;
using Mars.Core.ModelContainer.Entities;
using System.IO;
using System.Runtime.CompilerServices;
using HillClimberABMExample.General;
using System.Runtime.InteropServices;

#if WINDOWS
using GUI;
using System.Windows.Forms;
#endif

public static class Program
{
    public const string OUTPUT_FILENAME = "Animal.csv";
    public const string FITNESS_COLUMNNAME = "BioEnergy";
    public static string[] terrainFilePaths;
    public static string terrainFilePath;
    public const int STEPS = 250;

    [STAThread]
    public static void Main(string[] args)
    {
        // Check if we're on Windows and have Windows Forms available
        #if WINDOWS
        // Windows-specific GUI approach
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // show selection form
        using (var form = new SelectionForm())
        {
            if (form.ShowDialog() == DialogResult.OK)
            {
                //get selections from GUI
                int terrainType = form.SelectedTerrainType;
                int rewardType = form.SelectedRewardType;

                // call terrain generator with type selected
                var terrainArgs = new string[] { terrainType.ToString() };
                TerrainGenerator.TerrainGenerator.GenerateTerrain(terrainType);

                // call reward generator with type
                var rewardArgs = new string[] { rewardType.ToString() };
                RewardGenerator.RewardGenerator.GenerateReward(rewardType);

                RunSimulation(args);
            }
        }
        #else
        // Terminal approach for Mac and other platforms
        Console.WriteLine("Running in terminal mode for non-Windows platform");
        RunTerminalMode(args);
        #endif
    }

    #if !WINDOWS
    private static void RunTerminalMode(string[] args)
    {
        // Call terrain generator and reward generator
        TerrainGenerator.TerrainGenerator.Main(args);
        RewardGenerator.RewardGenerator.Main(args);
        RunSimulation(args);
    }
    #endif

    private static void RunSimulation(string[] args)
    {
        File.WriteAllText("./layers/qMapGenerated8x8.csv", string.Empty);

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
            if (terrainFilePath != terrainFilePaths[0])
            {
                trainGenomes = FileUtils.ReadGenomesFromFile("./output/genomes.csv");
            }
            HillClimberFCM fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);
            ABM abm = new ABM(modelDescription: GetModelDescription());

            numTrain = random.Next(100, 105);
            // Train
            abm.Train(fcm, numTrain, 0, true, terrainFilePath, args);

            // Test
            fcm = new HillClimberFCM(population: 96, numberOfValues: 2020, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME, trainGenomes);
            fitnessVals.Add(abm.Test(fcm, 1, terrainFilePath, args));
        }

        // Upload data to HDFS
        bool uploadToHadoop = false;
        if (uploadToHadoop)
        {
            HDFS.Upload.UploadToHDFS();
            Console.WriteLine("\nProgram finish.");
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
