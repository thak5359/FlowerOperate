Shader "Custom/URP_2.5D_Sprite"
{
    Properties
    {
        // SpriteRenderer의 스프라이트 데이터는 자동으로 _MainTex에 들어옵니다.
        [MainTexture] _BaseMap("Sprite Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Tint Color", Color) = (1, 1, 1, 1)
        _Cutoff("Alpha Cutout", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
        }

        Pass
        {
            Name "ForwardLit"
            
            // 2.5D를 위한 설정: 겹침 문제를 해결하려면 ZWrite On, 알파 테스트라면 AlphaClip
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // GPU 인스턴싱 지원 (우리가 구현한 그것!)

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // SpriteRenderer의 색상을 받아오기 위해 필수!
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // 텍스처와 변수 선언
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Cutoff;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.color = input.color * _BaseColor; // 틴트와 정점 색상 결합
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // 1. 텍스처 샘플링
                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                half4 finalColor = texColor * input.color;

                // 2. 알파 테스트 (2.5D 상에서 외곽선 깔끔하게 처리)
                // 만약 알파 클리핑 기능을 쓰고 싶다면 아래 주석을 푸세요.
                // clip(finalColor.a - _Cutoff);

                return finalColor;
            }
            ENDHLSL
        }
    }
}