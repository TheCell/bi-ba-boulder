For the script to work you need Blender installed and added Blender.exe as a System variable

Use the following format in the command line:
``blender --background --python meshreducer.py -- "input_model_path" reduction_factor``

Example:
``blender --background --python meshreducer.py -- "/path/to/your/model.obj" 0.01``
