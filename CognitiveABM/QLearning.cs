using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;


namespace CognitiveABM.QLearning
{

    public class QLearning
    {
        //for simple version, only using these inst vars
        private float[,] qMap = new float[4,4];//Houses qMap used in MSE
        private List<float[,]> prototypes = new List<float[,]>(); //prototype list used for MSE
        public static List<float> fitness;
        public static List<int> animalIDHolder;
        public static Dictionary<int, List<float[]>> patchDict;
        public static Dictionary<int, List<float>> fitDict;
        public static Dictionary<int, List<(int,int)>> agentQmapPath;
        public static int usePerfectQMap = 1;

        //--------------------------------//
        /**
         * For now, all qMap will be of four different angles:
         * 45, 90, 135, and 180 degrees. Values are either .7,.125, or .05
         * All landscapes will have one of these angles
         */
        //--------------------------------//


        //constructor that sets the qMap and prototypes
        public QLearning(){
          setQlearnMap();
          setPrototype();
          fitness = new List<float>();
          animalIDHolder = new List<int>();
          patchDict = new Dictionary<int, List<float[]>>();
          patchDict.Add(-1, new List<float[]>());
          fitDict = new Dictionary<int, List<float>>();
          fitDict.Add(-1, new List<float>());
          agentQmapPath = new Dictionary<int, List<(int,int)>>();
          agentQmapPath.Add(-1, new List<(int,int)>());

        }

        //---SETTERS---//
        //Sets the QlearnMap
        //can be way more efficent, but this is just a temp job
        public void setQlearnMap(){
          //var filePath = @"..\HillClimberABMExample\layers\LandScapeSlopeHard.csv";
          float[,] data = new float[4,4]; //4x4 qmap matrix hard coded
          string qMapFile = @"..\HillClimberABMExample\layers\qMapPerfect.csv";
          if(usePerfectQMap == 0){
            qMapFile = @"..\HillClimberABMExample\layers\qMapGenerated.csv";
          }
          using(var reader = new StreamReader(qMapFile))
         {
             int counter = 0;
             while (!reader.EndOfStream)
             {
                 var line = reader.ReadLine();
                 var values = line.Split(',');
                 if(counter < 4){
                 data[counter,0] = float.Parse(values[0]);
                 data[counter,1] = float.Parse(values[1]);
                 data[counter,2] = float.Parse(values[2]);
                 data[counter,3] = float.Parse(values[3]);
                 // data[counter,4] = float.Parse(values[4]);
                 // data[counter,5] = float.Parse(values[5]);
                 // data[counter,6] = float.Parse(values[6]);
                 // data[counter,7] = float.Parse(values[7]);
                 counter++;
               }
             }
         }

          this.qMap = data;
        }

        //hard codes the prototypes in
        //bad code that could be done way better but im just going for working right now
        public void setPrototype(){
          float[,] protoN = new float[,] {{1f,1f,1f},{.5f,.5f,.5f},{0f,0f,0f}};
          float[,] protoE = new float[,] {{0f,.5f,1f},{0f,.5f,1f},{0f,.5f,1f}};
          float[,] protoS = new float[,] {{0f,0f,0f},{.5f,.5f,.5f},{1f,1f,1f}};
          float[,] protoW = new float[,] {{1f,.5f,0f},{1f,.5f,0f},{1f,.5f,0f}};

          this.prototypes.Add(protoN);
          this.prototypes.Add(protoE);
          this.prototypes.Add(protoS);
          this.prototypes.Add(protoW);

          // float[,] protoNE = new float[,] {{.5f,1f,1f},{0f,.5f,1f},{0f,0f,.5f}};
          // float[,] protoSE = new float[,] {{0f,0f,.5f},{0f,.5f,1f},{.5f,1f,1f}};
          // float[,] protoSW = new float[,] {{.5f,0f,0f},{1f,.5f,0f},{1f,1f,.5f}};
          // float[,] protoNW = new float[,] {{1f,1f,.5f},{1f,.5f,0f},{.5f,0f,0f}};
          //
          // this.prototypes.Add(protoNE);
          // this.prototypes.Add(protoSE);
          // this.prototypes.Add(protoSW);
          // this.prototypes.Add(protoNW);
        }

