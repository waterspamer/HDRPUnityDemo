// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/DepthShader"
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
	{

		CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

		uniform sampler2D _CameraDepthTexture; //the depth texture

	struct v2f
	{
		float4 pos : SV_POSITION;
		float4 projPos : TEXCOORD1; //Screen position of pos
	};

	v2f vert(appdata_base v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.projPos = ComputeScreenPos(o.pos);

		return o;
	}

	half4 frag(v2f i) : COLOR
	{
		float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD(i.projPos)).r);
		half4 c = half4(depth,depth,depth,1);
		return c;
	}
		ENDCG
	}
	}
		FallBack "VertexLit"
}