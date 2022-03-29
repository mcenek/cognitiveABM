import numpy as np
from matplotlib import cm
import matplotlib.pyplot as plt
import csv
from celluloid import Camera

NUM_STEPS = 250

# AgentData = './output/landScape_exportInfo.csv'
# LayerFile = './layers/landScape.csv'

# AgentData = './output/grid_exportInfo.csv'
# LayerFile = './layers/grid.csv'

AgentData = './output/gradient_exportInfo.csv'
LayerFile = './layers/gradient.csv'

x = []
y = []
fitness = []
terrain = list(csv.reader(open(LayerFile),
                          quoting=csv.QUOTE_NONNUMERIC))

with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)

    for row in reader:
        x.append(int(float(row['X Pos'])))
        y.append(int(float(row['Y Pos'])))
        fitness.append(int(float(row['Current Elevation'])))

numpoints = 96
points = np.random.random((2, numpoints))
colors = cm.rainbow(np.linspace(0, 1, numpoints))

fig = plt.figure("Agents")
agent_pos = fig.add_subplot(211)
fitness_map = fig.add_subplot(223)
heatmap = fig.add_subplot(224)
camera = Camera(fig)
avg_fit = []

fitness_map.set_xlabel('Steps')
fitness_map.set_ylabel('Average Fitness')

heatmap.imshow(terrain)

for i in range(NUM_STEPS):
    fit_val = fitness[i*96: i*96 + 96]
    avg_fit.append(sum(fitness[i*96: i*96 + 96])/96)
    norm = [(float(i)/(max(fit_val) + 1)) for i in fit_val]
    colors = cm.rainbow(norm)
    fitness_map.plot(avg_fit)
    heatmap.imshow(terrain)
    agent_pos.scatter(x[i*96: i*96 + 96], y[i*96: i*96 + 96], c=colors, s=100)
    camera.snap()
anim = camera.animate(blit=True)
plt.show()
