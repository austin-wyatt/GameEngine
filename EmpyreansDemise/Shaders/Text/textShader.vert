#version 430 core

out vec2 texCoord;
out vec4 appliedColor;

out float atlasIndex;

struct TransformInfo
{
	mat4 Transform;
	vec4 Color;
	float GlyphIndex;
	float AtlasIndex;

	vec2 RESERVED;
};

struct GlyphVertexInfo
{
	float VertexInfo[30]; //interleaved array in the form of vertex1 x, y, z .. texture1 x, y .. vertex2 x, y, z etc
};

layout(std430, binding = 2) buffer glyphSSBO
{
	GlyphVertexInfo GlyphInfo[];
};

layout(std430, binding = 3) buffer transformSSBO
{
	TransformInfo Info[];
};

void main()
{
	appliedColor = Info[gl_InstanceID].Color;

	atlasIndex = Info[gl_InstanceID].AtlasIndex;

	const int INTERLEAVED_VERTEX_SIZE = 5;

	int baseIndex = gl_VertexID * INTERLEAVED_VERTEX_SIZE;

	int glyphIndex = int(Info[gl_InstanceID].GlyphIndex);

	vec4 position = vec4(GlyphInfo[glyphIndex].VertexInfo[baseIndex], 
		GlyphInfo[glyphIndex].VertexInfo[baseIndex + 1], 
		GlyphInfo[glyphIndex].VertexInfo[baseIndex + 2],
		1);

	texCoord = vec2(GlyphInfo[glyphIndex].VertexInfo[baseIndex + 3],
		GlyphInfo[glyphIndex].VertexInfo[baseIndex + 4]);

	gl_Position = position * Info[gl_InstanceID].Transform; 
}