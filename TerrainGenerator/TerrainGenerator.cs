using System;
using System.IO;
using System.Text;

namespace TerrainGenerator
{
    public class TerrainGenerator
    {
        public static void Main(string[] args)
        {
            ElevationLandscape landscape = new ElevationLandscape(50, 50, 30, 1000, 9);
            landscape.Initialize();
            landscape.printMap();

            // Made specifically to run from Examples/HillClimberABMExample/Program.cs
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string txtFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscapeInvert.txt");
            string csvFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "grid.csv");
            string guassFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "moatGauss.csv");
            string landscapeFile = Path.Combine(baseDirectory, "..", "..", "..", "layers", "landscape.csv");

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
            //landscape.TxtToCsv(txtFile,guassFile);
            landscape.TxtToCsv(txtFile,landscapeFile);
            Console.WriteLine("Wrote to terrain file.");
        }
    }
}
