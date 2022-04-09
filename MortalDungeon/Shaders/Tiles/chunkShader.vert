#version 420

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in mat4 transform;
layout(location = 7) in vec2 aSpritesheetInfo;


out vec2 texCoord;

out float alpha_threshold;

out float enableLighting;
out vec3 Normal;
out vec3 FragPos;

out float materialIndexF;

uniform mat4 camera;

vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row);

void main(void)
{
	texCoord = vec2(aTexCoord);

	alpha_threshold = 0.15;

	const float columns = 5;
	const float rows = 5;

    float row =  floor(aSpritesheetInfo[0] / rows);
	float column = aSpritesheetInfo[0] - row * rows;

	texCoord = vec2(setTexCoord(texCoord, columns, rows, column, row));
	materialIndexF = aSpritesheetInfo[1];

	//Position handling

	vec3 pos = aPosition; 

	enableLighting = 1;
	Normal = (vec4(aNormal, 0) * transform).xyz;

	gl_Position = vec4(pos, 1.0) * transform * camera;

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

