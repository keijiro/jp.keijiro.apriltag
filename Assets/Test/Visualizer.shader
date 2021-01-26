Shader "Hidden/AprilTag/WebCam/Visualizer"
{
    Properties
    {
        _CameraFeed("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    // Camera feed blit

    sampler2D _CameraFeed;

    void VertexBlit(uint vid : SV_VertexID,
                    out float4 position : SV_Position,
                    out float2 uv : TEXCOORD0)
    {
        float x = vid >> 1;
        float y = (vid & 1) ^ (vid >> 1);

        position = float4(float2(x, y) * 2 - 1, 1, 1);
        uv = float2(x, y);
    }

    float4 FragmentBlit(float4 position : SV_Position,
                        float2 uv : TEXCOORD0) : SV_Target
    {
        return tex2D(_CameraFeed, uv);
    }

    // Tag position visualizer

    float2 _Corner1, _Corner2, _Corner3, _Corner4;

    void VertexKeyPoints(uint vid : SV_VertexID,
                         out float4 position : SV_Position)
    {
        float2 v = vid == 0 || vid == 4 ? _Corner1 :
                               vid == 1 ? _Corner2 :
                               vid == 2 ? _Corner3 : _Corner4;

        position = float4(v * 2 - 1, 1, 1);
    }

    float4 FragmentKeyPoints(float4 position : SV_Position) : SV_Target
    {
        return float4(1, 0, 0, 1);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexBlit
            #pragma fragment FragmentBlit
            ENDCG
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            CGPROGRAM
            #pragma vertex VertexKeyPoints
            #pragma fragment FragmentKeyPoints
            ENDCG
        }
    }
}
