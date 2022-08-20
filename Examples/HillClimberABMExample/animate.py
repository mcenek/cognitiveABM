from datetime import date
import numpy as np
from matplotlib import cm
import matplotlib.pyplot as plt
import csv
from celluloid import Camera
import pandas as pandasForSortingCSV

NUM_STEPS = 150
TRESHOLD = 47

Data = ['./output/landscape_exportInfo.csv','./output/moatGauss_exportInfo.csv', '.\output\grid_exportInfo.csv']
Layer = ['./layers/landscape.csv', './layers/moatGauss.csv', '.\layers\grid.csv']
rewardLayer = ['./layers/landscape_reward.csv', './layers/moatGauss_reward.csv', '.\layers\grid_reward.csv']
terrain_num = 2
AgentData = Data[terrain_num]
LayerFile = Layer[terrain_num]
rewardFile = rewardLayer[terrain_num]

#rewites file so it goes from ascending order of ticknumber followed by the animal id
for file in Data:
    csvData = pandasForSortingCSV.read_csv(file)
    sorted_data = csvData.sort_values(by=["TickNum", "AnimalID"],ascending=[True, True])
    sorted_data.to_csv(file, index=False)


x = []
y = []
fitness = []
bestAgentID = 0
bestAgentXPos = []
bestAgentYPos = []
bestFit = 0
terrain = list(csv.reader(open(LayerFile),
                          quoting=csv.QUOTE_NONNUMERIC))

with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)

    for row in reader:
        x.append(int(float(row['X Pos'])))
        y.append(int(float(row['Y Pos'])))
        fitness.append(int(float(row['Current Elevation'])))
        if int(float(row['TickNum'])) == 249 and int(float(row['Total Fitness'])) > bestFit:
            bestAgentID = int(float(row['AnimalID']))
            bestFit = int(float(row['Total Fitness']))


with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)

    for row in reader:
        if int(float(row['AnimalID'])) == bestAgentID:
            bestAgentXPos.append(int(float(row['X Pos'])))
            bestAgentYPos.append(int(float(row['Y Pos'])))


numpoints = 96
points = np.random.random((2, numpoints))
colors = cm.rainbow(np.linspace(0, 1, numpoints))

fig = plt.figure("Agents")
agent_pos = fig.add_subplot(221)
bestAgent_pos = fig.add_subplot(222)
fitness_map = fig.add_subplot(223)
heatmap = fig.add_subplot(224)
reward_pos = fig.add_subplot(224)
camera = Camera(fig)
avg_fit = []

fitness_map.set_xlabel('Epoch')
fitness_map.set_ylabel('Fitness')

agent_pos.set_xlabel('X')
agent_pos.set_ylabel('Y')
agent_pos.set_title('Agent Position')
agent_pos.set_aspect('equal', adjustable='box');


bestAgent_pos.set_xlabel('X')
bestAgent_pos.set_ylabel('Y')
bestAgent_pos.set_title('Best Agent Position')
bestAgent_pos.set_aspect('equal', adjustable='box');
bestAgent_pos.axes.set_xlim(0, 50)
bestAgent_pos.axes.set_ylim(0, 50)


#reward_pos.axes.get_xaxis().set_visible(False)
# reward_pos.axes.set_xlim(-2.5, 52.5)
# reward_pos.axes.set_ylim(-2, 52.5)
reward_pos.set_aspect('equal', adjustable='box')
heatmap.set_aspect('equal', adjustable='box')
# reward_pos.set_aspect('equal');
#reward_pos.axes.get_yaxis().set_visible(False)
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
    bestAgent_pos.scatter(bestAgentXPos[i], bestAgentYPos[i], c='red', s=100)
    heatmap.imshow(terrain[::-1], origin = 'lower')
    reward_pos.get_xaxis().set_visible(False);
    reward_pos.get_yaxis().set_visible(False);
    reward_pos.scatter(xVals, yVals, marker='o', color = 'red', s=25)
    camera.snap()
anim = camera.animate(blit=True)
plt.show()
