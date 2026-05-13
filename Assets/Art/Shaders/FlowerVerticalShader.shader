Shader "FlowerOperate/Flower Wind Skew Depth"
{
    Properties
    {
        [NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}

        [Header(Wind)]
        _WindSpeed("Wind Speed", Float) = 1
        _WindScale("Wind Scale", Float) = 1
        _WindStrength("Wind Strength", Float) = 0.03
        _WindDirection("Wind Direction", Vector) = (1, 0, 0, 0)

        [Header(Skew)]
        _SkewStrength("Skew Strength", Float) = 1

        [Header(Alpha)]
        _AlphaClipThreshold("Alpha Clip Threshold", Range(0, 1)) = 0.5

        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType"="Unlit"
            "Queue"="Transparent"
            "DisableBatching"="true"
        }

        Pass
        {
            Name "Universal Forward"

            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;

                float _WindSpeed;
                float _WindScale;
                float _WindStrength;
                float4 _WindDirection;

                float _SkewStrength;
                float _AlphaClipThreshold;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 Hash22(float2 p)
            {
                p = float2(
                    dot(p, float2(127.1, 311.7)),
                    dot(p, float2(269.5, 183.3))
                );

                return frac(sin(p) * 43758.5453123);
            }

            float GradientNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float2 u = f * f * (3.0 - 2.0 * f);

                float2 g00 = Hash22(i + float2(0, 0)) * 2.0 - 1.0;
                float2 g10 = Hash22(i + float2(1, 0)) * 2.0 - 1.0;
                float2 g01 = Hash22(i + float2(0, 1)) * 2.0 - 1.0;
                float2 g11 = Hash22(i + float2(1, 1)) * 2.0 - 1.0;

                float n00 = dot(g00, f - float2(0, 0));
                float n10 = dot(g10, f - float2(1, 0));
                float n01 = dot(g01, f - float2(0, 1));
                float n11 = dot(g11, f - float2(1, 1));

                float nx0 = lerp(n00, n10, u.x);
                float nx1 = lerp(n01, n11, u.x);

                return lerp(nx0, nx1, u.y) + 0.5;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS);
                float3 originWS = TransformObjectToWorld(float3(0, 0, 0));

                // 1. Wind deformation
                float2 windDir = _WindDirection.xy;
                windDir = normalize(windDir + float2(0.0001, 0.0001));

                float time = _Time.y * _WindSpeed;

                float noise = GradientNoise(worldPos.xy * _WindScale + time.xx);
                float centeredNoise = noise - 0.5;

                // uv.y가 낮은 밑동은 덜 움직이고, 위쪽 꽃/잎은 더 움직인다.
                float heightMask = saturate(input.uv.y);

                float2 windOffset =
                    centeredNoise
                    * _WindStrength
                    * windDir
                    * heightMask;

                worldPos.xy += windOffset;

                // 2. 45도 계열 depth skew
                // _SkewStrength = 0 : skew 없음
                // _SkewStrength = 1 : 기존 Fixed 3d Skew와 같은 1:1 기울기
                worldPos.z = worldPos.z + (worldPos.y - originWS.y) * _SkewStrength;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                clip(col.a - _AlphaClipThreshold);

                return col;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            Cull Off
            ZTest LEqual
            ZWrite On
            ColorMask 0

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_TexelSize;

                float _WindSpeed;
                float _WindScale;
                float _WindStrength;
                float4 _WindDirection;

                float _SkewStrength;
                float _AlphaClipThreshold;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float2 Hash22(float2 p)
            {
                p = float2(
                    dot(p, float2(127.1, 311.7)),
                    dot(p, float2(269.5, 183.3))
                );

                return frac(sin(p) * 43758.5453123);
            }

            float GradientNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float2 u = f * f * (3.0 - 2.0 * f);

                float2 g00 = Hash22(i + float2(0, 0)) * 2.0 - 1.0;
                float2 g10 = Hash22(i + float2(1, 0)) * 2.0 - 1.0;
                float2 g01 = Hash22(i + float2(0, 1)) * 2.0 - 1.0;
                float2 g11 = Hash22(i + float2(1, 1)) * 2.0 - 1.0;

                float n00 = dot(g00, f - float2(0, 0));
                float n10 = dot(g10, f - float2(1, 0));
                float n01 = dot(g01, f - float2(0, 1));
                float n11 = dot(g11, f - float2(1, 1));

                float nx0 = lerp(n00, n10, u.x);
                float nx1 = lerp(n01, n11, u.x);

                return lerp(nx0, nx1, u.y) + 0.5;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS);
                float3 originWS = TransformObjectToWorld(float3(0, 0, 0));

                float2 windDir = normalize(_WindDirection.xy + float2(0.0001, 0.0001));

                float time = _Time.y * _WindSpeed;
                float noise = GradientNoise(worldPos.xy * _WindScale + time.xx);
                float centeredNoise = noise - 0.5;

                float heightMask = saturate(input.uv.y);

                float2 windOffset =
                    centeredNoise
                    * _WindStrength
                    * windDir
                    * heightMask;

                worldPos.xy += windOffset;
                worldPos.z = worldPos.z + (worldPos.y - originWS.y) * _SkewStrength;

                output.positionCS = TransformWorldToHClip(worldPos);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _AlphaClipThreshold);

                return 0;
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}