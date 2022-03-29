using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;

//using CognitiveABM.ABM;


namespace CognitiveABM.QLearning
{

    public class QLearning
    {
        // private bool logging = true;
        // private List<int[,]> priorLandscapes = new List<int[,]>();//2d array for prior lanscapes
        // private List<float[,]> priorLandscapeResults = new List<float[,]>();
        // private int similarLandscapeExperience;
        // private int direction = 0;
        // private int state = 0;//current position
        // private int minVal, maxVal = 0;//NOT CONSTANT, TAKEN FROM LANDSCAPE VALUES FROM X TO Y

        //for simple version, only using these inst vars
        private float[,] qMap = new float[4,4];//Houses qMap used in MSE
        private List<float[,]> prototypes = new List<float[,]>(); //prototype list used for MSE
        public static List<float> fitness;
        public static List<int> animalIDHolder;
        public static Dictionary<int, List<float[]>> patchDict;
        public static Dictionary<int, List<float>> fitDict;

        //--------------------------------//
        /**
         * For now, all qMap will be of four different angles:
         * 45, 90, 135, and 180 degrees. Values are either .7,.125, or .05
         * All landscapes will have one of these angles
         */
        //--------------------------------//


        // //Will find a similar landscape within 5% similiar. If none is found, or there are no other landscape,
        // //A normallised lanscape of landscape array is added to the list of prior landscapes
        // public int findSimlarLanscapeIndex(int[,] landscapeArray, int minVal, int maxVal)
        // {
        //     //normallise the array;
        //     int[,] normallizedLandscape = new int[landscapeArray.GetLength(0), landscapeArray.GetLength(1)];
        //     for (int row = 0; row < landscapeArray.GetLength(0); row += 1)
        //     {
        //         for (int col = 0; col < landscapeArray.GetLength(1); col += 1)
        //         {
        //             normallizedLandscape[row, col] = (landscapeArray[row, col] - minVal) / (maxVal - minVal);
        //         }
        //     }
        //
        //     //if this is the first array, add it to the experience array and return index 0
        //     if (priorLandscapes.Count == 0)
        //     {
        //         if (logging)
        //             Console.WriteLine("First landscape, adding a new one to the array.");
        //         Console.WriteLine("normallizedLanscape" + normallizedLandscape.GetLength(0));
        //         priorLandscapes.Add(normallizedLandscape);
        //         Console.WriteLine("LISTCOUNTER " + priorLandscapes.Count);
        //         priorLandscapeResults.Add(new float[landscapeArray.GetLength(0), landscapeArray.GetLength(1)]);
        //         return 0;
        //     }
        //
        //     // compute absolute value of pairwise diff
        //     for (int listIndex = 0; listIndex < priorLandscapes.Count; listIndex += 1)
        //     {
        //         int comparedSum = 0;
        //         for (int row = 0; row < landscapeArray.GetLength(0); row += 1)
        //         {
        //             for (int col = 0; col < landscapeArray.GetLength(1); col += 1)
        //             {
        //                 comparedSum += Math.Abs(normallizedLandscape[row, col] - priorLandscapes[listIndex][row, col]);
        //             }
        //         }
        //
        //         // if (logging)
        //         //   Console.WriteLine("ComparedSum: " + comparedSum);
        //         //if there is a prior landscape within +/- 5% of the current landscape return its index
        //         if (comparedSum >= -.05 && comparedSum <= 0.5)
        //         {
        //             if (logging)
        //                 Console.WriteLine("Found a similar landscape");
        //             return listIndex;
        //         }
        //     }//end for listIndex
        //
        //     //if there are not any similar landscapes add it to the expereince array and return its index
        //     if (logging)
        //         Console.WriteLine("No similar landscape found, adding a new one");
        //     priorLandscapes.Add(normallizedLandscape);
        //     priorLandscapeResults.Add(new float[landscapeArray.GetLength(0), landscapeArray.GetLength(1)]);
        //     return priorLandscapes.Count - 1;
        //
        // }
        //
        // //sets min and max val
        // public QLearning(int minVal, int maxVal)
        // {
        //     this.minVal = minVal;
        //     this.maxVal = maxVal;
        // }
        // //COMES FROM FCM
        // //sets the current state
        // public void setState(int state)
        // {
        //     this.state = state;
        // }
        //
        // //get the current state
        // public int getState()
        // {
        //     Console.WriteLine("State: " + state);
        //     return state;
        // }
        //
        // //finds a similar landscape and chooses a direction off said landscape
        // public void newLandscape(int[,] landscapeArray)
        // {
        //     similarLandscapeExperience = findSimlarLanscapeIndex(landscapeArray, minVal, maxVal);
        //     direction = PickDirection(state, similarLandscapeExperience);
        //     //setState(direction);
        //     // if (logging)
        //     //   Console.WriteLine("Direction: " + direction);
        //     //So, it finds a lanscape of similar experience, then it chooses a new direction based off said landscape
        // }
        //
        // //Returns current direction
        // public int getDirection()
        // {
        //     Console.WriteLine("Direction: " + direction);
        //     return direction;
        // }
        //    //qmatrix for each action type of thing
        //    //This is the random rouletteWheel in action
        //    //comment out for now to deal with hard coding
        // public int PickDirection(int state, int similarLandscapeExperience) // state chosen will be coming FCM (flee, eat, do nothing, etc)
        // {
        //     Random rand = new Random();
        //
        //     if (rand.NextDouble() < (1 / priorLandscapes.Count)) // rand.NextDouble() < (1 / priorLandscapes.Count) //use prior experience
        //     {
        //         float[] experienceArray = Enumerable.Range(0, priorLandscapeResults[similarLandscapeExperience].GetLength(0)).Select(x => priorLandscapeResults[similarLandscapeExperience][x, state]).ToArray();
        //         float totalArrayValue = experienceArray.Sum();
        //         List<float> biasAnswerList = new List<float>();
        //
        //         for (int i = 0; i < experienceArray.Length; i++)
        //         {
        //             // Console.WriteLine("experienceArray.Length: " + ((experienceArray[i] + 1) / (totalArrayValue + 1) * 100));
        //             for (int j = 0; j < (experienceArray[i] + 1) / (totalArrayValue + 1) * 100; j++)
        //             {
        //                 biasAnswerList.Add(experienceArray[i]);
        //             }
        //         }
        //
        //         float value = biasAnswerList[rand.Next(0, biasAnswerList.Count)];
        //
        //         for (int x = 0; x < priorLandscapeResults[similarLandscapeExperience].GetLength(0); ++x)
        //         {
        //             for (int y = 0; y < priorLandscapeResults[similarLandscapeExperience].GetLength(1); ++y)
        //             {
        //                 if (priorLandscapeResults[similarLandscapeExperience][x, y].Equals(value))
        //                 {
        //                     return y;
        //                 }
        //             }
        //         }
        //         return 0;
        //     }
        //     else //explore
        //     {
        //         if (logging)
        //             Console.WriteLine("Exploring");
        //         float[] experienceArray = Enumerable.Range(0, priorLandscapeResults[similarLandscapeExperience].GetLength(0)).Select(x => priorLandscapeResults[similarLandscapeExperience][x, state]).ToArray();
        //         float totalArrayValue = experienceArray.Sum();
        //         List<int> biasAnswerList = new List<int>();
        //
        //         int answer = rand.Next(0, experienceArray.Length - 1) + rand.Next(0, experienceArray.Length - 1);
        //         return answer;
        //     }
        // }
        //
        // //learning
        // public void UpdateQTable(float oldFitness, float newFitness)
        // {
        //
        //     float fitnessDelta = Math.Abs((newFitness - oldFitness) / newFitness);
        //     priorLandscapeResults[similarLandscapeExperience][state, direction] += fitnessDelta;
        //     if (priorLandscapeResults[similarLandscapeExperience][state, direction] < 0) priorLandscapeResults[similarLandscapeExperience][state, direction] = 0; // do not let it be negative
        //
        //     if (logging)
        //     {
        //         Console.WriteLine("Updated QTable");
        //         for (int i = 0; i < priorLandscapeResults[similarLandscapeExperience].GetLength(0); i++)
        //         {
        //             for (int j = 0; j < priorLandscapeResults[similarLandscapeExperience].GetLength(1); j++)
        //             {
        //                 Console.Write(priorLandscapeResults[similarLandscapeExperience][i, j] + "\t");
        //             }
        //             Console.WriteLine();
        //         }
        //     }
        //     if (logging)
        //         Console.WriteLine("------------------------------------");
        //
        // }

