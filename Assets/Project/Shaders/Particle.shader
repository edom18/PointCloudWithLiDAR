Shader "Unlit/Particle"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Off
        Cull Back
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "ParticleCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            StructuredBuffer<Particle> _ParticleBuffer;

            v2f vert (appdata v, uint instanceId : SV_InstanceID)
            {
                Particle p = _ParticleBuffer[instanceId];
                
                v2f o;

                // Billboarding. Refer to https://stackoverflow.com/questions/57204343/can-a-shader-rotate-shapes-to-face-camera
                o.vertex = mul(UNITY_MATRIX_P,
                    mul(UNITY_MATRIX_V, float4(p.position, 1))
                    + float4(v.vertex.x, v.vertex.y, 0, 0)
                    * float4(p.scale, p.scale, 1.0, 1.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = p.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col * i.color;
            }
            ENDCG
        }
    }
}
