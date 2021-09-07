#version 410 core

layout(location = 0) in vec3 aPosition;

layout(location = 1) in vec2 localCoordinates;
layout(location = 2) in float spritesheetPosition;

out vec2 texCoords;
out vec2 obstructorCoordinates;

const int TILE_OFFSET = 30; //how many extra tiles we're using (to cover the edges)

const int TILE_LIGHTING_WIDTH = 32;
const int TILES_PER_ROW = 150 + TILE_OFFSET;
const int TOTAL_WIDTH = TILE_LIGHTING_WIDTH * TILES_PER_ROW; //just assume constant width for now

vec2 setTexCoord(vec2 texCoordinates, float columns, float rows, float column, float row);

void main(void)
{
	obstructorCoordinates = vec2(localCoordinates.x, localCoordinates.y);
	
	if(int(obstructorCoordinates[0]) % 2 == 1){
		obstructorCoordinates[1] -= 0.5; 
	}

	obstructorCoordinates[1] += TILE_OFFSET / 2; 
	obstructorCoordinates[0] += TILE_OFFSET / 2;

	const float columns = 10;
	const float rows = 10;

    float row =  floor(spritesheetPosition / rows);
	float column = spritesheetPosition - row * rows;

	vec2 texPos = vec2(aPosition.xy);
	texPos += 1;
	texPos /= 2; 

	texCoords = setTexCoord(texPos, columns, rows, column, row);

	float tileProportion = TILE_LIGHTING_WIDTH / TOTAL_WIDTH;

	vec4 pos = vec4(0, 0, 0, 1);

	pos.x = obstructorCoordinates[0] / TILES_PER_ROW * 2 - 1;
	pos.y = obstructorCoordinates[1] / TILES_PER_ROW * 2 - 1;

	pos.x += aPosition.x / TILES_PER_ROW;
	pos.y += aPosition.y / TILES_PER_ROW;

//
	gl_Position = pos;
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


