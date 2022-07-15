using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CognitiveABM.agentInformationHolder;

namespace CognitiveABM.QLearningABMAdditional{

  public class QLearningABMAdditional{

    public QLearningABMAdditional(){
      //Nothing to put here
    }

    /**
     * @param generations: how many generations are being made
     * @return: lambda values for each generation
     * @description: comes up with an array of lambda values to use for each generation
     */
    public float[] getLambda(int generations){
      float[] array = new float[generations];
      float lambda = .1f;

      for(int i = 0; i < generations; i++){
        if(i == 0 || i > 0 && i < (generations/3)){
          array[i] = .1f;
        }
        else if(i == generations-1 || i < generations-1 && i > (generations/1.5)){
          array[i] = .01f;
        }
        else{
          array[i] = .05f;
        }
      }
      return array;
    }//end getLambda


    /**
     * @param lambda: value used to calculate how much an affect current generation has on qmap
     * @param agentHolder: agentInfoHolder object that contains information for the agents
     * @description: calculates the score each agent accumulated based from their run
     * @return: returns a dictionary containing the score for each agent
     */
    public Dictionary<int, float> getAgentScore(float lambda, agentInfoHolder agentHolder){
      Dictionary<int, float> scoreValue = new Dictionary<int, float>();
      Dictionary<int,int> maxSteps = new Dictionary<int,int>();

      List<float[]> patchList = new List<float[]>();
      foreach (KeyValuePair<int, (List<float[]>, List<(int,int)>)> entry in agentHolder.getInfo()){
        patchList = entry.Value.Item1;
        scoreValue.Add(entry.Key, patchList.Last()[15]);
      }//end for each id

      var temp = scoreValue.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
      scoreValue = temp;
      scoreValue = setScoreValue(scoreValue);

      maxSteps = getStepsToMax(agentHolder);
      scoreValue = calculateAgentScore(scoreValue, maxSteps, lambda, agentHolder);
      return scoreValue;
    }//end getAgentScore

    /**
     * @param scoreValue: Dictionary containing scores for each agent ranging from 1 through -1
     * @param maxSteps: Dictionary containing how many steps it took an agent to reach it's max elevation
     * @param lambda: Lambda value for calculation
     * @return: Dictionary containing final scores for each agent to be used in updating qMap
     * @description: Calculates an agent's score using the below equation
     * (ScoreValue (ranges from 1 to -1))/(Steps to max elevation * lambda)
     */
    public Dictionary<int, float> calculateAgentScore(Dictionary<int, float> scoreValue, Dictionary<int,int> maxSteps, float lambda, agentInfoHolder agentHolder){
      Dictionary<int, float> score = new Dictionary<int,float>();
      float pathBonus = 0.0f;
      foreach(var item in scoreValue){
        
        int maxStep = maxSteps[item.Key];
        if(maxSteps[item.Key] == 0){
          score.Add(item.Key,(scoreValue[item.Key]/(lambda)));
        }
        else{
          score.Add(item.Key,(scoreValue[item.Key]/(maxStep * lambda)));
        }
      }
      return score;
    }//end calculateAgentScore



    /**
     * @param patchDict: dictionary containing the patches an agent has traversed
     * @param animalIdList: list of all animal ids
     * @return: dictionary containing how many steps it took an agent to reach max elevation
     * @description: finds how many steps it took for an agent to reach its max elevation
     */
    public Dictionary<int,int> getStepsToMax(agentInfoHolder agentHolder){
      Dictionary<int,int> maxSteps = new Dictionary<int,int>();
      List<float[]> patchList = new List<float[]>();

      float AME = 0.0f;
      int maxStep = 0;

      //finds the max steps it took an agent to reach it's peak elevation
      foreach (KeyValuePair<int, (List<float[]>, List<(int,int)>)> entry in agentHolder.getInfo()){
        patchList = entry.Value.Item1;
        AME = 0.0f;
        foreach (float[] array in patchList){
         if(AME < array[11]){ //finds max elevation an agent reached
           AME = array[11];
           maxStep = (int)array[1];
         }
        }//end for each float[] array
        maxSteps.Add(entry.Key,maxStep);
      }//end for each id

      return maxSteps;
    }//end getStepsToMax

