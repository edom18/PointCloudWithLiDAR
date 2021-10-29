Shader "Hidden/PointCloud/MakeDepth2Hue"
{
    Properties
    {
        _EnvironmentDepth("", 2D) = "black" {}
    }
    SubShader
    {
        Pass
        {
            Cull Off ZTest Always ZWrite Off
            
            CGPROGRAM

            #include "UnityCG.cginc"
            
            #pragma multi_compile RCAM_MULTIPLEXER RCAM_MONITOR
            #pragma vertex Vertex
            #pragma fragment Fragment
            
            sampler2D _EnvironmentDepth;
            float4x4 _UnityDisplayTransform;

            float2 _DepthRange;
            float _AspectFix;

            // Hue encoding
            float3 Hue2RGB(float hue)
            {
                float h = hue * 6 - 2;
                float r = abs(h - 1) - 1;
                float g = 2 - abs(h);
                float b = 2 - abs(h - 2);
                return saturate(float3(r, g, b));
            }

            void Vertex(float4 vertex : POSITION,
                        float2 texCoord : TEXCOORD,
                        out float4 outVertex : SV_Position,
                        out float2 outTexCoord : TEXCOORD)
            {
                outVertex = UnityObjectToClipPos(vertex);
                outTexCoord = texCoord;
            }

            float4 Fragment(float4 vertex : SV_POSITION, float2 texcoord : TEXCOORD) : SV_Target
            {
                float2 uv = texcoord.xy;
                
                float depth = tex2D(_EnvironmentDepth, uv).x;
                depth = (depth - 0.1) / (5.0 - 0.1);
                float3 color = Hue2RGB(clamp(depth, 0, 1.0));

                return float4(GammaToLinearSpace(color), 1);
            }
            ENDCG
        }
    }
}