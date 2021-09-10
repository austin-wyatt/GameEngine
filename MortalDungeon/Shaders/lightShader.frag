#version 410 core

layout(location = 0) out vec4 outputColor;

const int TILE_LIGHTING_WIDTH = 32;
const int TILES_PER_ROW = 150;
const int TOTAL_WIDTH = TILE_LIGHTING_WIDTH * TILES_PER_ROW; //just assume constant width for now

in vec4 color;
in float alpha_falloff;
in vec2 centerTexel;

in vec4 envColor;

uniform sampler2D texture0;

vec2 lerp(vec2 a, vec2 b, float t);

void main(void)
{
	ivec2 texel = ivec2(gl_FragCoord[0], gl_FragCoord[1]);

	vec2 currTexel = vec2(gl_FragCoord);

	float dist = sqrt((centerTexel.x - texel.x) * (centerTexel.x - texel.x) + (centerTexel.y - texel.y) * (centerTexel.y - texel.y));
	float step_length = 1 / dist;


	vec4 obstructionColor;

	outputColor = vec4(color);

	 

	bool hitRed = false;
	for(int i = 0; i < dist; i++){
//		currTexel = lerp(texel, centerTexel, step_length * i);
		currTexel = lerp(centerTexel, texel, step_length * i);

		obstructionColor = texelFetch(texture0, ivec2(currTexel), 0);

		if(obstructionColor[0] > 0.9 && obstructionColor[3] > 0){
//			outputColor[3] = 0;
//			discard;
//			hitRed = true;
			outputColor[3] -= alpha_falloff * 5;
		}

//		if(hitRed && obstructionColor[0] < 0.9){
//			discard;
//		}
			

		outputColor[3] -= alpha_falloff;

		if(outputColor[3] <= 0){
//			outputColor = vec4(0, 0, 1, 1);
//			break;
			discard;
		}
	}

	float mixPercent = outputColor[3] / (color[3] == 0 ? 1 : color[3]);

	outputColor = mix(envColor, color, mixPercent);

//	outputColor = vec4(0, 0, 0, 0);
}

vec2 lerp(vec2 a, vec2 b, float t)
{
    return a + (b - a) * t;
}
