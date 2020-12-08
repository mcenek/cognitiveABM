using System;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveABM.QLearning
{

  public class QLearning
  {
    private bool logging = true;
    private List<int[,]> priorLandscapes = new List<int[,]>();
    private List<float[,]> priorLandscapeResults = new List<float[,]>();
    private int similarLandscapeExperience;
    private int direction = 0;
    private int state = 0;
    private int minVal, maxVal = 0;

    public int findSimlarLanscapeIndex(int[,] landscapeArray, int minVal, int maxVal)
    {
      //normallise the array;
      int[,] normallizedLandscape = new int[landscapeArray.GetLength(0), landscapeArray.GetLength(1)];
      for (int row = 0; row < landscapeArray.GetLength(0); row += 1)
      {
        for (int col = 0; col < landscapeArray.GetLength(1); col += 1)
        {
          normallizedLandscape[row, col] = (landscapeArray[row, col] - minVal) / (maxVal - minVal);
        }
      }

      //if this is the first array, add it to the experience array and return index 0
      if (priorLandscapes.Count == 0)
      {
        if (logging)
          Console.WriteLine("First landscape, adding a new one to the array.");
        priorLandscapes.Add(normallizedLandscape);
        priorLandscapeResults.Add(new float[landscapeArray.GetLength(0), landscapeArray.GetLength(1)]);
        return 0;
      }

      // compute absolute value of pairwise diff
      for (int listIndex = 0; listIndex < priorLandscapes.Count; listIndex += 1)
      {
        int comparedSum = 0;
        for (int row = 0; row < landscapeArray.GetLength(0); row += 1)
        {
          for (int col = 0; col < landscapeArray.GetLength(1); col += 1)
          {
            comparedSum += Math.Abs(normallizedLandscape[row, col] - priorLandscapes[listIndex][row, col]);
          }
        }

        // if (logging)
        //   Console.WriteLine("ComparedSum: " + comparedSum);
        //if there is a prior landscape within +/- 5% of the current landscape return its index
        if (comparedSum >= -.05 && comparedSum <= 0.5)
        {
          if (logging)
            Console.WriteLine("Found a similar landscape");
          return listIndex;
        }
      }

      //if there are not any similar landscapes add it to the expereince array and return its index
      if (logging)
        Console.WriteLine("No similar landscape found, adding a new one");
      priorLandscapes.Add(normallizedLandscape);
      priorLandscapeResults.Add(new float[landscapeArray.GetLength(0), landscapeArray.GetLength(1)]);
      return priorLandscapes.Count - 1;

    }



    public QLearning(int minVal, int maxVal)
    {
      this.minVal = minVal;
      this.maxVal = maxVal;
    }

    public void setState(int state)
    {
      this.state = state;
    }

    public void newLandscape(int[,] landscapeArray)
    {
      similarLandscapeExperience = findSimlarLanscapeIndex(landscapeArray, minVal, maxVal);
      direction = PickDirection(state, similarLandscapeExperience);
      // if (logging)
      //   Console.WriteLine("Direction: " + direction);
    }

    public int getDirection()
    {
      Console.WriteLine("Direction: " + direction);
      return direction;
    }

    public int PickDirection(int state, int similarLandscapeExperience) // state chosen will be coming FCM (flee, eat, do nothing, etc)
    {
      Random rand = new Random();

      if (rand.NextDouble() < (1 / priorLandscapes.Count)) // rand.NextDouble() < (1 / priorLandscapes.Count) //use prior experience
      {
        float[] experienceArray = Enumerable.Range(0, priorLandscapeResults[similarLandscapeExperience].GetLength(0)).Select(x => priorLandscapeResults[similarLandscapeExperience][x, state]).ToArray();
        float totalArrayValue = experienceArray.Sum();
        List<float> biasAnswerList = new List<float>();

        for (int i = 0; i < experienceArray.Length; i++)
        {
          // Console.WriteLine("experienceArray.Length: " + ((experienceArray[i] + 1) / (totalArrayValue + 1) * 100));
          for (int j = 0; j < (experienceArray[i] + 1) / (totalArrayValue + 1) * 100; j++)
          {
            biasAnswerList.Add(experienceArray[i]);
          }
        }

        float value = biasAnswerList[rand.Next(0, biasAnswerList.Count)];

        for (int x = 0; x < priorLandscapeResults[similarLandscapeExperience].GetLength(0); ++x)
        {
          for (int y = 0; y < priorLandscapeResults[similarLandscapeExperience].GetLength(1); ++y)
          {
            if (priorLandscapeResults[similarLandscapeExperience][x, y].Equals(value))
            {
              return y;
            }
          }
        }
        return 0;
      }
      else //explore
      {
        if (logging)
          Console.WriteLine("Exploring");
        float[] experienceArray = Enumerable.Range(0, priorLandscapeResults[similarLandscapeExperience].GetLength(0)).Select(x => priorLandscapeResults[similarLandscapeExperience][x, state]).ToArray();
        float totalArrayValue = experienceArray.Sum();
        List<int> biasAnswerList = new List<int>();

        int answer = rand.Next(0, experienceArray.Length - 1) + rand.Next(0, experienceArray.Length - 1);
        return answer;
      }
    }

    public void UpdateQTable(float oldFitness, float newFitness)
    {

      float fitnessDelta = Math.Abs((newFitness - oldFitness) / newFitness);
      priorLandscapeResults[similarLandscapeExperience][state, direction] += fitnessDelta;
      if (priorLandscapeResults[similarLandscapeExperience][state, direction] < 0) priorLandscapeResults[similarLandscapeExperience][state, direction] = 0; // do not let it be negative

      if (logging)
      {
        Console.WriteLine("Updated QTable");
        for (int i = 0; i < priorLandscapeResults[similarLandscapeExperience].GetLength(0); i++)
        {
          for (int j = 0; j < priorLandscapeResults[similarLandscapeExperience].GetLength(1); j++)
          {
            Console.Write(priorLandscapeResults[similarLandscapeExperience][i, j] + "\t");
          }
          Console.WriteLine();
        }
      }
      if (logging)
        Console.WriteLine("------------------------------------");

    }
  }
}