    /**
     * @param agentTotalFit: dictionary containing list of agent fitness scores
     * @return: middle value from dictionary of agent fitness values
     * @description: finds the middle distinct value from the list of agent scores
     */
    public float getMiddleValue(Dictionary<int,float> agentTotalFit){
      List<float> tempList = new List<float>();
      tempList = agentTotalFit.Values.ToList();
      tempList = tempList.Distinct().ToList();
      return tempList.ElementAt((tempList.Count()-1)/2);
    }//end getMiddleValue

    /**
     * @param agentTotalFit: dictionary containing list of agent fitness scores
     * @return: dictionary of agent scores ranging from 1 to -1
     * @description: creates a gradient of scores for each agent ranging from 1 to -1
     */
    public Dictionary<int,float> setScoreValue(Dictionary<int,float> agentTotalFit){
      Dictionary<int, float> scoreValue = new Dictionary<int, float>();

      float[] threeValues = {agentTotalFit.First().Value, getMiddleValue(agentTotalFit), agentTotalFit.Last().Value};//values that should get 1, 0, -1 for score
      int totalFitSize = agentTotalFit.Values.Distinct().Count(); //total size of agentTotalFit dictionary
      int halfWay = totalFitSize/2; //half way point of dictionary
      float scoreNumber = 1.0f; //score value for an agent
      float deltaVal; //delta value that scorenumber gets subtracted by for all values 1-0
      if(halfWay == 0){
        deltaVal = 0.0f;
      }
      else{
        deltaVal = 1.0f/halfWay;
      }

      //Console.Write("Total: " + totalFitSize + "; Half: " + halfWay + ", nextHalf:" + (totalFitSize-halfWay));

      //get delta for distinct vals
      float lowest = 0.0f; //lowest agentFitness seen


      //Console.Write("Start: " + threeValues[0] + " Mid: " + threeValues[1] + " Last: " + threeValues[2] + "\n");

      //goes through each agent's fitness and assigns it a score from 1 to -1
      foreach(var item in agentTotalFit){
        switch (item.Value){

          case float n when n == threeValues[0]:
            scoreNumber = 1.0f;
            scoreValue.Add(item.Key, scoreNumber);
            //Console.Write("Score of 1: " + item.Key + ", " + item.Value + ", " + scoreNumber + "\n");
            lowest = item.Value;
            break;

          case float n when n == threeValues[1]:
            scoreNumber = 0.0f;
            scoreValue.Add(item.Key, scoreNumber);
            //Console.Write("Score of 0: " + item.Key + ", " + item.Value + ", " + scoreNumber + "\n");
            lowest = item.Value;
            deltaVal = deltaVal;
            break;

          case float n when n == threeValues[2]:
            scoreValue.Add(item.Key, scoreNumber);
            //Console.Write("Score of -1: " + item.Key + ", " + item.Value + ", " + scoreNumber + "\n");
            lowest = item.Value;
            break;

          case float n when n == lowest:
            scoreValue.Add(item.Key, (float)Math.Round(scoreNumber,3));
            //Console.Write("Is previous: " + item.Key + ", " + item.Value + ", " + scoreNumber + "\n");
            break;

          case float n when n < lowest:
            lowest = item.Value;
            scoreNumber -= deltaVal;
            scoreValue.Add(item.Key, (float)Math.Round(scoreNumber,3));
            //Console.Write("Less than previous: " + item.Key + ", " + item.Value + ", " + scoreNumber + "\n");
            break;


          default:
            Console.Write("NUMBER ERROR: " + item.Value + "\n");
            break;
        }

      }

      return scoreValue;
    }//end setScoreValue

