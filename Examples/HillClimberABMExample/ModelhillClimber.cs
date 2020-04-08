public static class Program {

	public static string OUTPUT_FILENAME = "Animal.csv";
	public static void Main(string[] args) {

		int epochs = 10;

		// loop that runs through the MARS simulations
		for (int i = 0; i < epochs; i++)
		{

			// before the MARS sim we need to initialize the agent init values
			// for MARS to load them we have to put them into an agent init file names animal_init.csv



			if (args != null && System.Linq.Enumerable.Any(args, s => s.Equals("-l")))
			{
				Mars.Common.Logging.LoggerFactory.SetLogLevel(Mars.Common.Logging.Enums.LogLevel.Info);
				Mars.Common.Logging.LoggerFactory.ActivateConsoleLogging();
			}

			var description = new Mars.Core.ModelContainer.Entities.ModelDescription();
			description.AddLayer<hillClimber.Terrain>();
			description.AddAgent<hillClimber.Animal, hillClimber.Terrain>();

			var task = Mars.Core.SimulationStarter.SimulationStarter.Start(description, args);

			
			var loopResults = task.Run();

			System.Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");

			// after the MARS simulation is finished we need to take the results (the end fitness score)

			// read csv fitness values

			// generate new agents
		}
	}

	private static void CreateAgentInitFile()
	{

	}
}
