// Original source: https://github.com/HungryProton/scatter
// License: MIT

shader_type spatial;

render_mode depth_draw_opaque, cull_disabled;

// Texture settings
uniform sampler2D texture_albedo : hint_default_white, repeat_disable;
uniform sampler2D texture_gradient : hint_default_white;
uniform sampler2D texture_noise : hint_default_white;
uniform float alpha_scissor_threshold : hint_range(0.0, 1.0);
uniform float total_height = 1.0;

// Wind settings
uniform vec2 wind_direction = vec2(1, -0.5);
uniform float wind_speed = 1.0;
uniform float wind_strength = 2.0;
uniform float noise_scale = 20.0;

// Lighting
uniform vec4 light_color : source_color;
uniform vec3 light_direction;

// Backlighting
uniform vec4 backlighting_color : source_color = vec4(0.5, 0.8, 0.3, 1.0); // Default: Light greenish
uniform float backlighting_intensity : hint_range(0.0, 2.0) = 0.5;
uniform float backlighting_power : hint_range(0.0, 8.0) = 1.0;

varying float color;
varying float height;
varying vec3 v_world_normal;

void vertex()
{
	height = VERTEX.y;

	vec4 world_pos = MODEL_MATRIX * vec4(VERTEX, 1.0);
	vec2 uv = (world_pos.xz + VERTEX.yy) / (noise_scale + 1e-2) ;
	vec2 panning_uv = uv + fract(TIME * wind_direction * wind_speed);
	float wind = texture(texture_noise, panning_uv).r * 2.0 - 0.4;
	color = texture(texture_noise, uv).r;

	float wind_influence = smoothstep(0, 1, 1.0 - UV.y);
	vec2 wind_offset = -wind_direction * wind_strength * wind_influence * wind;
	world_pos.xz += wind_offset;
	world_pos.y -= wind * wind_influence * wind_strength * 0.45;

	vec4 local_pos = inverse(MODEL_MATRIX) * world_pos;

	VERTEX = local_pos.xyz;
	v_world_normal = mat3(MODEL_MATRIX) * NORMAL;
}

void fragment()
{
	vec4 tex = texture(texture_albedo, UV);
	if (tex.a < alpha_scissor_threshold)
	{
		discard;
	}

	vec3 world_normal = normalize(v_world_normal);

	// Get the direction of the directional light
	vec3 light_dir = normalize(light_direction);

	// Calculate diffuse lighting
	float diff = max(dot(world_normal, light_dir), 0.0);
	vec3 diffuse = diff * light_color.rgb;

	// Backlighting (Transmission)
	float back_dot = dot(world_normal, -light_dir); 
	float backlighting = pow(max(back_dot, 0.0), backlighting_power) * backlighting_intensity;
	vec3 backlighting_result = backlighting * backlighting_color.rgb;

	vec4 gradient = texture(texture_gradient, vec2(height / total_height, 0.0));

	// Combine texture, gradient, diffuse lighting, and backlighting
	ALBEDO = tex.rgb * gradient.rgb * (diffuse + backlighting_result);
}
