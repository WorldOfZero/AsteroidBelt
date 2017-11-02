// Upgrade NOTE: commented out 'float3 _WorldSpaceCameraPos', a built-in variable

Shader "Asteroids/Planet Ring Shadow Caster" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_DensityMap("Density Map", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_InnerRingDiameter("Inner Ring Diameter", Range(0, 1)) = 0.5
		_Cutoff("Cutoff", Range(0, 1)) = 0.5
	}
		SubShader{
		Tags{ "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }
		LOD 200
		CULL OFF

		CGPROGRAM

		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard alphatest:_Cutoff addshadow 

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

		sampler2D _MainTex;
	sampler2D _DensityMap;

	struct Input {
		float2 uv_MainTex;
		float3 worldPos;
	};

	half _Glossiness;
	half _Metallic;
	fixed4 _Color;
	float _MinimumRenderDistance;
	float _MaximumFadeDistance;
	float _InnerRingDiameter;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		// Albedo comes from a texture tinted by color
		float distance = length(_WorldSpaceCameraPos - IN.worldPos);
		//fixed distance = length(WorldSpaceViewDir(IN.worldPos));
		//fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;

		float2 position = float2((0.5 - IN.uv_MainTex.x) * 2, (0.5 - IN.uv_MainTex.y) * 2);
		float ringDistanceFromCenter = sqrt(position.x * position.x + position.y * position.y);

		clip(ringDistanceFromCenter - _InnerRingDiameter);
		clip(1 - ringDistanceFromCenter);
		clip(distance - _MinimumRenderDistance);

		fixed opacity = clamp((distance - _MinimumRenderDistance) / (_MaximumFadeDistance - _MinimumRenderDistance), 0, 1);

		fixed4 density = tex2D(_DensityMap, float2(clamp((ringDistanceFromCenter - _InnerRingDiameter) / (1 - _InnerRingDiameter), 0, 1), 0.5));
		fixed3 color = fixed3(position.x, position.y, density.a);
		o.Albedo = color;
		// Metallic and smoothness come from slider variables
		o.Metallic = _Metallic * opacity;
		o.Smoothness = _Glossiness * opacity;
		o.Alpha = opacity * density.a;
	}
	ENDCG
	}
		FallBack "Diffuse"
}