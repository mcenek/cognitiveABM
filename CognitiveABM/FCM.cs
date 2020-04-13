using System;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveABM.FCM
{
    public abstract class FCM
    {
        private List<List<double>> _agents;

        protected int NumberOfValues { get; }
        protected int Population { get; }

        protected int Iterations { get; }

        private double FitnessTarget = 135;

        public static List<List<double>> Agents;

        public FCM(int population, int numberOfValues, int iterations)
        {
            Population = population;
            NumberOfValues = numberOfValues;
            Iterations = iterations;
            Agents = new List<List<double>>(population);
            for (int i = 0; i < population; i++)
            {
                Agents.Add(CreateRandomArray(numberOfValues));
            }
        }

        public abstract List<double> Fitness(List<List<double>> agents);

        public abstract List<List<double>> GenerateOffspring(List<double> agentFitness);

        public List<double> Run()
        {
            List<double> agentFitness = new List<double>();

            for (int epoch = 0; epoch < Iterations; epoch++)
            {
                agentFitness = Fitness(Agents);
                
                var avg = agentFitness.Average();
                var sum = agentFitness.Sum();
                var max = agentFitness.Max();

                if (Iterations > 1)
                {
                    Console.WriteLine("Epoch: {0} Avg: {1,1:F4}, Max: {2,1:F4}", epoch, avg, max);
                }
                else
                {
                    Console.WriteLine("Average fitness: {0:F2}, Max fitness: {1:F2}", avg, max);
                }

                if (avg >= FitnessTarget)
                {
                    Console.WriteLine("FitnessTarget met.");
                    Environment.Exit(0);
                }

                //for (int i = 0; i < agentFitness.Count; i++)
                //{

                //    if (agentFitness[i] > avg)
                //    {
                //    	agentFitness[i] *= 1.5;
                //    }
                //    else
                //    {
                //    	agentFitness[i] *= 0.5;
                //    }

                //}

                if (sum == 0)
                {
                    for (int i = 0; i < agentFitness.Count; i++)
                    {
                        agentFitness[i] = 1;
                    }
                }

                List<double> agentReproductionPercentages = CalculateReproductionPercent(agentFitness.ToList());
                Agents = GenerateOffspring(agentReproductionPercentages);
            }

            return agentFitness;
        }

        protected Tuple<List<double>, List<double>> PickParents(List<double> agentReproductionProbabilites)
        {
            int firstParentIndex = SelectRandomWeightedIndex(agentReproductionProbabilites);
            double temp = agentReproductionProbabilites[firstParentIndex];

            agentReproductionProbabilites[firstParentIndex] = 0; // first parent cannot be picked twice

            int secondParentIndex = SelectRandomWeightedIndex(agentReproductionProbabilites);

            agentReproductionProbabilites[firstParentIndex] = temp;

            return Tuple.Create(Agents[firstParentIndex], Agents[secondParentIndex]);
        }

        private int SelectRandomWeightedIndex(List<double> weights)
        {
            Random random = new Random();
            double value = random.NextDouble() * weights.Sum();
            double sum = 0;
            for (int i = 0; i < weights.Count; i++)
            {
                sum += weights.ElementAt(i);
                if (value < sum)
                    return i;
            }
            throw new Exception("SelectRandomWeightedIndex did not find index.");
        }

        private List<double> CreateRandomArray(int length)
        {
            Random random = new Random();
            return Enumerable.Repeat(0, length).Select(i => random.NextDouble()).ToList();
            //return Enumerable.Repeat(0, length).Select(i => 1.0).ToList();
        }

        private List<double> CalculateReproductionPercent(List<double> agentFitness)
        {
            List<double> reproductionPercent = new List<double>();
            double sumOfFitnessValues = agentFitness.Sum();
            double averageFitness = AverageFitness();

            foreach (double fitnessValue in agentFitness)
            {
                double multiplier = 1;
                if (fitnessValue > averageFitness)
                {
                    multiplier = 1.25;
                }
                else
                {
                    multiplier = 1;
                }

                double agentReproductionPercent = (fitnessValue * multiplier) / sumOfFitnessValues;
                reproductionPercent.Add(agentReproductionPercent);
            }

            return reproductionPercent;
        }

        public override String ToString()
        {
            var fitness = Fitness(Agents);
            String output = "\n";
            for (int i = 0; i < Population; i++)
            {
                output += ("Agent[" + i + "] Fitness: " + fitness[i] + "\nValues: " + string.Join(",", Agents[i]) + "\n");
            }
            return output;
        }

        public double AverageFitness()
        {
            return Fitness(Agents).Sum() / Agents.Count;
        }

        public double MaxFitness()
        {
            return Fitness(Agents).Max();
        }
    }
}