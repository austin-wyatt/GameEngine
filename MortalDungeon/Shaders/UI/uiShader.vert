#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in mat4 transform;
layout(location = 7) in vec4 aColor;
layout(location = 8) in vec4 compositeType; //the spritesheet position (0), the z position (1), inline thickness (2), material index (3)
layout(location = 9) in vec4 spritesheetComposite; //side lengths X and Y (1, 2), spritesheet columns and rows (2, 3)


out vec2 texCoord;
out vec4 appliedColor;

out float alpha_threshold;

out float fragDepth;

out float inlineThickness;
out vec4 inlineColor;

out float materialIndex;

uniform mat4 camera;

flat out int InstanceID; 

vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row);

void main(void)
{
	appliedColor = aColor;
	texCoord = vec2(aTexCoord);

	materialIndex = compositeType[3];

	alpha_threshold = 0.15;

	float columns = spritesheetComposite[2];
	float rows = spritesheetComposite[3];

    float row =  floor(compositeType[0] / rows);
	float column = compositeType[0] - row * rows;

	texCoord = vec2(setTexCoord(texCoord, columns, rows, column, row));


	inlineThickness = compositeType[2];
	inlineColor = vec4(0, 0, 0, 1);

	//Position handling

	vec3 pos = aPosition; 

	gl_Position = vec4(pos, 1.0) * transform;

//	fragDepth = compositeType[1];

	InstanceID = gl_InstanceID; 
}

vec2 setTexCoord(vec2 texCoords, float columns, float rows, float column, float row)
{
	float minBoundX = column / columns;
    float maxBoundX = (column + spritesheetComposite[0]) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + spritesheetComposite[1]) / rows;


	float xDiff = maxBoundX - minBoundX;
	float yDiff = maxBoundY - minBoundY;


	vec2 outTex = vec2(0, 0);

	outTex[0] = minBoundX;
	outTex[1] = minBoundY;


	outTex[0] += (texCoords[0] * xDiff);
	outTex[1] += (texCoords[1] * yDiff);

	return outTex;
}

