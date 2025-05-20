from datetime import date
import numpy as np
from matplotlib import cm
import matplotlib.pyplot as plt
import csv
from celluloid import Camera
import pandas as pandasForSortingCSV
import os

NUM_STEPS = 250
TRESHOLD = 47

current_dir = os.path.dirname(os.path.abspath(__file__))
Data = [os.path.join(current_dir, 'output', 'landscape_exportInfo.csv'),
        os.path.join(current_dir, 'output', 'moatGauss_exportInfo.csv'),
        os.path.join(current_dir, 'output', 'grid_exportInfo.csv')]

Layer = [os.path.join(current_dir, 'layers', 'landscape.csv'),
         os.path.join(current_dir, 'layers', 'moatGauss.csv'),
         os.path.join(current_dir, 'layers', 'grid.csv')]

rewardLayer = [os.path.join(current_dir, 'layers', 'landscape_reward.csv'),
               os.path.join(current_dir, 'layers', 'moatGauss_reward.csv'),
               os.path.join(current_dir, 'layers', 'grid_reward.csv')]

# Number represents each generation
terrain_num = 0
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

fig = plt.figure("Agents", figsize=(14, 8))
agent_pos = fig.add_subplot(231)
bestAgent_pos = fig.add_subplot(232)
population_breakdown = fig.add_subplot(233)
fitness_map = fig.add_subplot(234)
heatmap = fig.add_subplot(235)

camera = Camera(fig)
avg_fit = []

climbers = []
descenders = []
collectors = []

fitness_map.set_xlabel('Epoch')
fitness_map.set_ylabel('Fitness')

agent_pos.set_xlabel('X')
agent_pos.set_ylabel('Y')
agent_pos.set_title('Agent Position')
agent_pos.set_aspect('equal', adjustable='box');

bestAgent_pos.set_xlabel('X')
bestAgent_pos.set_ylabel('Y')
#bestAgent_pos.set_title('Best Agent Position')
bestAgent_pos.set_aspect('equal', adjustable='box');
bestAgent_pos.axes.set_xlim(0, 50)
bestAgent_pos.axes.set_ylim(0, 50)

heatmap.set_aspect('equal', adjustable='box')
# reward_pos.set_aspect('equal');
#reward_pos.axes.get_yaxis().set_visible(False)
maxIndex = 0
data = list(csv.reader(open(rewardFile)))
xVals = []
yVals = []

# MAKE ALL NEGATIVE TERRAIN HEIGHTS WEIGHT =================
terrain2 = np.array(terrain)
mask = terrain2 < 0
overlay = np.zeros_like(terrain2, dtype=np.float32)
overlay = np.zeros((*terrain2.shape, 4), dtype=np.float32)
overlay[mask] = [1.0, 1.0, 1.0, 1.0]  # white
overlay[~mask] = [0.0, 0.0, 0.0, 0.0]  # transparent
cmap = plt.cm.colors.ListedColormap(['white'])
# ==========================================================

for height in range(50):
        for length in range(50):
            if data[height][length] == '1':
                xVals.append(length)
                yVals.append(49-height)

# store previous steps
prev_positions = {agent_id: {'x': [], 'y': []} for agent_id in range(numpoints)}
prev_best_agent_position = {'x': [], 'y': []}
agent_colors = {agent_id: colors[agent_id] for agent_id in range(numpoints)}

