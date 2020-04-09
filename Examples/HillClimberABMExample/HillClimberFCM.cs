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
using System.IO;

namespace hillClimber
{
	class HillClimberFCM : FCM
	{

		// get from config.json maybe
		private int steps;
		private string fitnessFileName;
		private string fitnessColumnName;

		public HillClimberFCM(int population, int numberOfValues, int steps, string fitnessFileName, string fitnessColumnName) : base(population, numberOfValues, 1) 
		{
			this.fitnessFileName = fitnessFileName;
			this.fitnessColumnName = fitnessColumnName;
			this.steps = steps;
		}

		public override List<double> Fitness(List<List<double>> agents)
		{
			var fitnessValues = new List<double>();

			using (var reader = new StreamReader(fitnessFileName))
			{
				List<string> listA = new List<string>();
				List<string> listB = new List<string>();
				var header = reader.ReadLine();
				List<string> headerValues = new List<string>(header.Split(','));
				int indexOfFitnessValues = headerValues.FindIndex((str) => str == fitnessColumnName);
				while (!reader.EndOfStream)
				{
					var line = reader.ReadLine();
					var values = line.Split(',');

					if (Convert.ToInt32(values[0]) == steps)
					{
						fitnessValues.Add(Convert.ToDouble(values[indexOfFitnessValues]));
					}
				}
			}

			return fitnessValues;
		}

		public override List<List<double>> GenerateOffspring(List<double> agentFitnessValues)
		{
			ConcurrentBag<List<double>> bag = new ConcurrentBag<List<double>>();
			Random random = new Random();

			Parallel.For(0, Population, index => {
				Tuple<List<double>, List<double>> parents = PickParents(agentFitnessValues.ToList());
				int splitIndex = random.Next(0, NumberOfValues);

				List<double> child = new List<double>();

				List<double> parent1Genomes = new List<double>(parents.Item1.GetRange(0, splitIndex));
				List<double> parent2Genomes = new List<double>(parents.Item2.GetRange(splitIndex, parents.Item2.Count - splitIndex));

				child.AddRange(parent1Genomes);
				child.AddRange(parent2Genomes);

				for (int i = 0; i < 3; i++)
				{
					var randomIndex = random.Next(child.Count);
					child[randomIndex] += random.NextDouble() - 0.5;
					if (child[randomIndex] > 1)
						child[randomIndex] = 1;

					if (child[randomIndex] < 0)
						child[randomIndex] = 0;
				}

				bag.Add(child);
			});

			return bag.ToList();
		}
	}
}