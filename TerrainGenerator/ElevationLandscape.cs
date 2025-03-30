using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq.Expressions;

namespace TerrainGenerator
{
    class ElevationLandscape : Landscape
    {
        int numberOfPeaks;
        int maximumElevation;
        int smoothingLevel;
        int whichTerrain;
        int blocker;

        public ElevationLandscape(int width, int height, int numberOfPeaks, int maximumElevation, int smothingLevel, int whichTerrain) : base(width, height)
        {
            this.numberOfPeaks = numberOfPeaks;
            this.maximumElevation = maximumElevation;
            this.smoothingLevel = smothingLevel;
            this.whichTerrain = whichTerrain;
            blocker = -31;
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
            switch (this.whichTerrain){
                case 1:
                    createPeaks1(peakCells, random); // 1. Normal peaks
                    break;
                case 2:
                    createPeaks2(peakCells, random); // 2. Inverted on Creation
                    break;
                case 3:
                    createPeaks3(peakCells, random); // 3. Inverted after Creation
                    break;
                case 4:
                    createPeaks4(peakCells, random); // 4. Only create Peaks around Perimeter
                    break;
                case 5:
                    this.maximumElevation = 2000;
                    createPeaks5(peakCells, random); // 5. Hill with blocker
                    break;
                case 6:
                    this.maximumElevation = 2000;
                    createPeaks6(peakCells, random); // 6. Canyon
                    break;
                case 7: 
                    this.maximumElevation = 2000;
                    createPeaks7(peakCells, random); // 7. Hill with perimeter opening
                    break;
                case 8:
                    createPeaks8(peakCells, random); // 8. Terrain going top left to bottom right
                    break;
                case 9:
                    createPeaks9(peakCells, random); // 9. Fractal    
                    break;
                case 10:
                    createPeaks10(peakCells, random); // 10. Inverted Perimeter Opening
                    break;
                case 11:
                    createPeaks11(peakCells, random); // 11. Gradient like mountain
                    break;
                default:
                    Console.WriteLine("Invalid option selected. Defaulting to 1");
                    createPeaks1(peakCells, random);
                    break;
            }
        }
        /**
         * ===================================================================================================================================== 
         * ===================================================== Different landscape peaks ===================================================== 
         * =====================================================================================================================================
         */

