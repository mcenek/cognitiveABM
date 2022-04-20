using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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
      float delta = .1f/generations;
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


    public Dictionary<int, float> getAgentScore(List<int> animalIdList, Dictionary<int,List<float[]>> patchDict, float lambda){
      Dictionary<int, float> scoreValue = new Dictionary<int, float>();
      Dictionary<int,int> maxSteps = new Dictionary<int,int>();

      List<float[]> patchList = new List<float[]>();
      foreach (int id in animalIdList){
        patchList = patchDict[id];
        scoreValue.Add(id, patchList.Last()[15]);
      }//end for each id

      var temp = scoreValue.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
      scoreValue = temp;

      scoreValue = setScoreValue(scoreValue);
      maxSteps = getStepsToMax(patchDict,animalIdList);
      scoreValue = calculateAgentScore(scoreValue, maxSteps, lambda);

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
    public Dictionary<int, float> calculateAgentScore(Dictionary<int, float> scoreValue, Dictionary<int,int> maxSteps, float lambda){
      Dictionary<int, float> score = new Dictionary<int,float>();
      foreach(var item in scoreValue){
        score.Add(item.Key,(scoreValue[item.Key]/(maxSteps[item.Key] * lambda)));
      }
      return score;
    }//end calculateAgentScore

    /**
     * @param patchDict: dictionary containing the patches an agent has traversed
     * @param animalIdList: list of all animal ids
     * @return: dictionary containing how many steps it took an agent to reach max elevation
     * @description: finds how many steps it took for an agent to reach its max elevation
     */
    public Dictionary<int,int> getStepsToMax(Dictionary<int,List<float[]>> patchDict, List<int> animalIdList){
      Dictionary<int,int> maxSteps = new Dictionary<int,int>();
      List<float[]> patchList = new List<float[]>();

      float AME = 0.0f;
      int maxStep = 0;

      //finds the max steps it took an agent to reach it's peak elevation
      foreach (int id in animalIdList){
        patchList = patchDict[id];
        AME = 0.0f;
        foreach (float[] array in patchList){
         if(AME < array[11]){ //finds max elevation an agent reached
           AME = array[11];
           maxStep = (int)array[1];
         }
        }//end for each float[] array
        maxSteps.Add(id,maxStep);
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
      float deltaVal = 1.0f/halfWay; //delta value that scorenumber gets subtracted by for all values 1-0

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
            scoreNumber = -1.0f;
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
    public void updateQMap(Dictionary<int, float> agentScore, Dictionary<int, List<(int,int)>> agentQmapPath, List<int> animalIdList){
      float[,] qmap = getQMap(); //4x4 qmap matrix hard coded

      foreach(int id in animalIdList){
        foreach((int,int)tuple in agentQmapPath[id]){
          qmap[tuple.Item1,tuple.Item2] += agentScore[id];
        }
        qmap = normalliseQMap(qmap);
      }

      qmap = roundQMap(qmap);
      printNewQMap(qmap);
    }//end updateQMap

    /**
     * @return: qmap from selected file
     * @description: grabs the qmap from a selected csv file
     */
    public float[,] getQMap(){
      float[,] data = new float[4,4]; //4x4 qmap matrix hard coded
      using(var reader = new StreamReader(@"..\HillClimberABMExample\layers\qMapRandom.csv"))
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
     return data;
    }//end getQMap


    /**
     * @param values: list of string arrays containg values to print
     * @param terrianFilePath: String of full pathway to selected terrian File
     * @description: prints all values of the values parameter into a csv file named after the terrian
     */
    public void exportInfo(Dictionary<int,List<float[]>> patchDict, List<int> animalIdList, string terrianFilePath){
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
          foreach (int id in animalIdList){
            patchList = patchDict[id];
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
          total += Math.Abs(qmap[row,col]);
          rowVals[row] = Math.Abs(qmap[row,col]);
        }

        for (int row = 0; row < qmap.GetLength(0); row++){
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
          qMapCol[row,col] = (float)Math.Round(qmap[row,col],3);
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
      string fileName = @"..\HillClimberABMExample\layers\qMapGenerated.csv";
      var w = new StreamWriter(path: fileName);
      float[] qMapRow = new float[qmap.GetLength(0)];


      for(int row = 0; row < qmap.GetLength(0); row++){
        for(int col = 0; col < qmap.GetLength(1); col++){
          qMapRow[col] = qmap[row,col];
        }
        w.Write(String.Join(",", qMapRow) + "\n");
      }
      w.Close();
    }//end printNewQMap
  }

}
