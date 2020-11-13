Shader "Organic/Skin Gore" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		_NrmStrength ("Normal Strength", Range(-2,2)) = 1
		[NoScaleOffset] _Metallic ("Smoothness/Metallic Map", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 1
		_Metal ("Metallic", Range(0,1)) = 1
		_DetailMap ("Blending Detail Map", 2D) = "grey" {}
		_Hardness ("Blending Hardness", Range(0,1)) = 0.5
		_EdgeSize ("Edge Size", Range(0,1)) = 0.2
		_EdgeColor("Edge Color", Color) = (1,1,1,1)
		_EdgeGlossiness ("Edge Smoothness", Range(0,1)) = 1
		[PerRendererData] _GoreDamage ("Gore Damage Map", 2D) = "white" {}

	}

	SubShader {
		Tags { "RenderType"="TransparentCutout" "Queue"="Geometry"}
		LOD 200

		AlphaToMask On
		
		CGPROGRAM
		#pragma surface surf Standard nolightmap alphatest:_Cutoff

		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _Metallic;
		sampler2D _DetailMap;

		sampler2D _GoreDamage;

		struct Input {
			float2 uv_MainTex;
			float2 uv_DetailMap;
			float2 uv2_GoreDamage;
			float4 color : Color;
		};

		half _NrmStrength;
		half _Glossiness;
		half _Metal;
		fixed4 _Color;
		half _Hardness;
		half _EdgeSize;
		fixed4 _EdgeColor;
		half _EdgeGlossiness;

		// overlay blending
		half overlay(float a, float b)
		{
			if(a<0.5) return 2*a*b;
			else return 1-2*(1-a)*(1-b);
		}

		// inverse lerp function
		float alerp(float a, float b, float t) {
			return (t - a) / (b - a);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 goreData = tex2D (_GoreDamage, IN.uv2_GoreDamage);

			// get alpha from gore data with details overlaid
			half mask = tex2D (_DetailMap, IN.uv_DetailMap).r;
			mask = overlay(overlay(goreData.r, mask), mask);
			_Hardness = 1 - _Hardness;
			o.Alpha = saturate(alerp(0.5 - _Hardness / 2, 0.5 + _Hardness / 2, mask));
			// get edges for better transition
			half edge = saturate(alerp(0.5-(_Hardness / 2) - _EdgeSize, 0.5- _EdgeSize, 1-mask));
			clip(mask-0.1);

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb * lerp(1, _EdgeColor, edge);

			fixed3 nrm = UnpackNormal(tex2D (_NormalMap, IN.uv_MainTex));
			nrm = lerp(fixed3(0,0,1), nrm, _NrmStrength);

			// squish normals at edge to appear inset
			nrm.xy = lerp(nrm.xy, nrm.xy * -3, edge);

			o.Normal = nrm;

			o.Metallic = tex2D (_Metallic, IN.uv_MainTex).r * _Metal;
			o.Smoothness =  tex2D (_Metallic, IN.uv_MainTex).a * lerp(_Glossiness, _EdgeGlossiness, edge);
		}
		ENDCG

		
	}
	FallBack "Transparent"
}
