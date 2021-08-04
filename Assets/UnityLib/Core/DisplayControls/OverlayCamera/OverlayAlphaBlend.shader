Shader "Nettle/Overlay/AlphaBlend"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OverlayTex("Overlay Texture", 2D) = "white"{}
		_BlendValue("BlendValue",Range(0,1)) = 1
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
	#pragma multi_compile __ FLIP_UVS
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _OverlayTex;
			float _BlendValue;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 over = tex2D(_OverlayTex, i.uv);
				#if FLIP_UVS
				i.uv.y = 1 - i.uv.y;
				#endif
				fixed3 col = tex2D(_MainTex, i.uv);
				return fixed4(over.rgb *_BlendValue * over.a + col * (1-_BlendValue * over.a),1);
			}
			ENDCG
		}
	}
}
