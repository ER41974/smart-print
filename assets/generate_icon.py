import os
from PIL import Image, ImageDraw

def create_icon():
    sizes = [(16, 16), (32, 32), (48, 48), (256, 256)]
    images = []

    for size in sizes:
        img = Image.new('RGBA', size, (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)
        
        # Simple blue rounded rect background
        draw.rounded_rectangle([(0, 0), size], radius=size[0]//6, fill="#007ACC")
        
        # White paper/printer body
        margin = size[0] // 4
        draw.rectangle([(margin, margin), (size[0]-margin, size[1]-margin)], fill="white")
        
        images.append(img)

    img.save('assets/SmartPrint.ico', format='ICO', sizes=sizes, append_images=images)
    print("Created assets/SmartPrint.ico")

if __name__ == "__main__":
    try:
        create_icon()
    except Exception as e:
        print(f"Error: {e}")
