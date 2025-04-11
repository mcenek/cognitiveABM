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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CognitiveABM.FCM;

namespace HillClimberABMExample.General
{
    public class HillClimberFCM : FCM
    {

        private readonly int steps;
        private readonly string fitnessFileName;
        private readonly string fitnessColumnName;

        public HillClimberFCM(int population, int numberOfValues, int steps, string fitnessFileName, string fitnessColumnName, List<List<float>> genomes) : base(population, numberOfValues, genomes)
        {
            this.fitnessFileName = fitnessFileName;
            this.fitnessColumnName = fitnessColumnName;
            this.steps = steps;
        }

        //grabs the fitness values from the corresponding csv file (animal.csv)
        public override List<float> Fitness(List<List<float>> agents)
        {
            var fitnessValues = new List<float>();

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
                        fitnessValues.Add(float.Parse(values[indexOfFitnessValues]));
                    }
                }
            }

            return fitnessValues;
        }

        //Splits the chosen parent's genomes and combines them to make a child
        //The child genomes will then be mutated
        public override List<List<float>> GenerateOffspring(List<float> agentReproductionPercentages)
        {
            ConcurrentBag<List<float>> newAgents = new ConcurrentBag<List<float>>();
            Random random = new Random();

            Parallel.For(0, Population, index =>
            {
                Tuple<List<float>, List<float>> parents = PickParents(agentReproductionPercentages.ToList());
                int splitIndex = random.Next(0, NumberOfValues);

                List<float> child = new List<float>();

                List<float> parent1Genomes = new List<float>(parents.Item1.GetRange(0, splitIndex));
                List<float> parent2Genomes = new List<float>(parents.Item2.GetRange(splitIndex, parents.Item2.Count - splitIndex));
                //Console.WriteLine("{0} {1} {2} {3}", parent1Genomes.Count, parent2Genomes.Count, parents.Item2.Count - splitIndex, splitIndex);
                //System.Environment.Exit(0);

                child.AddRange(parent1Genomes);
                child.AddRange(parent2Genomes);

                for (int i = 0; i < 1000; i++)
                {
                    var randomIndex = random.Next(child.Count);
                    child[randomIndex] += noiseGen();
                    if (child[randomIndex] > 1)
                        child[randomIndex] = 1;

                    if (child[randomIndex] < 0)
                        child[randomIndex] = 0;
                }

                newAgents.Add(child);
            });
            return newAgents.ToList();
        }
        public float noiseGen()
        {
            var random = new Random();
            float noise = (float)random.NextDouble() / 50; //noise between 0 and 0.005
            int sign = random.Next(1, 3);
            if (sign == 2)
            { //noise becomes negative;
                noise = noise * -1;
            } //end for
            return noise;
        }
    }
}
