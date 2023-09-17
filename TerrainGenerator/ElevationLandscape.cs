﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TerrainGenerator
{
    class ElevationLandscape : Landscape
    {
        int numberOfPeaks;
        int maximumElevation;
        int smoothingLevel;

        public ElevationLandscape(int width, int height, int numberOfPeaks, int maximumElevation, int smothingLevel) : base(width, height)
        {
            this.numberOfPeaks = numberOfPeaks;
            this.maximumElevation = maximumElevation;
            this.smoothingLevel = smothingLevel;
        }

        public override void Initialize()
        {
            if(Width * Height < numberOfPeaks)
            {
                Console.WriteLine("Number of peaks must be less than total number of cells.");
                return;
            }

            Random random = new Random();

            List<int> peakCells = new List<int>();

            while(peakCells.Count != this.numberOfPeaks)
            {
                int randomValue = random.Next(Width * Height);

                if (!peakCells.Contains(randomValue))
                    peakCells.Add(randomValue);
            }

            createPeaks(peakCells, random);
        }

        private void createPeaks(List<int> peakCells, Random random)
        {
            int elevation = 0;
            for(int i = 0; i < this.numberOfPeaks; i++)
            {
                elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                elevation = elevation*-1;
                this.map[peakCells[i] / this.map.GetLength(1), peakCells[i] % this.map.GetLength(0)] = elevation;
                /**
                 * Invert terrain, flip x, and y cords for peaks by map-x,map-y
                 */
                //this.map[this.map.GetLength(1) - (peakCells[i] / this.map.GetLength(1)), this.map.GetLength(0) - (peakCells[i] % this.map.GetLength(0))] = elevation;
            }

            for(int i = 0; i < this.smoothingLevel; i++)
            {
                diffuseElevation(peakCells, random);
            }
        }

        private void diffuseElevation(List<int> peakCells, Random random)
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (peakCells.Contains(i*j))
                    {
                        continue;
                    }

                    adjustCell(i, j, random);
                    adjustCell(j, i, random);
                }
            }

            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    if (peakCells.Contains(i * j))
                    {
                        continue;
                    }

                    adjustCell(i, j, random);
                    adjustCell(j, i, random);
                }
            }
        }

        private void adjustCell(int i, int j, Random random)
        {
            int[] adj = getAdjacent(i, j);
            int sum = 0;

            int max = adj[0];

            for (int a = 1; a < adj.Length; a++)
            {

                if (adj[a] == 0)
                {
                    continue;
                }
                if (adj[a] > max)
                {
                    max = adj[a];
                }
            }

            int total = 1;
            for (int k = 0; k < adj.Length; k++)
            {
                if (adj[k] == 0)
                {
                    continue;
                }
                sum = sum + adj[k];
                if (adj[k] > 0)
                {
                    total++;
                }
            }

            double average = sum / total;

            int newElevation = (int)max - (int)(random.NextDouble() * average);

            if (newElevation == 0)
            {
                newElevation = random.Next(this.maximumElevation) / this.smoothingLevel - smoothingLevel;
            }

            this.map[i,j] = newElevation;
        }

        private int[] getAdjacent(int y, int x)
        {
            int[] result = new int[8];
            int index = 0;

            int[,] pad = new int[this.height + 2, this.width + 2];

            for (int i = 0; i < this.map.GetLength(0); i++)
            {
                for (int j = 0; j < this.map.GetLength(1); j++)
                {
                    pad[i + 1,j + 1] = this.map[i,j];
                }
            }

            for (int dx = -1; dx <= 1; ++dx)
            {
                for (int dy = -1; dy <= 1; ++dy)
                {
                    if (dx != 0 || dy != 0)
                    {
                        result[index] = pad[y + dy + 1, x + dx + 1];
                        index++;
                    }
                }
            }
            return result;
        }

        /**
         * Convert txt file created to csv file
         */
        public void TxtToCsv(string inputFilePath, string outputFilePath){
            try
            {
                string[] lines = File.ReadAllLines(inputFilePath);
                // Make sure it got 1. X dimen, 2. Y dimen, 3. values
                if (lines.Length < 3)
                {
                    Console.WriteLine("Input file does not contain enough data.");
                    return;
                }
                // dimensions
                int rows = int.Parse(lines[0]);
                int cols = int.Parse(lines[1]);
                int[,] data = new int[rows, cols];
                string[] values = lines[2].Split(' ');
                int dataIndex = 0;

                for (int i = 0; i < rows; i++){
                    for (int j = 0; j < cols; j++){
                        if (dataIndex < values.Length){
                            data[i, j] = int.Parse(values[dataIndex]);
                            dataIndex++;
                        } // if
                    } // for j
                } // for i

                // --> CSV
                using (StreamWriter writer = new StreamWriter(outputFilePath)){
                    for (int i = 0; i < rows; i++){
                        for (int j = 0; j < cols; j++){
                            writer.Write(data[i, j]);
                            if (j < cols - 1){
                                writer.Write(",");
                            }
                        }
                        writer.WriteLine(); // next row
                    }
                    writer.Close();
                }
            }
            catch (Exception ex){
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        } // TxtToCsv
    }
}
