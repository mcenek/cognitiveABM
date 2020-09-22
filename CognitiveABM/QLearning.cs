using System;
using System.Collections.Generic;

namespace CognitiveABM.QLearning
{

  public abstract class QLearning
  {
    private double[,] QTable;
    private int[] RewardTable;
    double Gamma;
    double LearnRate;
    double maxQ;


    public QLearning(int numberOfAcations, int numberOfStates, double gamma, double learnRate)
    {

      Gamma = gamma;
      LearnRate = learnRate;
      InitializeQTable(numberOfAcations, numberOfStates);
      RewardTable = new int[] { -5, 5 };
      maxQ = double.MinValue;
    }

    protected void InitializeQTable(int numberOfActions, int numberOfStates)
    {
      QTable = new double[numberOfStates, numberOfActions];
    }

    protected void SetInvalidStates(int actionNumber, List<int> setOfStates)
    {
      for (int i = 0; i < setOfStates.Count; i++)
      {
        QTable[actionNumber, i] = -1;
      }
    }

    protected int[] PickAction(double epsilonRate, int state) // action chosen will be coming FCM (flee, eat, do nothing, etc)
    {
      Random rand = new Random();

      if (rand.NextDouble() < epsilonRate) //use value from QTable
      {
        double max = QTable[state, 0];
        int maxCol = 0;
        for (int index = 0; index < QTable.GetLength(1); index++)
        {
          if (QTable[state, index] > max)
          {
            max = QTable[state, index];
            maxCol = index;
          }
        }
        return new int[] { state, maxCol };
      }
      else //explore: random
      {
        return new int[] { state, rand.Next(QTable.GetLength(1)) };
      }
    }

    protected void UpdateQTable(float oldFitness, float newFitness, int state, int action)
    {
      int wasChoiceGood;
      if (newFitness > oldFitness) wasChoiceGood = 1;
      else wasChoiceGood = 0;

      QTable[state, action] = ((1 - LearnRate) * QTable[state, action]) +
        (LearnRate * (RewardTable[wasChoiceGood] + (Gamma * maxQ)));
    }


  }
}