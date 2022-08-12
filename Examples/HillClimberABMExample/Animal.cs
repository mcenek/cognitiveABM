/**

This code represents the decision making and information for a single hill climber agent.

What does it do: Upon each step the agent moves to what it thinks is the next best location, based upon either the perceptron output or the q-learning module.

This code was originally generated by the Mars library, then we altered it to use the perceptron as a part of it's decision making process.

*/

namespace HillClimberExample
{
    using System;
    using System.Collections.Concurrent;
    using Mars.Interfaces.Layer;
    using Mars.Components.Environments;
    using Mars.Common.Logging;
    using System.Collections.Generic;
    using CognitiveABM.Perceptron;
    using CognitiveABM.QLearning;
    using System.IO;
    //using CognitiveABM.Program;


    public class Animal : Mars.Interfaces.Agent.IMarsDslAgent
    {
        private static readonly ILogger _Logger = LoggerFactory.GetLogger(typeof(Animal));


        private readonly float[] AgentMemory;

        private bool useDistantView = false;

        private readonly int startingElevation;
        //values for size of reward map
        private int height;
        private int length;


        public Guid ID { get; }

        public Mars.Interfaces.Environment.Position Position { get; set; }

        public bool Equals(Animal other) => Equals(ID, other.ID);

        public override int GetHashCode() => ID.GetHashCode();

        public QLearning qLearn = new QLearning();

        public float[,] rewardMap;

        public static ConcurrentDictionary<int, List<(int,int)>> rewardMemory = new ConcurrentDictionary<int, List<(int,int)>>();

        public Boolean containsReward = true;

        public int tickNum = 0;

        private string rule = default;

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

            rewardMap = readRewards();
        }

