from datetime import date
import numpy as np
from matplotlib import cm
import matplotlib.pyplot as plt
import csv
from celluloid import Camera
import pandas as pandasForSortingCSV

NUM_STEPS = 250
TRESHOLD = 47

Data = ['./output/landScape_exportInfo.csv','./output/moatGauss_exportInfo.csv', './output/flatTerrain_exportInfo.csv']
Layer = ['./layers/landScape.csv', './layers/moatGauss.csv', './layers/flatTerrain.csv']
rewardLayer = ['./layers/landScape.csv', './layers/moatGauss_reward.csv', './layers/flatTerrain_reward.csv']
terrain_num = 2
AgentData = Data[terrain_num]
LayerFile = Layer[terrain_num]
rewardFile = rewardLayer[terrain_num]

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
reward_pos = fig.add_subplot(211)
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

#reward_pos.axes.get_xaxis().set_visible(False)
reward_pos.axes.set_xlim(-2.5, 52.5)
reward_pos.axes.set_ylim(-2, 52.5)
#reward_pos.axes.get_yaxis().set_visible(False)

heatmap.imshow(terrain[::-1], origin = 'lower')
maxIndex = 0
data = list(csv.reader(open(rewardFile)))
xVals = []
yVals = []

for height in range(50):
        for length in range(50):
            if data[height][length] == '1':
                xVals.append(height)
                yVals.append(length)


for i in range(NUM_STEPS):
    reward_pos.scatter(xVals, yVals, marker='^', color = 'green', s=100)
    fit_val = fitness[i*numpoints: i*numpoints + numpoints]
    avg_fit.append(sum(fitness[i*numpoints: i*numpoints + numpoints])/numpoints)
    norm = [(float(i)/(max(fit_val) + 1)) for i in fit_val]
    for index in range(len(avg_fit)):
        if(avg_fit[index]) >= TRESHOLD:
            maxIndex = index
            break
    if maxIndex != 0 :
        fitness_map.plot([maxIndex, maxIndex], [0, 50],color='black')
        fitness_map.plot(maxIndex, TRESHOLD, marker='o', color='r')
        fitness_map.text(maxIndex, TRESHOLD, str(maxIndex), horizontalalignment='right')
    colors = cm.rainbow(norm)
    fitness_map.plot(avg_fit)
    agent_pos.scatter(x[i*numpoints: i*numpoints + numpoints], y[i*numpoints: i*numpoints + numpoints], c=colors, s=100)
    camera.snap()
anim = camera.animate(blit=True)
plt.show()
