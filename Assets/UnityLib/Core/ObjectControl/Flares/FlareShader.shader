// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/NettleFlare"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque"  "Queue" = "Transparent+1"}
		LOD 100
		Blend SrcAlpha One
		ZWrite Off
		ZTest Always
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma geometry geom
			#pragma fragment frag
			
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv:TEXCOORD0;
			};

			struct v2g
			{
				float4 vertex : SV_POSITION;
				float2 uv:TEXCOORD0;
				float4 color : TEXCOORD1;
			};

			struct g2f {
				float4 pos:SV_POSITION;
				float2 uv:TEXCOORD0;
				float4 color : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			
			float _MinSizeDistance;
			float _MaxSizeDistance;
			float _MinSize;
			float _MaxSize;
			float _MaxMeshPenetration;
			
			v2g vert (appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.color = v.color;
				o.uv = v.uv;
				return o;
			}

			struct transform {				
				float3 right;
				float3 up; 
				float3 forward;
				float3 worldPos;
				float scale;
				float4 color;
			};

			void TransferGeomToFrag(v2g v, inout g2f o, transform t, float2 offset) {				
				float3 world = t.worldPos + t.scale*(t.right * offset.x + t.up* offset.y + t.forward * 0.01f);
				o.pos = UnityObjectToClipPos(mul(unity_WorldToObject,world));
				o.color = t.color;
				o.uv = (offset + 1) / 2;
			}

			[maxvertexcount(6)]
			void geom(point v2g IN[1], inout TriangleStream<g2f> triStream)
			{
				v2g v = IN[0];

				float4 clip = UnityObjectToClipPos(v.vertex);
				float4 screenPos = ComputeScreenPos(clip);
				float depthSample = tex2Dlod(_CameraDepthTexture,float4(screenPos.xy / screenPos.w,0,0));
				if (LinearEyeDepth(depthSample) < clip.w - _MaxMeshPenetration) {
					return;
				}
				transform t;
				
				t.worldPos = mul(unity_ObjectToWorld, v.vertex);
				
				float3 look = _WorldSpaceCameraPos - t.worldPos;
				t.forward = normalize(look);
				t.right = normalize(cross(float3(1, 0, 0), t.forward));
				t.up = cross(t.forward, t.right);
				
				t.color = v.color;
				t.color.rgb*= v.uv.y;
				//t.scale = max(_MinSize,min(_MaxSize, v.uv.x * length(look)));
				float distanceT = saturate((length(look) - _MinSizeDistance)/(_MaxSizeDistance - _MinSizeDistance));
				t.scale = lerp(_MinSize,_MaxSize,distanceT);
				g2f o;
				//Triangle1
				o.color = v.color;
				TransferGeomToFrag(v, o, t, float2(1, -1));
				triStream.Append(o);
				TransferGeomToFrag(v, o, t, float2(1, 1));
				triStream.Append(o);
				TransferGeomToFrag(v, o, t, float2(-1,-1));
				triStream.Append(o);

				triStream.RestartStrip();
				//Triangle2
				TransferGeomToFrag(v, o, t, float2(1, 1));
				triStream.Append(o);
				TransferGeomToFrag(v, o, t, float2(-1, 1));
				triStream.Append(o);
				TransferGeomToFrag(v, o, t, float2(-1, -1));
				triStream.Append(o);
				triStream.RestartStrip();
			}
			
			fixed4 frag (g2f i) : SV_Target
			{
				return tex2D(_MainTex, i.uv) *i.color;
			}
			ENDCG
		}
	}
}
