using System;
using System.IO;
using System.Text;

namespace TerrainGenerator
{
    public class TerrainGenerator
    {
        public static void Main(string[] args)
        {
            // Ask user for specific map --
            int userInput = 1;
            do {
                Console.WriteLine("--------------------------------------------------------------------");
                Console.WriteLine(" Select a terrain - ");
                Console.WriteLine("1. Normal peaks");
                Console.WriteLine("2. Inverted on Creation");
                Console.WriteLine("3. Inverted after Creation");
                Console.WriteLine("4. Only create Peaks around Perimeter");
                Console.WriteLine("5. Hill with blocker");
                Console.WriteLine("6. Canyon");
                Console.WriteLine("7. Hill with perimeter opening");
                Console.WriteLine("8. Terrain going top left to bottom right");
                Console.WriteLine("--------------------------------------------------------------------");
                // int?
                // Checks if user input is an INT and is between 1-8
                if (int.TryParse(Console.ReadLine(), out userInput)){
                    // in range
                    if (userInput >= 1 && userInput <= 8){
                        break; // Valid input, exit the loop
                    } else {
                        Console.WriteLine("Invalid input. Please enter a number between 1 and 8.");
                    }
                } else {
                    Console.WriteLine("Invalid input. Please enter a valid number.");
                }
            } while (true);

            ElevationLandscape landscape = new ElevationLandscape(50, 50, 30, 1500, 9, userInput);
            landscape.Initialize();
            landscape.printMap();

            // Made specifically to run from Examples/HillClimberABMExample/Program.cs
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string txtFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscapeInvert.txt");
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "grid.csv");
            string guassFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "moatGauss.csv");
            string landscapeFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape.csv");

            // new TextWriter for writing characters to a stream 
            StreamWriter writer = new StreamWriter(txtFile);
            
            writer.WriteLine(landscape.Width);
            writer.WriteLine(landscape.Height);

            for (int i = 0; i < landscape.map.GetLength(0); i++)
            {
                for (int j = 0; j < landscape.map.GetLength(1); j++)
                    writer.Write(landscape.map[i,j] + " ");
            }
            // Change from txt format to csv format (bandage)
            writer.Close();
            landscape.TxtToCsv(txtFile,csvFile);
            landscape.TxtToCsv(txtFile,guassFile);
            landscape.TxtToCsv(txtFile,landscapeFile);
            Console.WriteLine("Wrote to terrain file.");
        }
    }
}
