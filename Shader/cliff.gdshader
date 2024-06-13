shader_type spatial;

uniform float blend_offset : hint_range(-1.0, 1.0) = 0.5;
uniform float blend_fade : hint_range(0.0, 0.1) = 0.1;

uniform vec3 uv_scale_fg = vec3(1.0);
uniform vec3 uv_scale_bg = vec3(1.0);

uniform sampler2D albedo_texture_fg : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D normal_texture_fg : hint_normal, filter_linear_mipmap, repeat_enable;

uniform sampler2D albedo_texture_bg : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D normal_texture_bg : hint_normal, filter_linear_mipmap, repeat_enable;

varying vec3 world_pos;
varying vec3 normal;

void vertex()
{
	normal = NORMAL;
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

vec3 triplanar_map(sampler2D tex, vec3 pos, vec3 scale)
{
    vec3 abs_normal = abs(normal);
    vec3 blend_weights = abs_normal / (abs_normal.x + abs_normal.y + abs_normal.z);

    vec2 uv_x = pos.yz * scale.yz;
    vec2 uv_y = pos.xz * scale.xz;
    vec2 uv_z = pos.xy * scale.xy;

    vec3 tex_x = texture(tex, uv_x).rgb;
    vec3 tex_y = texture(tex, uv_y).rgb;
    vec3 tex_z = texture(tex, uv_z).rgb;

    return tex_x * blend_weights.x + tex_y * blend_weights.y + tex_z * blend_weights.z;
}

void fragment()
{
    vec3 albedo_fg = triplanar_map(albedo_texture_fg, world_pos, uv_scale_fg);
    vec3 normal_fg = triplanar_map(normal_texture_fg, world_pos, uv_scale_fg);

    vec3 albedo_bg = triplanar_map(albedo_texture_bg, world_pos, uv_scale_bg);
    vec3 normal_bg = triplanar_map(normal_texture_bg, world_pos, uv_scale_bg);

    vec3 norm_bg = normal_bg * 2.0 - 1.0;
    norm_bg.z = sqrt(max(1.0 - norm_bg.x * norm_bg.x - norm_bg.y * norm_bg.y, 0.0));
    vec3 applied_normal = norm_bg.x * TANGENT + norm_bg.y * BINORMAL + norm_bg.z * NORMAL;
    vec3 up_vector_view = mat3(VIEW_MATRIX) * vec3(0.0, 1.0, 0.0);
    float view_dot_normal = dot(up_vector_view, applied_normal);
    float blend_mask = smoothstep(blend_offset - blend_fade, blend_offset + blend_fade, view_dot_normal);

    ALBEDO = mix(albedo_bg, albedo_fg, blend_mask);
    NORMAL_MAP = mix(normal_bg, normal_fg, blend_mask);
}