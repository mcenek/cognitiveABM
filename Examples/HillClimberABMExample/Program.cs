using System;
using System.Collections.Generic;
using CognitiveABM.QLearning;
using Mars.Core.ModelContainer.Entities;
using System.IO;
using System.Runtime.CompilerServices;
using HillClimberABMExample.General;
using GUI;
using System.Windows.Forms;

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

                // call rewrad generator with type
                var rewardArgs = new string[] { rewardType.ToString() };
                RewardGenerator.RewardGenerator.GenerateReward(rewardType);

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

                    numTrain = random.Next(0,2);
                    // train
                    abm.Train(fcm, numTrain, 0, true, terrainFilePath, args);
                }
            }
            else
            {
                // terminate program
                return;
            }
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
