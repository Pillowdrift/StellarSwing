// Rock of the swing blocks: albedo + normal map, wrapped diffuse so the unlit side
// keeps its texture, a fill term for faces pointing away from the sun, and a rim light that separates the block from the sky.
Shader "Stellar/OreStone"
{
	Properties
	{
		_Color ("Tint", Color) = (0.6, 0.62, 0.66, 1)
		_MainTex ("Albedo", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_BumpScale ("Normal Strength", Float) = 1
		_Wrap ("Wrap (half-Lambert)", Range(0, 1)) = 0.7
		_Shininess ("Shininess", Range(0, 1)) = 0.35
		_SpecStrength ("Highlight Strength", Range(0, 2)) = 0.25
		_SpecTint ("Highlight Colour", Color) = (1, 1, 1, 1)
		_FillColor ("Fill (lights the side away from the sun)", Color) = (0.22, 0.22, 0.26, 1)
		_RimColor ("Rim Colour", Color) = (0.55, 0.6, 0.75, 1)
		_RimPower ("Rim Power", Range(0.5, 8)) = 3
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 300

		CGPROGRAM
		#pragma surface surf Wrap fullforwardshadows
		#pragma target 3.0
		#include "WrapLighting.cginc"

		sampler2D _MainTex;
		sampler2D _BumpMap;
		half _BumpScale;
		fixed4 _Color;
		fixed4 _FillColor;
		fixed4 _RimColor;
		half _RimPower;

		struct Input { float2 uv_MainTex; float3 viewDir; };

		void surf (Input IN, inout SurfaceOutputWrap o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = tex.rgb * _Color.rgb;
			o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			o.Emission = o.Albedo * _FillColor.rgb + _RimColor.rgb * pow(rim, _RimPower) * _RimColor.a;
			o.Gloss = 1;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
