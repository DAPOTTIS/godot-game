shader_type spatial;
render_mode specular_toon;

uniform float height_scale = 0.25;
uniform sampler2D noise;
uniform sampler2D normalmap;

varying vec2 tex_position;


float wave(vec2 position){
	position += texture(noise, position / 10.0).x * 2.0 - 1.0;
	vec2 wv = 1.0 - abs (sin(position));
	return pow(1.0 - pow(wv.x * wv.y, 0.65), 4.0);
}

float height(vec2 position, float time){
	float h = wave((position+time) * 0.4) * 0.3;
	h += wave((position - time) *0.3) *0.3;
	h += wave((position + time) * 0.5) * 0.2;
	h += wave((position - time) *0.6) * 0.2;
	return h;
}

void vertex() {
	vec2 pos = VERTEX.xz;
	float k = height(pos, TIME);
	NORMAL = normalize(vec3(k - height(pos + vec2(0.1, 0.0), TIME), 0.1, k - height(pos + vec2(0.0, 0.1), TIME)));
	VERTEX.y = k;
}

void fragment(){
	float fresnel = sqrt(1.0 - dot(NORMAL,VIEW));
	//NORMAL_MAP = texture(normalmap, tex_position).xyz;
	ALBEDO = vec3(0.01, 0.03, 0.05) + (0.1 * fresnel);
	METALLIC = 0.0;
	ROUGHNESS = 0.001 * (1.0 - fresnel);
	RIM = 0.2;
}


