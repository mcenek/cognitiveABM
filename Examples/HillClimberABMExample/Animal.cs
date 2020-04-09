namespace hillClimber
{
    using System;
    using Mars.Interfaces.Layer;
    using Mars.Components.Environments;
    using Mars.Common.Logging;
    using System.Collections.Generic;
    using CognitiveABM.Perceptron;

    // Pragma and ReSharper disable all warnings for generated code
#pragma warning disable 162
#pragma warning disable 219
#pragma warning disable 169

    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "InconsistentNaming")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedParameter.Local")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantNameQualifier")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberInitializerValueIgnored")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "RedundantCheckBeforeAssignment")]

    public class Animal : Mars.Interfaces.Agent.IMarsDslAgent
    {
        private static readonly ILogger _Logger = Mars.Common.Logging.LoggerFactory.GetLogger(typeof(Animal));

        private readonly Random random = new Random();

        private string rule = "";

        private double[] agentMemory;

        public string Rule
        {
            get => rule;
            set
            {
                if (rule != value) rule = value;
            }
        }

        private int animalId = default(int);

        public int AnimalId
        {
            get => animalId;
            set
            {
                if (animalId != value) animalId = value;
            }
        }

        private int bioEnergy = default(int);

        public int BioEnergy
        {
            get => bioEnergy;
            set
            {
                if (bioEnergy != value) bioEnergy = value;
            }
        }

        private int animalElevation = default(int);

        public int Animal_elevation
        {
            get => animalElevation;
            set
            {
                if (animalElevation != value) animalElevation = value;
            }
        }

        private int animalGainFromElevation = default(int);

        public int Animal_gain_from_elevation
        {
            get => animalGainFromElevation;
            set
            {
                if (animalGainFromElevation != value) animalGainFromElevation = value;
            }
        }

        private int animalReproduce = default(int);

        public int Animal_reproduce
        {
            get => animalReproduce;
            set
            {
                if (animalReproduce != value) animalReproduce = value;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public virtual void ChangeElevationUp()
        {
            // BioEnergy = BioEnergy + Animal_gain_from_elevation;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public virtual void ChangeElevationDown()
        {

            //BioEnergy = BioEnergy - Animal_gain_from_elevation;
            //if (BioEnergy < 0)
            //{
            //    BioEnergy = 0;
            //}

        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public virtual void Spawn(int percent)
        {
            if (random.Next(100) < percent)
            {
                hillClimber.Animal newAnimal = new System.Func<hillClimber.Animal>(() =>
                {
                    var _target65_1275 = new System.Tuple<double, double>(this.Position.X, this.Position.Y);
                    return Terrain._SpawnAnimal(_target65_1275.Item1, _target65_1275.Item2);
                }).Invoke();

                newAnimal.SecondInitialize(BioEnergy, Animal_gain_from_elevation, Animal_reproduce, terrain.GetIntegerValue(this.Position.X, this.Position.Y), AnimalId);
                BioEnergy = BioEnergy / 2;
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public virtual void RandomMove()
        {

            new System.Func<Tuple<double, double>>(() =>
            {
                Animal currentAgent = this;
                Func<double[], bool> predicate = null;


                int x = (int)Position.X;
                int y = (int)Position.Y;
                Tuple<int, int> relativePeakPosition = new Tuple<int, int>(x, y);
                double maxAltitude = animalElevation;

                int dx, dy;

                for (dx = -1; dx <= 1; ++dx)
                {
                    for (dy = -1; dy <= 1; ++dy)
                    {
                        if (dx != 0 || dy != 0)
                        {
                            double altitude = Terrain.GetRealValue(dx + x, dy + y);
                            if (maxAltitude < altitude)
                            {
                                maxAltitude = altitude;
                                relativePeakPosition = new Tuple<int, int>(dx + x, dy + y);
                            }
                        }
                    }
                }

                var randomPosition = new Tuple<int, int>(random.Next(terrain.DimensionX()), random.Next(terrain.DimensionY()));

                Terrain._AnimalEnvironment.MoveTo(currentAgent, relativePeakPosition.Item1, relativePeakPosition.Item2, 1, predicate);

                return new Tuple<double, double>(Position.X, Position.Y);

            }).Invoke();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void Killed()
        {
            var currentAgent = this;
            if (currentAgent != null)
            {
                Terrain.KillAnimal(currentAgent, currentAgent.executionFrequency);
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        public void SecondInitialize(int animalEnergy, int animailGain, int animalReproduce, int animalElevation, int agentId)
        {
            BioEnergy = animalEnergy;
            Animal_gain_from_elevation = animailGain;
            Animal_reproduce = animalReproduce;
            Animal_elevation = animalElevation;
            AnimalId = agentId;
        }

        internal bool isAlive;
        internal int executionFrequency;

        public hillClimber.Terrain _Layer_ => Terrain;
        public hillClimber.Terrain Terrain { get; set; }
        public hillClimber.Terrain terrain => Terrain;

        [Mars.Interfaces.LIFECapabilities.PublishForMappingInMars]
        public Animal(
            System.Guid _id,
            Terrain _layer,
            RegisterAgent _register,
            UnregisterAgent _unregister,
            SpatialHashEnvironment<Animal> _AnimalEnvironment,
            int Animal_gain_from_elevation,
            int Animal_reproduce,
            int AnimalId,
            double xcor = 0,
            double ycor = 0,
            int freq = 1)
        {
            Terrain = _layer;
            ID = _id;
            Position = Mars.Interfaces.Environment.Position.CreatePosition(xcor, ycor);
            random = new System.Random(ID.GetHashCode());
            this.Animal_gain_from_elevation = Animal_gain_from_elevation;
            this.Animal_reproduce = Animal_reproduce;
            this.AnimalId = AnimalId;
            Terrain._AnimalEnvironment.Insert(this);
            _register(_layer, this, freq);
            isAlive = true;
            executionFrequency = freq;

            var pos = new System.Func<System.Tuple<double, double>>(() =>
            {
                var _taget19_331 = new System.Tuple<int, int>(random.Next(terrain.DimensionX()), random.Next(terrain.DimensionY()));
                var _object19_331 = this;
                Terrain._AnimalEnvironment.PosAt(_object19_331, _taget19_331.Item1, _taget19_331.Item2);

                return new Tuple<double, double>(Position.X, Position.Y);

            }).Invoke();

            Position = Mars.Interfaces.Environment.Position.CreatePosition(pos.Item1, pos.Item2);

            Animal_elevation = terrain.GetIntegerValue(this.Position.X, this.Position.Y);
            //BioEnergy = random.Next(2 * Animal_gain_from_elevation);
        }

        public void Tick()
        {
            if (!isAlive) return;

            // bioEnergy--;

            //Spawn(Animal_reproduce);

            /*
             *   Start of own (non-generated) code
             */

            //RandomMove();

            PerceptronFactory perceptron = new PerceptronFactory(9, 9, 1, 9);

            var inputs = getTerrainElevations();

            int highestInput = 0;
            for (int i = 0; i < 9; i++)
            {
                if (inputs[i] > inputs[highestInput])
                {
                    highestInput = i;
                }
            }

            double[] outputs = perceptron.CalculatePerceptronFromId(AnimalId, inputs);
            // store outputs for the agent to have a memory
            agentMemory = new double[outputs.Length * 2];
            // concat values in outputs to itselft and store in the agentMemory so it can be used in the perceptron again
            outputs.CopyTo(agentMemory, 0);
            outputs.CopyTo(agentMemory, outputs.Length);
            List<int[]> locations = getTerrainLocations();
            int[] newLocation = new int[2];



            int highestOutput = 0;
            for (int i = 0; i < 9; i++)
            {
                if (outputs[i] > outputs[highestOutput])
                {
                    highestOutput = i;
                }
            }

            //if (AnimalId < 3)
            //{
            //    Console.WriteLine("Highest Input: {0}, Highest Output: {1}", highestInput, highestOutput);
            //}


            //newLocation = locations[Array.IndexOf(outputs, highestOutput)];
            newLocation = locations[highestOutput];

            Animal currentAgent = this;
            Func<double[], bool> predicate = null;
            Terrain._AnimalEnvironment.MoveTo(currentAgent, newLocation[0], newLocation[1], 1, predicate);



            /*
             *  End of own code
             */


            var oldElevation = Animal_elevation;

            Animal_elevation = terrain.GetIntegerValue(this.Position.X, this.Position.Y);
            //BioEnergy = Animal_elevation;

            BioEnergy += Animal_elevation - oldElevation;

            Rule = "Moved from elevation: " + oldElevation + " to elevation: " + Animal_elevation;

            //if (random.Next(100) < 50)
            //{
            //    Rule = "R1 - Move up in elevation";
            //    ChangeElevationUp();
            //}
            //else
            //{
            //    if (random.Next(100) < 50)
            //    {
            //        Rule = "R3 - Move down in elevation";
            //        ChangeElevationDown();
            //    }
            //    else
            //    {
            //        Rule = "R2 - Stay at same elevation";
            //    }
            //}
            //if (BioEnergy <= 0)
            //{
            //    Rule = "R4 - Death by BioEnergy loss";

            //    var currentAnimal = this;

            //    if (currentAnimal != null)
            //    {
            //     //   Terrain.KillAnimal(currentAnimal, currentAnimal.executionFrequency);
            //    }
            //}
        }

        /* Own Metods */
        public double[] getTerrainElevations()
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

        public List<int[]> getTerrainLocations()
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

        public System.Guid ID { get; }
        public Mars.Interfaces.Environment.Position Position { get; set; }
        public bool Equals(Animal other) => Equals(ID, other.ID);
        public override int GetHashCode() => ID.GetHashCode();
    }
}
