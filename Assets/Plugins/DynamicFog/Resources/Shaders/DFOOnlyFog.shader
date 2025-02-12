Shader "DynamicFog/Image Effect/Orthographic Only Fog" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_NoiseTex ("Noise (RGB)", 2D) = "white" {}
		_FogAlpha ("Alpha", Range (0, 1)) = 0.8
		_FogDistance ("Distance Params", Vector) = (0.1, 0.001, 1.0, 0.15)
		_FogHeightData ("Baseline Height", Vector) = (1,0,0,0.1)  // x = height, y = base height, z = clipping minimum height, w = height fall off
		_FogColor ("Color", Color) = (1,1,1,1)
		_FogColor2 ("Color 2", Color) = (1,1,1,1)
		_FogNoiseData ("Noise Data", Vector) = (0,0,0,0.1)
		_FogSpeed ("Speed", Range (0, 0.5)) = 0.1
		_FogOfWarCenter("Fog Of War Center", Vector) = (0,0,0)
		_FogOfWarSize("Fog Of War Size", Vector) = (1,1,1)
		_FogOfWar ("Fog of War Mask", 2D) = "white" {}
		_ClipDir("Camera Direction", Vector) = (0,0,1)
		_FogDither ("Fog Dither Strength", Float) = 0.03
	}
	SubShader {
    ZTest Always Cull Off ZWrite Off
   	Fog { Mode Off }
	Pass{

	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma fragmentoption ARB_precision_hint_fastest
	#pragma multi_compile __ FOG_OF_WAR_ON
	#pragma multi_compile __ DITHER_ON
	#pragma target 3.0
			
	#include "UnityCG.cginc"
	#include "DynamicFogDither.cginc"

	sampler2D _MainTex;
	sampler2D _NoiseTex;
	sampler2D_float _CameraDepthTexture;
	float4 _MainTex_ST;
	fixed _FogAlpha;
	float4 _FogDistance; // x = min distance, y = min distance falloff, x = max distance, y = max distance fall off
	float4 _FogHeightData;
	float4 _FogNoiseData; // x = noise, y = turbulence, z = depth attenuation
	float _FogSpeed;
	fixed4 _FogColor, _FogColor2;

    #if FOG_OF_WAR_ON 
    sampler2D _FogOfWar;
    float3 _FogOfWarCenter;
    float3 _FogOfWarSize;
    float3 _FogOfWarCenterAdjusted;
    #endif
    
    float4x4 _ClipToWorld;
    float3 _ClipDir;
           
	struct v2f {
	    float4 pos : SV_POSITION;
    	float2 uv : TEXCOORD0;
    	float2 depthUV : TEXCOORD1;
    	float3 cameraToFarPlane : TEXCOORD2;
	};

	v2f vert(appdata_img v) {
    	v2f o;
    	o.pos = UnityObjectToClipPos(v.vertex);
    	o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
    	o.depthUV = o.uv;
       	
    	#if UNITY_UV_STARTS_AT_TOP
    	if (_MainTex_TexelSize.y < 0) {
	        // Depth texture is inverted WRT the main texture
    	    o.depthUV.y = 1 - o.depthUV.y;
    	}
    	#endif
               
    	// Clip space X and Y coords
    	float2 clipXY = o.pos.xy / o.pos.w;
               
    	// Position of the far plane in clip space
    	float4 farPlaneClip = float4(clipXY, 1, 1);
               
    	// Homogeneous world position on the far plane
    	farPlaneClip *= float4(1,_ProjectionParams.x,1,1);   
    	float4 farPlaneWorld4 = mul(_ClipToWorld, farPlaneClip);
               
    	// World position on the far plane
    	float3 farPlaneWorld = farPlaneWorld4.xyz / farPlaneWorld4.w;
               
    	// Vector from the camera to the far plane
    	o.cameraToFarPlane = farPlaneWorld;
 
    	return o;
	}

	//Fragment Shader
	fixed4 frag (v2f i) : SV_Target {
   		fixed4 color = tex2D(_MainTex, i.uv);

    	// Reconstruct the world position of the pixel
		float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.depthUV));
		#if UNITY_REVERSED_Z
		depth = 1.0 - depth;
		#endif
		if (depth > _FogDistance.z) return color;
		
    	float3 worldPos = i.cameraToFarPlane - _ClipDir * _ProjectionParams.z * (1.0 - depth);
		worldPos.y -= _FogHeightData.y;
    	if (worldPos.y<_FogHeightData.z || worldPos.y>_FogHeightData.x+_FogNoiseData.y) return color;

    	#if FOG_OF_WAR_ON
 		fixed voidAlpha = 1.0;
		float2 fogTexCoord = worldPos.xz / _FogOfWarSize.xz - _FogOfWarCenterAdjusted.xz;
		voidAlpha = tex2D(_FogOfWar, fogTexCoord).a;
		if (voidAlpha <=0) return color;
   		#endif
		
    	// Compute noise
		float noise = tex2D(_NoiseTex, worldPos.xz * _FogNoiseData.w * 0.1 + _Time[1]*_FogSpeed).g;
		float nt = noise * _FogNoiseData.y;
		noise /= (depth*_FogNoiseData.z); // attenuate with distance

		// Compute ground fog color		
		worldPos.y -= nt;
		float d = (depth-_FogDistance.x)/_FogDistance.y;
		float dmax = (_FogDistance.z - depth) / _FogDistance.w;
		d = min(d, dmax);
		float fogHeight = _FogHeightData.x + nt;
		float h = (fogHeight - worldPos.y) / (fogHeight*_FogHeightData.w);
		float groundColor = saturate(min(d,h))*saturate(_FogAlpha*(1-noise*_FogNoiseData.x));
		
		#if FOG_OF_WAR_ON
		groundColor *= voidAlpha;
		#endif
		
		// Compute final blended fog color
		fixed4 fogColor = lerp(_FogColor, _FogColor2, saturate(worldPos.y / fogHeight));
	 	color = lerp(color, fogColor, groundColor);
		#if DITHER_ON
		ApplyColor(i.uv, color);
		#endif
		return color;
	}
	ENDCG
	}
}
FallBack Off
}