Shader "Custom/VertexDisplacement"
{
        //https://www.ronja-tutorials.com/

    Properties {
		_TimeScale ("Scrolling Speed", Range(0, 2)) = .15
        _Freq ("Frequency", Range(0, 10)) = .5
        _Color ("Tint", Color) = (1, 0, 0, 1)
        _MainTex ("Texture", 2D) = "white" {}
	}
	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

		CGPROGRAM

        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
		#pragma target 3.0

		#include "Random.cginc"

		float _TimeScale, _Freq;
        sampler2D _MainTex;
        fixed4 _Color;

		struct Input {
            float2 uv_MainTex;
        };

		void vert(inout appdata_full data)
        {
            float4 modifiedPos = data.vertex;
            modifiedPos.y += sin(data.vertex.x * 2 + _Time.y * _Freq) * _TimeScale;
            
            float3 posPlusTangent = data.vertex + data.tangent * 0.01;
            posPlusTangent.y += sin(posPlusTangent.x * 2 + _Time.y * _Freq) * _TimeScale;

            float3 bitangent = cross(data.normal, data.tangent);
            float3 posPlusBitangent = data.vertex + bitangent * 0.01;
            posPlusBitangent.y += sin(posPlusBitangent.x * 2 + _Time.y * _Freq) * _TimeScale;

            float3 modifiedTangent = posPlusTangent - modifiedPos;
            float3 modifiedBitangent = posPlusBitangent - modifiedPos;

            float3 modifiedNormal = cross(modifiedTangent, modifiedBitangent);
            data.normal = normalize(modifiedNormal);
            data.vertex = modifiedPos;
        }

        void surf (Input i, inout SurfaceOutputStandard o) 
        {
            fixed4 col = tex2D(_MainTex, i.uv_MainTex);
            col *= _Color;
            o.Albedo = col.rgb;
        }

		ENDCG
	}
	FallBack "Standard"
}