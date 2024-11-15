using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Collections.Concurrent;
using Mars.Core.SimulationManager.Implementation;
using FitnessFeatures;

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
        temp[13] = FitnessFunctions.getElevationDifference(temp[11], temp[12]);

        float[] avgMax = FitnessFunctions.getAverageandTotal(tempList, temp[13]); 
        temp[14] = avgMax[0];
        temp[15] = avgMax[1];
        fitness.Add(temp[13]); // ? TODO can comment depending on impact
        tempList.Add(temp);
        return tempList;
      }

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
