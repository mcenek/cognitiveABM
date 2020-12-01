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

        public float[] Genomes { get; set; }

        private int _totalLayers;

        private int _weightIndex = 0;

        public float[] memory;

        Boolean useMemory = false;

        public PerceptronFactory(int numberOfInputs, int numberOfOutputs, int numberOfHiddenLayers, int neuronsPerHiddenLayer)
        {
            NumberOfInputs = numberOfInputs;
            NumberOfOutputs = numberOfOutputs;
            NumberOfHiddenLayers = numberOfHiddenLayers;
            NeuronsPerHiddenLayer = neuronsPerHiddenLayer;
            _totalLayers = 2 + numberOfHiddenLayers;
            memory = new float[numberOfInputs];
        }

        public float[] CalculatePerceptronFromId(int agentId, float[] inputs, float[] agentMemory)
        {
            return CalculatePerceptron(FCM.FCM.Agents[agentId].ToArray(), inputs, agentMemory);
        }

        public float[] CalculatePerceptron(float[] genomes, float[] inputs, float[] agentMemory)
        {


            Genomes = genomes;
            // float[] outputs = new float[NumberOfOutputs];

            // initialize and set currentValues to the inputs, then all zeros (for the backward and self faceing edges)
            float[] values;

            List<float> temp = new List<float>(inputs);

            // temp.AddRange(agentMemory);
            // temp.AddRange(agentMemory);
            temp.AddRange(new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            values = temp.ToArray();

            int previousLayerHeight = NumberOfInputs;

            // should only run twice
            for (int layerNumber = 0; layerNumber < _totalLayers - 1; layerNumber++)
            {

                // get how many nuerons are in the current layer
                int currentLayerHeight = CalculateLayerHeight(layerNumber); // should always be 9

                // get the needed weight matrix width and height

                // matrix width = 3 * NumberOfInputs based on the fact that we have forwards, self, and backwards leading edges
                int weightMatrixWidth = NumberOfInputs * 3 - (previousLayerHeight - currentLayerHeight); // should be 27
                int weightMatrixHeight = currentLayerHeight; // should be 9

                // should need 243 weights (genomes) per matrix (per layer)

                // get a matrix of weights
                float[,] weights = CreateWeightMatrix(weightMatrixWidth, weightMatrixHeight);

                // calculate the values of the neurons of the current layer
                values = MatrixMultiply(values, weights); // this

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

        private float[,] CreateWeightMatrix(int width, int height)
        {
            float[,] weights = new float[height, width];

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
        private float[] MatrixMultiply(float[] inputs, float[,] weights)
        {
            // the resulting array will be the same length as the number of weight columns in the weight matrix

            // ouput vector length
            int outputLength = weights.GetLength(0); // should be 9
            float[] result = new float[outputLength];

            Parallel.For(0, weights.GetLength(0) - 1, weightRow =>
            {
                float sum = 0;

                // iterate over the input values and the weights at the same time
                for (int i = 0; i < weights.GetLength(1); i++)
                {
                    sum += weights[weightRow, i] * inputs[weightRow];
                }
                result[weightRow] = sum;
            });

            // Console.WriteLine("Matrix Muliplication Result");
            return result;
        }
    }
}