        // Tick function is called on each step of the simulation
        public void Tick()
        {

            List<int[]> distantTerrainLocations = null;
            List<int[]> adjacentTerrainLocations = GetAdjacentTerrainPositions();

            Tuple<List<float>, List<float>> adjacentTerrainTuple = GetAdjacentTerrainInfo();

            Boolean onActiveReward = false;
            Boolean stayPut = false;

            float[] distantTerrainElevations = null;
            float[] adjacentTerrainElevations = GetAdjacentElevations();
            float[] rewards = adjacentTerrainTuple.Item2.ToArray();

            int xPos = (int)Position.X;
            int yPos = (int)Position.Y;



            //for if we want to try 18 inputs
            float[] inputs = new float[adjacentTerrainElevations.Length + rewards.Length];
            Array.Copy(adjacentTerrainElevations, inputs, adjacentTerrainElevations.Length);
            Array.Copy(rewards, 0, inputs, adjacentTerrainElevations.Length, rewards.Length);
            // Console.WriteLine(inputs.Length);
            // for(int i = 0; i < inputs.Length; i++){
            //   Console.WriteLine(inputs[i]);
            // }


            // Array.Copy(adjacentTerrainElevations, 0, inputs, rewards.Length, adjacentTerrainElevations.Length);

            // //try normalizing elevations
            // float[] inputs = new float[adjacentTerrainElevations.Length + rewards.Length];
            // Array.Copy(rewards, inputs, rewards.Length);
            // Array.Copy(rewards, 0, inputs, rewards.Length, rewards.Length);
            //try one hidden layer 18 inputs
            PerceptronFactory perceptron = new PerceptronFactory(18, 2, 1, 18);
            float[] outputs = perceptron.CalculatePerceptronFromId(AnimalId, inputs, AgentMemory);

            if(outputs[1] > outputs[0]){
              stayPut = true;
            }
            if(rewards[4] != 0.0f){
              // Console.WriteLine("checkingOnActive");
              //Console.WriteLine(rewards[4]);
              onActiveReward = isOnActiveReward(xPos, yPos);

            }
            if(stayPut && onActiveReward){
              //Console.WriteLine("YAY");
              pickUpReward(xPos, yPos);
              // Console.WriteLine("WOOPICKED Up");
              // foreach (KeyValuePair<int, List<(int,int)>> agent in rewardMemory){
              //   for(int i = 0; i < agent.Value.Count; i++)
              //   Console.WriteLine("id = {0}, Coords{1}", agent.Key, agent.Value[i]);
                
              //   }
            }

            //want to know if my current spot is an active reward spot

            // outputs.CopyTo(AgentMemory, 0);
            // outputs.CopyTo(AgentMemory, outputs.Length);

            //change terrainElevations into a matrix
            //adjacentTerrainElevations contains 9 elements, so we need 3x3 matrix
            int index = 0;
            float[,] landscapePatch = new float[3, 3];

            float min = adjacentTerrainElevations[index];
            float max = adjacentTerrainElevations[index];

            if(useDistantView == true){ //agent uses distant view
              distantTerrainLocations = GetDistantTerrainPositions();
              distantTerrainElevations = GetDistantTerrainElevations();
              min = distantTerrainElevations[index];
              max = distantTerrainElevations[index];
            }

            for (int x = 0; x < 3; x++) //Getting patch values and turning it into a matrix
            {
                for (int y = 0; y < 3; y++)
                {
                    if(useDistantView){//set landscape to distantTerrainElevations + 10*rewards
                      if(distantTerrainElevations[index] < min){
                         min = distantTerrainElevations[index];
                        }
                        if(distantTerrainElevations[index] > max){
                        max = distantTerrainElevations[index];
                        }
                      landscapePatch[x, y] = distantTerrainElevations[index]  + (50/distantTerrainElevations[index] * rewards[index]);
                    }
                    else{//set landscape to adjacentTerrainElevations + 10*rewards
                      if(adjacentTerrainElevations[index] < min){
                          min = adjacentTerrainElevations[index];
                      }
                      if(adjacentTerrainElevations[index] > max){
                          max = adjacentTerrainElevations[index];
                      }
                      landscapePatch[x, y] = adjacentTerrainElevations[index] + (10 * rewards[index]);
                    }
                    index++;
                }
            }


            int direction;

            if(stayPut){//don't move cus we on reward
              direction = 4;
              this.qLearn.savePathandExportValues(this.AnimalId,-1,-1,landscapePatch,this.tickNum, Elevation, xPos, yPos);

            }
            else{
              direction = this.qLearn.getDirection(landscapePatch, min, max,this.AnimalId, this.tickNum, Elevation, xPos, yPos); //Which dirction we should be moving
            }

            int[] newLocation = adjacentTerrainLocations[direction]; //direction = 4

            //add location to memory


            //MoveTo (animal object, location, traveling distance)

            Terrain._AnimalEnvironment.MoveTo(this, newLocation[0], newLocation[1], 1, predicate: null);
            Elevation = Terrain.GetIntegerValue(this.Position.X, this.Position.Y);

            BioEnergy = calculateBioEnergy(stayPut, onActiveReward);

            this.tickNum++;
        }

        // helper methods

        //Checks to see if agent is on a reward that it has already collected
        //Will pick up rewards if not already picked up
        //Returns true if reward picked up, false if reward was already picked up
        public void pickUpReward(int xPos, int yPos){
            if(!rewardMemory.ContainsKey(this.AnimalId)){//if no items in dictionary, safely TryAdd
              List<(int,int)> tempList = new List<(int,int)>();
              tempList.Add((xPos, yPos));
              rewardMemory.TryAdd(this.AnimalId, tempList);
            }

            //animal exists in dictionary
            //since onActiveReward is true, agent can update without checking if reward was already picked up
            else{
              List<(int,int)> tempList = rewardMemory[this.AnimalId];
              tempList.Add((xPos,yPos));
              rewardMemory.TryUpdate(this.AnimalId,tempList,rewardMemory[this.AnimalId]);
            }
        }

        //checks if current location contains an active reward
        public Boolean isOnActiveReward(int xPos, int yPos){
          if(rewardMemory.ContainsKey(this.AnimalId)){
            foreach((int,int) coord in rewardMemory[this.AnimalId]){
              if(coord.Item1 == xPos && coord.Item2 == yPos){
                return false;
              }
            }//end foreach
            return true;
          }//end if
          return true;
        }

