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

		private double FitnessTarget = 1000;

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

		public void Run()
		{
			for (int epoch = 0; epoch < Iterations; epoch++)
			{
				List<double> agentFitness = Fitness(Agents);
				Console.WriteLine("Epoch: {0} Avg: {1,1:F4}, Max: {2,1:F4}", epoch, AverageFitness(), MaxFitness());
				if (AverageFitness() >= FitnessTarget) {
					Console.WriteLine("FitnessTarget met.");
					break;
				}
				List<double> agentReproductionPercentages = CalculateReproductionPercent(agentFitness.ToList());
				
				var newAgents = GenerateOffspring(agentReproductionPercentages);
				Agents = newAgents;

				//for (int i = 0; i < 12; i++)
				//{
				//	Console.Write("{0:N2}\t", Agents[i][20]);
				//}
			}
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
		}

		private List<double> CalculateReproductionPercent(List<double> agentFitness)
		{
			List<double> reproductionPercent = new List<double>();
			double sumOfFitnessValues = agentFitness.Sum();
			double averageFitness = AverageFitness();

			foreach (double fitnessValue in agentFitness)
			{
				double multiplier = 1;
				if (fitnessValue > averageFitness) {
					multiplier = 1.25;
				} else
				{
					multiplier = 0.5;
				}
				double agentReproductionPercent = (fitnessValue*multiplier) / sumOfFitnessValues;
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

		private double AverageFitness()
		{
			return Fitness(Agents).Sum() / Agents.Count;
		}

		private double MaxFitness()
		{
			return Fitness(Agents).Max();
		}
	}
}