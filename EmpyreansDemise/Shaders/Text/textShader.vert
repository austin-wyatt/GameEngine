#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in mat4 transform;
layout(location = 7) in vec4 aColor;
layout(location = 8) in vec4 compositeType; //the spritesheet position (0), alpha threshold (1), outline (2), bold (3)
layout(location = 9) in vec4 scissorData; //scissor X, scissor Y, scissor width, scissor height

out vec2 texCoord;
out vec4 appliedColor;

out float alpha_threshold;

out vec3 FragPos;

out vec4 ScissorData;

flat out int InstanceID; 

out float BOLD;
out float OUTLINE;


vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row);

void main(void)
{
	appliedColor = aColor;
	texCoord = vec2(aTexCoord);

	const float columns = 16; //should be changed to 16x16 for 256 characters
	const float rows = 16;

    float row =  floor(compositeType[0] / rows);
	float column = compositeType[0] - row * rows;

	texCoord = vec2(setTexCoord(texCoord, columns, rows, column, row));

	ScissorData = scissorData;

	OUTLINE = compositeType[2];
	BOLD = compositeType[3];


	vec3 pos = aPosition; 

	alpha_threshold = compositeType[1];

	gl_Position = vec4(pos, 1.0) * transform;

	InstanceID = gl_InstanceID; 

	FragPos = vec3(vec4(pos, 1.0) * transform);
}

vec2 setTexCoord(vec2 texCoords, float columns, float rows, float column, float row)
{
	float minBoundX = column / columns;
    float maxBoundX = (column + 1) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + 1) / rows;


	float xDiff = maxBoundX - minBoundX;
	float yDiff = maxBoundY - minBoundY;


	vec2 outTex = vec2(0, 0);

	outTex[0] = minBoundX;
	outTex[1] = minBoundY;


	outTex[0] += (texCoords[0] * xDiff);
	outTex[1] += (texCoords[1] * yDiff);

	return outTex;
}

