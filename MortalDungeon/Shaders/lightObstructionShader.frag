#version 410 core

layout(location = 0) out vec4 outputColor;

in vec2 obstructorCoordinates;

in vec2 texCoords;

const int TILE_OFFSET = 60; //how many extra tiles we're using (to cover the edges)

const int TILE_LIGHTING_WIDTH = 32;
const int TILES_PER_ROW = 150 + TILE_OFFSET;


const int TOTAL_WIDTH = TILE_LIGHTING_WIDTH * TILES_PER_ROW; //just assume constant width for now

uniform sampler2D texture0;

void main(void)
{
	outputColor = texture(texture0, texCoords);

	if(outputColor[3] < 0.1)
		discard;
}
