//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for contents of the license.
// If neither of these are not available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_PRETTY 0

layout (points) in;
layout (triangle_strip, max_vertices = 4) out;

layout (location = 1) uniform mat4 proj_matrix = mat4(1.0);

in struct vox_out
{
#if MCM_PRETTY
	vec4 position;
	vec2 texcoord;
	vec4 color;
	mat3 tbn;
#else
	vec3 norm;
	vec2 texcoord;
	vec4 color;
#endif
} f[1];

out struct vox_fout
{
#if MCM_PRETTY
	vec4 position;
	vec3 texcoord;
	vec4 color;
	mat3 tbn;
#else
	vec3 norm;
	vec3 texcoord;
	vec4 color;
#endif
	float size;
} fi;

void main()
{
	vec3 pos = gl_in[0].gl_Position.xyz;
	 // TODO: Configurable decal render range cap!
	/*if (dot(pos, pos) > (50.0 * 50.0))
	{
		return;
	}*/
#if MCM_PRETTY
	vec3 norm = vec3(f[0].tbn[0][2], f[0].tbn[1][2], f[0].tbn[2][2]);
	fi.tbn = f[0].tbn;
#else
	vec3 norm = f[0].norm;
	fi.norm = norm;
#endif
	float scale = f[0].texcoord.x * 0.5;
	float tid = f[0].texcoord.y;
	fi.color = f[0].color;
	fi.size = 1.0 / scale;
	vec3 xp = vec3(norm.z, norm.y, norm.x) * scale;
	vec3 yp = vec3(xp.y, xp.x, xp.z) * scale * 2.0;
	// First Vertex
	gl_Position = proj_matrix * vec4(pos - (xp + yp) * scale, 1.0);
	fi.texcoord = vec3(0.0, 1.0, tid);
	EmitVertex();
	// Second Vertex
	gl_Position = proj_matrix * vec4(pos + (xp - yp) * scale, 1.0);
	fi.texcoord = vec3(1.0, 1.0, tid);
	EmitVertex();
	// Third Vertex
	gl_Position = proj_matrix * vec4(pos - (xp - yp) * scale, 1.0);
	fi.texcoord = vec3(0.0, 0.0, tid);
	EmitVertex();
	// Forth Vertex
	gl_Position = proj_matrix * vec4(pos + (xp + yp) * scale, 1.0);
	fi.texcoord = vec3(1.0, 0.0, tid);
	EmitVertex();
	EndPrimitive();
}