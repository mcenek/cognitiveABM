using System;
using System.IO;
using System.Text;

namespace RewardGenerator
{
    class RewardGenerator{
        static void Main(string[] args)
        {
            //string csvFile = "../Examples/HillClimberABMExample/layers/grid_reward3.csv";
            string csvFile = "./grid_reward3.csv";
            RewardMap rewardmap = new RewardMap(50 /*width*/, 50/*height*/, 100/*num rewards*/,csvFile/*csv file path*/);
            rewardmap.Initialize();
            rewardmap.PrintRewardMap();

            Console.WriteLine("Wrote to reward file.");
        }
    }
}
