using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessFeatures
{
    public class FitnessFunctions
    {
        // Global constant for the target fitness value (used as a benchmark or reference).
        public static float GlobalTargetFitnes = 2000.0f;

        // Number of recent steps to consider when calculating momentum.
        public static int MomentumSteps = 5;

        /**
        * Calculates momentum over the last N steps and adjusts the current fitness value accordingly.
        * Momentum is used to reward or penalize changes based on their alignment with recent trends.
        * 
        * @param tempList: List of float arrays, where each array stores past fitness data.
        * @param currentChange: Current elevation change (fitness adjustment for this step).
        * @return: Adjusted fitness value based on the calculated momentum.
        */
        private static float CalculateMomentum(List<float[]> tempList, float currentChange)
        {
            // List to store the current and recent elevation changes.
            List<float> recentChanges = new List<float>();
            
            // Add the current change to the list.
            recentChanges.Add(currentChange);
            
            // Retrieve up to MomentumSteps - 1 recent changes from tempList (index 13).
            int count = 0;
            for (int i = tempList.Count - 1; i >= 0 && count < MomentumSteps - 1; i--)
            {
                recentChanges.Add(tempList[i][13]); // Assuming index 13 contains elevation changes.
                count++;
            }

            // Calculate the sum of recent changes to determine momentum.
            float momentum = recentChanges.Sum();
            
            // If the momentum is near zero (no clear trend), return the absolute current change.
            if (Math.Abs(momentum) < 0.1f)
            {
                return Math.Abs(currentChange);
            }
            
            // If momentum is positive (upward trend), reward positive changes and penalize negative changes.
            if (momentum > 0)
            {
                return currentChange > 0 ? currentChange * 1.5f : currentChange * 0.5f;
            }
            // If momentum is negative (downward trend), reward negative changes and penalize positive changes.
            else
            {
                return currentChange < 0 ? Math.Abs(currentChange) * 1.5f : Math.Abs(currentChange) * 0.5f;
            }
        }

        /**
        * Calculates the average and total fitness for an animal, incorporating momentum.
        * 
        * @param tempList: List of float arrays containing previous fitness-related data.
        * @param currentFit: Current fitness value (based on the most recent elevation change).
        * @return: Array containing two values: [average fitness, total fitness].
        */
        public static float[] getAverageandTotal(List<float[]> tempList, float currentFit)
        {
            // Initialize the return values for average and total fitness.
            float[] returnVals = {0.0f, 0.0f};
            float momentumBasedFitness;
            
            // If there are no previous records, use the absolute value of the current fitness.
            if (tempList.Count < 1)
            {
                momentumBasedFitness = Math.Abs(currentFit);
                returnVals[0] = momentumBasedFitness; // Average fitness
                returnVals[1] = momentumBasedFitness; // Total fitness
                return returnVals;
            }

            // Calculate the momentum-adjusted fitness value.
            momentumBasedFitness = CalculateMomentum(tempList, currentFit);
            
            // Initialize total fitness and counter for calculating averages.
            float totalFitness = momentumBasedFitness;
            float avgFitness = momentumBasedFitness;
            int counter = 1;

            // Sum up the fitness values from the historical data (index 13).
            foreach (float[] array in tempList)
            {
                totalFitness += Math.Abs(array[13]);
                counter++;
            }

            // Calculate the average fitness if there are records.
            if (counter > 0)
            {
                avgFitness = totalFitness / counter;
            }

            // Update the return values.
            returnVals[0] = avgFitness; // Average fitness
            returnVals[1] = totalFitness; // Total fitness
            
            return returnVals;
        }

        /**
        * Calculates the elevation difference between two points.
        * 
        * @param init: Initial elevation value.
        * @param end: Ending elevation value.
        * @return: Difference in elevation (init - end).
        */
        public static float getElevationDifference(float init, float end)
        {
            return init - end;
        }

        /**
        * Resets all fitness values for an agent to 1.
        * 
        * @param agentFitness: List of fitness values for an agent.
        * @return: Updated list with all values reset to 1.
        */
        public static List<float> resetFitness(List<float> agentFitness)
        {
            // Iterate through the list and set each fitness value to 1.
            for (int i = 0; i < agentFitness.Count; i++)
            {
                agentFitness[i] = 1;
            }
            return agentFitness;
        }

        /**
        * Calculates the average fitness value from a list of fitness values.
        * 
        * @param agentFitness: List of fitness values.
        * @return: Average fitness value.
        */
        public static float AverageFitness(List<float> agentFitness)
        {
            // Sum up all fitness values and divide by the total number of values.
            return agentFitness.Sum() / agentFitness.Count;
        }

        /**
        * Finds the maximum fitness value in a list.
        * 
        * @param agentFitness: List of fitness values.
        * @return: Maximum fitness value.
        */
        public static float MaxFitness(List<float> agentFitness) 
        {
            // Use LINQ's Max method to find the largest value in the list.
            return agentFitness.Max();
        }
    }
}
