using System;
using System.Collections.Generic;
using System.IO;

namespace RewardGenerator
{
    public class RewardMap
    {
        private int Width;
        private int Height;
        private int NumberOfRewards;
        private string RewardFilePath;
        private string GaussFilePath;
        private string Landscape_rewardFile;
        public RewardMap(int width, int height, int numberOfRewards, string rewardFilePath, string gaussFilePath, string landscape_rewardFile)
        {
            this.Width = width;
            this.Height = height;
            this.NumberOfRewards = numberOfRewards;
            this.RewardFilePath = rewardFilePath;
            this.GaussFilePath = gaussFilePath;
            this.Landscape_rewardFile = landscape_rewardFile;
        }

        public void Initialize(){
            if (this.Width * Height < NumberOfRewards){
                Console.WriteLine("Number of rewards must be less than the total number of cells.");
                return;
            }
            Random random = new Random();
            //List<int> rewardCellIndices = GenerateRandomRewardIndices(Width, Height, NumberOfRewards, random);
            List<int> rewardCellIndices = GenerateRandomRewards(Width, Height, NumberOfRewards, random);
            int[,] rewardMap = new int[Width, Height];

            foreach (int index in rewardCellIndices){
                int x = index % Width;
                int y = index / Width;
                rewardMap[x, y] = 1;
            }
            // write to csv
            WriteRewardMapToCSV(rewardMap, this.RewardFilePath);
            WriteRewardMapToCSV(rewardMap, this.GaussFilePath);
            WriteRewardMapToCSV(rewardMap, this.Landscape_rewardFile);
        }
        private static List<int> GenerateRandomRewards(int width, int height, int numberOfRewards, Random random){
            List<int> rewardCellIndices = new List<int>();
            for (int i = 0; i < numberOfRewards; i++){
                int x = random.Next(width);
                int y = random.Next(height);
                int index = x + y * width;
                rewardCellIndices.Add(index);
                //Console.WriteLine(index);
            }
            return rewardCellIndices;
        }
        private static List<int> GenerateRandomRewardIndices(int width, int height, int numberOfRewards, Random random){
            List<int> rewardCellIndices = new List<int>();

            int edgeBuffer = 3; // buffer for edge sapce
            for (int i = 0; i < numberOfRewards; i++){
                bool onEdge = random.NextDouble() < 0.9; // probability on edge

                int x, y;
                if (onEdge){
                    // generate on edge
                    int side = random.Next(4); 
                    switch (side){
                        case 0: // top 
                            x = random.Next(width);
                            y = random.Next(edgeBuffer);
                            break;
                        case 1: // right 
                            x = width - 1 - random.Next(edgeBuffer);
                            y = random.Next(height);
                            break;
                        case 2: // bottom 
                            x = random.Next(width);
                            y = height - 1 - random.Next(edgeBuffer);
                            break;
                        case 3: // left 
                            x = random.Next(edgeBuffer);
                            y = random.Next(height);
                            break;
                        default:
                            throw new InvalidOperationException("Invalid side value");
                    } // switch
                } else {
                    // generate anywhere
                    x = random.Next(width);
                    y = random.Next(height);
                } // if onEdge
                int index = x + y * width;
                rewardCellIndices.Add(index);
                //Console.WriteLine(index);
            }
            return rewardCellIndices;
        }

        private static void WriteRewardMapToCSV(int[,] rewardMap, string filePath){
            using (StreamWriter writer = new StreamWriter(filePath)){
                for (int y = 0; y < rewardMap.GetLength(1); y++){
                    for (int x = 0; x < rewardMap.GetLength(0); x++){
                        writer.Write(rewardMap[y, x]);
                        if (x < rewardMap.GetLength(0) - 1){ // make sure not to end with comma for each row
                            writer.Write(",");
                        }
                    } // for x
                    writer.WriteLine();
                } // for y
            } // using
        } // WriteRewardMapToCSV

        /*------------------------- Print out csv file -----------------------------*/
        public void PrintRewardMap(){
            int[,] rewardMap = ReadRewardMapFromCSV(this.RewardFilePath);

            for(int y = 0; y < rewardMap.GetLength(1); y++){
                for (int x = 0; x < rewardMap.GetLength(0); x++){
                    Console.Write(rewardMap[x, y]);
                    Console.Write(",");
                } // for x
                Console.WriteLine();
            } // for y
        } // PrintRewardMap
        private static int[,] ReadRewardMapFromCSV(string filePath){
            string[] lines = File.ReadAllLines(filePath);
            int width = lines[0].Split(',').Length;
            int height = lines.Length;

            int[,] rewardMap = new int[width, height];
            for (int y = 0; y < height; y++){
                string[] values = lines[y].Split(',');

                for (int x = 0; x < width-1; x++){
                    if (int.TryParse(values[x], out int parsedValue)){
                        rewardMap[x, y] = parsedValue;
                    } else {
                        Console.WriteLine($"Error parsing value at ({x}, {y}). Using default value 0.");
                        rewardMap[x, y] = 0; // default
                    } // if else
                } // for x
            }
             // for y
            return rewardMap;
        } // ReadRewardMapFromCSV

    } // class
} // namespace
