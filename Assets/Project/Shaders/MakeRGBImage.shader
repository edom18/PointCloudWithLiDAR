Shader "Hidden/PointCloud/MakeRGBImage"
{
    Properties
    {
        _textureY("", 2D) = "black" {}
        _textureCbCr("", 2D) = "black" {}
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
            
            sampler2D _textureY;
            sampler2D _textureCbCr;

            float3 YCbCrToSRGB(float y, float2 cbcr)
            {
                float b = y + cbcr.x * 1.772 - 0.886;
                float r = y + cbcr.y * 1.402 - 0.701;
                float g = y + dot(cbcr, float2(-0.3441, -0.7141)) + 0.5291;
                return float3(r, g, b);
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

                // Texture samples
                float y = tex2D(_textureY, uv).x;
                float2 cbcr = tex2D(_textureCbCr, uv).xy;

                // Color plane
                float3 color = YCbCrToSRGB(y, cbcr);

                return float4(color, 1);
            }
            ENDCG
        }
    }
}