        //---HELPER FUNCTIONS---//
        /**
         * @param landScapePatch: 3x3 matrix of landscape agent is on
         * @param min: smallest elevation in landscapePatch
         * @param max: largest elevation in landscapePatch
         * @description: finds out direction agent should go using MSE and biasedRouletteWheel
         * @return: value ranging from 1,5,7,3 which represents N. E. S. W.
         */
        public int getDirection(float[,] landscapePatch, float min, float max, int animalId){
          float[,] normallisedLandscapePatch = normalliseLandscapePatch(landscapePatch, min, max);
          float[] MSE = new float[4];
          for(int i = 0; i < this.prototypes.Count; i++){
            MSE[i] = meanSquareError(normallisedLandscapePatch, prototypes.ElementAt(i));
          }
          //returns index of smallest value (Grabbed from: https://stackoverflow.com/questions/4204169/how-would-you-get-the-index-of-the-lowest-value-in-an-int-array)
          int minIndex = Enumerable.Range(0, MSE.Length).Aggregate((a, b) => (MSE[a] < MSE[b]) ? a : b);

          int direction = biasedRouletteWheel(minIndex);

          recordPath(animalId, direction, minIndex);

          //direction gives 0-3. The list of locations in animal.cs contains 0-8.
          //so, we need to change direction to work on a list
          //Direction will change via 0=>1, 1=>5, 2=>7, 3=>3
          int[] directionMap = {1,5,7,3};
          // int[] directionMap = {1,5,7,3,2,8,6,0};
          return directionMap[direction];
        }//end getDirection

        /**
         * @param landScapePatch: 3x3 matrix of landscape agent is on
         * @param min: smallest elevation in landscapePatch
         * @param max: largest elevation in landscapePatch
         * @description: normallises the landscapePatch
         * @return: normallised landscapePatch
         */
        public float[,] normalliseLandscapePatch(float[,] landscapePatch, float min, float max){
              float[,] normallizedLandscape = new float[landscapePatch.GetLength(0), landscapePatch.GetLength(1)];
              for (int row = 0; row < landscapePatch.GetLength(0); row += 1)
              {
                  for (int col = 0; col < landscapePatch.GetLength(1); col += 1)
                  {
                      normallizedLandscape[row, col] = (landscapePatch[row, col] - min) / (Math.Abs(max - min));
                  }
              }

              //System.Environment.Exit(0);
              return normallizedLandscape;
        }//end normalliseLandscapePatch

         /**
          * @param col: collumn of qMap matrix that will be looked at
          * @description: biasly chooses a direction to go based off of qMap
          * @return: index of row of qMap which represents direction
          */
        public int biasedRouletteWheel(int col){
          var random = new Random();//seed for random is just 18 so we can consitantly get the same result
          float rFloat = (float)random.NextDouble();
          float addedVal = 0.0f;
          //we add all values of the col together, when > rDouble, we choose last column
          for(int i = 0; i < 4; i++){
            addedVal += qMap[i,col];
            if(addedVal >= rFloat){
              return i;
            }
          }//end for
              return -1; //return -1 so we know that it's this method that causes an error later down the road
        }//end rouletteWheel

        /**
         * @param landScapePatch: 3x3 matrix of landscape agent is on
         * @param prototype: a 3x3 matrix of elevations for a direction of N. E. S. W.
         * @description: finds the mean square error of the two parameters
         * @return: the mean square error
         */
        public float meanSquareError(float[,] landScapePatch, float[,] prototype){
          float sum = 0.0f;
          int size = landScapePatch.Length + prototype.Length;
          for(int row = 0; row < landScapePatch.GetLength(0); row++){
            for(int col = 0; col < landScapePatch.GetLength(1); col++){
              sum += (float)Math.Pow(prototype[row,col] - landScapePatch[row,col],2);
            }//end col
          }//end row
          return sum/size;
        }//end MSE

