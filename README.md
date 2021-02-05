# MarsCognitiveABM
A library for creating large scale Cognitive Agent Based Models using Mars.Life.Simulations

MARS Group:
https://mars-group.org/

MARS NuGet Package:
https://www.nuget.org/packages/Mars.Life.Simulations/

## Code overview

### Root folders:

`CognitiveABM` the abstract CognitiveABM library, an abstraction of the interfaces and classes used to create our cognitive ABM proof of concept to make creating similar but more complex uses easier to implement.


`Examples/HillClimberABMExample` example showing usage of the CognitiveABM library and approach.

`TerrainGenerator` tool for generating CSV files for use in the HillClimberABMExample.


### CognitiveABM architecture

ABM - manages the execution of the ABM.

FCM - manages reproduction between generations

Perceptron - uses an agent's genomes as values in a perceptron matrix and is used to make decisions.

QLearning - the newest and most experimental module of code, I would visit this first if I was going to redo/verify a section of code. We implemented this last and it has given Daniel a lot of trouble. Go to Daniel Borg for questions about this code.

### What needs re-coding

#### Essential:

The QLearning module we had working for a short while, but it's currently not in use (would be used in `Animal.cs`), but Daniel has been having some issues with the data structure as of late. Just needs to be cleaned up and some index out of bounds errors removed.

#### Desired:

The slowest part of each iteration is running the simulation of the agents itself, and I think this is caused by the amount of math and calculations that is done each step when agents use the perceptron to decide where to move. Optimizing the perceptron (`Perceptron.cs`) math would greatly increase performance. 

## Useful Additions

A visualization tool that could include the decision making process for a single agent would be a great way of debugging the perceptron and/or the q-learning model.

## Building and running application

If you use VS Code you can simply hit `F5` and it will run the example with the following launch configuration: 

```json
{
    "name": ".NET Core Launch (console)",
    "type": "coreclr",
    "request": "launch",
    "preLaunchTask": "build",
    // If you have changed target frameworks, make sure to update the program path.
    "program": "${workspaceFolder}/Examples/HillClimberABMExample/bin/Debug/netcoreapp2.1/HillClimberABMExample.dll",
    "args": ["-sm config.json"],
    "cwd": "${workspaceFolder}/Examples/HillClimberABMExample",
    // For more information about the 'console' field, see https://aka.ms/VSCode-CS-LaunchJson-Console
    "console": "internalConsole",
    "stopAtEntry": false,
    "logging": {
        "moduleLoad": false
    }
}
```

`Examples/HillClimberABMExample/config.json` is where you can configure MARS specific parameters like number of agents, number of steps, layers, and terrains.

If you're not using VS Code you can run the example from the command line with `.NET Core 2.1` like so:

From within the `Examples\HillClimberABMExample` directory run `dotnet run -sm config.json`

