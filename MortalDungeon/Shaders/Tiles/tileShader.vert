#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec3 aNormal;
layout(location = 3) in mat4 transform;
layout(location = 7) in vec4 aColor;
layout(location = 8) in vec4 compositeType; //the spritesheet position (0), whether to enable lighting (1), overlay 0 (2), overlay 0 color (3)
layout(location = 9) in vec4 overlaysComposite; //overlay 1 (0), overlay 1 color (1), overlay 2 (2), overlay 2 color (3)


out vec2 texCoord;
out vec4 appliedColor;

out float alpha_threshold;

out vec2[3] overlays;
out float[3] overlayColors;

out float enableLighting;
out vec3 Normal;
out vec3 FragPos;


uniform mat4 camera;

flat out int InstanceID; 

vec2 setTexCoord(vec2 texCoord, float columns, float rows, float column, float row);

void main(void)
{
	appliedColor = aColor;
	texCoord = vec2(aTexCoord);

	alpha_threshold = 0.15;

	const float columns = 20;
	const float rows = 20;

    float row =  floor(compositeType[0] / rows);
	float column = compositeType[0] - row * rows;

	texCoord = vec2(setTexCoord(texCoord, columns, rows, column, row));

	//Overlays
	row =  floor(compositeType[2] / rows);
	column = compositeType[2] - row * rows;
	overlays[0] = compositeType[2] >= 0 ? vec2(setTexCoord(vec2(aTexCoord), columns, rows, column, row)) : vec2(-1, -1);

	row =  floor(overlaysComposite[0] / rows);
	column = overlaysComposite[0] - row * rows;
	overlays[1] = compositeType[2] >= 0 ? vec2(setTexCoord(vec2(aTexCoord), columns, rows, column, row)) : vec2(-1, -1);

	row =  floor(overlaysComposite[2] / rows);
	column = overlaysComposite[2] - row * rows;
	overlays[2] = overlaysComposite[2] >= 0 ? vec2(setTexCoord(vec2(aTexCoord), columns, rows, column, row)) : vec2(-1, -1);


	overlayColors[0] = compositeType[3];
	overlayColors[1] = overlaysComposite[1];
	overlayColors[2] = overlaysComposite[3];

	//Position handling

	vec3 pos = aPosition; 

	enableLighting = compositeType[1];
	Normal = (vec4(aNormal, 0) * transform).xyz;

	gl_Position = vec4(pos, 1.0) * transform * camera;

	FragPos = vec3(vec4(pos, 1.0) * transform);

	InstanceID = gl_InstanceID; 
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

