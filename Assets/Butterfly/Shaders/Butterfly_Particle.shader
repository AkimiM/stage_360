Shader "FlyBaby/Butterfly_Particle"
{
    Properties
    {
		_MainTex ("Texture", 2D) = "white" {}
		[Space]
		_EmissionRate("EmissionRate",Range(0,10)) = 0
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
                float3 normal : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color : COLOR;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
				float4 pos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _EmissionRate;
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
				o.color = v.color;
				o.normal = v.normal;
                
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				col = col * i.color;
				col.xyz = col.xyz * (1. + _EmissionRate);
                return col;
            }
            ENDCG
        }
    }
}
