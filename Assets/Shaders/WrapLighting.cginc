// Shared lighting for the swing blocks: wrapped (half-Lambert) diffuse so faces turned
// away from the sun stay readable, Blinn-Phong highlights, and a soft rim term.
#ifndef STELLAR_WRAP_LIGHTING
#define STELLAR_WRAP_LIGHTING

half _Wrap;          // 0 = plain Lambert, 1 = full half-Lambert
half _Shininess;     // highlight tightness, 0..1
half _SpecStrength;  // highlight brightness
fixed4 _SpecTint;    // highlight colour

struct SurfaceOutputWrap
{
	fixed3 Albedo;
	fixed3 Normal;
	fixed3 Emission;
	half Specular;
	fixed Gloss;
	fixed Alpha;
};

inline fixed4 LightingWrap (SurfaceOutputWrap s, half3 lightDir, half3 viewDir, half atten)
{
	half ndl = dot(s.Normal, lightDir);
	// Half-Lambert style: remap [-1,1] towards [0,1] by _Wrap, squared to keep some shape
	half wrapped = saturate((ndl + _Wrap) / (1.0 + _Wrap));
	half diffuse = lerp(saturate(ndl), wrapped * wrapped, _Wrap);

	half3 h = normalize(lightDir + viewDir);
	half spec = pow(saturate(dot(s.Normal, h)), exp2(10.0 * _Shininess + 1.0)) * _SpecStrength * s.Gloss;

	fixed4 c;
	c.rgb = (s.Albedo * diffuse + _SpecTint.rgb * spec) * _LightColor0.rgb * atten;
	c.a = s.Alpha;
	return c;
}

#endif
