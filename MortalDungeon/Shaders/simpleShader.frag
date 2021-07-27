﻿#version 410 core

layout(location = 0) out vec4 outputColor;
//out vec4 outputColor;

in vec2 texCoord;

uniform sampler2D texture0;


void main(void)
{
	outputColor = texture(texture0, vec2((texCoord.x + 1) / 2, (texCoord.y + 1) / 2));
//	outputColor = vec4((texCoord.y), 0, 0, 1);
}