         // ========================================================= Normal Peaks ========================================================= 
        private void createPeaks1(List<int> peakCells, Random random)
        {
            int elevation = 0;
            for(int i = 0; i < this.numberOfPeaks; i++)
            {
                elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                while (elevation == maximumElevation){
                    elevation = random.Next(this.maximumElevation / 2) + this.maximumElevation / 2;
                }
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
        private void createPeaks4(List<int> peakCells, Random random){
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
        // ===================================================== Hill with blocker ===================================================== 
        private void createPeaks5(List<int> peakCells, Random random)
        {
            int middleX = this.map.GetLength(0) / 2;
            int middleY = this.map.GetLength(1) / 2;
            int hillPeak = this.maximumElevation - 600;
            this.map[middleX, middleY] = hillPeak;

            int radius = this.map.GetLength(0) / 2;
            // hill
            for (int i = 0; i < this.map.GetLength(0); i++)
            {
                for (int j = 0; j < this.map.GetLength(1); j++)
                {
                    double distance = Math.Sqrt((i - middleX) * (i - middleX) + (j - middleY) * (j - middleY));
                    if (distance <= radius)
                    {
                        int fadeElevation = (int)Math.Round(hillPeak * (1.0 - distance / radius));
                        this.map[i, j] = fadeElevation;
                    }
                }
            }

            int arrowTipX = middleX + random.Next(-this.map.GetLength(0) / 4, this.map.GetLength(0) / 4);
            int arrowTipY = middleY + random.Next(-this.map.GetLength(1) / 4, this.map.GetLength(1) / 4);
            this.map[arrowTipX, arrowTipY] = this.blocker;
            // direction vector from the arrow tip to the center.
            int directionX = middleX - arrowTipX;
            int directionY = middleY - arrowTipY;

            // Normalize the direction vector.
            double magnitude = Math.Sqrt(directionX*directionX + directionY*directionY);
            directionX = (int)Math.Round(directionX / magnitude);
            directionY = (int)Math.Round(directionY / magnitude);

            //arrow values on the map.
            this.map[arrowTipX + directionX, arrowTipY + directionY] = this.blocker;
            this.map[arrowTipX + 2 * directionX, arrowTipY + 2 * directionY] = this.blocker;
            this.map[arrowTipX + 3 * directionX, arrowTipY + 3 * directionY] = this.blocker;

            this.map[arrowTipX - directionY, arrowTipY + directionX] = this.blocker;
            this.map[arrowTipX - 2 * directionY, arrowTipY + 2 * directionX] = this.blocker;
            this.map[arrowTipX - 3 * directionY, arrowTipY + 3 * directionX] = this.blocker;
        }
        // ===================================================== canyon v1 ==================================================
        private void createPeaks6(List<int> peakCells, Random random)
        {
            // 1. Set everything to maximum elevation.
            for (int row = 0; row < this.map.GetLength(0); row++)
            {
                for (int col = 0; col < this.map.GetLength(1); col++)
                {
                    this.map[row, col] = this.blocker;
                }
            }

            // 2. [initialize] Set canyon width and select random edge
            // 3. [initialize] select random spot on edge for first cliff and random +- for second cliff
            // 4. [loop] add to width -2 // -1 // 0 // 1 // 2, get random point next to connected cliff start then start1= -width/2, end1 = width/2

            // 2. [initialize]
            int width = random.Next(this.map.GetLength(0) / 2);
            int side = random.Next(4);
            int canyonElevation = 0;

            // 3. [initialize]
            int firstCliff = random.Next(width, this.map.GetLength(0)) - width;

            // Ensure width is at least 1
            width = Math.Max(1, width);

            switch (side)
            {
                case 0: // Top
                    for (int i = 0; i < this.map.GetLength(1); i++)
                    {
                        width = width + random.Next(-1, 2);
                        if (width == 0) { width = 1; }
                        firstCliff = firstCliff + random.Next(-Math.Abs(width) + 1, Math.Abs(width) - 1);
                        if (firstCliff < 0) { firstCliff = 0; }
                        for (int conn = 0; conn < width; conn++)
                        {
                            if (firstCliff + conn < this.map.GetLength(1))
                            {
                                this.map[i, firstCliff + conn] = canyonElevation;
                            }
                        }
                        Console.WriteLine(canyonElevation);
                        canyonElevation += 10;
                    }
                    break;
                case 1: // Bottom 
                    for (int i = 0; i < this.map.GetLength(1); i++)
                    {
                        width = width + random.Next(-1, 2);
                        if (width == 0) { width = 1; }
                        firstCliff = firstCliff + random.Next(-Math.Abs(width), Math.Abs(width));
                        if (firstCliff < 0) { firstCliff = 0; }
                        for (int conn = 0; conn < width; conn++)
                        {
                            if (firstCliff + conn < this.map.GetLength(1))
                            {
                                this.map[this.map.GetLength(0) - i - 1, firstCliff + conn] = canyonElevation;
                            }
                        }
                        Console.WriteLine(canyonElevation);
                        canyonElevation += 10;
                    }
                    break;
                case 2: // Left 
                    for (int i = 0; i < this.map.GetLength(0); i++)
                    {
                        width = width + random.Next(-1, 2);
                        if (width == 0) { width = 1; }
                        firstCliff = firstCliff + random.Next(-Math.Abs(width), Math.Abs(width));
                        if (firstCliff < 0) { firstCliff = 0; }
                        for (int conn = 0; conn < width; conn++)
                        {
                            if (firstCliff + conn < this.map.GetLength(0))
                            {
                                this.map[firstCliff + conn, i] = canyonElevation;
                            }
                        }
                        Console.WriteLine(canyonElevation);
                        canyonElevation += 10;
                    }
                    break;
                case 3: // Right
                    for (int i = 0; i < this.map.GetLength(0); i++)
                    {
                        width = width + random.Next(-1, 2);
                        if (width == 0) { width = 1; }
                        firstCliff = firstCliff + random.Next(-Math.Abs(width), Math.Abs(width));
                        if (firstCliff < 0) { firstCliff = 0; }
                        for (int conn = 0; conn < width; conn++)
                        {
                            if (firstCliff + conn < this.map.GetLength(0))
                            {
                                this.map[conn + firstCliff, this.map.GetLength(0) - i] = canyonElevation;
                            }
                        }
                        Console.WriteLine(canyonElevation);
                        canyonElevation += 10;
                    }
                    break;
            }
        }
        // ===================================================== Hill with perimeter opening ===================================================== 
        private void createPeaks7(List<int> peakCells, Random random)
        {
            int middleX = this.map.GetLength(0) / 2;
            int middleY = this.map.GetLength(1) / 2;
            int hillPeak = this.maximumElevation - 600;
            int hillRadius = this.map.GetLength(0) / 2; // Hill radius
            int ringInnerRadius = hillRadius; // Inner radius of the ring
            int ringOuterRadius = hillRadius + 1; // Outer radius of the ring (initially same as hill radius)
            int gapWidth = 1; // Initial gap width

            // Create the hill
            for (int i = 0; i < this.map.GetLength(0); i++)
            {
                for (int j = 0; j < this.map.GetLength(1); j++)
                {
                    double distance = Math.Sqrt((i - middleX) * (i - middleX) + (j - middleY) * (j - middleY));
                    if (distance <= hillRadius)
                    {
                        int fadeElevation = (int)Math.Round(hillPeak * (1.0 - distance / hillRadius));
                        this.map[i, j] = fadeElevation;
                    }
                }
            }

            // Decrease the ring radius and set the gap width
            ringInnerRadius = (int) (hillRadius / 1.8); // Smaller inner radius of the ring
            ringOuterRadius = ringInnerRadius + 2; // Outer radius of the ring
            gapWidth = 1; // Smaller gap width

            // Create the circular ring on top of the hill with a smaller gap
            for (int i = 0; i < this.map.GetLength(0); i++)
            {
                for (int j = 0; j < this.map.GetLength(1); j++)
                {
                    double distance = Math.Sqrt((i - middleX) * (i - middleX) + (j - middleY) * (j - middleY));
                    if (distance > ringInnerRadius && distance <= ringOuterRadius) // Check if the point lies within the ring
                    {
                        // Ensure there's a gap in the ring
                        double angle = Math.Atan2(j - middleY, i - middleX);
                        double angleDegrees = angle * (180 / Math.PI);
                        if (angleDegrees < 45 - gapWidth || angleDegrees > 75 + gapWidth)
                        {
                            this.map[i, j] = this.blocker; // Set the elevation to maximum for the ring
                        }
                    }
                }
            }
        }

        // Terrain that will be going from top left to bottom right
        private void createPeaks8(List<int> peakCells, Random random) {
            for (int i = 0; i < this.map.GetLength(0); i++) {

                for (int j = 0; j < this.map.GetLength(1); j++) {
                    
            // Calculate the normalized distance based on the column index
            double normalizedDistance = (double) j / (this.map.GetLength(1) - 1);

            // Set the elevation based on the normalized distance
            this.map[i, j] = (int)(normalizedDistance * this.maximumElevation);
         }
            }
        }

        // ===================================================== Fractal Terrain ====================================================== 
        //TODO: need to edit this   
        private void createPeaks9(List<int> peakCells, Random random) {
            int sideLength = this.map.GetLength(0);
            
            // call method to be able to get corner points to start the recursive method
            CornerPoints(sideLength, 0.5, random);
        }

        private void CornerPoints(int size, double changeVal, Random random) {
            // get the 4 corners points of the array and set random value for it
            this.map[0, 0] = random.Next(this.maximumElevation);
            this.map[0, size - 1] = random.Next(this.maximumElevation);
            this.map[size - 1, 0] = random.Next(this.maximumElevation);
            this.map[size - 1, size - 1] = random.Next(this.maximumElevation);

            DiamondSquareRecursive(0, 0, size - 1, size - 1, changeVal, random); // call the recursive method using the whole 2d array
        }

        private void DiamondSquareRecursive(int x1, int y1, int x2, int y2, double changeVal, Random random) {
            // base case
            if (x2 - x1 < 2 || y2 - y1 < 2) return;

            // get midponts of x y
            int midX = (x1 + x2) / 2;
            int midY = (y1 + y2) / 2;


            // Ensure the center point is within the map boundaries
            if (midX < 0 || midX >= this.map.GetLength(0) || midY < 0 || midY >= this.map.GetLength(1)) return;

            // gete random offset
            double offset1 = (random.NextDouble() - 0.5) * changeVal * this.maximumElevation;
            double offset2 =(random.NextDouble() - 0.5) * changeVal * this.maximumElevation;

            // Get corner values
            double centerValue = (this.map[x1, y1] + this.map[x1, y2] + this.map[x2, y1] + this.map[x2, y2]) / 4.0 + offset1;
            this.map[midX, midY] = (int)(centerValue < 0 ? 0 : centerValue);

            // Square step: calculate the side point values if < then 0 return 0
            double sideValue1 = (this.map[x1, y1] + this.map[x1, y2] + this.map[midX, midY])/ 3.0 + offset2;
            this.map[x1, midY] = (int)(sideValue1 < 0 ? 0 : sideValue1);

            double sideValue2 = (this.map[x1, y2] + this.map[x2, y2] + this.map[midX, midY]) / 3.0 + offset2;
             this.map[x2, midY] = (int)(sideValue2 < 0 ? 0 : sideValue2);

            double sideValue3 = (this.map[x1, y1] + this.map[x2, y1] + this.map[midX, midY]) / 3.0 + offset2;
            this.map[midX, y1] = (int)(sideValue3 < 0 ? 0 : sideValue3);

    
             double sideValue4 = (this.map[x1, y2] + this.map[x2, y2] + this.map[midX, midY]) / 3.0 + offset2;
             this.map[midX, y2] = (int)(sideValue4 < 0 ? 0 : sideValue4);

            // recall on 4 regions
            DiamondSquareRecursive(x1, y1, midX, midY, changeVal, random);
            DiamondSquareRecursive(midX, y1, x2, midY, changeVal, random);
            DiamondSquareRecursive(x1, midY, midX, y2, changeVal, random);
            DiamondSquareRecursive(midX, midY, x2, y2, changeVal, random);
        }

    // ===================================================== Inverted Perimeter Opening ===================================================== 
        private void createPeaks10(List<int> peakCells, Random random) {
            int middleX = this.map.GetLength(0) / 2;
            int middleY = this.map.GetLength(1) / 2;
            
            // height values
            int baseHeight = 500;  // max height
            int vertexBottom = 100;  // vertex y point
            double slopeRadius = this.map.GetLength(0) / 2.0;  // terrain radius
            int wallBaseHeight = 100;  // height base
            int wallHeight = 400;  // height of wall
            
            // Create base inverted hill
            for (int i = 0; i < this.map.GetLength(0); i++) {
                for (int j = 0; j < this.map.GetLength(1); j++) {
                    double distance = Math.Sqrt((i - middleX) * (i - middleX) + (j - middleY) * (j - middleY));
                    
                    if (distance <= slopeRadius) {
                        //linear slope to vertex
                        double normalizedDist = distance / slopeRadius;
                        int elevation = (int)(baseHeight - (baseHeight - vertexBottom) * (1 - normalizedDist));
                        this.map[i, j] = elevation;
                    }
                    else {
                        this.map[i, j] = baseHeight;
                    }
                }
            }

            int hillRadius = this.map.GetLength(0) / 2;
            int ringInnerRadius = (int)(hillRadius / 1.8); // inner radius of ring
            int ringOuterRadius = ringInnerRadius + 2; //outer radius of ring
            int gapWidth = 1; // Smaller gap width

            // Create the circular ring with a smaller gap
            for (int i = 0; i < this.map.GetLength(0); i++) {
                for (int j = 0; j < this.map.GetLength(1); j++) {
                    double distance = Math.Sqrt((i - middleX) * (i - middleX) + (j - middleY) * (j - middleY));
                    if (distance > ringInnerRadius && distance <= ringOuterRadius) {
                        double angle = Math.Atan2(j - middleY, i - middleX);
                        double angleDegrees = angle * (180 / Math.PI);
                        if (angleDegrees < 45 - gapWidth || angleDegrees > 75 + gapWidth) {
                            this.map[i, j] = wallBaseHeight + wallHeight; // start at base and add wall height
                        }
                    }
                }
            }
        }

    // ===================================================== Gradient like mountain ===================================================== 
    private void createPeaks11(List<int> peakCells, Random random) {
        int middleX = this.map.GetLength(0) / 2;
        int middleY = this.map.GetLength(1) / 2;
        
        // create multiple gradient waves
         for (int i = 0; i < this.map.GetLength(0); i++) {
             for (int j = 0; j < this.map.GetLength(1); j++) {
                // Base elevation use multiple sin waves
                double elevation = 0;
                
                 //main mountain shape
                 double dist = Math.Sqrt(Math.Pow(i - middleX, 2) + Math.Pow(j - middleY, 2));
                 elevation += (this.maximumElevation * 0.8) * Math.Exp(-dist / (this.map.GetLength(0)* 0.3));
                
                // add randomness
                double wave1 = Math.Sin(i * 0.2) * Math.Cos(j * 0.2) * (this.maximumElevation * 0.2);
                 elevation += wave1;
                
                // add little details
                double wave2 = Math.Sin(i * 0.1 + j * 0.1) * (this.maximumElevation * 0.1);
                elevation += wave2;
                
                // add texture
                double noise = random.NextDouble() * (this.maximumElevation * 0.05);
                elevation += noise;
                
                 // make sure elevation is in bounds
                 elevation = Math.Max(0, Math.Min(this.maximumElevation, elevation));
                
                // smooth edges
                double edgeFade = 1.0;
                if (i < 5 || i > this.map.GetLength(0) - 5 || j < 5 || j > this.map.GetLength(1) - 5) {
                    int distFromEdge = Math.Min(
                        Math.Min(i, this.map.GetLength(0) - i),
                        Math.Min(j, this.map.GetLength(1) - j)
                    );
                    edgeFade = distFromEdge / 5.0;
                    elevation *= edgeFade;
                }
                
                this.map[i, j] = (int)elevation;
          }
        }
        
        // apply smoothing to have small changes
        for (int smooth = 0; smooth < 2; smooth++) {
            int[,] tempMap = new int[this.map.GetLength(0), this.map.GetLength(1)];
            for (int i = 1; i < this.map.GetLength(0) - 1; i++) {
                for (int j = 1; j < this.map.GetLength(1) - 1; j++) {
                    // average with neighboring cells
                    double avgElevation = (
                        this.map[i-1, j] + this.map[i+1, j] +
                        this.map[i, j-1] + this.map[i, j+1] +
                        this.map[i, j]
                    ) / 5.0;
                    tempMap[i, j] = (int)avgElevation;
                }
            }
            //copy values back to the main map
            for (int i = 1; i < this.map.GetLength(0) - 1; i++) {
                for (int j = 1; j < this.map.GetLength(1) - 1; j++) {
                    this.map[i, j] = tempMap[i, j];
                }
            }
        }
    }
        
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