    /**
     * @param agentScore: dictionary containing the scores for each agent
     * @param agentQmapPath: dictionary containing pathway an agent walked
     * @param animalIdList: list of all animal ids
     * @description: updates the current qmap and prints it
     */
    public void updateQMap(Dictionary<int, float> agentScore, agentInfoHolder agentHolder){
            float[,] qmap = new float[8,8]; //4x4 qmap matrix hard coded
      string path = @"..\HillClimberABMExample\layers\qMapGenerated8x8.csv";
      if(File.Exists(path)){
        using(var reader = new StreamReader(path))
       {
           int counter = 0;
           while (!reader.EndOfStream)
           {
               var line = reader.ReadLine();
               var values = line.Split(',');
               if(counter < 8){
               qmap[counter,0] = float.Parse(values[0]);
               qmap[counter,1] = float.Parse(values[1]);
               qmap[counter,2] = float.Parse(values[2]);
               qmap[counter,3] = float.Parse(values[3]);
               qmap[counter,4] = float.Parse(values[4]);
               qmap[counter,5] = float.Parse(values[5]);
               qmap[counter,6] = float.Parse(values[6]);
               qmap[counter,7] = float.Parse(values[7]);
               counter++;
             }
           }
           reader.Close();
       }//end using

     }//end if
     //4x4 qmap matrix hard coded
      foreach(KeyValuePair<int, (List<float[]>, List<(int,int)>)> entry in agentHolder.getInfo()){
        foreach((int,int)tuple in entry.Value.Item2){
          //  Console.WriteLine(tuple.Item1 + " " + tuple.Item2);
          if(float.IsNaN(qmap[tuple.Item1,tuple.Item2])){
            Console.WriteLine(qmap[tuple.Item1,tuple.Item2]);
            Console.WriteLine("OVERHERE    " + tuple.Item1 + " " + tuple.Item2);
          }
          qmap[tuple.Item1,tuple.Item2] += agentScore[entry.Key];
        }
        qmap = normalliseQMap(qmap);
        qmap = roundQMap(qmap);
      }
      //qmap = roundQMap(qmap);

      //qmap = setColToOne(qmap);

      printNewQMap(qmap);
    }//end updateQMap

    /**
     * @param qmap: matrix of the current qmap
     * @description: checks to see if the columns of the qmap add to one
     * If a column doesn't add to one, the difference of that column and 1 is added to largest row
     * @return: fixed qmap where all columns add to one
     */
    public float[,] setColToOne(float[,] qmap){
      float colMaxTotal = 0.0f;
      float colMax = 0.0f;
      int rowMaxLoc = -1;

      for(int col = 0; col < qmap.GetLength(1); col++){
        for(int row = 0; row < qmap.GetLength(0); row++){
          if(qmap[row,col] >= colMax){
            colMax = qmap[row,col];
            rowMaxLoc = col;
          }
          colMaxTotal += qmap[row,col];
        }//end row for

        if(colMaxTotal < 1.0f){
          qmap[rowMaxLoc,col] += (1.0f - colMaxTotal);
        }
        colMaxTotal = 0.0f;
        colMax = 0.0f;
        rowMaxLoc = -1;
      }//end col for

      return qmap;
    }//end setColToOne

    /**
     * @return: qmap from selected file
     * @description: grabs the qmap from a selected csv file
     */
    public float[,] getQMap(){
      float[,] data = new float[8,8]; //4x4 qmap matrix hard coded
      string path = @"..\HillClimberABMExample\layers\qMapGenerated8x8.csv";
      if(File.Exists(path)){
        using(var reader = new StreamReader(path))
       {
           int counter = 0;
           while (!reader.EndOfStream)
           {
               var line = reader.ReadLine();
               var values = line.Split(',');
               if(counter < 8){
               data[counter,0] = float.Parse(values[0]);
               data[counter,1] = float.Parse(values[1]);
               data[counter,2] = float.Parse(values[2]);
               data[counter,3] = float.Parse(values[3]);
               data[counter,4] = float.Parse(values[4]);
               data[counter,5] = float.Parse(values[5]);
               data[counter,6] = float.Parse(values[6]);
               data[counter,7] = float.Parse(values[7]);
               for(int i = 0; i < 8; i++){
                Console.Write(data[counter, i]);
               }
               Console.WriteLine();
               counter++;
             }
           }
           reader.Close();
       }//end using

     }//end if
     else{
       for(int i = 0; i < 8; i++){
         for(int k = 0; k < 8; k++){
           data[i,k] = 0.0f;
         }
       }
     }
     
     return data;
    }//end getQMap


