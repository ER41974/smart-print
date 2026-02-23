import os
from PIL import Image, ImageDraw

def create_icon():
    sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    images = []

    try:
        base_img = Image.open('assets/icon/avatar.png').convert("RGBA")
    except FileNotFoundError:
        print("Erreur : Le fichier assets/icon/avatar.png est introuvable.")
        return

    for size in sizes:
        img = base_img.resize(size, Image.Resampling.LANCZOS)

        draw = ImageDraw.Draw(img)
        printer_w, printer_h = size[0] // 2.5, size[1] // 3
        px, py = size[0] - printer_w - 2, size[1] - printer_h - 2
        
        # Ensure coordinates are ordered correctly for PIL
        r1 = [min(px + 4, px + printer_w - 4), min(py - 4, py), max(px + 4, px + printer_w - 4), max(py - 4, py)]
        draw.rectangle(r1, fill="white", outline="black")
        
        r2 = [px, py, px + printer_w, py + printer_h]
        draw.rectangle(r2, fill="#333333", outline="white")

        r3 = [min(px + 4, px + printer_w - 4), min(py + 4, py + printer_h + 4), max(px + 4, px + printer_w - 4), max(py + 4, py + printer_h + 4)]
        draw.rectangle(r3, fill="white")
        
        images.append(img)

    os.makedirs('SmartPrint.App/assets/icon', exist_ok=True)
    img.save('SmartPrint.App/assets/icon/smartprint.ico', format='ICO', sizes=sizes, append_images=images)
    img.save('assets/icon/SmartPrint.ico', format='ICO', sizes=sizes, append_images=images)
    print("Nouvelles icônes générées et remplacées avec succès !")

if __name__ == "__main__":
    create_icon()
