Shader "metaBloom/Flower"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EmissionRate("EmissionRate",Float) = 0.75
		_BloomBegin("BloomBeginTime",Range(0.,1.)) = 0.
		_BloomEnd("BloomEndTime",Range(0.,1.)) = 1.
		[Toggle]_DontEmitTilBloom("DontEmitTilBloom",Float) = 1.
		[Toggle]_FixNormal("FixNormal",Float) = 0.
	}
	SubShader
	{
		Tags {"Queue"="AlphaTest" "RenderType"="TransparentCutout"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata_t 
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
				float4 st : TEXCOORD1;
				float3 normal : NORMAL;
				
				fixed4 particleColor : COLOR;
			};

			struct v2f
			{
				float4 uv : TEXCOORD0;
				float4 st : TEXCOORD1;
				float3 normal : NORMAL;
				fixed4 particleColor : COLOR;
				float4 vertex : SV_POSITION;
				
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _EmissionRate;
			float _BloomBegin;
			float _BloomEnd;
			float _DontEmitTilBloom;
			float _FixNormal;
			v2f vert (appdata_t  v)
			{
			    float PI = 3.141592;
				v2f o;
				o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
				
				o.st = v.st;
				float perRad = length(v.uv.xy - 0.5) * 2.;
				float3 fromcenter = float3(v.vertex.x - v.uv.w, v.vertex.y - v.st.x, v.vertex.z - v.st.y);
				float3 proj = fromcenter - (dot(fromcenter,v.normal) * v.normal);
				float r = length(proj);
				float progress = min(1.,(v.uv.z - _BloomBegin)/(_BloomEnd - _BloomBegin));
				progress = clamp(progress,sin(perRad * PI) * 0.25,1. - sin(perRad * PI / 2.) * 0.25);
				o.uv.z = progress;
				v.vertex.xyz -= v.normal * r * cos(progress * PI / 2.) * ((_FixNormal > 0.) ? -1. : 1.);
				v.vertex.xyz += proj * (sin(progress * PI / 2.) - 1.);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.particleColor = v.particleColor;
				o.normal = v.normal;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv.xy);
				float age = i.uv.w;
				clip(col.a - 0.1);
				col = i.particleColor;
				col.xyz *= (1. + _EmissionRate * ((_DontEmitTilBloom > 0.) ? i.uv.z : 1.));
				return col;
			}
			ENDCG
		}
	}
}
