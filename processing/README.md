# Reality Scan steps
- Alignment > Import and Align Images
- Put some Control Points in
- Mesh Model
  - Pre-steps > Set Reconstruction Area
  - Create Model > High Detail
- Tools (Scene 3D) > Mesh Model > Cut by Box
  - Set Ground Plane
  - Cut with "Fill cut holes"
- Mesh Model
  - Check Integrity
  - Check Topology
  - if needed Clean Model and repeat
- Tools (Scene 1D)
  - Simplify Tool
    - Target 100'000 triangle count
    - Part merging: Create a singleton
    - Smoothing Tool (Noise Removal (Only do this on the original model))
    - If needed reduce in multiple steps
- Mesh Model
  - Mesh Color & Texture: Unwrap
    - (Or just Load the settings file)
    - Settings > Texture resolution 4096 x 4096
    - Settings > Unwrap Method: Geometric
  - Export Levels of Detail
    - (Or just Load the settings file)
    - Model Count: 3
    - Relative simplification percentage: 50.0
    - Simplification type: Absolute
    - Minimal triangles: 500
    - Export textures: Yes
      - Texel size: 4 x optimal (25% texture quality)
      - Texture format: WebP
    - Export vertex normals: No
    - Export vertex colors: No

# Prepare Model for export
- Open the LOD Models in Blender
- Reposition the Model and rotate
  - Front should face in -y direction
- Export as glb (binary gltf file)
- TODO: Scale Model to match real world size

# UV Grouping
UVs of Holds are grouped on the Texture as follows: R and G are just unique identifiers. Holds on top of other Holds are grouped by B values.

# Highlighting images
The highlighting images are 128x128 pixels in 24 bit depth. If a file is opened and saved in paint it will be converted to 32 bit.
Use Paint.NET.

# WIP Reducing the final mesh
For the script to work you need Blender installed and added Blender.exe as a System variable

Use the following format in the command line:
``blender --background --python meshreducer.py -- "input_model_path" reduction_factor texture_scale``

Example:
``blender --background --python meshreducer.py -- "/path/to/your/model.obj" 0.01 0.5``
