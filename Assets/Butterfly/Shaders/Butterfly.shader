Shader "FlyBaby/Butterfly"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		_MainColor("MainColor",Color) = (1,1,1,1)
		[Space]
		_EmissionRate("EmissionRate",Range(0,10)) = 0
		[Space]
		_RainbowRate("RainbowRate",Range(0,1)) = 0
		_RainbowOffSet("RainbowOffSet",float) = 0
		_RainbowCycle("RainbowCycle",float) = 0.1
		[Space]
		_DirectionUp("DirectionOfUp",Vector) = (0,1,0,1)
		_PhaseShiftRate("PhaseShiftWithUpVec",Range(0,3)) = 1
		_WingsSpeed("WingsSpeed",Range(0,100)) = 10
		_WingsAmount("WingsAmount",Range(0,10)) = 1
    }
    SubShader
    {
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
				float4 pos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainColor;
			float _EmissionRate;
			float _RainbowRate;
			float _RainbowOffSet;
			float _RainbowCycle;
			float4 _DirectionUp;
			float _PhaseShiftRate;
			float _WingsSpeed;
			float _WingsAmount;

			float rand(float2 co){
				return frac(sin(dot(co.xy, float2(12.9898,78.233))) * 43758.5453);
			}

            v2f vert (appdata v)
            {
                v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				float2 st = o.uv - 0.5;
				v.vertex.y += sin(_Time.w * _WingsSpeed + dot(mul(unity_ObjectToWorld, v.vertex),_DirectionUp.xyz) * _PhaseShiftRate)*abs(st.x) * _WingsAmount;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.pos = mul(unity_ObjectToWorld, v.vertex);
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				float param = i.pos.x + i.pos.y + i.pos.z;
				param *= _RainbowCycle;
				param += _RainbowOffSet;
				fixed4 ccol;
				ccol.x = sin(param);
				ccol.y = sin(param + 6.28/3.);
				ccol.z = sin(param - 6.28/3.);
				col.xyz *= ccol.xyz * _RainbowRate;
				col.xyz *= 0.5;
				col.xyz += 0.5;
				col = col * _MainColor;
				col.xyz = col.xyz * (1. + _EmissionRate);
                return col;
            }
            ENDCG
        }
    }
}
