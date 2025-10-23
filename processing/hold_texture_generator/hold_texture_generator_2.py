from PIL import Image

# --- Configuration ---
width = 64
height = 64
img_size = (width, height)

# Create a new RGB image
img = Image.new("RGB", img_size)
pixels = img.load()

# Generate color pattern
r_value = 0
g_value = 0
for py in range(height):
    for px in range(width):
        pixels[px, py] = (r_value, g_value, 0)
        r_value += 1  # next block gets next R value
        if (r_value > 255):
            r_value = 0
            g_value += 1

# Save image
img.save(f"rgb_ids_{img_size[0]}x{img_size[1]}.png")
print(f"Generated {img_size[0]}x{img_size[1]} image as 'rgb_ids_{img_size[0]}x{img_size[1]}.png'")
