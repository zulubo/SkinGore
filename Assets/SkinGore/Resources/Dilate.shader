Shader "Hidden/Dilate"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
		LOD 100
		ZWrite Off
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			void sample(float2 uv, int x, int y, inout fixed4 color)
			{
				fixed4 s = tex2Dlod(_MainTex, float4(uv + _MainTex_TexelSize * float2(x, y), 0, 0));
				if(s.a > 0.5)
				{
					color = s;
				}
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// return same pixel if has alpha
				fixed4 col = tex2Dlod(_MainTex, float4(i.uv, 0, 0));
				if(col.a > 0.5) return col;

				//otherwise, perform dilation by searching for nearest pixel with alpha
				sample(i.uv, 1, -1, col);
				sample(i.uv, 1, 0, col);
				sample(i.uv, 1, 1, col);
				sample(i.uv, 0, 1, col);
				sample(i.uv, -1, 1, col);
				sample(i.uv, -1, 0, col);
				sample(i.uv, -1, -1, col);
				sample(i.uv, 0, -1, col);

				// if dilation fails, return old value
				return col;
			}
			ENDCG
		}


	}
}
