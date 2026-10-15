// Distant background asteroids. Drawn in the queue just after the skybox (which the
// built-in pipeline renders after all opaque geometry), with normal depth testing,
// so every level object occludes them and they paint over the sky only.
Shader "Stellar/DistantRock"
{
	Properties
	{
		_MainTex ("Albedo", 2D) = "white" {}
		_Color ("Tint", Color) = (0.75, 0.75, 0.78, 1)
		_Wrap ("Wrap (half-Lambert)", Range(0, 1)) = 0.5
		_Shininess ("Shininess", Range(0, 1)) = 0.3
		_SpecStrength ("Highlight Strength", Range(0, 2)) = 0.15
		_SpecTint ("Highlight Colour", Color) = (1, 1, 1, 1)
		_FillColor ("Fill", Color) = (0.06, 0.06, 0.08, 1)
	}
	SubShader
	{
		Tags { "Queue" = "AlphaTest+60" "RenderType" = "Opaque" "IgnoreProjector" = "True" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Wrap noshadow nolightmap nodynlightmap nodirlightmap nofog
		#pragma target 3.0
		#include "WrapLighting.cginc"

		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _FillColor;

		struct Input { float2 uv_MainTex; float4 color : COLOR; };

		void surf (Input IN, inout SurfaceOutputWrap o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			// Particle colour carries a per-rock brightness and fade in/out
			o.Albedo = tex.rgb * _Color.rgb * IN.color.rgb * IN.color.a;
			o.Emission = o.Albedo * _FillColor.rgb;
			o.Gloss = 1;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack Off
}
