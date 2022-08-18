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

            List<int[]> distantTerrainLocations = GetDistantTerrainPositions();
            List<int[]> adjacentTerrainLocations = GetAdjacentTerrainPositions();

            Tuple<List<float>, List<float>> adjacentTerrainTuple = GetAdjacentTerrainInfo();
            Tuple<List<float>, List<float>> distantTerrainTuple = GetDistantTerrainInfo();

            Boolean onActiveReward = false;
            Boolean stayPut = false;

            float[] distantTerrainElevations = distantTerrainTuple.Item1.ToArray();
            float[] adjacentTerrainElevations = adjacentTerrainTuple.Item1.ToArray();
            float[] rewards = new float[9];

            int xPos = (int)Position.X;
            int yPos = (int)Position.Y;
            float[] inputs = null;

            if(useDistantView){
              rewards = distantTerrainTuple.Item2.ToArray();
              rewards = agentReward(rewards, distantTerrainLocations);
              inputs = new float[distantTerrainElevations.Length + rewards.Length];
              NormInput(distantTerrainElevations).CopyTo(inputs,0);
              NormInput(rewards).CopyTo(inputs,distantTerrainElevations.Length);
            }
            else{
              rewards = adjacentTerrainTuple.Item2.ToArray();
              rewards = agentReward(rewards, adjacentTerrainLocations);
              inputs = new float[adjacentTerrainElevations.Length + rewards.Length];
              NormInput(adjacentTerrainElevations).CopyTo(inputs,0);
              NormInput(rewards).CopyTo(inputs,adjacentTerrainElevations.Length);
            }



            //try one hidden layer 18 inputs
            PerceptronFactory perceptron = new PerceptronFactory(18, 2, 1, 18);
            float[] outputs = perceptron.CalculatePerceptronFromId(AnimalId, inputs, AgentMemory);

            if(outputs[1] > outputs[0]){
              stayPut = true;
            }
            if(rewards[4] != 0.0f){
              onActiveReward = isOnActiveReward(xPos, yPos);
            }
            if(stayPut && onActiveReward){
              pickUpReward(xPos, yPos);
            }

            //change terrainElevations into a matrix
            //adjacentTerrainElevations contains 9 elements, so we need 3x3 matrix
            float[,] landscapePatch = new float[3, 3];
            float min = adjacentTerrainElevations[0];
            float max = adjacentTerrainElevations[0];
            Tuple<float[,],float,float> tupleItem = null;

            if(useDistantView){ //agent uses distant view
              min = distantTerrainElevations[0];
              max = distantTerrainElevations[0];
              tupleItem = setLandscapeMinMax(landscapePatch, min, max, distantTerrainElevations);
            }
            else{
              tupleItem = setLandscapeMinMax(landscapePatch, min, max, adjacentTerrainElevations);
            }

            landscapePatch = tupleItem.Item1;
            min = tupleItem.Item2;
            max = tupleItem.Item3;


            Tuple<float[,], float, float> landTuple = addRewardToLand(landscapePatch,rewards,min,max);
            landscapePatch = landTuple.Item1;
            min = landTuple.Item2;
            max = landTuple.Item3;

            int direction;

            if(stayPut){//don't move cus we on reward
              direction = 4;
              this.qLearn.savePathandExportValues(this.AnimalId,-1,-1,landscapePatch,this.tickNum, Elevation + (25 * rewards[4]), xPos, yPos);
            }
            else{
              direction = this.qLearn.getDirection(landscapePatch, min, max,this.AnimalId, this.tickNum, Elevation + (25 * rewards[4]), xPos, yPos); //Which dirction we should be moving
            }

            int[] newLocation = null;
            newLocation = adjacentTerrainLocations[direction];

            Terrain._AnimalEnvironment.MoveTo(this, newLocation[0], newLocation[1], 1, predicate: null);
            Elevation = Terrain.GetIntegerValue(this.Position.X, this.Position.Y);

            BioEnergy += calculateBioEnergy(stayPut, onActiveReward);
            if(BioEnergy < 0){
              BioEnergy = 0;
            }
            stayPut = false;
            this.tickNum++;
        }


        public Tuple<float[,],float,float> setLandscapeMinMax(float[,] landscapePatch, float min, float max, float[] elevationArr){
          int index = 0;
          for (int x = 0; x < 3; x++) //Getting patch values and turning it into a matrix
          {
              for (int y = 0; y < 3; y++)
              {
                  if(useDistantView){//set landscape to distantTerrainElevations + 10*rewards
                    if(elevationArr[index] < min){
                       min = elevationArr[index];
                      }
                      if(elevationArr[index] > max){
                      max = elevationArr[index];
                      }
                    landscapePatch[x, y] = elevationArr[index];
                  }
                  else{//set landscape to adjacentTerrainElevations + 10*rewards
                    if(elevationArr[index] < min){
                        min = elevationArr[index];
                    }
                    if(elevationArr[index] > max){
                        max = elevationArr[index];
                      }
                    landscapePatch[x, y] = elevationArr[index];
                  }
                  index++;
              }
          }

          return new (landscapePatch,min,max);

        }

        //temp normilization method that will normilize the first part of input
        //This normalizes the elevations and leaves the rewards alone
        //Rewards currently already normalized so for time sake this is done this way
        public float[] NormInput(float[] input){
          float min = 0.0f;
          float max = 0.0f;
          for(int i = 0; i < input.Length; i++){
            if(min > input[i]){
              min = input[i];
            }
            if(max < input[i]){
              max = input[i];
            }
          }

          float diff = Math.Abs(max - min);

          for(int k = 0; k < input.Length; k++){
            if(diff == 0.0f){
              input[k] = 0;
            }
            else{
              input[k] = (input[k] - min)/(max-min);
            }
          }
          return input;
        }

        //Will create a landscape map with reward bonuses added
        //Changes min and max accordingly
        public Tuple<float[,],float,float> addRewardToLand(float[,] landScape, float[] rewards, float min, float max){
          int index = 0;
          Boolean foundMin = false;
          Boolean foundMax = true;

          for(int row = 0; row < 3; row++){
            for(int col = 0; col < 3; col++){
              if(!foundMin && landScape[row,col] == min){
                min += 25 * rewards[index];
                foundMin = true;
              }
              if(!foundMax && landScape[row,col] == max){
                max += 25 * rewards[index];
                foundMax = true;
              }

              landScape[row,col] += 25 * rewards[index];
              index++;
            }
          }

          return  new (landScape,min,max);

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
            BioEnergy = 10;
            ABM.pickUpStat[0]++;
          }
          // else{
          //   BioEnergy = 0;
          // }
          //if staying put on non-reward
          if(stayPut && !onActiveReward){
            BioEnergy = -2;
            }
          //if moving on reward
          if(!stayPut && onActiveReward){
            BioEnergy = -2;
            ABM.pickUpStat[1]++;
          }
          //if moving on non-reward
          if(!stayPut && !onActiveReward){
            BioEnergy = 1;
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

        private Tuple<List<float>, List<float>> GetDistantTerrainInfo()
        {
            List<float> elevations = new List<float>();
            List<float> rewards = new List<float>();
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
                    //if looking out of bounds of reward Maps
                    //having a variable that says the map size would be more ideal
                    float reward;
                    if(newX >= length || newY >= height || newX < 0 || newY < 0){
                      reward = 0.0f;
                    }
                    else{
                        reward = rewardMap[newX, newY];
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
            string line = "";
            string[] values = null;
            if(File.Exists(filePath)){
            using(var reader = new StreamReader(filePath)){//gets dimentions of reward map
                while(!reader.EndOfStream){
                  line = reader.ReadLine();
                  values = line.Split(',');
                  for(y = 0; y < 50; y++){
                    rewardMap[x, y] = float.Parse(values[x]);
                  }
                  length = values.Length;
                  height = length;
                  x++;
                }
            }
          }
            return rewardMap;
        }

        //checks if reward on reward map was already collected (already done in different method now)
        private float[] agentReward(float[] rewards, List<int[]> adjacentTerrainLocations){
          if(rewardMemory.ContainsKey(this.AnimalId) == false){
            return rewards;
          }
          List<(int, int)> temp = new List<(int, int)>();
          for(int i = 0; i < adjacentTerrainLocations.Count; i++ ){
              temp.Add((adjacentTerrainLocations[i][0], adjacentTerrainLocations[i][1]));
          }
          for(int i = 0; i < rewardMemory[this.AnimalId].Count; i++){
            for(int j = 0; j < temp.Count; j++ ){
              if(temp[j] == rewardMemory[this.AnimalId][i]){
                rewards[j] = 0;
              }
            }
          }
          return rewards;

        }
    }
}
