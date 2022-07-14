/**

This code represents the decision making and information for a single hill climber agent.

What does it do: Upon each step the agent moves to what it thinks is the next best location, based upon either the perceptron output or the q-learning module.

This code was originally generated by the Mars library, then we altered it to use the perceptron as a part of it's decision making process.

*/

namespace HillClimberExample
{
    using System;
    using Mars.Interfaces.Layer;
    using Mars.Components.Environments;
    using Mars.Common.Logging;
    using System.Collections.Generic;
    using CognitiveABM.Perceptron;
    using CognitiveABM.QLearning;
    using System.IO;


    public class Animal : Mars.Interfaces.Agent.IMarsDslAgent
    {
        private static readonly ILogger _Logger = LoggerFactory.GetLogger(typeof(Animal));


        private readonly float[] AgentMemory;

        private readonly int startingElevation;

        public Guid ID { get; }

        public Mars.Interfaces.Environment.Position Position { get; set; }

        public bool Equals(Animal other) => Equals(ID, other.ID);

        public override int GetHashCode() => ID.GetHashCode();

        public QLearning qLearn = new QLearning();

        public int tickNum = 0;

        private string rule = default;

        protected List<Tuple<int, int>> memory; //list of where its been

        public string Rule
        {
            get => rule;
            set
            {
                if (rule != value) rule = value;
            }
        }

        private int animalId = default;

        public int AnimalId
        {
            get => animalId;
            set
            {
                if (animalId != value) animalId = value;
            }
        }

        private int bioEnergy = default;

        public int BioEnergy
        {
            get => bioEnergy;
            set
            {
                if (bioEnergy != value) bioEnergy = value;
            }
        }

        private int elevation = default;

        public int Elevation
        {
            get => elevation;
            set
            {
                if (elevation != value) elevation = value;
            }
        }

        internal int executionFrequency;

        public Terrain Terrain { get; set; }

