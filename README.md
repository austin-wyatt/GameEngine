# Game Engine
This is a custom game engine I developed for fun.

It's baseline functional but anyone attempting to develop with this engine will need to create their own tooling. 

Included in the Game directory is the skeleton of a game I am developing with this engine. (Data files not included, sorry)

Although I've never checked specifically, there shouldn't be too many/any dependencies in the Engine Classes directory for items defined in the Game directory 
so with minor adjustments you can strip out the game code and implement your own if you so wish.

# Build
Most required dependencies are included in the visual studio solution file so those will be able to be acquired via NuGet and installed that way.

SharpFont must be compiled from source and included as a reference. 

Additionally, FreeType must be compiled from source to freetype6.dll and copied to the project's output directory. If compiling for Win64, FreeType must be patched according to the directions here: https://github.com/Robmaister/SharpFont.Dependencies/tree/master/freetype2

Microsoft Visual C++ and OpenAL redistributables are also required.
