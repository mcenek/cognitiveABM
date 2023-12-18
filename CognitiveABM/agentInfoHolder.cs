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
        List<float[]> tempList = agentInfo[id].Item1;
        float[] temp = exportVals.Last();
        temp[12] = (tempList.Last())[11];
         /* 
          calculate the altitude by getting the differences 
          from the path that the agent took: temp[11] (initial val) temp[12] compared value
          stored into temp[13] which is the difference between the two altitudes
        */
        temp[13] = temp[11] - temp[12];
        // Avoid cliffs
        bool onCliff = false;
        /*if (temp[13] > 400 || temp[13] < 0) {
          onCliff = true;
        }
        float[] avgMax = getAverageandTotal(tempList, temp[13], onCliff);
        */
        // Hill Descending
        //temp[13] = -1*temp[13];

        float[] avgMax = getAverageandTotal(tempList, temp[13]); //? If not avoiding cliffs, uncomment this
        temp[14] = avgMax[0];
        temp[15] = avgMax[1];
        //fitness.Add(temp[13]);
        tempList.Add(temp);
        return tempList;
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

      public float[] getAverageandTotal(List<float[]> tempList, float currentFit, bool onCliff){
        int counter;
        float avgCurrentFit;
        float[] returnVals = {0.0f, 0.0f};

        if (onCliff){
          avgCurrentFit = -1000.0f;
          counter = 0;
        }
        else if(currentFit > 0){
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