        /**
         * @param landScapePatch: array of current landScape
         * @param animalId: id of current animal
         * @param tickNum: tick number
         * @param currentEle: current elevation
         * @param x: x value of position
         * @param y: y value of position
         * @description: puts needed exported values into patchDict inst var
         */
        public void setExportValues(float[,] landScapePatch, int animalId, int tickNum, int currentEle, int x, int y){
          //spots 0,1,11,12,13,16,17 are reserved values
          float[] temp = new float[18];
          temp[0] = (float)animalId;
          temp[1] = (float)tickNum;
          temp[11] = (float)currentEle;
          temp[16] = (float)x;
          temp[17] = (float)y;

          //put landscape matrix in array form
          int counter = 2;
          for(int row = 0; row < landScapePatch.GetLength(0); row++){
            for(int col = 0; col < landScapePatch.GetLength(1); col++){
              temp[counter] = landScapePatch[row,col];
              counter++;
            }
          }


          if(!animalIDHolder.Contains(animalId)){
            animalIDHolder.Add(animalId);
          }

          List<float[]> tempList = new List<float[]>();
          //if animal has yet to be seen
          if(patchDict.ContainsKey(animalId) == null || !patchDict.ContainsKey(animalId)){
            temp[12] = 0.0f;
            temp[13] = temp[14] = temp[15]= 0.0f;
            tempList.Add(temp);
            patchDict.Add(animalId, tempList);
          }

          else{
            tempList = patchDict[animalId];
            temp[12] = (tempList.Last())[11];
            //temp[13] = Math.Abs((float)currentEle - temp[12]);
            temp[13] = currentEle - temp[12];
            float[] avgMax = getAverageandTotal(tempList, temp[13]);
            temp[14] = avgMax[0];
            temp[15] = avgMax[1];
            fitness.Add(temp[13]);
            tempList.Add(temp);
            patchDict[animalId] = tempList;
          }
        }//end exportValues

        /**
         * @param tempList: List of float arrays containing previous animal info
         * @param currentFit: current fitness gained value from this animal
         * @return: the average and total of all fitness gains of animal
         * @description: calculates the average and total fitness of an animal at current moment in time
         */
        public float[] getAverageandTotal(List<float[]> tempList, float currentFit){
          int counter;
          float avgCurrentFit;
          float[] returnVals = {0.0f, 0.0f};

          if(currentFit > 0){
            avgCurrentFit = currentFit;
            counter = 1;
          }
          else{
            avgCurrentFit = 0.0f;
            counter = 0;
          }

          foreach(float[] array in tempList){
            if(array[13] > 0){
              avgCurrentFit += array[13];
              currentFit += array[13];
              counter++;
            }
          }

          if(counter != 0){
            returnVals[0] = avgCurrentFit/counter;
            returnVals[1] = currentFit;
          }
          return returnVals;
        }//end getAverage

        /**
         * @param animalId: id of animal agent
         * @param row: row of qmap chosen
         * @param col: col of qmap chosen
         * @description: Records and saves the pathway and agent travelled
         */
        public void recordPath(int animalId, int row, int col){
          List<(int,int)> tempList = new List<(int,int)>();
          if(agentQmapPath.ContainsKey(animalId) == null || !agentQmapPath.ContainsKey(animalId)){
            tempList.Add((row,col));
            agentQmapPath.Add(animalId, tempList);
          }

          else{
            tempList = agentQmapPath[animalId];
            tempList.Add((row,col));
            agentQmapPath[animalId] = tempList;
          }
        }//end recordPath

    }
}
