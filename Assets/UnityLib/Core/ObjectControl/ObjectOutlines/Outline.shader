// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/Outline"
{
	Properties
	{
		_MainTex("Main Texture",2D) = "black"{}
	_SceneTex("Scene Texture",2D) = "black"{}
	_ScreenRectHeight("Screen rect height", Float) = 1
		_ScreenRectY("Screen rect Y", Float) = 0
	}
		SubShader
	{
		CGINCLUDE
		float4 OutlineColor;
	float OutlineStrength;
	float OutlineSize;
#pragma multi_compile __ FLIP_UVS
		ENDCG
		
		Pass
	{
		CGPROGRAM

		sampler2D _MainTex;

	//<SamplerName>_TexelSize is a float2 that says how much screen space a texel occupies.
	float2 _MainTex_TexelSize;

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uvs : TEXCOORD0;
	};

	v2f vert(appdata_base v)
	{
		v2f o;

		//Despite the fact that we are only drawing a quad to the screen, Unity requires us to multiply vertices by our MVP matrix, presumably to keep things working when inexperienced people try copying code from other shaders.
		o.pos = UnityObjectToClipPos(v.vertex);

		//Also, we need to fix the UVs to match our screen space coordinates. There is a Unity define for this that should normally be used.
		o.uvs = o.pos.xy / 2 + 0.5;
#if FLIP_UVS
		o.uvs.y = 1 - o.uvs.y;
#endif
		return o;
	}



	half4 frag(v2f i) : COLOR
	{

		float NumberOfIterations = 20;
	//split texel size into smaller words
	float TX_x = _MainTex_TexelSize.x;

	//and a final intensity that increments based on surrounding intensities.
	float4 BlurredGlowColor;

	//for every iteration we need to do horizontally
	for (int k = 0;k<NumberOfIterations;k += 1)
	{
		//increase our output color by the pixels in the area
		BlurredGlowColor += tex2D(_MainTex, float2(i.uvs.x,1-i.uvs.y) + float2(OutlineSize*(k - NumberOfIterations / 2)*TX_x,0)) / NumberOfIterations;
	}
	return BlurredGlowColor;
	}

		ENDCG

	}
		//end pass    

		GrabPass{}

		Pass
	{
		CGPROGRAM

		sampler2D _MainTex;
	sampler2D _SceneTex;


	//we need to declare a sampler2D by the name of "_GrabTexture" that Unity can write to during GrabPass{}
	sampler2D _GrabTexture;

	//<SamplerName>_TexelSize is a float2 that says how much screen space a texel occupies.
	float2 _GrabTexture_TexelSize;

#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

	struct v2f
	{
		float4 pos : SV_POSITION;
		float2 uvs : TEXCOORD0;
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uvs = o.pos.xy / 2 + 0.5;

#if FLIP_UVS
		o.uvs .y = 1 - o.uvs.y;
#endif
		return o;
	}


	half4 frag(v2f i) : COLOR
	{

		float NumberOfIterations = 20;
	//split texel size into smaller words
	float TX_y = _GrabTexture_TexelSize.y;

	//and a final intensity that increments based on surrounding intensities.
	float4 BlurredGlowColor = 0;
	float4 pixelColor = tex2D(_MainTex,i.uvs);
	if (pixelColor.r + pixelColor.g + pixelColor.b>0)
	{
		return tex2D(_SceneTex, i.uvs);
	}

#if FLIP_UVS

	float grabY = 1 - i.uvs.y;
#else
	float grabY = i.uvs.y;
#endif
	//for every iteration we need to do vertically
	for (int j = 0;j<NumberOfIterations;j += 1)
	{
		//increase our output color by the pixels in the area
		BlurredGlowColor += tex2D(_GrabTexture,float2(i.uvs.x, grabY) + float2(0, OutlineSize*(j - NumberOfIterations / 2)*TX_y)) / NumberOfIterations;
	}

	//this is alpha blending, but we can't use HW blending unless we make a third pass, so this is probably cheaper.
	half4 outcolor = BlurredGlowColor* OutlineColor * OutlineStrength + (1 - BlurredGlowColor)*tex2D(_SceneTex,float2(i.uvs.x, i.uvs.y));
	return outcolor;
	}

		ENDCG

	}
		//end pass    
	}
		//end subshader
}
//end shader