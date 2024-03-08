using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace FitnessFeatures
{
  public class FitnessFunctions{
    public static float GlobalTargetFitnes = 2000.0f;
  

    /** // From CognitiveABM/QLearning.cs
      * @param tempList: List of float arrays containing previous animal info
      * @param currentFit: current fitness gained value from this animal
      * @return: the average and total of all fitness gains of animal
      * @description: calculates the average and total fitness of an animal at current moment in time
      */
      // !! NEVER USED FUNCTION (Similar function used in /CognitveABM/agentInfoHolder.cs)
    public static float[] getAverageandTotal(List<float[]> tempList, float currentFit){
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
      * @param tempList[11],tempList[12]: float values from info array
      * @return: float difference in elevation
      * @description: calculates the difference in elevation in a step
      */
    public static float getElevationDifference(float init, float end){
        float diff =  init - end;
        //diff *= -1; // for hill descending
        return diff;
    }

        /** 
      * @param list<float> fitness: an agent's fitness
      * @return: list<float> fitness: the same input param
      * @description: resets agent's fitness to 1
      */
    public static List<float> resetFitness(List<float> agentFitness){
      for (int i = 0; i < agentFitness.Count; i++)
        {
            agentFitness[i] = 1;
        }
      return agentFitness;
    }

  }// class
}//name space