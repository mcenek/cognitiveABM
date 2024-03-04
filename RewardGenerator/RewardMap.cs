using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;

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
        private string rewardTest_File;
        private int rewardMapType;
        public RewardMap(int width, int height, int numberOfRewards, string rewardFilePath, string gaussFilePath, string landscape_rewardFile, string rewardTest_File, int rewardMapType)
        {
            this.Width = width;
            this.Height = height;
            this.NumberOfRewards = numberOfRewards;
            this.RewardFilePath = rewardFilePath;
            this.GaussFilePath = gaussFilePath;
            this.Landscape_rewardFile = landscape_rewardFile;
            this.rewardTest_File = rewardTest_File;
            this.rewardMapType = rewardMapType;
        }

        public void Initialize(){
            if (this.Width * Height < NumberOfRewards){
                Console.WriteLine("Number of rewards must be less than the total number of cells.");
                return;
            }
            Random random = new Random();
            List<int> rewardCellIndices;
            bool rand = true;
            switch (this.rewardMapType){
                case 1:
                    rewardCellIndices = GenerateRandomRewards(Width, Height, NumberOfRewards, random);
                    break;
                case 2:
                    rewardCellIndices = GenerateRewardMiddle(Width, Height, NumberOfRewards, random, 5); // current radius 1/10th of width
                    rand = false;
                    break;
                default:
                    Console.WriteLine("Invalid option selected. Defaulting reward to 1");
                    rewardCellIndices = GenerateRandomRewards(Width, Height, NumberOfRewards, random);
                    break;
            }

            int[,] rewardMap = new int[Width, Height];
            if(rand == true){
                foreach (int index in rewardCellIndices){
                    int x = index % Width;
                    int y = index / Width;
                    rewardMap[x, y] = 1;
                }
            } else {
                for (int i = 0; i < rewardCellIndices.Count; i += 2)
                {
                    int x = rewardCellIndices[i];
                    int y = rewardCellIndices[i + 1];
                    Console.WriteLine("X:" +x+" Y:"+y);
                    rewardMap[x, y] = 1;
                }
            }
            // write to csv
            WriteRewardMapToCSV(rewardMap, this.RewardFilePath);
            WriteRewardMapToCSV(rewardMap, this.GaussFilePath);
            WriteRewardMapToCSV(rewardMap, this.Landscape_rewardFile);
            WriteRewardMapToCSV(rewardMap, this.rewardTest_File);
        }
        private static List<int> GenerateRandomRewards(int width, int height, int numberOfRewards, Random random){
            List<int> rewardCellIndices = new List<int>();
            for (int i = 0; i < numberOfRewards; i++){
                int x = random.Next(width);
                int y = random.Next(height);
                int index = x + y*width;
                rewardCellIndices.Add(index);
                //Console.WriteLine(index);
            }
            return rewardCellIndices;
        }
        private static List<int> GenerateRewardMiddle(int width, int height, int numberOfRewards, Random random, int radius){
            int centerX = (int)(width/2);
            int centerY = (int)(height/2);
            List<int> rewardCellIndices = new List<int>();

            for (int i = 0; i < numberOfRewards; i++)
            {
                int x, y;
                x = random.Next(centerX-radius,centerX+radius);
                y = random.Next(centerY-radius,centerY+radius);
                rewardCellIndices.Add(x);
                rewardCellIndices.Add(y);
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
