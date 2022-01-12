﻿#version 330 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in vec4 appliedColorPrimary;
layout(location = 3) in vec4 compositeType_PPMO;  //spritesheet position [0], second texture spritesheet position [1], mix percent [2], whether to outline [3]
layout(location = 4) in vec4 appliedColorOutline;
layout(location = 5) in vec4 tileParameters;	  //tile X [0] and Y [1] position, empty [2] empty [3]
layout(location = 6) in vec4 compositeType_WH;    //framebuffer texture width [0] and height [1], client size x [2] and y [3]
layout(location = 7) in vec4 overlayIndexes;    
layout(location = 8) in vec4 overlayMixPercents;    

out vec2 primaryTextureCoordinates;
out vec2 mixedTextureCoordinates;
out vec4 primaryColor;
out float mixPercent;

out vec2 outlineTextureCoordinates;
out vec4 outlineColor;
out float outline;

out float alpha_threshold;

flat out int InstanceID; 

out vec2[4] _overlayTextureCoordinates;
out vec4 _overlayMixPercents;


const int tile_width = 124; //individual tile width
const int tile_width_partial = 92; //stacked width
const int tile_height = 108; //individual tile height
const int tile_height_partial = 54; //stacked height

const int xTilePlacementOffset = 0;
const int yTilePlacementOffset = 0;
const int yOffsetAmount = 0;

vec2 setTexCoord(vec2 primaryTextureCoordinates, float columns, float rows, float column, float row);

void main(void)
{
	primaryColor = appliedColorPrimary;
	mixPercent = compositeType_PPMO[2];

	

	const float columns = 20;
	const float rows = 20;

    float row =  floor(compositeType_PPMO[0] / rows);
	float column = compositeType_PPMO[0] - row * rows;

    
	primaryTextureCoordinates = setTexCoord(aTexCoord, columns, rows, column, row);

	row =  floor(compositeType_PPMO[1] / rows);
	column = compositeType_PPMO[1] - row * rows;

	mixedTextureCoordinates = setTexCoord(aTexCoord, columns, rows, column, row);

	row = 2;
	column = 0;
	outlineTextureCoordinates = setTexCoord(aTexCoord, columns, rows, column, row);


	_overlayMixPercents = overlayMixPercents;

	for(int i = 0; i < 4; i++)
	{
		row =  floor(overlayIndexes[i] / rows);
		column = overlayIndexes[i] - row * rows;

		_overlayTextureCoordinates[i] = setTexCoord(aTexCoord, columns, rows, column, row);
	}

	//Outline handling
	outlineColor = appliedColorOutline;
	outline = compositeType_PPMO[3];


	alpha_threshold = 0.05;


	//figure out the tile's position based on its X and Y coordinates
	int yOffset = int(tileParameters[0]) % 2 == 1 ? tile_height_partial - yOffsetAmount
		: 0;
	vec2 tileOriginPoint = vec2(0, 0);

	tileOriginPoint.x = (tileParameters[0] * tile_width_partial + tile_width_partial - xTilePlacementOffset * tileParameters[0]);
    tileOriginPoint.y = (tileParameters[1] + 1) * tile_height - yOffset + tile_height_partial - yTilePlacementOffset * tileParameters[1];

	tileOriginPoint.x /= compositeType_WH[0]; //value from 0 to 1 based on where it is in the texture
	tileOriginPoint.y /= compositeType_WH[1];

	tileOriginPoint.x = tileOriginPoint.x * 2 - 1; //convert to opengl coordinates [-1: 1]
	tileOriginPoint.y = tileOriginPoint.y * 2 - 1;
//	tileOriginPoint.y *= -1;

	float width_of_tile_clip_space = 128 / compositeType_WH[0];
	float height_of_tile_clip_space = 128 / compositeType_WH[1];

	vec4 pos = vec4(0, 0, 0, 1);

	pos.x = tileOriginPoint.x + aPosition.x * width_of_tile_clip_space * 2;
	pos.y = tileOriginPoint.y + aPosition.y * height_of_tile_clip_space * 2;

	gl_Position = pos;

	InstanceID = gl_InstanceID; 
}

vec2 setTexCoord(vec2 texCoordinates, float columns, float rows, float column, float row)
{
	vec2 coord = vec2(texCoordinates);

	float minBoundX = column / columns;
    float maxBoundX = (column + 1) / columns;

    float maxBoundY = row / rows;
    float minBoundY = (row + 1) / rows;

    if (maxBoundX > 1)
    {
        minBoundX = (column - 1) / columns;
        maxBoundX = 1;
    }

    if (maxBoundY > 1) 
    {
        maxBoundY = (row - 1) / rows;
        minBoundY = 1;
    }

	if(coord[0] == 0)
	{
		coord[0] = minBoundX;
	}
	else
	{
		coord[0] = maxBoundX;
	}

	if(coord[1] == 0)
	{
		coord[1] = minBoundY;
	}
	else
	{
		coord[1] = maxBoundY;
	}

	return coord;
}

