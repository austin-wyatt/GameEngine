﻿#version 330

in vec3 aPosition;

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    gl_PointSize = 10.0;
}
