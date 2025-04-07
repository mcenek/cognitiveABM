import tkinter as tk
from tkinter import messagebox
from PIL import Image, ImageTk
import os
import sys


class SelectionForm(tk.Tk):

  def __init__(self):
    super().__init__()

    self.title("Terrain and Reward Selection")
    self.geometry("600x500")
    self.selected_terrain = tk.IntVar()
    self.selected_reward = tk.IntVar()

    self.canvas = None # inital image should be empty

    self.init_gui()


  def init_gui(self):
    # Terrain Group
    terrain_frame = tk.LabelFrame(self, text="Select Terrain Type")
    terrain_frame.place(x=20, y=20, width=260, height=380)
    
    # list of terrain options
    terrain_options = [
      "1. Normal peaks",
      "2. Inverted on Creation",
      "3. Inverted after Creation",
      "4. Only create Peaks around Perimeter",
      "5. Hill with blocker",
      "6. Canyon",
      "7. Hill with perimeter opening",
      "8. Terrain going top left to bottom right",
      "9. Fractal Terrain",
      "10. Inverted Perimeter Opening",
      "11. Gradient Terrain",
      "12. Binomal"
    ]

    # list of reward options
    reward_options = [
      "1. Normal", 
      "2. Centered"
    ]


    # iterate through terrain options and assign each option to a radio button
    for i, option in enumerate(terrain_options, start=1):
      radio_button = tk.Radiobutton(terrain_frame, text=option, variable=self.selected_terrain, value=i, command=self.update_image)
      radio_button.pack(anchor='w', padx=10, pady=2)

    # Reward Group
    reward_frame = tk.LabelFrame(self, text="Select Reward Type")
    reward_frame.place(x=290, y=20, width=260, height=120)

    for i, option in enumerate(reward_options, start = 1):
      radio_button = tk.Radiobutton(reward_frame, text=option, variable = self.selected_reward, value=i)
      radio_button.pack(anchor='w', padx=10, pady=2)

    # Image Display
    self.canvas = tk.Canvas(self, width=250, height=200, bg="white", relief="solid")
    self.canvas.place(x=290, y=160)

     # Submit button
    submit_btn = tk.Button(self, text="Submit", command=self.submit)
    submit_btn.place(x=440, y=380, width=100, height=30)



  def update_image(self):
    terrain_value = self.selected_terrain.get()

    script_dir = os.path.dirname(os.path.abspath(__file__))
    image_path = os.path.join(script_dir, "images", f"Terrain{terrain_value}.png")

    if os.path.exists(image_path):
      # Open and resize image
      img = Image.open(image_path).resize((250, 200), Image.LANCZOS)

      # Create PhotoImage and keep reference
      self.img_tk = ImageTk.PhotoImage(img)

      # Update Canvas
      self.canvas.delete("all")
      self.canvas.create_image(0, 0, anchor="nw", image=self.img_tk)
    else: 
      self.canvas.delete("all")
      messagebox.showerror("Image Not Found", f"Image not found: {image_path}")

  def submit(self):
    terrain = self.selected_terrain.get() # get selected terrain
    reward = self.selected_reward.get() # get selected reward type

    # make sure user selects both a terrain or a reward type
    if terrain == 0 or reward == 0:
        messagebox.showwarning("Selection Incomplete", "Please select both a terrain and a reward type.")
        return

    print(f"terrain={terrain};reward={reward}")
    sys.stdout.flush()
    self.destroy()

if __name__ == "__main__":
  app = SelectionForm()
  app.mainloop()

    

    



    

