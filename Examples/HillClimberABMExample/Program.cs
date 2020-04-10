using System;
using System.IO;
using HillClimberExample;
using Mars.Common.Logging;

// todo make this part of our library also?
public static class Program {

	// todo get these from somewhere else automatically
	public static string OUTPUT_FILENAME = "Animal.csv";
	public static string FITNESS_COLUMNNAME = "BioEnergy";
	public static int STEPS = 2500;


	public static void Main(string[] args) {


		int epochs = 100;


		HillClimberFCM fcm = new HillClimberFCM(96, 486, STEPS, OUTPUT_FILENAME, FITNESS_COLUMNNAME);

		// loop that runs through the MARS simulations
		for (int i = 0; i < epochs; i++)
		{
			Console.WriteLine("\n\nEpoch {0}\n", i);

			// before the MARS sim we need to initialize the agent init values
			// for MARS to load them we have to put them into an agent init file names animal_init.csv

			if (args != null && System.Linq.Enumerable.Any(args, s => s.Equals("-l")))
			{
				LoggerFactory.SetLogLevel(Mars.Common.Logging.Enums.LogLevel.Off);
				LoggerFactory.DeactivateConsoleLogging();
			}

			LoggerFactory.SetLogLevel(Mars.Common.Logging.Enums.LogLevel.Warning);
			LoggerFactory.DeactivateConsoleLogging();

			var logger = LoggerFactory.GetLogger(typeof(Animal));

			var description = new Mars.Core.ModelContainer.Entities.ModelDescription();
			description.AddLayer<Terrain>();
			description.AddAgent<Animal, Terrain>();
			

			// run a single MARS sim
			var task = Mars.Core.SimulationStarter.SimulationStarter.Start(description, args);

			Mars.Core.SimulationManager.Entities.SimulationWorkflowState loopResults = task.Run();

			if (loopResults.IsFinished)
			{


				System.Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");

				// run the FCM which updates the genome values

				var agentFitnessValues = fcm.Run();

				using (var writer = new StreamWriter(path: "fitness.csv", append: true))
				{
					foreach (double value in agentFitnessValues)
					{
						writer.Write(value + ",");
					}
					writer.WriteLine();
				}

				GC.Collect();
			}

		}
	}
}
