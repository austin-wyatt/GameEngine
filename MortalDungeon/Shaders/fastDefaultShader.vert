#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in mat4 transform;
layout(location = 6) in vec4 aColor;
layout(location = 7) in vec4 compositeType; //composite vector of whether to enable the cam (0), the spritesheet position (1), the X and Y lengths of the spritesheet (2, 3)

out vec2 texCoord;
out vec4 appliedColor;
out float mixPercent;

uniform mat4 camera;

flat out int InstanceID; 

void main(void)
{
	appliedColor = aColor;
	mixPercent = 0;
	texCoord = aTexCoord;

	const int columns = 10; //maybe make these parameters at some point
	const int rows = 10;

    float row =  floor(compositeType[1] / rows);
	float column = compositeType[1] - row * rows;

    float minBoundX = column / columns;
    float maxBoundX = (column + compositeType[2]) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + compositeType[3]) / rows;

    if (maxBoundX > 1)
    {
        minBoundX = (column - compositeType[2]) / columns;
        maxBoundX = 1;
    }

    if (maxBoundY > 1) 
    {
        maxBoundY = (row - compositeType[3]) / rows;
        minBoundY = 1;
    }

	if(texCoord[0] == 0)
	{
		texCoord[0] = minBoundX;
	}
	else
	{
		texCoord[0] = maxBoundX;
	}

	if(texCoord[1] == 0)
	{
		texCoord[1] = minBoundY;
	}
	else
	{
		texCoord[1] = maxBoundY;
	}

	float aspectRatio = compositeType[2] / compositeType[3];

	vec3 pos = aPosition; 
	pos[0] *= aspectRatio; //allow for non-square objects

	if(compositeType[0] == 1)
	{
		gl_Position = vec4(pos, 1.0) * transform * camera;
	}
	else
	{
		gl_Position = vec4(pos, 1.0) * transform;
	}

	InstanceID = gl_InstanceID; 
}