# Reality Scan steps
- Alignment > Import and Align Images
- Mesh Model
  - Pre-steps > Set Reconstruction Area
  - Create Model > High Detail
- Tools (Scene 3D) > Mesh Model > Cut by Box
  - Cut with "Fill cut holes"
- Mesh Model
  - Check Integrity
  - Check Topology
  - if needed Clean Model and repeat
- Tools (Scene 1D)
  - Simplify Tool
    - Target 100'000 triangle count
    - Smoothing Tool (Noise Removal)
- Mesh Model
  - Mesh Color & Texture: Unwrap
    - (Or just Load the settings file)
    - Settings > Texture resolution 4096 x 4096
    - Settings > Unwrap Method: Mosaicing based
  - Export Levels of Detail
    - (Or just Load the settings file)
    - Model Count: 3
    - Relative simplification percentage: 50.0
    - Simplification type: Absolute
    - Minimal triangles: 500
    - Export textures: Yes
    - Export vertex normals: No
    - Export vertex colors: No

# Prepare Model for export
- Open the LOD Models in Blender
- Reposition the Model
- Export as glb (binary gltf file)
- TODO: Scale Model to match real world size

# WIP Reducing the final mesh
For the script to work you need Blender installed and added Blender.exe as a System variable

Use the following format in the command line:
``blender --background --python meshreducer.py -- "input_model_path" reduction_factor texture_scale``

Example:
``blender --background --python meshreducer.py -- "/path/to/your/model.obj" 0.01 0.5``
