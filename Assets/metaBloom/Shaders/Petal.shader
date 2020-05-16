Shader "metaBloom/Petal"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_EmissionRate ("EmissionRate",Float) = 0.5
		_CutOffThreshold ("CutOffThreshold",Range(0.,1.)) = 0.5
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

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color :COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color :COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _EmissionRate;
			float _CutOffThreshold;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				if(col.a < _CutOffThreshold) discard;
				col *= i.color;
				col.xyz *= (1. + _EmissionRate);
				return col;
			}
			ENDCG
		}
	}
}
