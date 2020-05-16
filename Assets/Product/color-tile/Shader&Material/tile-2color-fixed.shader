﻿Shader "Custom/tile-2color-fixed" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Color("Color",Color) = (1,0,0,1)
		_Color2("Color2",Color) = (0,0,1,1)
		_SplitX("SplitX",Int) = 10
		_SplitY("SplitY",Int) = 10
		[Toggle(FILL)]_Fill("IsFill",Float) = 1
		_OutLineC("_OutLineC",Color) = (0,0,0,1)
		_OutLineWidth("OutLineWidth",Range(0,1)) = 0.8
		_EmptyC("_EmptyC",Color) = (0,0,0,0)
		_Offset("Offset",Vector) = (0,0,0,0)
		_Rot("Rot",Float) = 0
	}
	SubShader {
		Pass {
			ZWrite On
			ColorMask 0
		}
		Tags { "Queue"="Transparent"}
		LOD 200
		Cull Off
		CGPROGRAM
		#pragma surface surf Standard alpha:fade
		#pragma target 3.0
		#pragma multi_compile _ FILL

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		fixed4 _Color;
		fixed4 _Color2;
		half _Glossiness;
		half _Metallic;
		int _SplitX;
	 	int _SplitY;
		fixed4 _OutLineC;
		float _OutLineWidth;
		fixed4 _EmptyC;
		float4 _Offset;
		float _Alpha;
		float _Rot;

		float rand(float2 co){
			return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
		}

		float3 HSVToRGB(float3 hsv){
			return ((clamp(abs(frac(hsv.x+float3(0,2,1)/3.)*6.-3.)-1.,0.,1.)-1.)*hsv.y+1.)*hsv.z;
		}

		void RotUV(inout float2 uv,float rad){
			float2x2 rot = {cos(rad),-sin(rad),
			                sin(rad),cos(rad)};
      uv = mul(rot,uv);
		}

		void P1(float2 ori_uv,float2 uv,float4 color,inout SurfaceOutputStandard o,float seed) {
			float2 uv_f = float2(frac(uv.x*_SplitX),frac(uv.y*_SplitY));
			float2 uv_id =float2(trunc(uv.x*_SplitX),trunc(uv.y*_SplitY));
			float2 zuv_f = 2*uv_f-1;
			zuv_f = abs(zuv_f);
			float zuv_val = zuv_f.x+zuv_f.y;
			float duv_id = rand((uv_id)+1.87369);
			if(zuv_val<=1){
				if(zuv_val<=_OutLineWidth){
						float3 c = color * tex2D(_MainTex,ori_uv);
						o.Albedo = c;
						o.Emission = c;
						o.Alpha = color.a;
				}else{
					o.Albedo = _OutLineC.rgb;
					o.Alpha = _OutLineC.a;
				}
			}
		}


		void surf (Input IN, inout SurfaceOutputStandard o) {
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Albedo = _EmptyC.rgb;
			o.Alpha = _EmptyC.a;
			float2 uv = IN.uv_MainTex;
			RotUV(uv,_Rot*UNITY_PI/180);
			P1(IN.uv_MainTex,uv,_Color,o,0);
			#ifdef FILL
				uv.x = uv.x+(1.0/_SplitX)/2;
				uv.y = uv.y+(1.0/_SplitY)/2;
				uv += _Offset.xy;
				P1(IN.uv_MainTex,uv,_Color2,o,0.2795);
			#endif
		}
		ENDCG
	}
	FallBack "Diffuse"
}
