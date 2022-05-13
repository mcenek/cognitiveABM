import numpy as np
from matplotlib import cm
import matplotlib.pyplot as plt
import csv
from celluloid import Camera
import pandas as pandasForSortingCSV

NUM_STEPS = 150

Data = ['./output/landScape_exportInfo.csv','./output/grid_exportInfo.csv', './output/gradient_exportInfo.csv']
Layer = ['./layers/landScape.csv', './layers/grid.csv', './layers/gradient.csv']
terrain_num = 0
AgentData = Data[terrain_num]
LayerFile = Layer[terrain_num]

for file in Data:
    csvData = pandasForSortingCSV.read_csv(file)
    sorted_data = csvData.sort_values(by=["TickNum", "AnimalID"],ascending=[True, True])
    sorted_data.to_csv(file, index=False)


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
fitness_map.set_ylabel('Average Elevation')

agent_pos.set_xlabel('X')
agent_pos.set_ylabel('Y')
agent_pos.set_title('Agent Position')

heatmap.imshow(terrain[::-1], origin = 'lower')

for i in range(NUM_STEPS):
    fit_val = fitness[i*numpoints: i*numpoints + numpoints]
    avg_fit.append(sum(fitness[i*numpoints: i*numpoints + numpoints])/numpoints)
    norm = [(float(i)/(max(fit_val) + 1)) for i in fit_val]
    colors = cm.rainbow(norm)
    fitness_map.plot(avg_fit)
    agent_pos.scatter(x[i*numpoints: i*numpoints + numpoints], y[i*numpoints: i*numpoints + numpoints], c=colors, s=100)
    camera.snap()
anim = camera.animate(blit=True)
plt.show()
