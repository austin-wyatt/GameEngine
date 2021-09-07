#version 410 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 localCoordinates;
layout(location = 2) in vec4 lightColor;
layout(location = 3) in float alphaFalloff;
layout(location = 4) in float radius;
layout(location = 5) in vec4 environmentColor;


out vec4 color;
out float alpha_falloff;
out vec2 centerTexel;

out vec4 envColor;

const int TILE_OFFSET = 30; //how many extra tiles we're using (to cover the edges)

const int TILE_LIGHTING_WIDTH = 32;
const int TILES_PER_ROW = 150 + TILE_OFFSET;
const int TOTAL_WIDTH = TILE_LIGHTING_WIDTH * TILES_PER_ROW; //just assume constant width for now

void main(void)
{
	color = lightColor;
	alpha_falloff = alphaFalloff;
	centerTexel = vec2(localCoordinates);
	envColor = vec4(environmentColor);

	vec4 pos = vec4(0, 0, 0, 1);

	pos.x = localCoordinates[0] / TOTAL_WIDTH * 2 - 1;
	pos.y = localCoordinates[1] / TOTAL_WIDTH * 2 - 1;

	pos.x += (aPosition.x * radius) / TILES_PER_ROW;
	pos.y += (aPosition.y * radius) / TILES_PER_ROW;

	gl_Position = pos;
}



