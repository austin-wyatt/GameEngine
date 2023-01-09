#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in mat4 transform;
layout(location = 7) in vec4 aColor;
layout(location = 8) in vec4 compositeType; //empty(0), the spritesheet position (1), the X and Y lengths of the spritesheet (2, 3)
layout(location = 9) in vec4 compositeType_2; //composite vector of spritesheet width, spritesheet height, empty (2, 3)

out vec2 texCoord;
out vec4 appliedColor;

out vec3 Normal;
out vec3 FragPos;

uniform mat4 camera;

flat out int InstanceID; 

vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row);

void main(void)
{
	appliedColor = aColor;

	float columns = compositeType_2[0];
	float rows = compositeType_2[1];

    float row =  floor(compositeType[1] / columns);
	float column = compositeType[1] - row * columns;

	texCoord = vec2(setTexCoord(aTexCoord, columns, rows, column, row));

	//Position handling
	float aspectRatio = compositeType[2] / compositeType[3];

	vec3 pos = aPosition; 
	pos[0] *= aspectRatio; //allow for non-square objects


	gl_Position = vec4(pos, 1.0) * transform * camera;

	FragPos = vec3(vec4(pos, 1.0) * transform);

	Normal = (vec4(aNormal, 0) * transform).xyz;

	InstanceID = gl_InstanceID; 
}

vec2 setTexCoord(vec2 texCoords, float columns, float rows, float column, float row)
{
	float minBoundX = column / columns;
    float maxBoundX = (column + compositeType[2]) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + compositeType[3]) / rows;


	float xDiff = maxBoundX - minBoundX;
	float yDiff = maxBoundY - minBoundY;


	vec2 outTex = vec2(0, 0);

	outTex[0] = minBoundX;
	outTex[1] = minBoundY;


	outTex[0] += (texCoords[0] * xDiff);
	outTex[1] += (texCoords[1] * yDiff);

	return outTex;
}

