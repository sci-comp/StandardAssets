shader_type spatial;

uniform float blend_offset : hint_range(-1.0, 1.0) = 0.5;
uniform float blend_fade : hint_range(0.0, 0.1) = 0.1;

uniform vec3 uv_scale_fg = vec3(1.0);
uniform vec3 uv_scale_bg = vec3(1.0);

uniform sampler2D albedo_texture_fg : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D normal_texture_fg : hint_normal, filter_linear_mipmap, repeat_enable;

uniform sampler2D albedo_texture_bg : source_color, filter_linear_mipmap, repeat_enable;
uniform sampler2D normal_texture_bg : hint_normal, filter_linear_mipmap, repeat_enable;

uniform sampler2D normal_texture_uv : hint_normal, filter_linear_mipmap, repeat_enable;

uniform float normal_strength_uv : hint_range(0.0, 2.0) = 1.0;

varying vec3 world_pos;
varying vec3 world_normal;

void vertex()
{
    world_normal = normalize((MODEL_MATRIX * vec4(NORMAL, 0.0)).xyz);
    world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

vec3 triplanar_map(sampler2D tex, vec3 pos, vec3 scale, vec3 normal)
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

vec3 combine_normals(vec3 normal1, vec3 normal2, float strength)
{
    normal1 = normal1 * 2.0 - 1.0;
    normal2 = normal2 * 2.0 - 1.0;
    normal2.xy *= strength;

    // Normalize inputs and smooth blend to avoid sharp transitions
    normal1 = normalize(normal1);
    normal2 = normalize(normal2);

    vec3 combined = normalize(vec3(
        normal1.x * normal2.z + normal2.x * normal1.z,
        normal1.y * normal2.z + normal2.y * normal1.z,
        normal1.z * normal2.z - dot(normal1.xy, normal2.xy)
    ));

    // Apply bias to minimize sharp-angle artifacts
    combined = normalize(combined + vec3(0.01, 0.01, 0.01)); 

    return combined * 0.5 + 0.5;
}

void fragment()
{
    vec3 albedo_fg = triplanar_map(albedo_texture_fg, world_pos, uv_scale_fg, world_normal);
    vec3 albedo_bg = triplanar_map(albedo_texture_bg, world_pos, uv_scale_bg, world_normal);

    vec3 normal_fg = triplanar_map(normal_texture_fg, world_pos, uv_scale_fg, world_normal);
    vec3 normal_bg = triplanar_map(normal_texture_bg, world_pos, uv_scale_bg, world_normal);

    float blend_factor = clamp(smoothstep(blend_offset - blend_fade, blend_offset + blend_fade, world_normal.y), 0.0, 1.0);

    vec3 blended_albedo = mix(albedo_bg, albedo_fg, blend_factor);
    vec3 blended_normal = normalize(mix(normal_bg, normal_fg, blend_factor)); // Ensure normals stay unit-length

    vec3 uv_normal = texture(normal_texture_uv, UV).rgb;
    vec3 final_normal = combine_normals(blended_normal, uv_normal, normal_strength_uv);

    ALBEDO = blended_albedo;
    NORMAL_MAP = final_normal;
}