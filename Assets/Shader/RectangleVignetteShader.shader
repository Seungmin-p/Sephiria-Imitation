Shader "Hidden/Custom/RectangleVignetteShader" //에디터에서 공개하지 않는 상태(Hidden)
{
    //외부에서 수정하는 값
    Properties
    {
        _Color("Color", Color) = (0, 0, 0, 1)
        _Intensity("Intensity", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0.01, 1)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }
        ZTest Always ZWrite Off Cull Off //뎁스 버퍼, 컬링 관련

        Pass
        {
            HLSLPROGRAM
            //버텍스 쉐이더, 픽셀 쉐이더
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            half4 _Color;
            float _Intensity;
            float _Smoothness;

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord.xy;

                //소스 텍스쳐 가져오기
                half4 color = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, uv, _BlitMipLevel);

                //화면 중앙 기준 거리
                float2 dist = abs(uv - 0.5) * _Intensity;

                //둥근 직사각형 형태
                float rectangleDistance =
                    pow(dist.x, 8.0) +
                    pow(dist.y, 8.0);

                float vfactor =
                    pow(saturate(1.0 - rectangleDistance), _Smoothness);

                color.rgb *= lerp(_Color.rgb, (1.0).xxx, vfactor);

                return color;
            }
            ENDHLSL
        }
    }
}