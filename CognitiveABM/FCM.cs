using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CognitiveABM.FCM
{
    public abstract class FCM
    {
        private List<List<float>> _agents;

        protected int NumberOfValues { get; }
        protected int Population { get; }

        public static List<List<float>> Agents;

        /**
         * @param population: population number
         * @param numberOfValues: number of values to have in the agents
         * @param genomes: 2d list of float values representing the genomes of agents
         * @description: creates the agents
         */
        public FCM(int population, int numberOfValues, List<List<float>> genomes)
        {
            Population = population;
            NumberOfValues = numberOfValues;
            if (genomes == null)
            {
                Agents = new List<List<float>>(population);
                for (int i = 0; i < population; i++)
                {
                    Agents.Add(CreateRandomArray(numberOfValues));
                }
            }
            else
            {
                Agents = genomes;
            }
        }

        public abstract List<float> Fitness(List<List<float>> agents);

        public abstract List<List<float>> GenerateOffspring(List<float> agentReproductionPercentages);

        /**
         * @param train: says if to train the agents
         * @param fitnessTarget: Target fitness for agents to meet
         * @param writeGenomes: says if the genomes should be written to a file
         * @description: This will train the agents. If already at target fitness it may print genomes
         * @return: agentFitness
         */
        public List<float> Run(Boolean train, float fitnessTarget, Boolean writeGenomes)
        {
            List<float> agentFitness = Fitness(Agents);

            var avg = agentFitness.Average();
            var sum = agentFitness.Sum();
            var max = agentFitness.Max();

            Console.WriteLine("Average fitness: {0:F2}, Max fitness: {1:F2}", avg, max);

            if (train)
            {
                if (avg >= fitnessTarget)
                {
                    Console.WriteLine("FitnessTarget met.");
                    if (writeGenomes)
                    {
                        WriteGenomes("genomes.csv");
                    }
                    ABM.GlobalTargetFitnes = avg;
                }

                if (sum == 0)
                {
                    for (int i = 0; i < agentFitness.Count; i++)
                    {
                        agentFitness[i] = 1;
                    }
                }

                List<float> agentReproductionPercentages = CalculateReproductionPercent(agentFitness.ToList());
                var index = agentFitness.IndexOf(agentFitness.Max());
                var bestAgent = Agents[index];
                //Console.WriteLine( " BEST FITNESS {0}", agentFitness[index]);
                Agents = GenerateOffspring(agentReproductionPercentages);

                Agents[95] = bestAgent;

            }

            return agentFitness;
        }

        /**
         * @param agentReproductionProbability: list of the probabilities of an agent reproducing
         * @description: Picks two random parents via a weight system based on parent fitness
         * @return: A tuple of the parents
         */
        protected Tuple<List<float>, List<float>> PickParents(List<float> agentReproductionProbabilites)
        {
            int firstParentIndex = SelectRandomWeightedIndex(agentReproductionProbabilites);
            float temp = agentReproductionProbabilites[firstParentIndex];
            //Console.WriteLine(firstParentIndex);
            //System.Environment.Exit(0);

            agentReproductionProbabilites[firstParentIndex] = 0; // first parent cannot be picked twice

            int secondParentIndex = SelectRandomWeightedIndex(agentReproductionProbabilites);
            //Console.WriteLine(secondParentIndex);

            agentReproductionProbabilites[firstParentIndex] = temp;
            //Console.WriteLine("{0} {1}", agentReproductionProbabilites[firstParentIndex], agentReproductionProbabilites[secondParentIndex]);
            //System.Environment.Exit(0);
            return Tuple.Create(Agents[firstParentIndex], Agents[secondParentIndex]);
        }

        /**
         * @param weights: list of weight values
         * @description: Makes a random value and compares it to the summation of weights
         * @return: If the value < weight, it returns the position of the last weight
         */
        private int SelectRandomWeightedIndex(List<float> weights)
        {
            Random random = new Random();
            float value = (float)random.NextDouble() * weights.Sum();
            float sum = 0;
            return weights.IndexOf(weights.Max());
            for (int i = 0; i < weights.Count; i++){
                sum += weights.ElementAt(i);
                if (value < sum){
                    return i;
                }
            }
            return weights.Count-1;
        }

        /**
         * @param length: length of array to be made
         * @returns a random array
         */
        private List<float> CreateRandomArray(int length)
        {
            Random random = new Random();
            return Enumerable.Repeat(0, length).Select(i => (float)random.NextDouble()).ToList();
            // return Enumerable.Repeat(0, length).Select(i => 1.0).ToList();
        }

        /**
         * @param agentFitness: list of fitness values for the agents
         * @description: calculates the reproduction percent using a multiplier
         * @description: multipler is 1.5 if fitness value > average fitness, 1 otherwise
         * @return: reproduction percent
         */
        private List<float> CalculateReproductionPercent(List<float> agentFitness)
        {
            List<float> reproductionPercent = new List<float>();
            float sumOfFitnessValues = agentFitness.Sum();
            if(sumOfFitnessValues < 0){
                sumOfFitnessValues = 1;
            }
            float averageFitness = AverageFitness();

            foreach (float fitnessValue in agentFitness) //goes through agent list and calculates their reproduction odds
            {
                float agentReproductionPercent;
                float multiplier = 0.25f;
                if(fitnessValue > averageFitness){
                    multiplier = 2.5f;
                }
                if(fitnessValue == agentFitness.Max()){ //gives bonus if agent was the highest scoring
                    multiplier = 5.0f;
                }
                if(fitnessValue < 0){
                    agentReproductionPercent = 0;
                }
                else{
                    agentReproductionPercent = (fitnessValue * multiplier) / sumOfFitnessValues;
                }
                reproductionPercent.Add(agentReproductionPercent);
            }

            return reproductionPercent;
        }

        /**
         * @description: gets the genomes and combines them into a string
         * @return: a string of the genomes
         */
        public StringBuilder GetGenomes()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < Population; i++)
            {
                output.Append(string.Join(",", Agents[i]) + "\n");
            }
            return output;
        }

        /**
         * @param filename: name of file to be outputed
         * @description: Writes the genomes to an output file
         */
        public void WriteGenomes(string filename)
        {
            string path = ".\\output\\" + filename;
            var writer = new StreamWriter(path: path);
            for (int i = 0; i < Population; i++)
            {
                writer.Write(string.Join(",", Agents[i]) + "\n");
            }
            writer.Close();
        }

        /**
         * @return: the max fitness of the agents
         */
        public float AverageFitness()
        {
            return Fitness(Agents).Sum() / Agents.Count;
        }

        /**
         * @return: the max fitness of the agents
         */
        public float MaxFitness()
        {
            return Fitness(Agents).Max();
        }
    }
}
