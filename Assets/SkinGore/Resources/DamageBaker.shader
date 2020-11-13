Shader "Hidden/DamageBaker"
{
    // Renders a white blob onto a mesh projected using its UV coordinates
    // Set _DamagePosition and _DamageRadius in script

    Properties{}
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            half3 _DamagePosition;
            half _DamageRadius;
            half _DamageAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float3 localPosition : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(2 * v.uv1.xy * float2(1, -1) + float2(-1, 1), 1, 1);
                o.vertex.y *= -_ProjectionParams.x;
                
                o.uv1 = v.uv1;
                o.localPosition = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                half dist = saturate(length(_DamagePosition - i.localPosition) / (_DamageRadius * 1.5));
                fixed4 col = (1 - (dist*dist)) * _DamageAmount;
                col.a = 1;
                return col;
            }
            ENDCG
        }
    }
}