for i in range(NUM_STEPS):
    fit_val = fitness[i*numpoints: i*numpoints + numpoints]
    avg_fit.append(sum(fitness[i*numpoints: i*numpoints + numpoints])/numpoints)
    norm = [(float(i)/(max(fit_val) + 1)) for i in fit_val]

    # --- Population breakdown logic ---
    climber_count = 0
    descender_count = 0
    collector_count = 0
    agent_type_colors = []
    for j in range(numpoints):
        agent_id = i * numpoints + j
        if agent_id > 0:
            prev_elevation = fitness[agent_id - numpoints]
            current_elevation = fitness[agent_id]
            elevation_change = current_elevation - prev_elevation
            if elevation_change > 0:
                climber_count += 1
                agent_type_colors.append('red')
            elif elevation_change < 0:
                descender_count += 1
                agent_type_colors.append('blue')
            else:
                collector_count += 1
                agent_type_colors.append('green')
        else:
            collector_count += 1
            agent_type_colors.append('green')
    climbers.append(climber_count)
    descenders.append(descender_count)
    collectors.append(collector_count)

    # --- Population Braekdown ---
    population_breakdown.clear()
    population_breakdown.set_xlabel('Generation')
    population_breakdown.set_ylabel('Number of Agents')
    population_breakdown.set_title('Agent Type Distribution')
    if i > 0:
        x_gen = np.arange(i+1)
        population_breakdown.bar(x_gen, climbers[:i+1], color='red', label='Climbers', width=1.0)
        population_breakdown.bar(x_gen, descenders[:i+1], bottom=climbers[:i+1], color='blue', label='Descenders', width=1.0)
        bottom_sum = np.array(climbers[:i+1]) + np.array(descenders[:i+1])
        population_breakdown.bar(x_gen, collectors[:i+1], bottom=bottom_sum, color='green', label='Collectors', width=1.0)
        population_breakdown.set_xlim(0, NUM_STEPS)
        population_breakdown.set_ylim(0, numpoints)
        if i == 1:
            population_breakdown.legend(loc='upper right', fontsize=10)

    # --- Agent Position Plot ---
    agent_pos.clear()
    agent_pos.set_xlabel('X')
    agent_pos.set_ylabel('Y')
    agent_pos.set_title('Agent Position')
    agent_pos.set_aspect('equal', adjustable='box')
    for agent_id in prev_positions:
        agent_pos.plot(prev_positions[agent_id]['x'], prev_positions[agent_id]['y'], color=agent_colors[agent_id], alpha=0.5)
    agent_pos.scatter(x[i * numpoints: i * numpoints + numpoints], y[i * numpoints: i * numpoints + numpoints],
                     c=agent_type_colors, s=100)

    # --- Best Agent Path Plot --
    bestAgent_pos.clear()
    bestAgent_pos.set_xlabel('X')
    bestAgent_pos.set_ylabel('Y')
    #bestAgent_pos.set_title('Best Agent Path')
    bestAgent_pos.set_aspect('equal', adjustable='box')
    bestAgent_pos.set_xlim(0, 50)
    bestAgent_pos.set_ylim(0, 50)
    bestAgent_pos.plot(prev_best_agent_position['x'], prev_best_agent_position['y'], color='blue', lw=1, alpha=0.8)
    if len(prev_best_agent_position['x']) > 0:
        bestAgent_pos.scatter(prev_best_agent_position['x'][-1], prev_best_agent_position['y'][-1], c='blue', s=30, zorder=3)

    for index in range(len(avg_fit)):
        if(avg_fit[index]) >= TRESHOLD:
            maxIndex = index
            break
    if maxIndex != 0 :
        fitness_map.plot([maxIndex, maxIndex], [0, 50],color='black')
        fitness_map.plot(maxIndex, TRESHOLD, marker='o', color='r')
        fitness_map.text(maxIndex, TRESHOLD, str(maxIndex), horizontalalignment='right')
    
    # save current positions to the prev dict
    for agent_id in range(len(x[i * numpoints: i * numpoints + numpoints])):
        agent_x = x[i * numpoints + agent_id]
        agent_y = y[i * numpoints + agent_id]
        if agent_id not in prev_positions:
            prev_positions[agent_id] = {'x': [agent_x], 'y': [agent_y]}
        else:
            prev_positions[agent_id]['x'].append(agent_x)
            prev_positions[agent_id]['y'].append(agent_y)
    best_agent_x = bestAgentXPos[i]
    best_agent_y = bestAgentYPos[i]
    prev_best_agent_position['x'].append(best_agent_x)
    prev_best_agent_position['y'].append(best_agent_y)

    colors = cm.rainbow(norm)
    fitness_map.plot(avg_fit, color='blue')
    agent_pos.scatter(x[i * numpoints: i * numpoints + numpoints], y[i * numpoints: i * numpoints + numpoints],
                      c=agent_type_colors, s=100)
    #agent_pos.scatter(x[i*numpoints: i*numpoints + numpoints], y[i*numpoints: i*numpoints + numpoints], c=colors, s=100)
    #bestAgent_pos.scatter(bestAgentXPos[i], bestAgentYPos[i], c='red', s=100)
    bestAgent_pos.scatter(best_agent_x, best_agent_y, c='blue', s=100)
    # --- Terrain Heatmap ---
    heatmap.clear()
    heatmap.set_aspect('equal', adjustable='box')
    heatmap.imshow(terrain[::-1], origin='lower', cmap='viridis')
    heatmap.imshow(overlay[::-1], cmap=cmap, origin='lower', alpha=1.0)
    heatmap.set_xlim(0, 50)
    heatmap.set_ylim(0, 50)
    heatmap.scatter(xVals, yVals, marker='o', color='red', s=25)
    camera.snap()

anim = camera.animate(blit=False, repeat=False)
plt.show()