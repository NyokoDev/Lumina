import os
import sys
from tkinter import Tk, Canvas, Button, Label, messagebox
from tkinter import ttk
from PIL import Image, ImageTk, ImageColor, ImageDraw

# Set the background photo
background_image_path = "background.jpg"
default_image_path = "default.png"

width = 4096
height = 64
colors = [
    "#ff0000", "#ff8000", "#ffff00", "#80ff00", "#00ff00", "#00ff80", "#00ffff",
    "#0080ff", "#0000ff", "#8000ff", "#ff00ff", "#ff0080", "#ff0000"
]

def create_lut():
    # Create a color selection window
    root = Tk()
    root.title("LUT Creator")
    root.geometry("600x400")
    root.resizable(False, False)
    icon_path = 'lut.ico'  # Replace with the path to your icon file
    root.iconbitmap(default=icon_path)

    # Set the background photo
    background_image = Image.open(background_image_path)
    background_photo = ImageTk.PhotoImage(background_image)
    background_label = Label(root, image=background_photo, bg="#333333")
    background_label.place(x=0, y=0, relwidth=1, relheight=1)
    background_label.image = background_photo

    # Initialize color variables
    red = green = blue = 0

    # Function to update the color variables
    def update_color(new_red, new_green, new_blue):
        nonlocal red, green, blue
        red, green, blue = new_red, new_green, new_blue
        color_label.config(text=f"R: {red}  G: {green}  B: {blue}")
        update_preview()

    # Function to save the LUT
    def save_lut():
        # Create the gradient for the LUT
        lut = Image.new("RGBA", (width, height))
        pixels = lut.load()

        for x in range(width):
            r = int((x / width) * red)    # Red component varies from 0 to red
            g = int((x / width) * green)  # Green component varies from 0 to green
            b = int((x / width) * blue)   # Blue component varies from 0 to blue

            for y in range(height):
                pixels[x, y] = (r, g, b, 255)  # Set pixel color

        # Save the LUT file in the specified folder
        folder_path = os.path.join(os.getenv('LOCALAPPDATA'), 'Colossal Order', 'Cities_Skylines', 'Addons', 'ColorCorrections')
        os.makedirs(folder_path, exist_ok=True)
        file_path = os.path.join(folder_path, 'custom_lut.png')
        lut.save(file_path)
        print("LUT file saved at:", file_path)

        # Load the default.png file from the background image folder
        background_folder = os.path.dirname(background_image_path)
        default_path = os.path.join(background_folder, 'default.png')
        default_image = Image.open(default_path).convert("RGBA")

        # Resize the default image to match the custom LUT size
        default_image = default_image.resize((width, height))

        # Create a new image with 50% opacity for the custom LUT
        lut_with_opacity = Image.new("RGBA", (width, height))
        lut_with_opacity = Image.blend(lut_with_opacity, lut, alpha=0.5)

        # Merge the default image and the custom LUT image
        merged_image = Image.alpha_composite(default_image, lut_with_opacity)

        # Save the merged image
        merged_file_path = os.path.join(folder_path, 'merged_lut.png')
        merged_image.save(merged_file_path)
        print("Merged LUT file saved at:", merged_file_path)

        # Delete the custom LUT file
        os.remove(file_path)
        print("Custom LUT file deleted.")

        # Show a popup message
        messagebox.showinfo("LUT Saved", "LUT saved at the ColorCorrections folder in AppData")

    # Create the color selection canvas
    canvas = Canvas(root, width=350, height=256, bg="#333333", highlightthickness=0)
    canvas.place(x=125, y=60)

    # Create color squares
    square_size = 30
    margin = 10
    num_squares = len(colors)
    total_width = num_squares * (square_size + margin) - margin
    start_x = (350 - total_width) // 2
    start_y = 10

    def select_color(event):
        nonlocal red, green, blue
        item_id = event.widget.find_closest(event.x, event.y)[0]
        color = event.widget.itemcget(item_id, "fill")
        red, green, blue = ImageColor.getcolor(color, "RGB")
        color_label.config(text=f"R: {red}  G: {green}  B: {blue}")
        update_preview()

    for i, color in enumerate(colors):
        x = start_x + i * (square_size + margin)
        y = start_y
        canvas.create_rectangle(x, y, x + square_size, y + square_size, fill=color, outline="", tags="color_square")
        canvas.tag_bind("color_square", "<Button-1>", select_color)

    # Create the color wheel
    color_wheel = Canvas(root, width=256, height=70, bg="#333333")
    color_wheel.place(x=175, y=310)

    def update_color_wheel(event):
        x = event.x
        red_val = int((x / 256) * red)
        green_val = int((x / 256) * green)
        blue_val = int((x / 256) * blue)
        color = (red_val, green_val, blue_val)
        color_hex = '#%02x%02x%02x' % color
        color_wheel.delete("color_wheel")
        color_wheel.create_rectangle(10, 10, 260, 60, fill=color_hex, outline="", tags="color_wheel")
        update_preview()

    color_wheel.bind("<B1-Motion>", update_color_wheel)

    # Create the save button
    save_button = ttk.Button(root, text="Save LUT", command=save_lut)
    save_button.place(x=250, y=350)

    # Create the color label
    color_label = Label(root, text="R: 0  G: 0  B: 0", fg="white", bg="#333333")
    color_label.place(x=240, y=300)

    # Create the preview canvas
    preview_canvas = Canvas(root, width=50, height=50, bg="#333333", highlightthickness=0)
    preview_canvas.place(x=275, y=200)
    preview_canvas.config(background='#333333')

    def update_preview():
        preview_image = Image.new("RGBA", (50, 50), color=(red, green, blue, 255))
        preview_photo = ImageTk.PhotoImage(preview_image)
        preview_canvas.create_image(0, 0, anchor="nw", image=preview_photo)
        preview_canvas.image = preview_photo  # Keep a reference to avoid garbage collection

    # Call the function initially to display the preview
    update_preview()

    # Start the Tkinter event loop
    root.mainloop()

# Call the function to create the LUT
create_lut()