        //-----Code written for hard coded values: SIMPLIFIED VERSION-----//

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

        }

        //---SETTERS---//
        //Sets the QlearnMap
        //can be way more efficent, but this is just a temp job
        public void setQlearnMap(){
          //var filePath = @"..\HillClimberABMExample\layers\LandScapeSlopeHard.csv";
          float[,] data = new float[4,4]; //4x4 qmap matrix hard coded
          using(var reader = new StreamReader(@"..\HillClimberABMExample\layers\LandScapeSlopeHard.csv"))
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
        }

        //---HELPER FUNCTIONS---//
        /**
         * @param landScapePatch: 3x3 matrix of landscape agent is on
         * @param min: smallest elevation in landscapePatch
         * @param max: largest elevation in landscapePatch
         * @description: finds out direction agent should go using MSE and biasedRouletteWheel
         * @return: value ranging from 1,5,7,3 which represents N. E. S. W.
         */
        public int getDirection(float[,] landscapePatch, float min, float max){
          float[,] normallisedLandscapePatch = normalliseLandscapePatch(landscapePatch, min, max);
          float[] MSE = new float[4];
          for(int i = 0; i < this.prototypes.Count; i++){
            MSE[i] = meanSquareError(normallisedLandscapePatch, prototypes.ElementAt(i));
          }
          //returns index of smallest value (Grabbed from: https://stackoverflow.com/questions/4204169/how-would-you-get-the-index-of-the-lowest-value-in-an-int-array)
          int minIndex = Enumerable.Range(0, MSE.Length).Aggregate((a, b) => (MSE[a] < MSE[b]) ? a : b);

          int direction = biasedRouletteWheel(minIndex);

          //direction gives 0-3. The list of locations in animal.cs contains 0-8.
          //so, we need to change direction to work on a list
          //Direction will change via 0=>1, 1=>5, 2=>7, 3=>3
          int[] directionMap = {1,5,7,3};
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
         * @param newEle: integer value of the new elevation
         * @param oldEle: integer value of the old elevation
         * @description: Gets the absolute of the difference of elevations and adds it to fitness
         * Writes value to file (should end up with single total fitness value)
         */
        public void getNewFit(int newEle, int oldEle, int animalId, int tickNum, float[,] landScapePatch, bool export, int x, int y){
          float fitDiff =  Math.Abs(newEle-oldEle);

          fitness.Add(fitDiff);
          List<float> tempList =  new List<float>();
          if(fitDict.ContainsKey(animalId) == null || !fitDict.ContainsKey(animalId)){
            tempList.Add(fitDiff);
            fitDict.Add(animalId, tempList);
          }
          else{
            tempList = fitDict[animalId];
            tempList.Add(fitDiff);
            fitDict[animalId] = tempList;
          }

          if(export){
            setExportValues(landScapePatch, animalId, tickNum, newEle, oldEle, fitDiff,x,y);
          }

        }

        /**
         * @param landScapePatch: array of current landScape
         * @param animalId: id of current animal
         * @param tickNum: tick number
         * @param newEle: next elevation
         * @param oldEle: current elevation
         * @param fitVal: current fitness gained from moving
         * @param x: x value of position
         * @param y: y value of position
         * @description: Puts the following parameters into a public list of arrays
         */
        public void setExportValues(float[,] landScapePatch, int animalId, int tickNum, int newEle, int oldEle, float fitVal, int x, int y){
          //spots 0,1,11,12,13 are reserved for these values
          float[] temp = new float[18];
          temp[0] = (float)animalId;
          temp[1] = (float)tickNum;
          temp[11] = (float)oldEle;
          temp[12] = (float)newEle;
          temp[13] = temp[14] = temp[15]= fitVal;
          temp[16] = (float)x;
          temp[17] = (float)y;

          int counter = 2;
          for(int row = 0; row < landScapePatch.GetLength(0); row++){
            for(int col = 0; col < landScapePatch.GetLength(1); col++){
              temp[counter] = landScapePatch[row,col];
              counter++;
            }
          }

          //look at previous fit val for all of it, take average and mwhatnot
          if(!animalIDHolder.Contains(animalId)){
            animalIDHolder.Add(animalId);
          }

          List<float[]> tempList = new List<float[]>();
          if(patchDict.ContainsKey(animalId) == null || !patchDict.ContainsKey(animalId)){
            tempList.Add(temp);
            patchDict.Add(animalId, tempList);
          }
          else{
            tempList = patchDict[animalId];
            int numberOfFitVals = 1;
            foreach(float[] array in tempList){
              temp[15] += array[13];
              numberOfFitVals++;
            }
            temp[14] = temp[15]/numberOfFitVals;
            tempList.Add(temp);
            patchDict[animalId] = tempList;
          }
        }//end exportValues


    }
}
