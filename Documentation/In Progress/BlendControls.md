# Blend Controls

Blend controls are how features interact with tile chunk blend maps. 

These can be used for just about anything: paths/roads, adding some randomness to terrain textures, making hilly areas have a rocky texture, etc.



### Challenges

A blend control will only have an origin point to make all of its changes from. This means that it will need to figure out all the maps/chunks that are effects by the control and apply any relevant changes.

Affected maps can be calculated in the tools and stored pretty cheaply.

Once we know where we need to apply blend controls, we need to actually apply them. The blend map doesn't correspond 1 to 1 to tile locations so some work will need to be done to line them up correctly.

Placing pixels might be complicated or it might be easy. The drawn pixels from the control needs to be the same regardless of whether the origin is on a loaded map or only a small fraction of the control is visible and etc. 



If we're reading pixels directly from a png image then the crux of what we need to know is when a blend map pixel is "invalid" and when to move to another chunk (and where in that chunk the adjacent pixel is). If a generalized method for determining this is found then using png backed blend controls should be entirely doable.



### Creating Blend Controls

Blend controls could possibly be entirely created with png images in the back end. This would simplify the creation greatly and these pngs would come with built in compression. All a control would need to do is specify which png it's reading from, supply a texture palette and color mapping, and then supply a scale (ie 1 to 1 png pixels to blend map pixels or 1 to 5 or 1 to 100, etc). 