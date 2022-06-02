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

      public void addItem(int id, List<float[]> exportVals, List<(int,int)> path){
        if(!agentInfo.TryAdd(id,(exportVals,path))){
          editItem(id,exportVals,path);
        }


      }

      public void editItem(int id, List<float[]> exportVals, List<(int,int)> path){
        agentInfo.TryUpdate(id,(getNewExportVals(id,exportVals),getNewPathWay(id,path)), (exportVals,path));
      }

      public List<float[]> getNewExportVals(int id, List<float[]> exportVals){
        //agent info is our patchdict
        List<float[]> tempList = agentInfo[id].Item1;
        float[] temp = exportVals.Last();
        temp[12] = (tempList.Last())[11];
        temp[13] = temp[11] - temp[12];
        float[] avgMax = getAverageandTotal(tempList, temp[13]);
        temp[14] = avgMax[0];
        temp[15] = avgMax[1];
        fitness.Add(temp[13]);
        tempList.Add(temp);
        return tempList;
      }

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

      public List<(int,int)> getNewPathWay(int id, List<(int,int)> path){
        List<(int,int)> tempList = agentInfo[id].Item2;
        foreach((int,int)tuple in path){
          tempList.Add(tuple);
        }
        return tempList;
      }

      public ConcurrentDictionary<int, (List<float[]>, List<(int,int)>)> getInfo(){
        return agentInfo;
      }

      public List<float> getFit(){
        return fitness;
      }

    }
}
