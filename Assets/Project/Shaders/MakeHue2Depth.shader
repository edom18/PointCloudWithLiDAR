Shader "Hidden/PointCloud/MakeHue2Depth"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Cull Off ZTest Always ZWrite Off

            CGPROGRAM
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            #pragma multi_compile RCAM_MULTIPLEXER RCAM_MONITOR
            #pragma vertex Vertex
            #pragma fragment Fragment

            sampler2D _MainTex;

            float RGB2Hue(float3 c)
            {
                float minc = min(min(c.r, c.g), c.b);
                float maxc = max(max(c.r, c.g), c.b);
                float div = 1 / (6 * max(maxc - minc, 1e-5));
                float r = (c.g - c.b) * div;
                float g = 1.0 / 3 + (c.b - c.r) * div;
                float b = 2.0 / 3 + (c.r - c.g) * div;
                return lerp(r, lerp(g, b, c.g < c.b), c.r < max(c.g, c.b));
            }

            float RGB2Depth(float3 rgb)
            {
                float hue = RGB2Hue(LinearToSRGB(rgb));
                return lerp(0.1, 5.0, hue);
            }

            void Vertex(float4 vertex : POSITION,
                        float2 texCoord : TEXCOORD,
                        out float4 outVertex : SV_Position,
                        out float2 outTexCoord : TEXCOORD)
            {
                outVertex = UnityObjectToClipPos(vertex);
                outTexCoord = texCoord;
            }

            float4 Fragment(float4 vertex : SV_Position,
                            float2 texCoord : TEXCOORD) : SV_Target
            {
                return RGB2Depth(tex2D(_MainTex, texCoord).xyz);
            }
            ENDCG
        }
    }
}