import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import csv

terrainFile = './layers/moatGauss.csv'
rewardFile = './layers/moatGauss_reward.csv'
AgentData = './output/moatGauss_exportInfo.csv'

terrain = list(csv.reader(open(terrainFile), quoting=csv.QUOTE_NONNUMERIC))

data = list(csv.reader(open(rewardFile)))
xVals = []
yVals = []
zVals = []

for height in range(50):
    for length in range(50):
        if data[height][length] == '1':
            xVals.append(length)
            yVals.append(49 - height)
            zVals.append(terrain[49 - height][length] + 0.5)  # slightly above the terrain height

bestAgentXPos = []
bestAgentYPos = []
bestFit = 0
bestAgentID = 0

with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in reader:
        if int(float(row['TickNum'])) == 249 and int(float(row['Total Fitness'])) > bestFit:
            bestAgentID = int(float(row['AnimalID']))
            bestFit = int(float(row['Total Fitness']))

with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in reader:
        if int(float(row['AnimalID'])) == bestAgentID:
            bestAgentXPos.append(int(float(row['X Pos'])))
            bestAgentYPos.append(int(float(row['Y Pos'])))

fig = plt.figure(figsize=(8, 6))
terrain_3d = fig.add_subplot(111, projection='3d')

X = np.arange(0, len(terrain[0]))
Y = np.arange(0, len(terrain))
X, Y = np.meshgrid(X, Y)
Z = np.array(terrain)

# terrain surface
terrain_3d.plot_surface(X, Y, Z, cmap='viridis', alpha=0.8)

# reward
terrain_3d.scatter(xVals, yVals, zVals, marker='o', color='red', s=25) # match tile height

# best agent line
bestAgentZPos = [terrain[49 - int(y)][int(x)] for x, y in zip(bestAgentXPos, bestAgentYPos)]
terrain_3d.plot(bestAgentXPos, bestAgentYPos, bestAgentZPos, color='purple', linewidth=2, label='Best Agent')

# plot
terrain_3d.view_init(elev=45, azim=45)
terrain_3d.set_xlabel('X')
terrain_3d.set_ylabel('Y')
terrain_3d.set_zlabel('Height')
terrain_3d.set_title('Terrain')
terrain_3d.legend()
plt.tight_layout()
plt.show()