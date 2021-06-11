Shader "Unlit/Lerp"
{
   //show values to edit in inspector
  Properties{
    _SrcZeroValue ("Src 0 Value", Color) = (0,0,0,1) //source min value
    _SrcOneValue ("Src 1 Color", Color) = (1,1,1,1) //source max value
    _TargetZeroValue ("Target 0 Value", Color) = (0,0,0,1) //target min value
    _TargetOneValue ("Target 1 Color", Color) = (1,1,1,1) //target max value
    _Wave ("Has Wave", Range(0, 1)) = 1
    _y ("Y lerp", Range(0, 1)) = 1
  }

  SubShader{
    //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
    Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

    Pass{
      CGPROGRAM

      //include useful shader functions
      #include "UnityCG.cginc"
      #include "Interpolation.cginc"

      //define vertex and fragment shader
      #pragma vertex vert
      #pragma fragment frag

      //the colors to blend between
      fixed4 _SrcZeroValue;
      fixed4 _SrcOneValue;
      fixed4 _TargetZeroValue;
      fixed4 _TargetOneValue;
      float _Wave;
      float _y;

      //the object data that's put into the vertex shader
      struct appdata{
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
      };

      //the data that's used to generate fragments and can be read by the fragment shader
      struct v2f{
        float4 position : SV_POSITION;
        float2 uv : TEXCOORD0;
      };

      //the vertex shader
      v2f vert(appdata v){
        v2f o;
        //convert the vertex positions from object space to clip space so they can be rendered
        o.position = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv;

        if (_Wave > 0)
        {
          o.uv.x += sin((o.uv.x + o.uv.y) + _Time.g * 1) * 0.01 * 15;
          o.uv.y += cos((o.uv.x - o.uv.y) + _Time.g * 1) * 0.01 * 15;
        }

        return o;
      }

      //the fragment shader
      fixed4 frag(v2f i) : SV_TARGET{
        float blend;

        if (_y > 0)
            blend = i.uv.y;
          else 
            blend = i.uv.x;

          fixed4 col = invLerp(_SrcZeroValue, _TargetZeroValue, blend);
        return col;
      }

      ENDCG
    }
  }
}