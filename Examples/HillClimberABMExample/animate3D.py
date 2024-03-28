import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
import csv

Data = ['./output/landscape_exportInfo.csv','./output/moatGauss_exportInfo.csv', './output/grid_exportInfo.csv']
Layer = ['./layers/landscape.csv', './layers/moatGauss.csv', './layers/grid.csv']
rewardLayer = ['./layers/landscape_reward.csv', './layers/moatGauss_reward.csv', './layers/grid_reward.csv']
    
terrain_num = 2 
AgentData = Data[terrain_num]
LayerFile = Layer[terrain_num]
rewardFile = rewardLayer[terrain_num]


terrain = list(csv.reader(open(LayerFile), quoting=csv.QUOTE_NONNUMERIC))
for i in range(len(terrain)):
    for j in range(len(terrain[i])):
        if terrain[i][j] < 0:
            terrain[i][j] = 2000

data = list(csv.reader(open(rewardFile)))
xVals = []
yVals = []
zVals = []
yValsBestAgent =[]
xValsBestAgent =[]
for height in range(50):
    for length in range(50):
        yValsBestAgent.append(height)
        xValsBestAgent.append(length)
        if data[height][length] == '1':
            xVals.append(length)
            yVals.append(49-height)
            zVals.append(terrain[height][length] + 0.5)  # slightly above the terrain height

bestAgentXPos = []
bestAgentYPos = []
bestAgentZPos = []
bestFit = 0
bestAgentID = 0

with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in reader:
        if int(float(row['TickNum'])) == 249 and int(float(row['Total Fitness'])) > bestFit:
            bestAgentID = int(float(row['AnimalID']))
            bestFit = int(float(row['Total Fitness']))

num_rows = len(terrain)
num_cols = len(terrain[0])
with open(AgentData, newline='') as csvfile:
    reader = csv.DictReader(csvfile)
    for row in reader:
        if int(float(row['AnimalID'])) == bestAgentID:
            x_pos = int(float(row['X Pos']))
            y_pos = int(float(row['Y Pos']))
            tick = int(float(row["TickNum"]))
            bestAgentXPos.append(x_pos)
            bestAgentZPos.append(y_pos)
            if y_pos == 50:
                y_pos =- 1
            if x_pos == 50:
                x_pos =- 1    
            if(terrain[x_pos][y_pos] >= 2000): # sometimes it's not on wall but still jumps in height
                if not bestAgentYPos:
                    bestAgentYPos.append(0)
                else:   
                    bestAgentYPos.append(bestAgentYPos[-1])
            else:
                bestAgentYPos.append(terrain[x_pos][y_pos] )
                #AgentHeight = terrain[yValsBestAgent[tick]][xValsBestAgent[tick]]
                #AgentHeight = terrain[x_pos][y_pos]
                #bestAgentYPos.append(AgentHeight+50)

fig = plt.figure(figsize=(8, 6))
terrain_3d = fig.add_subplot(111, projection='3d')

X = np.arange(0, len(terrain[0]))
Y = np.arange(0, len(terrain))
X, Y = np.meshgrid(X, Y)
Z = np.flipud(np.array(terrain))

# terrain surface
terrain_3d.plot_surface(X, Y, Z, cmap='viridis', alpha=0.8)

# reward
terrain_3d.scatter(xVals, yVals, zVals, marker='o', color='red', s=25) # match tile height

# best agent line
NUM_STEPS = min(len(bestAgentXPos),len(bestAgentYPos))
prev_best_agent_position = {'x': [], 'y': [], 'z': []}

for i in range(NUM_STEPS):
    x = bestAgentXPos[i]
    y = bestAgentYPos[i] 
    z = bestAgentZPos[i]
    prev_best_agent_position['x'].append(x)
    prev_best_agent_position['y'].append(y)
    prev_best_agent_position['z'].append(z)


terrain_3d.plot(prev_best_agent_position['x'], prev_best_agent_position['z'], prev_best_agent_position['y'], color='red', alpha=1)
# plot
terrain_3d.view_init(elev=45, azim=45)
terrain_3d.set_xlabel('X')
terrain_3d.set_ylabel('Y')
terrain_3d.set_zlabel('Height')
terrain_3d.set_title('Terrain')
terrain_3d.legend()
plt.title(" ")
plt.tight_layout()
plt.show()