    /**
     * @param values: list of string arrays containg values to print
     * @param terrianFilePath: String of full pathway to selected terrian File
     * @description: prints all values of the values parameter into a csv file named after the terrian
     */
    public void exportInfo(string terrianFilePath, agentInfoHolder agentHolder){
          string fileName = "./output/" + Path.GetFileNameWithoutExtension(terrianFilePath) + "_exportInfo.csv";
          var w = new StreamWriter(path: fileName);

          //write headers to csv
          string[] headers = new string[18];
          headers[0] = "AnimalID";
          headers[1] = "TickNum";
          for(int k = 2; k < headers.Length-3; k++){
            headers[k] = "LandscapePatch " + (k-2).ToString();
          }
          headers[11] = "Current Elevation";
          headers[12] = "Previous Elevation";
          headers[13] = "Fitness Gained";
          headers[14] = "Average Fitness";
          headers[15] = "Total Fitness";
          headers[16] = "X Pos";
          headers[17] = "Y Pos";
          w.Write(String.Join(",", headers) + "\n");

          //write data to csv
          List<float[]> patchList = new List<float[]>();
          foreach (KeyValuePair<int, (List<float[]>, List<(int,int)>)> entry in agentHolder.getInfo()){
            patchList = entry.Value.Item1;
            foreach (float[] array in patchList){
               w.Write(String.Join(",", array) + "\n");
            }//end for each float[] array
          }//end for each id
          w.Close();
    }//end exportInfo


    /**
     * @param qmap: qmap to be normallised
     * @return: normallised qmap
     * @description: normallises qmap so that each collumn adds to one
     */
    public float[,] normalliseQMap(float[,] qmap){
      float total = 0.0f;

      float[,] normallizedQMap = new float[qmap.GetLength(0), qmap.GetLength(1)];
      float[] rowVals = new float[qmap.GetLength(0)];

      for (int col = 0; col < qmap.GetLength(1); col++){
        for (int row = 0; row < qmap.GetLength(0); row++){
          if(float.IsNaN(Math.Abs(qmap[row,col]))){
            total += 0.0f;
            rowVals[row] = 0.0f;
            Console.WriteLine("NANAANANANANA");
          }
          else{
            total += Math.Abs(qmap[row,col]);
            rowVals[row] = Math.Abs(qmap[row,col]);
          }
        }

        for (int row = 0; row < qmap.GetLength(0); row++){
          if(total == 0.0f){
            total = 1.0f;
          }
          normallizedQMap[row,col] = rowVals[row]/total;
        }
        total = 0.0f;

      }

      return normallizedQMap;
    }//end normalliseQMap


    /**
     * param qmap: qmap to be rounded
     * @return: rounded qmap
     * @description: rounds values of a qmap to three decimal places
     */
    public float[,] roundQMap(float[,] qmap){
      float total = 0.0f;
      float[,] qMapCol= new float[qmap.GetLength(0),qmap.GetLength(1)];
      for(int col = 0; col < qmap.GetLength(1); col++){
        for(int row = 0; row < qmap.GetLength(0); row++){
          qMapCol[row,col] = (float)Math.Round(qmap[row,col],5);
          total += qMapCol[row,col];
        }

        if(total != 1.0f){
          float diff = 1.0f - total;
          float max = 0.0f;
          int rowIndex = 0;

          for(int row = 0; row < qMapCol.GetLength(0); row++){
            if(max < qMapCol[row,col]){
              max = qMapCol[row,col];
              rowIndex = row;
            }
          }//end inner col loop
          qMapCol[rowIndex,col] += diff;
        }//end if

        total = 0.0f;
      }
      return qMapCol;
    }//end roundQMap

    /**
     * @param qmap: qmap to be printed
     * @description: prints a qmap to file path
     */
    public void printNewQMap(float[,] qmap){
      string fileName = @"..\HillClimberABMExample\layers\qMapGenerated8x8.csv";
      var w = new StreamWriter(path: fileName);
      float[] qMapRow = new float[qmap.GetLength(0)];


      for(int row = 0; row < qmap.GetLength(0); row++){
        for(int col = 0; col < qmap.GetLength(1); col++){
          qMapRow[col] = qmap[row,col];
          // if(float.IsNaN(qMapRow[col])){
          //   Console.WriteLine("NAN" + row + col);
          //   Environment.Exit(0);
          // }
        }
        w.Write(String.Join(",", qMapRow) + "\n");
      }
      w.Close();
    }//end printNewQMap
  }//end class

}//end namespace
