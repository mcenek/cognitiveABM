namespace HillClimberExample
{
    using System;
    using Mars.Interfaces.Layer;
    using Mars.Components.Environments;
    using Mars.Common.Logging;
    using System.Collections.Generic;
    using CognitiveABM.Perceptron;

    public class Animal : Mars.Interfaces.Agent.IMarsDslAgent
    {
        private static readonly ILogger _Logger = LoggerFactory.GetLogger(typeof(Animal));


        private readonly double[] AgentMemory;

        private readonly int startingElevation;

        public Guid ID { get; }

        public Mars.Interfaces.Environment.Position Position { get; set; }

        public bool Equals(Animal other) => Equals(ID, other.ID);

        public override int GetHashCode() => ID.GetHashCode();

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
        public Animal(Guid _id, Terrain _layer, RegisterAgent _register, UnregisterAgent _unregister, SpatialHashEnvironment<Animal> _AnimalEnvironment, int animalId, double xcor = 0, double ycor = 0, int freq = 1)
        {
            ID = _id;
            Terrain = _layer;
            AnimalId = animalId;
            executionFrequency = freq;

            Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
            var pos = InitialPosition();
            Position = Mars.Interfaces.Environment.Position.CreatePosition(pos.Item1, pos.Item2);

            Terrain._AnimalEnvironment.Insert(this);
            _register(_layer, this, freq);


            Elevation = Terrain.GetIntegerValue(Position.X, Position.Y);
            startingElevation = Elevation;

            AgentMemory = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public void Tick()
        {


            var inputs = GetAdjacentTerrainElevations();

            int highestInput = 0;
            for (int i = 0; i < 9; i++)
            {
                if (inputs[i] > inputs[highestInput])
                {
                    highestInput = i;
                }
            }

            Boolean atPeak = highestInput == 4;

            PerceptronFactory perceptron = new PerceptronFactory(9, 9, 1, 9);
            double[] outputs = perceptron.CalculatePerceptronFromId(AnimalId, inputs, AgentMemory);
            outputs.CopyTo(AgentMemory, 0);
            outputs.CopyTo(AgentMemory, outputs.Length);

            List<int[]> locations = GetAdjacentTerrainPositions();

            int highestOutput = 0;
            for (int i = 0; i < 9; i++)
            {
                if (outputs[i] > outputs[highestOutput])
                {
                    highestOutput = i;
                }
            }

            int[] newLocation = locations[highestOutput];

            Terrain._AnimalEnvironment.MoveTo(this, newLocation[0], newLocation[1], 1, predicate: null);

            Elevation = Terrain.GetIntegerValue(this.Position.X, this.Position.Y);

            BioEnergy = Elevation - startingElevation;

            if (BioEnergy < 0)
            {
                BioEnergy = 0;
            }

            if (atPeak && highestOutput == highestInput)
            {
                BioEnergy = 150;
            }
        }

        private Tuple<int, int> InitialPosition()
        {
            var random = new Random(ID.GetHashCode());
            return new Tuple<int, int>(random.Next(Terrain.DimensionX()), random.Next(Terrain.DimensionY()));
        }

        private double[] GetAdjacentTerrainElevations()
        {
            List<double> elevations = new List<double>();
            int x = (int)Position.X;
            int y = (int)Position.Y;

            for (int dx = -1; dx <= 1; ++dx)
            {
                for (int dy = -1; dy <= 1; ++dy)
                {
                    elevations.Add(Terrain.GetRealValue(dx + x, dy + y));
                }
            }

            return elevations.ToArray();
        }

        private List<int[]> GetAdjacentTerrainPositions()
        {
            List<int[]> locations = new List<int[]>();
            int x = (int)Position.X;
            int y = (int)Position.Y;

            for (int dx = -1; dx <= 1; ++dx)
            {
                for (int dy = -1; dy <= 1; ++dy)
                {
                    int[] location = new int[] { dx + x, dy + y };
                    locations.Add(location);
                }
            }

            return locations;
        }
    }
}
