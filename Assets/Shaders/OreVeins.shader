// Crystalline ore veins drawn over the stone of the swing blocks.
// The texture's alpha masks the veins; inside them the surface is glossy and slightly
// metallic so it catches the light, plus a soft emissive glow in the ore's colour.
Shader "Stellar/OreVeins"
{
	Properties
	{
		_MainTex ("Ore (RGB) Mask (A)", 2D) = "white" {}
		_CrystalColor ("Crystal Colour", Color) = (1, 0.3, 0.25, 1)
		_Color ("Brightness (animated by Glower)", Color) = (1, 1, 1, 1)
		_Glow ("Glow", Range(0, 2)) = 0.35
		_Metallic ("Metallic", Range(0, 1)) = 0.5
		_Glossiness ("Smoothness", Range(0, 1)) = 0.85
		_Contrast ("Mask Contrast", Range(1, 4)) = 1.8
	}
	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
		LOD 300
		ZWrite Off
		Offset -1, -1

		CGPROGRAM
		#pragma surface surf Standard alpha:fade
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _CrystalColor;
		fixed4 _Color;
		half _Glow;
		half _Metallic;
		half _Glossiness;
		half _Contrast;

		struct Input { float2 uv_MainTex; };

		void surf (Input IN, inout SurfaceOutputStandard o)
		{
			fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
			// Keep the texture's crystal detail, recoloured towards the ore colour
			half detail = dot(tex.rgb, half3(0.3, 0.59, 0.11));
			// _Color is a grey brightness pulse driven at runtime by the Glower script
			half3 crystal = _CrystalColor.rgb * _Color.rgb * (0.35 + 1.3 * detail);
			o.Albedo = crystal;
			o.Emission = crystal * _Glow;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = saturate((tex.a - 0.5) * _Contrast + 0.5) * _Color.a;
		}
		ENDCG
	}
	FallBack "Legacy Shaders/Transparent/Diffuse"
}
