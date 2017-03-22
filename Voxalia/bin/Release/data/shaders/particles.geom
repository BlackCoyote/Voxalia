//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_PRETTY 0
#define MCM_FADE_DEPTH 0

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
	vec2 scrpos;
	float z;
#else
	vec3 norm;
	vec3 pos;
	vec3 texcoord;
	vec4 color;
#endif
#if MCM_FADE_DEPTH
	float size;
#endif
} fi;

vec4 qfix(in vec4 pos, in vec3 right, in vec3 pos_norm)
{
#if MCM_PRETTY
	fi.position = pos;
	vec4 npos = proj_matrix * pos;
	fi.scrpos = npos.xy / npos.w * 0.5 + vec2(0.5);
	fi.z = npos.z;
	fi.tbn = transpose(mat3(right, cross(right, pos_norm), pos_norm)); // TODO: Neccessity of transpose()?
#else
	fi.norm = pos_norm;
	fi.pos = pos.xyz;
#endif
	return pos;
}

void main()
{
	vec3 pos = gl_in[0].gl_Position.xyz;
	 // TODO: Configurable particles render range cap!
	/*if (dot(pos, pos) > (50.0 * 50.0))
	{
		return;
	}*/
	vec3 up = vec3(0.0, 0.0, 1.0);
	vec3 pos_norm = normalize(pos.xyz);
	if (abs(pos_norm.x) < 0.01 && abs(pos_norm.y) < 0.01)
	{
		up = vec3(0.0, 1.0, 0.0);
	}
	float scale = f[0].texcoord.x * 0.5;
	float tid = f[0].texcoord.y;
	vec3 right = cross(up, pos_norm);
	fi.color = f[0].color;
#if MCM_FADE_DEPTH
	fi.size = 1.0 / scale;
#endif
	float angle = (scale * 5.0) * (float(int(tid) % 2) * 2.0) - 1.0;
	float c = cos(angle);
	float s = sin(angle);
	float C = 1.0 - c;
	mat4 rot_mat = mat4(
		pos_norm.x * pos_norm.x * C + c, pos_norm.x * pos_norm.y * C - pos_norm.z * s, pos_norm.x * pos_norm.z * C + pos_norm.y * s, 0.0,
		pos_norm.y * pos_norm.x * C + pos_norm.z * s, pos_norm.y * pos_norm.y * C + c, pos_norm.y * pos_norm.z * C - pos_norm.x * s, 0.0,
		pos_norm.z * pos_norm.x * C - pos_norm.y * s, pos_norm.z * pos_norm.y * C + pos_norm.x * s, pos_norm.z * pos_norm.z * C + c, 0.0,
		0.0, 0.0, 0.0, 1.0);
	vec3 right_n = (rot_mat * vec4(right, 1.0)).xyz;
	vec3 up_n = (rot_mat * vec4(up, 1.0)).xyz;
	// First Vertex
	gl_Position = proj_matrix * qfix(vec4(pos - (right_n + up_n) * scale, 1.0), right_n, pos_norm);
	fi.texcoord = vec3(0.0, 1.0, tid);
	EmitVertex();
	// Second Vertex
	gl_Position = proj_matrix * qfix(vec4(pos + (right_n - up_n) * scale, 1.0), right_n, pos_norm);
	fi.texcoord = vec3(1.0, 1.0, tid);
	EmitVertex();
	// Third Vertex
	gl_Position = proj_matrix * qfix(vec4(pos - (right_n - up_n) * scale, 1.0), right_n, pos_norm);
	fi.texcoord = vec3(0.0, 0.0, tid);
	EmitVertex();
	// Forth Vertex
	gl_Position = proj_matrix * qfix(vec4(pos + (right_n + up_n) * scale, 1.0), right_n, pos_norm);
	fi.texcoord = vec3(1.0, 0.0, tid);
	EmitVertex();
	EndPrimitive();
}
