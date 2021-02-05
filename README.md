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

QLearning - the newest and most experimental module of code, I would visit this first if I was going to redo/verify a section of code. We implemented this last and it has given Daniel a lot of trouble. Go to Daniel Borg for questions about this code.

Perceptron - uses an agent's genomes as values in a perceptron matrix and is used to make decisions.

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

