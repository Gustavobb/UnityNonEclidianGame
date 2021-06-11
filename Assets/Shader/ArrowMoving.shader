Shader "Unlit/ArrowMoving"
{
    //show values to edit in inspector
    Properties{
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("BG Color", Color) = (0, 0, 0, 1)
        _ScrollingSpeed ("Scrolling Speed", Range(0, 10)) = 1
        _Frequency ("Frequency", Range(0, 40)) = 1
        _Amplituide ("Amplituide", Range(0, 40)) = 1
    }

    SubShader{
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        Pass{
            CGPROGRAM

            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag

            //texture and transforms of the texture
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _ScrollingSpeed;
            float _Frequency;
            float _Amplituide;
            float4 _Color;
            float4 _Color1;

            //the object data that's put into the vertex shader
            struct appdata{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f{
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };

            //the vertex shader
            v2f vert(appdata v){
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPosition = ComputeScreenPos(o.position);
                o.uv.x -= _ScrollingSpeed * _Time.x;
                o.uv.x += sin((o.uv.x + o.uv.y) + _Time.g * _Frequency) * 0.01 * _Amplituide;
                o.uv.y += cos((o.uv.x - o.uv.y) + _Time.g * _Frequency) * 0.01 * _Amplituide;

                return o;
            }

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET{
                float4 texColor = tex2D(_MainTex, i.uv);
                texColor *= _Color;

                return texColor;
            }

            ENDCG
        }
    }

    Fallback "Standard"
}