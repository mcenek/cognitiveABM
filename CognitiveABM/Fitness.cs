using System;
using System.Collections.Generic;
using System.Linq;
using CognitiveABM.agentInformationHolder;
using CognitiveABM.QLearningABMAdditional;
public class Fitness{

    // QLearningABMAdditional < -----------------------------------------------------------------------------------------

    /**
     * @param lambda: value used to calculate how much an affect current generation has on qmap
     * @param agentHolder: agentInfoHolder object that contains information for the agents
     * @description: calculates the score each agent accumulated based from their run
     * @return: returns a dictionary containing the score for each agent
     */
    public Dictionary<int, float> getAgentScore(float lambda, agentInfoHolder agentHolder){
      Dictionary<int, float> scoreValue = new Dictionary<int, float>();
      Dictionary<int,int> maxSteps = new Dictionary<int,int>();

      foreach (KeyValuePair<int, (List<float[]>, List<(int,int)>)> entry in agentHolder.getInfo()){
        float ElevationScore = entry.Value.Item1.Last()[15];
        scoreValue.Add(entry.Key,ElevationScore);
      }//end for each id

      var temp = scoreValue.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
      scoreValue = temp;
      scoreValue = setScoreValue(scoreValue);

      maxSteps = getStepsToMin(agentHolder);
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

    // agentInfoHolder < ----------------------------------------------------------------------------------------------

          /**
       * @param tempList: list containing the export values
       * @param currentFit: current fitness value
       * @description: calculates the average and total fitness an agent has accumulated
       * @returns: an array containing the average and total fitness
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

}