using System;
using System.IO;
using System.Text;

namespace RewardGenerator
{
    public class RewardGenerator{
        public static void Main(string[] args)
        {
            
            // Ask user for specific map --
            int userInput = 1;
            do {
                Console.WriteLine("--------------------------------------------------------------------");
                Console.WriteLine(" Select a Reward Style - ");
                Console.WriteLine("1. Normal");
                Console.WriteLine("2. Centered");
                Console.WriteLine("--------------------------------------------------------------------");
                // int?
                if (int.TryParse(Console.ReadLine(), out userInput)){
                    // in range
                    if (userInput >= 1 && userInput <= 2){
                        break; // Valid input, exit the loop
                    } else {
                        Console.WriteLine("Invalid input. Please enter a number between 1 and 5.");
                    }
                } else {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            } while (true);
            
            // call GenerateReward with the user's selection
            GenerateReward(userInput);
        }

        public static void GenerateReward(int rewardType)
        {
            // to run from Program.cs
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "grid_reward.csv");
            string guassFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "moatGauss_reward.csv");
            string landscape_rewardFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape_reward.csv");
            string rewardTest = Path.Combine(baseDirectory, "..", "..", "..", "layers", "rewardTest.csv");

            RewardMap rewardmap = new RewardMap(50 /*width*/, 50/*height*/, 25/*num rewards*/,csvFile/*csv file path*/,guassFile,landscape_rewardFile, rewardTest, rewardType);
            rewardmap.Initialize();
            rewardmap.PrintRewardMap();

            Console.WriteLine("Wrote to reward file.");
        }
    }
}
