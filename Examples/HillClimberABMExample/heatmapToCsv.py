from encodings import normalize_encoding
import cv2
import csv
import math
from sklearn import preprocessing

img = cv2.imread('./Heatmaps/heatmapExample.jpg')
height = img.shape[0]
width = img.shape[1]
print(height, ' ', width)

map = []

#create a 50 x 50 2d array
max = 0
for i in range(50):
    col = []
    for j in range(50):
        #do math here to figure out the values
        y = (height / 51) * i + (height / 51) * 2 -1 
        y = math.floor(y)
        x = (width / 51) * j + (width / 51) * 2 - 1
        x = math.floor(x)
        blue = img[y, x, 0]
        green = img[y, x, 1]
        red = img[y, x, 2]
        value = blue*0.1 + green*0.75 + red*5
        if value > max:
            max = value
        col.append(value)
    map.append(col)
#sets max to 50 
for i in range(50):
    for j in range(50):
        map[i][j] = math.floor(map[i][j]/max * 50)
#print(map)


with open("./layers/heatmapExample.csv", "w+", newline="") as newCSV:
    csvWriter = csv.writer(newCSV)
    csvWriter.writerows(map)