using System;
using System.IO;
using System.Text;

namespace RewardGenerator
{
    public class RewardGenerator{
        public static void Main(string[] args)
        {

            // to run from Program.cs
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "grid_reward.csv");
            //string csvFile = "../Example/HillClimberABMExample/layers/grid_reward.csv";

            RewardMap rewardmap = new RewardMap(50 /*width*/, 50/*height*/, 25/*num rewards*/,csvFile/*csv file path*/);
            rewardmap.Initialize();
            rewardmap.PrintRewardMap();

            Console.WriteLine("Wrote to reward file.");
        }
    }
}