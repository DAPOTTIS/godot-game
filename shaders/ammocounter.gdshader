shader_type spatial;

uniform sampler2D viewport;
varying vec2 uv;
uniform float angle;

void vertex(){
	uv = 0.5 - UV;
	mat2 rotation = mat2(vec2(sin(angle), -cos(angle)), vec2(cos(angle), sin(angle)));
	uv *= rotation;
	uv += 0.5;
}

void fragment() {
	ALBEDO = texture(viewport,uv).xyz * 5.0;
}
