from PIL import Image
import math

# --- Configuration ---
block_size = 4          # size of each block in pixels
blocks_x = 32           # how many blocks horizontally
blocks_y = blocks_x
img_size = (blocks_x * block_size, blocks_y * block_size)

# Create a new RGB image
img = Image.new("RGB", img_size)
pixels = img.load()

# Generate color pattern
r_value = 0
g_offset = 0
r_iteration = 0
for by in range(blocks_y):
    for bx in range(blocks_x):
        # Clamp R value to 255 max (wraps if >255)
        r = r_value % 256
        g_start = g_offset

        # Fill this 4x4 block
        g_value = g_start
        for y in range(block_size):
            for x in range(block_size):
                px = bx * block_size + x
                py = by * block_size + y
                g = g_value % 256
                pixels[px, py] = (0, 0, 0)
                g_value += 1  # increment G for each pixel

        r_value += 1  # next block gets next R value
        currentIteration = math.floor(r_value / 256)
        if (r_iteration != currentIteration):
            r_iteration = currentIteration
            g_offset += (block_size * block_size)

# Save image
img.save(f"rgb_blocks_{img_size[0]}x{img_size[1]}.png")
print(f"Generated {img_size[0]}x{img_size[1]} image as 'rgb_blocks_{img_size[0]}x{img_size[1]}.png'")
