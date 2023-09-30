using System;
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
        /**
         * ===================================================================================================================================== 
         * ===================================================== Different landscape peaks ===================================================== 
         * =====================================================================================================================================
         */

         // ========================================================= Normal Peaks ========================================================= 
        private void createPeaksOrig(List<int> peakCells, Random random)
        {
            int elevation = 0;
            for(int i = 0; i < this.numberOfPeaks; i++)
            {
                elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                this.map[peakCells[i] / this.map.GetLength(1), peakCells[i] % this.map.GetLength(0)] = elevation;
            }

            for(int i = 0; i < this.smoothingLevel; i++){
                diffuseElevation(peakCells, random);
            }
        }

        // ===================================================== Inverted on creation ===================================================== 
        private void createPeaks2(List<int> peakCells, Random random){
            int elevation = 0;
            for (int i = 0; i < this.numberOfPeaks; i++){
                elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                int cellIndex = peakCells[i];
                int x = cellIndex % this.map.GetLength(0);
                int y = cellIndex / this.map.GetLength(0);

                // invert
                elevation = this.maximumElevation - elevation;
                this.map[y, x] = elevation;
            }

            for (int i = 0; i < this.smoothingLevel; i++){
                diffuseElevation(peakCells, random);
            }
        }
        // ===================================================== Inverted after creation ===================================================== 
        private void createPeaks3(List<int> peakCells, Random random){
            int elevation = 0;

            for (int i = 0; i < this.numberOfPeaks; i++){
                elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                int cellIndex = peakCells[i];
                int y = cellIndex % this.map.GetLength(0);
                int x = cellIndex / this.map.GetLength(1);

                // store value of peaks
                int[,] peak = new int[this.map.GetLength(0), this.map.GetLength(1)];
                peak[y, x] = elevation;
                this.map[y, x] = elevation;
            } // for i 

            for (int i = 0; i < this.smoothingLevel; i++){
                diffuseElevation(peakCells, random);
            } // for i
            invertPeaks();
        }
        private void invertPeaks(){
            for (int y = 0; y < this.map.GetLength(0); y++){
                for (int x = 0; x < this.map.GetLength(1); x++){
                    if (this.map[y, x] > 0){
                        this.map[y, x] = this.maximumElevation - this.map[y, x];
                        // since max - val, peaks are in 900s, must scale down
                        this.map[y, x] = this.map[y, x] - (int)(this.maximumElevation*.93);
                        if(this.map[y,x] < 0){
                            this.map[y,x] = 0;
                        }
                    } // if
                } // for x
            } // for y
        } // invert peaks
        
        // ===================================================== createPerimeterPeaks ===================================================== 
        private void createPeaks(List<int> peakCells, Random random){
            int maxElevation = this.maximumElevation;
            int elevation = 0;

            for (int i = 0; i < this.numberOfPeaks; i++){
                elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                int cellIndex = peakCells[i];
                int y = cellIndex % this.map.GetLength(0);
                int x = cellIndex / this.map.GetLength(1);

                int nearPerimeterAmount = 5; // how close to perimeter?
                bool nearPerimeter = (x < nearPerimeterAmount || y < nearPerimeterAmount || x >= this.map.GetLength(1) - nearPerimeterAmount || y >= this.map.GetLength(0) - nearPerimeterAmount);

                if (nearPerimeter){
                    this.map[y, x] = elevation;
                } else {
                    this.map[y, x] = 0;
                }
            } // for

            for (int i = 0; i < this.smoothingLevel; i++){
                diffuseElevation(peakCells, random);
            }
        } // perimeter peaks

        /**
         * ===================================================================================================================================== 
         * ===================================================================================================================================== 
         * =====================================================================================================================================
         */

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
