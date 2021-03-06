//
// This file is part of the game Voxalia, created by Frenetic LLC.
// This code is Copyright (C) 2016-2017 Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

#version 430 core

#define MCM_TRANSP 0
#define MCM_VOX 0
#define MCM_GEOM_ACTIVE 0
#define MCM_NO_ALPHA_CAP 0
#define MCM_BRIGHT 0
#define MCM_INVERSE_FADE 0
#define MCM_FADE_DEPTH 0
#define MCM_LIGHTS 0
#define MCM_SHADOWS 0

#if MCM_VOX
layout (binding = 0) uniform sampler2DArray s;
layout (binding = 2) uniform sampler2DArray normal_tex;
#if MCM_LIGHTS
layout (binding = 3) uniform sampler2DArray htex;
#endif
#else
#if MCM_GEOM_ACTIVE
layout (binding = 0) uniform sampler2DArray s;
layout (binding = 1) uniform sampler2DArray normal_tex;
#if MCM_LIGHTS
layout (binding = 2) uniform sampler2DArray spec;
#endif
#else
layout (binding = 0) uniform sampler2D s;
layout (binding = 1) uniform sampler2D normal_tex;
#if MCM_LIGHTS
layout (binding = 2) uniform sampler2D spec;
#endif
#endif
#endif
layout (binding = 4) uniform sampler2D depth;
layout (binding = 5) uniform sampler2DArray shadowtex;

// ...

in struct vox_fout
{
	mat3 tbn;
	vec3 pos;
#if MCM_VOX
	vec3 texcoord;
	vec4 tcol;
	vec4 thv;
	vec4 thw;
#else
#if MCM_GEOM_ACTIVE
	vec3 texcoord;
#else
	vec2 texcoord;
#endif
#endif
	vec4 color;
#if MCM_INVERSE_FADE
	float size;
#endif
#if MCM_FADE_DEPTH
	float size;
#endif
} fi;

const int LIGHTS_MAX = 20;

// ...
layout (location = 4) uniform vec4 screen_size = vec4(1024, 1024, 0.1, 1000.0);
// ...
layout (location = 10) uniform vec3 sunlightDir = vec3(0.0, 0.0, -1.0);
layout (location = 11) uniform vec3 maximum_light = vec3(0.9, 0.9, 0.9);
layout (location = 12) uniform vec4 fogCol = vec4(0.0);
layout (location = 13) uniform float znear = 1.0;
layout (location = 14) uniform float zfar = 1000.0;
layout (location = 15) uniform float lights_used = 0.0;
layout (location = 16) uniform float minimum_light = 0.2;
#if MCM_LIGHTS
layout (location = 20) uniform mat4 shadow_matrix_array[LIGHTS_MAX];
layout (location = 40) uniform mat4 light_data_array[LIGHTS_MAX];
#endif

layout (location = 0) out vec4 color;

float linearizeDepth(in float rinput) // Convert standard depth (stretched) to a linear distance (still from 0.0 to 1.0).
{
	return (2.0 * znear) / (zfar + znear - rinput * (zfar - znear));
}

void applyFog()
{
	float dist = linearizeDepth(gl_FragCoord.z);
	float fogMod = dist * exp(fogCol.w) * fogCol.w;
	float fmz = min(fogMod, 1.0);
	color.xyz = min(color.xyz * (1.0 - fmz) + fogCol.xyz * fmz + vec3(fogMod - fmz), vec3(1.0));
}

float fix_sqr(in float inTemp)
{
	return 1.0 - (inTemp * inTemp);
}

