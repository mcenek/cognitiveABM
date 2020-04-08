using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveABM.Perceptron
{
    public class PerceptronFactory
    {
        protected int NumberOfInputs { get; }

        protected int NumberOfOutputs { get; }

        protected int NumberOfHiddenLayers { get; }

        protected int NeuronsPerHiddenLayer { get; }

        public double[] Genomes { get; set; }

        private int _totalLayers;

        private int _weightIndex = 0;

        public PerceptronFactory(int numberOfInputs, int numberOfOutputs, int numberOfHiddenLayers, int neuronsPerHiddenLayer)
        {
            NumberOfInputs = numberOfInputs;
            NumberOfOutputs = numberOfOutputs;
            NumberOfHiddenLayers = numberOfHiddenLayers;
            NeuronsPerHiddenLayer = neuronsPerHiddenLayer;
            _totalLayers = 2 + numberOfHiddenLayers;
        }

        public double[] CalculatePerceptron(double[] genomes, double[] inputs)
        {
            Genomes = genomes;
            // double[] outputs = new double[NumberOfOutputs];

            // initialize and set currentValues to the inputs, then all zeros (for the backward and self faceing edges)
            double[] values = new double[NumberOfInputs * 3];
            for (int i = 0; i < (inputs.Length * 3) - 1; i++)
            {
                if (i < inputs.Length)
                {
                    values[i] = inputs[i];
                }
                else
                {
                    values[i] = 0;
                }
            }

            int previousLayerHeight = NumberOfInputs;

            for (int i = 0; i < values.Length; i++)
            {
                Console.Write(values[i] + " ");
            }
            Console.WriteLine();

            for (int layerNumber = 0; layerNumber < _totalLayers - 1; layerNumber++)
            {

                // get how many nuerons are in the current layer
                int currentLayerHeight = CalculateLayerHeight(layerNumber);

                // get the needed weight matrix width and height

                // matrix width = 3 * NumberOfInputs based on the fact that we have forwards, self, and backwards leading edges
                int weightMatrixWidth = NumberOfInputs * 3 - (previousLayerHeight - currentLayerHeight);
                int weightMatrixHeight = currentLayerHeight;

                // get a matrix of weights
                double[,] weights = CreateWeightMatrix(weightMatrixWidth, weightMatrixHeight);

                // calculate the values of the neurons of the current layer
                values = MatrixMultiply(values, weights);

                for (int i = 0; i < values.Length; i++)
                {
                    Console.Write(values[i] + " ");
                }
                Console.WriteLine();

                // keep track of the hieght of the previous later
                previousLayerHeight = currentLayerHeight;
            }

            return values;
        }

        private int CalculateLayerHeight(int layer)
        {
            if (layer == 0) // input layer
            {
                return NumberOfInputs;
            }
            else if (layer != _totalLayers - 1) // hidden layer
            {
                return NeuronsPerHiddenLayer;
            }
            else // output layer
            {
                return NumberOfOutputs;
            }
        }

        private double[,] CreateWeightMatrix(int width, int height)
        {
            double[,] weights = new double[height, width];

            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                    weights[i, j] = Genomes[_weightIndex];
                    _weightIndex++;
                }
            }
            return weights;
        }

        // inputs are the values
        // weights are the weights associated with the edges going to the nodes we are calculating the values for
        private double[] MatrixMultiply(double[] inputs, double[,] weights)
        {
            // the resulting array will be the same length as the number of weight columns in the weight matrix
            double[] result = new double[weights.GetLength(1)];

            Parallel.For(0, weights.GetLength(1) - 1, weightRow =>
            {
                double sum = 0;

                // iterate over the input values and the weights at the same time
                for (int i = 0; i < weights.GetLength(0); i++)
                {
                    sum += weights[i, weightRow] * inputs[i];
                }
                result[weightRow] = sum;
            });

            return result;
        }
    }
}