using System;
using System.IO;
using System.Text;

namespace TerrainGenerator
{
    class TerrainGenerator
    {
        static void Main(string[] args)
        {
            ElevationLandscape landscape = new ElevationLandscape(50, 50, 30, 1000, 9);
            landscape.Initialize();
            landscape.printMap();

            string txtFile = "./landscapeInvert.txt";
            string csvFile = "../Examples/HillClimberABMExample/layers/landscapeInvert.csv";

            //StreamWriter writer = new StreamWriter("..//..//..//landscape.txt");
            StreamWriter writer = new StreamWriter(txtFile);
            //StreamWriter writer = new StreamWriter("../Examples/HillClimberABMExample/layers/landscape.csv");
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
            
            Console.WriteLine("Wrote to terrain file.");
        }
    }
}