void main()
{
	vec4 col = textureLod(s, fi.texcoord, textureQueryLod(s, fi.texcoord.xy).x);
#if MCM_VOX
	float rhBlur = 0.0;
	if (fi.tcol.w == 0.0 && fi.tcol.x == 0.0 && fi.tcol.z == 0.0 && fi.tcol.y > 0.3 && fi.tcol.y < 0.7)
	{
		rhBlur = (fi.tcol.y - 0.31) * ((1.0 / 0.38) * (3.14159 * 2.0));
	}
	else if (fi.tcol.w == 0.0 && fi.tcol.x == 0.0 && fi.tcol.z == 0.0 && fi.tcol.y > 0.3 && fi.tcol.y < 0.7)
	{
		col *= fi.tcol;
	}
	else if (fi.tcol.w == 0.0 && fi.tcol.x > 0.3 && fi.tcol.x < 0.7 && fi.tcol.y > 0.3 && fi.tcol.y < 0.7 && fi.tcol.z > 0.3 && fi.tcol.z < 0.7)
	{
		if (fi.tcol.z > 0.51)
		{
			col.xyz = vec3(1.0) - col.xyz;
		}
		else if (fi.tcol.x > 0.51)
		{
			col *= fi.tcol;
		}
		else
		{
			col *= vec4(texture(s, vec3(fi.texcoord.xy, 0)).xyz, 1.0);
		}
	}
	else
	{
		col *= fi.tcol;
	}
#if MCM_LIGHTS
	vec4 hintter = texture(htex, fi.texcoord);
	float specularStrength = hintter.x;
#endif // MCM_LIGHTS
#else // MCM_VOX
#if MCM_LIGHTS
	float specularStrength = texture(spec, fi.texcoord).x;
#endif // MCM_LIGHTS
#endif // else - MCM_VOX
#if MCM_NO_ALPHA_CAP
#else // MCM_NO_ALPHA_CAP
#if MCM_TRANSP
	if (col.w * fi.color.w >= 0.99)
	{
		discard;
	}
#else // MCM_TRANSP
	if (col.w * fi.color.w < 0.99)
	{
		discard;
	}
#endif // else - MCM_TRANPS
#endif // ELSE - MCM_NO_ALPHA_CAP
	color = col * fi.color;
#if MCM_BRIGHT
#else // MCM_BRIGHT
	float opac_min = 0.0;
	vec3 norms = texture(normal_tex, fi.texcoord).xyz * 2.0 - vec3(1.0);
	vec3 tf_normal = normalize(fi.tbn * norms);
#if MCM_LIGHTS
	vec3 res_color = vec3(0.0);
	int count = int(lights_used);
	float att = 0.0;
	for (int i = 0; i < count; i++)
	{
		mat4 light_data = light_data_array[i];
		mat4 shadow_matrix = shadow_matrix_array[i];
		// Light data.
		vec3 light_pos = vec3(light_data[0][0], light_data[0][1], light_data[0][2]); // The position of the light source.
		float diffuse_albedo = light_data[0][3]; // The diffuse albedo of this light (diffuse light is multiplied directly by this).
		float specular_albedo = light_data[1][0]; // The specular albedo (specular power is multiplied directly by this).
		float should_sqrt = light_data[1][1]; // 0 to not use square-root trick, 1 to use it (see implementation for details).
		vec3 light_color = vec3(light_data[1][2], light_data[1][3], light_data[2][0]); // The color of the light.
		float light_radius = light_data[2][1]; // The maximum radius of the light.
		vec3 eye_pos = vec3(light_data[2][2], light_data[2][3], light_data[3][0]); // The position of the camera eye.
		float light_type = light_data[3][1]; // What type of light this is: 0 is standard (point, sky, etc.), 1 is conical (spot light).
		float tex_size = light_data[3][2]; // If shadows are enabled, this is the inverse of the texture size of the shadow map.
		// float unused = light_data[3][3];
		vec4 f_spos = shadow_matrix * vec4(fi.pos, 1.0); // Calculate the position of the light relative to the view.
		f_spos /= f_spos.w; // Standard perspective divide.
		vec3 light_path = light_pos - fi.pos; // What path a light ray has to travel down in theory to get from the source to the current pixel.
		float light_length = length(light_path); // How far the light is from this pixel.
		float d = light_length / light_radius; // How far the pixel is from the end of the light.
		float atten = clamp(1.0 - (d * d), 0.0, 1.0); // How weak the light is here, based purely on distance so far.
		att += light_radius / 1e10;
		if (light_type >= 0.5) // If this is a conical (spot light)...
		{
			atten *= 1.0 - (f_spos.x * f_spos.x + f_spos.y * f_spos.y); // Weaken the light based on how far towards the edge of the cone/circle it is. Bright in the center, dark in the corners.
		}
		if (atten <= 0.0) // If light is really weak...
		{
			continue; // Forget this light, move on already!
		}
		if (should_sqrt >= 0.5) // If inverse square trick is enabled (generally this will be 1.0 or 0.0)
		{
			f_spos.x = sign(f_spos.x) * fix_sqr(1.0 - abs(f_spos.x)); // Inverse square the relative position while preserving the sign. Shadow creation buffer also did this.
			f_spos.y = sign(f_spos.y) * fix_sqr(1.0 - abs(f_spos.y)); // This section means that coordinates near the center of the light view will have more pixels per area available than coordinates far from the center.
		}
		// Create a variable representing the proper screen/texture coordinate of the shadow view (ranging from 0 to 1 instead of -1 to 1).
		vec3 fs = f_spos.xyz * 0.5 + vec3(0.5, 0.5, 0.5); 
		if (fs.x < 0.0 || fs.x > 1.0
			|| fs.y < 0.0 || fs.y > 1.0
			|| fs.z < 0.0 || fs.z > 1.0) // If any coordinate is outside view range...
		{
			continue; // We can't light it! Discard straight away!
		}
		// TODO: maybe HD well blurred shadows?
#if MCM_SHADOWS
#if 1 // TODO: MCM_SHADOW_BLURRING?
		float depth = 0.0;
		int loops = 0;
		for (float x = -1.0; x <= 1.0; x += 0.5)
		{
			for (float y = -1.0; y <= 1.0; y += 0.5)
			{
				loops++;
				float rd = texture(shadowtex, vec3(fs.x + x * tex_size, fs.y + y * tex_size, float(i))).r; // Calculate the depth of the pixel.
				depth += (rd >= (fs.z - 0.001) ? 1.0 : 0.0);
			}
		}
		depth /= loops;
#else
		float rd = texture(shadowtex, vec3(fs.x, fs.y, float(i))).r; // Calculate the depth of the pixel.
		float depth = (rd >= (fs.z - 0.001) ? 1.0 : 0.0); // If we have a bad graphics card, just quickly get a 0 or 1 depth value. This will be pixelated (hard) shadows!
#endif
		if (depth <= 0.0)
		{
			continue;
		}
#else
		const float depth = 1.0;
#endif
		vec3 L = light_path / light_length; // Get the light's movement direction as a vector
		vec3 diffuse = max(dot(tf_normal, L), 0.0) * vec3(diffuse_albedo); // Find out how much diffuse light to apply
		vec3 reller = normalize(fi.pos - eye_pos);
		float spec_res = pow(max(dot(reflect(L, -tf_normal), reller), 0.0), 200.0) * specular_albedo * specularStrength;
		opac_min += spec_res;
		vec3 specular = vec3(spec_res); // Find out how much specular light to apply.
		res_color += (vec3(depth, depth, depth) * atten * (diffuse * light_color) * color.xyz) + (min(specular, 1.0) * light_color * atten * depth); // Put it all together now.
	}
	color.xyz = min(res_color + color.xyz * 0.2, vec3(1.0));
#else // MCM_LIGHTS
	color.xyz *= min(max(dot(-tf_normal, sunlightDir) * maximum_light, max(0.2, minimum_light)), 1.0);
#endif // else - MCM_LIGHTS
	applyFog();
#if MCM_TRANSP
#if MCM_VOX
	if (rhBlur > 0.0)
	{
		float opacity_mod = length(fi.pos.xyz) * 0.05;
		color.w *= min(opacity_mod + opac_min, 0.9);
	}
#endif // MCM_VOX
#endif // MCM_TRANSP
#endif // else - MCM_BRIGHT
	if (fogCol.w > 1.0)
	{
		applyFog();
	}
#if MCM_INVERSE_FADE
	float dist = linearizeDepth(gl_FragCoord.z);
	vec2 fc_xy = gl_FragCoord.xy / screen_size.xy;
	float depthval = linearizeDepth(texture(depth, fc_xy).x);
	float mod2 = min(max(0.001 / max(depthval - dist, 0.001), 0.0), 1.0);
	if (mod2 < 0.8)
	{
		discard;
	}
#endif // MCM_INVERSE_FADE
#if MCM_FADE_DEPTH
	float dist = linearizeDepth(gl_FragCoord.z);
	vec2 fc_xy = gl_FragCoord.xy / screen_size.xy;
	float depthval = linearizeDepth(texture(depth, fc_xy).x);
	color.w *= min(max((depthval - dist) * fi.size * 0.5 * (screen_size.w - screen_size.z), 0.0), 1.0);
#endif // MCM_FADE_DEPTH
}