        [Mars.Interfaces.LIFECapabilities.PublishForMappingInMars]
        public Animal(Guid _id, Terrain _layer, RegisterAgent _register, UnregisterAgent _unregister, SpatialHashEnvironment<Animal> _AnimalEnvironment, int AnimalId, double xcor = 0, double ycor = 0, int freq = 1)
        {
            ID = _id;
            Terrain = _layer;
            this.AnimalId = AnimalId;
            executionFrequency = freq;

            Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
            var pos = InitialPosition();
            Position = Mars.Interfaces.Environment.Position.CreatePosition(pos.Item1, pos.Item2);

            Terrain._AnimalEnvironment.Insert(this);
            _register(_layer, this, freq);


            Elevation = Terrain.GetIntegerValue(Position.X, Position.Y);
            startingElevation = Elevation;

            //AgentMemory is functionally useless right now
            //it goes into perceptron, but it's useage is commented out
            AgentMemory = new float[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        // Tick function is called on each step of the simulation
        public void Tick()
        {

            /**FCM*/
            //-----FCM----//
            // var inputs = GetAdjacentTerrainElevations();
            // int highestInput = 0;
            // for (int i = 0; i < 9; i++)
            // {
            //     if (inputs[i] > inputs[highestInput])
            //     {
            //         highestInput = i;
            //     }
            // }
            //
            // Boolean atPeak = highestInput == 4;
            // PerceptronFactory perceptron = new PerceptronFactory(9, 9, 1, 9);
            // float[] outputs = perceptron.CalculatePerceptronFromId(AnimalId, inputs, AgentMemory);
            // outputs.CopyTo(AgentMemory, 0);
            // outputs.CopyTo(AgentMemory, outputs.Length);
            // //more want outputs, aka a list of floats
            //List<int[]> locations = GetAdjacentTerrainPositions();//leave alone
            //
            // int highestOutput = 0;
            // for (int i = 0; i < 9; i++)
            // {
            //     if (outputs[i] > outputs[highestOutput])
            //     {
            //         highestOutput = i;
            //     }
            // }
            //int[] newLocation = locations[highestOutput];

            /**QLearn*/
            List<int[]> adjacentTerrainLocations = GetAdjacentTerrainPositions();
            List<int[]> distantTerrainLocations = GetDistantTerrainPositions();

            Tuple<List<float>, List<float>> adjacentTerrainTuple = GetAdjacentTerrainInfo(); 
            float[] adjacentTerrainElevations = adjacentTerrainTuple.Item1.ToArray();
            float[] rewards = adjacentTerrainTuple.Item2.ToArray();
            float[] distantTerrainElevations = GetDistantTerrainElevations();

            //get reward 
            //convert back

            //change terrainElevations into a matrix
            //adjacentTerrainElevations contains 9 elements, so we need 3x3 matrix
            int index = 0;
            float[,] landscapePatch = new float[3, 3];
            float min = adjacentTerrainElevations[index];
            float max = adjacentTerrainElevations[index];
            // float min = distantTerrainElevations[index];
            // float max = distantTerrainElevations[index];
            for (int x = 0; x < 3; x++) //Getting patch values and turning it into a matrix
            {
                for (int y = 0; y < 3; y++)
                {
                    if(adjacentTerrainElevations[index] < min){
                      min = adjacentTerrainElevations[index];
                    }
                    if(adjacentTerrainElevations[index] > max){
                      max = adjacentTerrainElevations[index];
                    }
                    landscapePatch[x, y] = adjacentTerrainElevations[index];
                    index++;

                    // if(distantTerrainElevations[index] < min){
                    //   min = distantTerrainElevations[index];
                    // }
                    // if(distantTerrainElevations[index] > max){
                    //   max = distantTerrainElevations[index];
                    // }
                    // landscapePatch[x, y] = distantTerrainElevations[index];
                    // index++;

                }
            }
            int xPos = (int)Position.X;
            int yPos = (int)Position.Y;
            int direction = this.qLearn.getDirection(landscapePatch, min, max, this.AnimalId, this.tickNum, Elevation, xPos, yPos); //Which dirction we should be moving
            if(onReward(rewards)){
                direction = 4;
            }
            int[] newLocation = adjacentTerrainLocations[direction]; //direction = 4 

            //add location to memory


            //MoveTo (animal object, location, traveling distance)

            Terrain._AnimalEnvironment.MoveTo(this, newLocation[0], newLocation[1], 1, predicate: null);
            this.qLearn.setExportValues(landscapePatch,this.AnimalId, this.tickNum, Elevation, xPos, yPos);
            Elevation = Terrain.GetIntegerValue(this.Position.X, this.Position.Y);
            BioEnergy = (Elevation < 0) ? 0 : Elevation;
            this.tickNum++;
        }

        // helper methods

        private Tuple<int, int> InitialPosition()
        {
            //var random = new Random(18);
            //var random = new Random(ID.GetHashCode()); //using hard coded value for testing
            //return new Tuple<int, int>(random.Next(Terrain.DimensionX()), random.Next(Terrain.DimensionY()));
            //make all agents start at same spot
            var random = new Random(); //seed

            //Puts agents on border of map
            //Case 0: Along Y axis (left)
            //Case 1: Along X axis (bottom)
            //Case 2: Along opposite Y axis (right)
            //Case 3: Along opposite X axis (top)
            switch (random.Next(4)){
              case 0:
                return new Tuple<int, int>(0, random.Next(Terrain.DimensionY()));
                break;
              case 1:
                return new Tuple<int, int>(random.Next(Terrain.DimensionX()), 0);
                break;
              case 2:
                return new Tuple<int, int>(Terrain.DimensionX(), random.Next(Terrain.DimensionY()));
                break;
              case 3:
                return new Tuple<int, int>(random.Next(Terrain.DimensionX()),Terrain.DimensionY());
                break;
              default:
                return new Tuple<int, int>(0, 0);
                Console.Write("Default Position");
                break;
            }

        }
        private bool onReward(float[] rewards){

            if(rewards[4] == 1){
                for(int i = 0; i < memory.Count; i++ ){
                    if(memory[i].Item1 == (int)Position.X && memory[i].Item2 == (int)Position.X){//reward has already been collected by agent
                        return false;
                    }
                }
                Tuple<int, int> position = new ((int)Position.X, (int)Position.X);
                memory.Add(position);
                return true;
            }
            return false;
        }

        private Tuple<List<float>, List<float>> GetAdjacentTerrainInfo()
        {
            List<float> elevations = new List<float>();
            List<float> rewards = new List<float>();
            int x = (int)Position.X;
            int y = (int)Position.Y;

            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    elevations.Add((int)Terrain.GetRealValue(dx + x, dy + y));
                    float reward = (float)Terrain.GetRealValue(dx + x, dy + y)  - (int)Terrain.GetRealValue(dx + x, dy + y); //Should be 0.1 0.0
                    if(reward == 0.0){
                        rewards.Add(0);
                    }
                    else if (reward == 0.1){
                        rewards.Add(1);
                    }
                }
            }
            Tuple<List<float>, List<float>> terrain = new (elevations, rewards);
            
            return terrain;
        }
        //upgrade to 7x7 or 9x9
        private float[] GetDistantTerrainElevations(){
            List<float> elevations = new List<float>();
            int x = (int)Position.X;
            int y = (int)Position.Y;
            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    elevations.Add((float)Terrain.GetRealValue(7*dx + x, 7*dy + y));
                }
            }

            return elevations.ToArray();
        }

        private List<int[]> GetAdjacentTerrainPositions()
        {
            List<int[]> locations = new List<int[]>();
            int x = (int)Position.X;
            int y = (int)Position.Y;

            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    int[] location = new int[] { dx + x, dy + y };
                    locations.Add(location);
                }
            }

            return locations;
        }

        private List<int[]> GetDistantTerrainPositions()
        {
            List<int[]> locations = new List<int[]>();
            int x = (int)Position.X;
            int y = (int)Position.Y;

            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    int[] location = new int[] { 7*dx + x, 7*dy + y };
                    locations.Add(location);
                }
            }

            return locations;
        }


    }
}
