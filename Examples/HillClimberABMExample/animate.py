import numpy as np
from matplotlib import cm
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
import csv
from celluloid import Camera

NUM_STEPS = 150

x = []
y = []
fitness = []
with open('./Animal.csv', newline='') as csvfile:
    reader = csv.DictReader(csvfile)

    for row in reader:
        x.append(int(float(row['0_Position'])))
        y.append(int(float(row['1_Position'])))
        fitness.append(int(float(row['BioEnergy'])))

numpoints = 96
points = np.random.random((2, numpoints))
colors = cm.rainbow(np.linspace(0, 1, numpoints))

fig, (g, l) = plt.subplots(2)
camera = Camera(fig)

avg_fit = []

for i in range(NUM_STEPS):
    fit_val = fitness[i*96: i*96 + 96]
    avg_fit.append(sum(fitness[i*96: i*96 + 96])/96)
    norm = [(float(i)/(max(fit_val) + 1)) for i in fit_val]
    colors = cm.rainbow(norm)
    l.plot(avg_fit)
    g.scatter(x[i*96: i*96 + 96], y[i*96: i*96 + 96], c=colors, s=100)
    camera.snap()
anim = camera.animate(blit=True)
plt.show()
