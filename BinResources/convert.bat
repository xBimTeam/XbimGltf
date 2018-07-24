IfcConvert.exe %1 "%~n1.dae"
COLLADA2GLTF-bin.exe -i "%~n1.dae" -o "%~n1.gltf"
del "%~n1.dae"