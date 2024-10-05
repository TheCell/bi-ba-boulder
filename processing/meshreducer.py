import bpy
import sys
import os

# Get command-line arguments
argv = sys.argv

# Blender includes its own arguments, so skip until '--' (this separates Blender's args from custom args)
argv = argv[argv.index("--") + 1:]

# Parse custom arguments (input_path, reduction_factor, output_path)
input_path = argv[0]  # Path to the input model file
reduction_factor = float(argv[1])  # Mesh reduction factor (e.g., 0.5)
texture_scale = float(argv[2])  # Factor to scale textures (e.g., 0.5 for 50% reduction)


# Extract the directory, filename, and extension from the input path
input_dir = os.path.dirname(input_path)
input_filename = os.path.splitext(os.path.basename(input_path))[0]
input_extension = os.path.splitext(input_path)[1]


# Create the output filename with the reduction factor included (e.g., model_0.5.gltf)
output_filename = f"{input_filename}_reduced_{reduction_factor}{'.gltf'}"
output_path = os.path.join(input_dir, output_filename)

# Import the model (choose the correct import operator based on the file format)
if input_path.lower().endswith(".obj"):
    bpy.ops.wm.obj_import(filepath=input_path)
elif input_path.lower().endswith(".fbx"):
    bpy.ops.import_scene.fbx(filepath=input_path)
elif input_path.lower().endswith(".gltf") or input_path.lower().endswith(".glb"):
    bpy.ops.import_scene.gltf(filepath=input_path)
else:
    print("Unsupported file format")
    sys.exit()

# Deselect all objects
bpy.ops.object.select_all(action='DESELECT')

# Get all mesh objects in the scene
mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == 'MESH']

# Loop through each mesh object and apply the decimate modifier
for obj in mesh_objects:
    # Set object as active and select it
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    
    # Add decimate modifier to reduce the mesh
    decimate_mod = obj.modifiers.new(name="Decimate", type='DECIMATE')
    decimate_mod.ratio = reduction_factor  # Set reduction factor
    
    # Apply the decimate modifier (to make the changes permanent)
    bpy.ops.object.modifier_apply(modifier=decimate_mod.name)

    # Optionally: You can also save different objects as individual glTF files
    # output_path = f"/path/to/export/{obj.name}.gltf"

    # Deselect the object after applying the modifier
    obj.select_set(False)

# Function to resize textures in Blender without using PIL
def resize_texture_blender(image, scale):
    if image.size[0] > 0 and image.size[1] > 0:
        new_width = int(image.size[0] * scale)
        new_height = int(image.size[1] * scale)
        image.scale(new_width, new_height)
        print(f"Resized {image.name} to {new_width}x{new_height}")

# Find all textures used in the scene and resize them
for image in bpy.data.images:
    if image.filepath:  # Only process images that have an associated file path
        resize_texture_blender(image, texture_scale)

# Export the entire scene as GLTF
bpy.ops.export_scene.gltf(filepath=output_path)

# Print confirmation
print(f"Imported model from {input_path}, reduced the mesh by {reduction_factor}, resized textures by {texture_scale}, and exported to {output_path}")