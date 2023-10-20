using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;

namespace CognitiveABM.agentInformationHolder
{
    public class agentInfoHolder
    {
      public static ConcurrentDictionary<int, (List<float[]>, List<(int,int)>)> agentInfo;
      public static List<float> fitness;


      public agentInfoHolder(){
        agentInfo = new ConcurrentDictionary<int, (List<float[]>, List<(int,int)>)>();
        fitness = new List<float>();

      }

      /**
       * @param id: id of animal agent
       * @param exportVals: list all values to be exported to a csv
       * @param path: pathway agent took
       * @description: adds the parameters to the agentInfo dictionary
       */
      public void addItem(int id, List<float[]> exportVals, List<(int,int)> path){
        if(!agentInfo.TryAdd(id,(exportVals,path))){
          editItem(id,exportVals,path);
        }


      }

      /**
       * @param id: id of animal agent
       * @param exportVals: list all values to be exported to a csv
       * @param path: pathway agent took
       * @description: updates the dictionary with the new items
       */
      public void editItem(int id, List<float[]> exportVals, List<(int,int)> path){
        agentInfo.TryUpdate(id,(getNewExportVals(id,exportVals),getNewPathWay(id,path)), (exportVals,path));
      }

      /**
       * @param id: id of animal agent
       * @param exportVals: list all values to be exported to a csv
       * @description: updates the export values based from the previous export value array
       * @return: returns a list containing the updated export values
       */
      public List<float[]> getNewExportVals(int id, List<float[]> exportVals){
        //agent info is our patchdict
        List<float[]> infoList = agentInfo[id].Item1;
        float[] holdInfo = exportVals.Last();
        holdInfo[12] = (infoList.Last())[11];
        /* 
          calculate the altitude by getting the differences 
          from the path that the agent took: temp[11] (initial val) temp[12] compared value
          stored into temp[13] which is the difference between the two altitudes
        */
        holdInfo[13] = holdInfo[11] - holdInfo[12];
        
        // =============================================================================================== //
        // 1. hill descending -> flips the values (3 5 7 8 4 4 1 -> differences -> 2 2 2 1 -4 0 -3)
        //holdInfo[13] = -1*holdInfo[13];

        // 2. both ascend/descend (favor the bigger gap? or smaller)
        /*holdInfo[13] = holdInfo[11] - holdInfo[12];
        if((holdInfo[12] - holdInfo[11]) > (holdInfo[11] - holdInfo[12])){ // if descending is bigger gap, then descend
          holdInfo[13] = -1*holdInfo[13];
        }*/
        // 3. maybe if the hill is too high, then don't climb
        /*holdInfo[13] = holdInfo[11] - holdInfo[12];
        int maxClimb = 7;
        if(holdInfo[11] - holdInfo[12] > maxClimb){
          holdInfo[13] = -1*holdInfo[13];
        }*/
        /* =====================================================================================*/
        float[] avgMax = getAverageandTotal(infoList, holdInfo[13]);
        holdInfo[14] = avgMax[0];
        holdInfo[15] = avgMax[1];
        //fitness.Add(temp[13]);
        infoList.Add(holdInfo);
        return infoList;
      }


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

      /**
       * @param id: id of animal agent
       * @param path: list of the pathway the agent traversed
       * @description: updates the pathway based from the previous pathways
       * @return: returns a list containing the updated pathways
       */
      public List<(int,int)> getNewPathWay(int id, List<(int,int)> path){
        List<(int,int)> tempList = agentInfo[id].Item2;
        foreach((int,int)tuple in path){
          tempList.Add(tuple);
        }
        return tempList;
      }

      //------GETTER METHODS------\\


      //Returns the dictionary
      public ConcurrentDictionary<int, (List<float[]>, List<(int,int)>)> getInfo(){
        return agentInfo;
      }

      //returns list containing fitness values
      public List<float> getFit(){
        return fitness;
      }

    }
}
