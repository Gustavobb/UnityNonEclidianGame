﻿Shader "Custom/PerlinMoving"
{    //https://www.ronja-tutorials.com/

   Properties {
		_CellSize ("Cell Size", Range(0, 4)) = 1
		_ScrollSpeed ("Scroll Speed", Range(0, 20)) = 1
		_VerticalMovement ("Vertical movement", Range(0, 1)) = 0
	}
	SubShader {
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

		CGPROGRAM

		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		#include "Random.cginc"

		float _CellSize;
		float _ScrollSpeed, _VerticalMovement;

		struct Input {
			float3 worldPos;
		};

		float easeIn(float interpolator){
			return interpolator * interpolator;
		}

		float easeOut(float interpolator){
			return 1 - easeIn(1 - interpolator);
		}

		float easeInOut(float interpolator){
			float easeInValue = easeIn(interpolator);
			float easeOutValue = easeOut(interpolator);
			return lerp(easeInValue, easeOutValue, interpolator);
		}

		float perlinNoise(float3 value){
			float3 fraction = frac(value);

			float interpolatorX = easeInOut(fraction.x);
			float interpolatorY = easeInOut(fraction.y);
			float interpolatorZ = easeInOut(fraction.z);

			float cellNoiseZ[2];
			[unroll]
			for(int z=0;z<=1;z++){
				float cellNoiseY[2];
				[unroll]
				for(int y=0;y<=1;y++){
					float cellNoiseX[2];
					[unroll]
					for(int x=0;x<=1;x++){
						float3 cell = floor(value) + float3(x, y, z);
						float3 cellDirection = rand3dTo3d(cell) * 2 - 1;
						float3 compareVector = fraction - float3(x, y, z);
						cellNoiseX[x] = dot(cellDirection, compareVector);
					}
					cellNoiseY[y] = lerp(cellNoiseX[0], cellNoiseX[1], interpolatorX);
				}
				cellNoiseZ[z] = lerp(cellNoiseY[0], cellNoiseY[1], interpolatorY);
			}
			float noise = lerp(cellNoiseZ[0], cellNoiseZ[1], interpolatorZ);
			return noise;
		}

		void surf (Input i, inout SurfaceOutputStandard o) {
			float3 value = i.worldPos / _CellSize;
			if (_VerticalMovement)
			value.x += _Time.y * _ScrollSpeed;
			else
			value.z += _Time.x * _ScrollSpeed;
			//get noise and adjust it to be ~0-1 range
			float noise = perlinNoise(value) + 0.5;
			noise = frac(noise * 6);
			
			float pixelNoiseChange = fwidth(noise);

			float heightLine = smoothstep(1-pixelNoiseChange, 1, noise);
			heightLine += smoothstep(pixelNoiseChange, 0, noise);
			
			o.Albedo = heightLine;
		}
		ENDCG
	}
	FallBack "Standard"
}