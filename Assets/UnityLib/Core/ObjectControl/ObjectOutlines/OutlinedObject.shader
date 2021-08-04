Shader "Hidden/OutlinedObject"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		[HDR]
		_UniqueOutlineColor("Unique outline color",Color) = (0,0,0,1)
	}
	SubShader
	{
		Lighting Off
		Pass
	{
		CGPROGRAM
#pragma vertex VShader
#pragma fragment FShader
#include "UnityCG.cginc"

		struct v2f
		{
			float4 pos:POSITION;
			float2 uv:TEXCOORD0;
		};


		sampler2D _MainTex;
		float4 _MainTex_ST;
		float4 _UniqueOutlineColor;

		//just get the position correct
		v2f VShader(v2f i)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(i.pos);
			o.uv = TRANSFORM_TEX(i.uv, _MainTex);
			return o;
		}

		//return white
		half4 FShader(v2f i) :COLOR
		{
			return _UniqueOutlineColor*tex2D(_MainTex,i.uv).a;
		}

		ENDCG
	}
	}
}