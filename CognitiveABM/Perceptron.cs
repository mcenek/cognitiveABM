using System;
using System.Collections.Generic;
using System.Numerics;
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

        Boolean onOutPut = false;


        protected int[] indexArr { get; }

        /**
         * Constructor
         */
        public PerceptronFactory(int numberOfInputs, int numberOfOutputs, int numberOfHiddenLayers, int neuronsPerHiddenLayer)
        {
            NumberOfInputs = numberOfInputs;
            NumberOfOutputs = numberOfOutputs;
            NumberOfHiddenLayers = numberOfHiddenLayers;
            NeuronsPerHiddenLayer = neuronsPerHiddenLayer;
            _totalLayers = 2 + numberOfHiddenLayers;
            onOutPut = false;

        }

        /**
         * [Genomes description]
         * @type {[type]}
         */
        public float[] CalculatePerceptronFromId(int agentId, float[] inputs, float[] agentMemory)
        {
            return CalculatePerceptron(FCM.FCM.Agents[agentId].ToArray(), inputs, agentMemory);
        }

        /**
         * @param genomes: list of all genomes
         * @param inputs:
         * @param agentMemory:
         * @description:
         * @return:
         */
        public float[] CalculatePerceptron(float[] genomes, float[] inputs, float[] agentMemory)
        {

            Genomes = genomes;


            // initialize and set currentValues to the inputs, then all zeros (for the backward and self faceing edges)
            float[] values = inputs;


            int previousLayerHeight = NumberOfInputs;

            // should only run twice
            for (int layerNumber = 0; layerNumber < _totalLayers; layerNumber++)
            {

                // get how many nuerons are in the current layer
                int currentLayerHeight = CalculateLayerHeight(layerNumber); // should always be 9

                // get the needed weight matrix width and height

                // matrix width = 3 * NumberOfInputs based on the fact that we have forwards, self, and backwards leading edges
                int weightMatrixWidth = NumberOfInputs * 3 - (previousLayerHeight - currentLayerHeight); // should be 27
                //27 * 2
                int weightMatrixHeight = currentLayerHeight; //18,18,2 for a 18,2,1,18


                // get a matrix of weights
                float[,] weights = CreateWeightMatrix(weightMatrixWidth, weightMatrixHeight);

                if(layerNumber == _totalLayers - 1){
                  onOutPut = true;
                }

                // calculate the values of the neurons of the current layer
                values = MatrixMultiply(values, weights, layerNumber); // this


                // keep track of the hieght of the previous later
                previousLayerHeight = currentLayerHeight;

            }
            return values;

        }

        /**
         * @param layer: layer to find height
         * @description: finds the height of a given layer
         * @returns: layer height
         */
        private int CalculateLayerHeight(int layer)
        {
          //for 18 2 1 18
            if (layer == 0) // input layer
            {
                return NumberOfInputs;//18
            }
            else if (layer != _totalLayers - 1) // hidden layer
            {
                return NeuronsPerHiddenLayer;//18
            }
            else // output layer
            {
                return NumberOfOutputs;//2
            }
        }

        /**
         * @param width: width of weighted matrix
         * @param height: height of weighted matrix
         * @description: creates a matrix of weights
         * @return: weighted matrix named weights
         */
        private float[,] CreateWeightMatrix(int width, int height)
        {
            float[,] weights = new float[height, width];

            for (int i = 0; i < weights.GetLength(0); i++)
            {
                for (int j = 0; j < weights.GetLength(1); j++)
                {
                  try{

                    weights[i, j] = Genomes[_weightIndex];
                  }
                  catch{
                    Console.WriteLine(Genomes.Length);

                    System.Environment.Exit(0);
                  }
                    _weightIndex++;
                }
            }
            // Console.WriteLine(_weightIndex);
            return weights;
        }

        /**
         * @param inputs: values of matrix
         * @param weights: weights associated with the edges going to the nodes
         * @description: Multiples Matrix
         * @returns: the resulting matrix from multiplication
         */
        private float[] MatrixMultiply(float[] inputs, float[,] weights, int layerNumber)
        {
            // the resulting array will be the same length as the number of weight columns in the weight matrix

            // ouput vector length
            int outputLength = weights.GetLength(0); // should be 9
            float[] result = new float[outputLength];
            if(onOutPut){
              // // System.Environment.Exit(0);
              float first = 0.0f;
              float second = 0.0f;
              for(int i = 0; i < inputs.Length; i++){
                if( i < 9){
                  first += inputs[i];
                }
                else{
                  second += inputs[i];
                }
              }

              inputs[0] = first;
              inputs[1] = second;

            }

            Parallel.For(0, weights.GetLength(0), weightRow =>
            {
                float sum = 0;

                // iterate over the input values and the weights at the same time
                // usuage of strassen's algorithm, or one based off of it, would improve run time
                for (int i = 0; i < weights.GetLength(1); i++)
                {

                  sum += weights[weightRow, i] * inputs[weightRow];

                    if(weightRow >= weights.GetLength(0) * 3 / 4){
                    sum += weights[weightRow, i] * inputs[weightRow] * (float)4.0;
                    }
                    else if (weightRow >= weights.GetLength(0) * 2 / 4) {
                    sum += weights[weightRow, i] * inputs[weightRow] * (float)3.0;
                    }
                    else if (weightRow >= weights.GetLength(0) * 1 / 4) {
                    sum += weights[weightRow, i] * inputs[weightRow] * (float)2.0;
                    }
                    else {
                      sum += weights[weightRow, i] * inputs[weightRow];
                    }
                }
                result[weightRow] = sum;

            });

            // Console.WriteLine("Matrix Muliplication Result");
            return result;
        }



    }
}
