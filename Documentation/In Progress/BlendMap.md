

### Texturing

Each chunk can have a theoretical maximum of 15 textures present on it (1 texture needs to be used for the blend map)



Data in the blend map texture can be encoded in myriad ways. If we only wanted to support 4 textures we could use the RGBA values as is. For anything above 4 we need to subdivide the RGBA values. 8 or 16 textures could be achieved by taking the first half and second half of each color and normalizing it to the range of [0, 1]. For 8 this would look like blend.R * 2 and blend.R * 2 - 1



#### Per Chunk Blend Maps

Each chunk should contain it's own blend map. Additionally, each chunk will need to keep track of the textures it is using and build its blend map according to how it will pass those textures to the shader.



### Positioning

The extremities of the blend map in local coordinates [-1, 1] could be passed to the chunk shader and then by using the position of the vertex (this is the FragPos variable) we can find the coordinate on the overall map by finding the proportion of the the X and Y coordinates to the extremities