using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainGenerator
{
    abstract class Landscape
    {
        protected int width = 100;
        protected int height = 100;

        public int[,] map;

        public Landscape(int width, int height)
        {
            this.width = width;
            this.height = height;

            map = new int[this.height,this.width];
        }

        public abstract void Initialize();

        public void printMap()
        {
            for (int row = 0; row < this.map.GetLength(0); row++)
            {
                for (int col = 0; col < this.map.GetLength(1); col++)
                    Console.Write(String.Format("{0}\t", this.map[row, col]));
                Console.WriteLine();
            }
        }


        public int Width { get=> width; set=> width = value;  }
        public int Height { get=> height; set => height = value; }


    }
}
