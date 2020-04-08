/*
 * HillClimberFCM.cs
 * Alex Weininger
 * Daniel Borg
 * 
 * This class implements the abstract FCM class from the CognitiveABM package. 
 * This will control the genetic algorithm for the HillClimber agents.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using CognitiveABM.FCM;

namespace hillClimber
{
	class HillClimberFCM : FCM
	{
		public HillClimberFCM(int population, int numberOfValues, int iterations) : base(population, numberOfValues, iterations) { }

		public override List<double> Fitness(List<List<double>> agents)
		{
			List<double> agentFitnessValues = new List<double>();
			foreach (List<double> agent in agents)
				agentFitnessValues.Add(agent.Average());
			return agentFitnessValues;
		}

		public override List<List<double>> GenerateOffspring(List<double> agentFitnessValues)
		{
			List<List<double>> offspring = new List<List<double>>();
			ConcurrentBag<List<double>> bag = new ConcurrentBag<List<double>>();
			Random random = new Random();

			Parallel.For(0, Population, index => {
				Tuple<List<double>, List<double>> parents = PickParents(agentFitnessValues.ToList());
				int splitIndex = random.Next(0, NumberOfValues);

				List<double> child = parents.Item1.GetRange(0, splitIndex).Concat(parents.Item2.GetRange(splitIndex, parents.Item2.Count - splitIndex)).ToList();

				var randomIndex = random.Next(child.Count);
				child[randomIndex] += random.NextDouble() - 0.5;

				if (child[randomIndex] > 1)
					child[randomIndex] = 1;

				if (child[randomIndex] < 0)
					child[randomIndex] = 0;

				bag.Add(child);
			});

			return bag.ToList();
		}
	}
}