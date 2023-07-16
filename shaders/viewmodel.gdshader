
shader_type spatial;
render_mode depth_draw_opaque,cull_back;


uniform float fov : hint_range(20, 120) = 60;
uniform sampler2D albedo : source_color,filter_linear_mipmap,repeat_enable;
varying vec2 uv;
uniform float angle;

void vertex(){
	if (angle>0.0) {
		uv = 0.5 - UV;
		mat2 rotation = mat2(vec2(sin(angle), -cos(angle)), vec2(cos(angle), sin(angle)));
		uv *= rotation;
		uv += 0.5;
	}
	else { 
		uv = UV; 
	}
    // recreate the camera projection matrix with our custom fov value 
    float scale = 1.0 / tan(fov * PI * 0.5 / 180.0); 
    PROJECTION_MATRIX[0][0] = scale / (VIEWPORT_SIZE.x / VIEWPORT_SIZE.y);
    PROJECTION_MATRIX[1][1] = -scale;
}

void fragment() {
	// Scale the depth value 70%, this prevents most clipping.
	vec4 albedo_tex = texture(albedo,uv);
	DEPTH = FRAGCOORD.z;
	DEPTH = FRAGCOORD.z * 0.7;
	ALBEDO = albedo_tex.rgb;
}
