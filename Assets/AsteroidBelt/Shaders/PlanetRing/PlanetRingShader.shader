// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

Shader "Asteroids/Planet Ring" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_ColorMap ("Color Map (RGB)", 2D) = "white" {}
		_DensityMap("Density Map", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_MinimumRenderDistance ("Minimum Render Distance", Float) = 10
		_MaximumFadeDistance ("Maximum Fade Distance", Float) = 20
		_InnerRingDiameter ("Inner Ring Diameter", Range(0, 1)) = 0.5
		_LightWidth("Planet Size", Range(0, 1)) = 0.9
		_LightScale("Light Scale", Float) = 5
	}
	SubShader {
		Tags { "RenderType" = "Transparent" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		LOD 200
		CULL OFF
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf StandardDefaultGI fullforwardshadows alpha:fade

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "UnityPBSLighting.cginc"

		inline half4 LightingStandardDefaultGI(SurfaceOutputStandard s, half3 viewDir, UnityGI gi)
		{
			half4 lighting = LightingStandard(s, viewDir, gi);
			lighting.rgb *= s.Occlusion * 0.9f;
			return lighting;
		}

		inline void LightingStandardDefaultGI_GI(
			SurfaceOutputStandard s,
			UnityGIInput data,
			inout UnityGI gi)
		{
			LightingStandard_GI(s, data, gi);
		}

		sampler2D _ColorMap;
		sampler2D _DensityMap;

		struct Input {
			float2 uv_ColorMap;
			float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _MinimumRenderDistance;
		float _MaximumFadeDistance;
		float _InnerRingDiameter;
		float _LightWidth;
		float _LightScale;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			float distance = length(_WorldSpaceCameraPos - IN.worldPos);
			//fixed distance = length(WorldSpaceViewDir(IN.worldPos));
			//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

			float2 position = float2((0.5 - IN.uv_ColorMap.x) * 2, (0.5 - IN.uv_ColorMap.y) * 2);
			float ringDistanceFromCenter = sqrt(position.x * position.x + position.y * position.y);

			clip(ringDistanceFromCenter - _InnerRingDiameter);
			clip(1 - ringDistanceFromCenter);
			clip(distance - _MinimumRenderDistance);

			fixed opacity = clamp((distance - _MinimumRenderDistance) / (_MaximumFadeDistance - _MinimumRenderDistance), 0, 1);

			float2 uv = float2(clamp((ringDistanceFromCenter - _InnerRingDiameter) / (1 - _InnerRingDiameter), 0, 1), 0.5);
			fixed4 density = tex2D(_DensityMap, uv);
			fixed4 color = tex2D(_ColorMap, uv) * _Color;
			o.Albedo = color.rgb; 
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic * opacity;
			o.Smoothness = _Glossiness * opacity;
			o.Alpha = min(1, opacity * density.a * ((color.r + color.g + color.b)));

			//TODO: Calculate Occlusion
			//_WorldSpaceLightPos0
			float3 lightToPoint = _WorldSpaceLightPos0.xyz - IN.worldPos;
			float3 lightToObject = _WorldSpaceLightPos0 - float3(0,0,0);
			lightToPoint = normalize(lightToPoint);
			lightToObject = normalize(lightToObject);
			o.Occlusion = clamp((-dot(lightToPoint, lightToObject) + _LightWidth) * _LightScale, 0, 1);
		}
		ENDCG
	}
	FallBack "Diffuse"
}