        //calculates the BioEnergy score based from agent's action when on a reward
        //playing with values is a good idea
        public int calculateBioEnergy(Boolean stayPut, Boolean onActiveReward){
          int BioEnergy = 0;

          //if staying put on reward
          if(stayPut && onActiveReward){
            BioEnergy = (Elevation < 0) ? 100 : 50 * Elevation;
          }
          //if staying put on non-reward
          if(stayPut && !onActiveReward){
            BioEnergy = -10 ;
          }
          //if moving on reward
          if(!stayPut && onActiveReward){
            BioEnergy = 0;
          }
          //if moving on non-reward
          if(!stayPut && !onActiveReward){
            BioEnergy = Elevation;
          }


          return BioEnergy;
        }

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
                    elevations.Add((float)Terrain.GetRealValue(dx + x, dy + y));
                    //if looking out of bounds of reward Maps
                    //having a variable that says the map size would be more ideal
                    float reward;
                    if(dx + x >= length || dy + y >= height || dx + x < 0 || dy + y < 0){
                      reward = 0.0f;
                    }
                    else{

                      reward = rewardMap[dx + x, dy + y];
                    }
                    rewards.Add(reward);
                }

            }
            Tuple<List<float>, List<float>> terrain = new (elevations, rewards);

            return terrain;
        }
        private float[] GetAdjacentElevations(){
            List<float> elevations = new List<float>();
            int x = (int)Position.X;
            int y = (int)Position.Y;
            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    elevations.Add((float)Terrain.GetRealValue(dx + x,dy + y));
                }
            }

            return elevations.ToArray();
        }
        /**
            * @description: creates a 3x3 grid of the agents terrain distant surroundings,
            * condences its 7x7 view into 3x3 only saving the cardinal and diagonal information
            * @return: list of the terrains elevation
        */
        private float[] GetDistantTerrainElevations(){
            List<float> elevations = new List<float>();
            int x = (int)Position.X;
            int y = (int)Position.Y;
            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    int newX = 7*dx + x;
                    int newY = 7*dy + y;
                    //need check for newX < 0
                    if(newX >= length || newX < 0){ //if the newX value is out of bounds use adjacent view
                        newX = dx + x;
                    }
                    if(newY >= height || newY < 0){
                        newY = dy + y;
                    }
                    elevations.Add((float)Terrain.GetRealValue(newX, newY));
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

        /**
         *  @description: creates a 3x3 grid of the agents terrain distant surroundings,
        * condences its 7x7 view into 3x3 only saving the cardinal and diagonal information
        * @return: list of the distant positions coordinates
        */
        private List<int[]> GetDistantTerrainPositions()
        {
            List<int[]> locations = new List<int[]>();
            int x = (int)Position.X;
            int y = (int)Position.Y;

            for (int dy = 1; dy >= -1; --dy)
            {
                for (int dx = -1; dx <= 1; ++dx)
                {
                    int newX = 7*dx + x;
                    int nexY = 7*dy + y;
                    if(newX >= 49){
                        newX = dx + x;
                    }
                    if(nexY >= 49){
                        nexY = dx + y;
                    }
                    int[] location = new int[] { newX, nexY };
                    locations.Add(location);
                }
            }

            return locations;
        }

         private float[,] readRewards(){
            string path = Program.terrainFilePath;
            string filePath = path.Replace(".csv", "_reward.csv");
            int counter = 0;
            int x = 0;
            int y = 0;
            height = 50;
            length = 50;
            float[,] rewardMap = new float[50,50];
            if(File.Exists(filePath)){
            using(var reader = new StreamReader(filePath)){//gets dimentions of reward map
                while(!reader.EndOfStream){
                  string line = reader.ReadLine();
                  string[] values = line.Split(',');
                  for(x = 0; x < 50; x++){
                    rewardMap[y, x] = float.Parse(values[x]);
                  }
                  length = values.Length;
                  height = length;
                  y++;
                }
            }
          }
            return rewardMap;
        }
    }
}
