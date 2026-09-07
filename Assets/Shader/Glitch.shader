Shader "Hidden/Custom/GlitchShader" //에디터에서 공개하지 않는 상태(Hidden)
{
    Properties
    {
        _SplitAmount("Split Amount", Range(0, 1)) = 0
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

            float _SplitAmount;

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.texcoord.xy;

                //0~1 값을 실제 UV 이동량으로 변환
                float offset = _SplitAmount * 0.05;

                //R은 화면 왼쪽, G는 제자리, B는 화면 오른쪽으로 분리
                float2 redUV = uv + float2(offset, 0);
                float2 greenUV = uv;
                float2 blueUV = uv - float2(offset, 0);

                //각 위치의 화면 색상 가져오기
                half4 redSample = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, redUV, _BlitMipLevel);
                half4 greenSample = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, greenUV, _BlitMipLevel);
                half4 blueSample = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, blueUV, _BlitMipLevel);

                //각 샘플에서 해당 RGB 채널만 가져와 다시 조합
                half3 finalColor = half3(redSample.r, greenSample.g, blueSample.b);

                return half4(finalColor, greenSample.a);
            }

            ENDHLSL
        }
    }
}