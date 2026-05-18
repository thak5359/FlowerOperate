Shader "Shader Graphs/GateOfDeny"
{
    Properties
    {
        _WaveSpeed("WaveSpeed", Float) = 1
        _WaveStrength("WaveStrength", Float) = 1
        _Base_Color("Base Color", Color) = (0.5886836, 0, 1, 0.3921569)
        _WaveDensity("WaveDensity", Float) = 1
        _Gradient_Offset("Gradient Offset", Float) = 0
        _AuroraPower("AuroraPower", Float) = 1
        _Alpha("Alpha", Float) = 0.7
        _EdgeFadeStart("Edge Fade Start", Range(0, 1)) = 0.65
        _EdgeFadeEnd("Edge Fade End", Range(0, 1)) = 1
        _NoiseScale("NoiseScale", Float) = 40
        _NoiseIntensity("NoiseIntensity", Float) = 1.48
        _ScrollSpeed("ScrollSpeed", Vector) = (0.5, 0, 0, 0)
        _Tiling("Tiling", Vector) = (0, 0, 0, 0)
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Lit"
            "Queue"="Transparent"
            "DisableBatching"="LODFading"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalLitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Replace
        }
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _LIGHT_LAYERS
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _LIGHT_COOKIES
        #pragma multi_compile _ _FORWARD_PLUS
        #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_FORWARD
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        // #define _ALPHAPREMULTIPLY_ON 1
        #define USE_UNITY_CROSSFADE 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP3;
            #endif
             float4 tangentWS : INTERP4;
             float4 texCoord0 : INTERP5;
             float4 fogFactorAndVertexLight : INTERP6;
             float3 positionWS : INTERP7;
             float3 normalWS : INTERP8;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_SampleGradientV1_float(Gradient Gradient, float Time, out float4 Out)
        {
            // convert to OkLab if we need perceptual color space.
            float3 color = lerp(Gradient.colors[0].rgb, LinearToOklab(Gradient.colors[0].rgb), Gradient.type == 2);
        
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                float3 color2 = lerp(Gradient.colors[c].rgb, LinearToOklab(Gradient.colors[c].rgb), Gradient.type == 2);
                color = lerp(color, color2, lerp(colorPos, step(0.01, colorPos), Gradient.type % 2)); // grad.type == 1 is fixed, 0 and 2 are blends.
            }
            color = lerp(color, OklabToLinear(color), Gradient.type == 2);
        
        #ifdef UNITY_COLORSPACE_GAMMA
            color = LinearToSRGB(color);
        #endif
        
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type % 2));
            }
        
            Out = float4(color, alpha);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        float Unity_SimpleNoise_ValueNoise_Deterministic_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0; Hash_Tchou_2_1_float(c0, r0);
            float r1; Hash_Tchou_2_1_float(c1, r1);
            float r2; Hash_Tchou_2_1_float(c2, r2);
            float r3; Hash_Tchou_2_1_float(c3, r3);
            float bottomOfGrid = lerp(r0, r1, f.x);
            float topOfGrid = lerp(r2, r3, f.x);
            float t = lerp(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        
        void Unity_SimpleNoise_Deterministic_float(float2 UV, float Scale, out float Out)
        {
            float freq, amp;
            Out = 0.0f;
            freq = pow(2.0, float(0));
            amp = pow(0.5, float(3-0));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            Gradient _Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient = _ColorGradient;
            float4 _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4 = IN.uv0;
            float _Split_4ef266625d014cc0ba426547b31d0c8b_R_1_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[0];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[1];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_B_3_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[2];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_A_4_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[3];
            float _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float = _Gradient_Offset;
            float _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float;
            Unity_Add_float(_Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float, _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float);
            float4 _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float, _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4);
            Gradient _Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient = _ContrastGradient;
            float3 _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpacePosition, _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3);
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[0];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_G_2_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[1];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[2];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_A_4_Float = 0;
            float2 _Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2 = float2(_Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float, _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float);
            float2 _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2 = _Tiling;
            float2 _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2 = _ScrollSpeed;
            float2 _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2);
            float2 _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2;
            Unity_TilingAndOffset_float(_Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2, _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2, _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2);
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_R_1_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[0];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[1];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_B_3_Float = 0;
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_A_4_Float = 0;
            float4 _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient, _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float, _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4);
            float4 _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4;
            Unity_Absolute_float4(_SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4, _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4);
            float _Property_415c963f9fd64887ad28506dae40c290_Out_0_Float = _AuroraPower;
            float _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float;
            Unity_Absolute_float(_Property_415c963f9fd64887ad28506dae40c290_Out_0_Float, _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float);
            float4 _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4;
            Unity_Power_float4(_Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4, (_Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float.xxxx), _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4);
            float2 _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 0.02), float2 (0, 0), _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2);
            float _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float = _NoiseScale;
            float _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float;
            Unity_SimpleNoise_Deterministic_float(_TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2, _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float, _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float);
            float _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float = _NoiseIntensity;
            float _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float;
            Unity_Multiply_float_float(_SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float, _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float, _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float);
            float4 _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4, (_Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float.xxxx), _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4);
            float4 _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4, _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4, _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4);
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.BaseColor = (_Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = float(0);
            surface.Smoothness = float(0.5);
            surface.Occlusion = float(1);
            float2 _GateOfDeny_UV = IN.uv0.xy;
            float2 _GateOfDeny_CenteredUV = abs(_GateOfDeny_UV - float2(0.5, 0.5)) * 2.0;
            float _GateOfDeny_RectEdgeDistance = max(_GateOfDeny_CenteredUV.x, _GateOfDeny_CenteredUV.y);
            float _GateOfDeny_RectEdgeMask = 1.0 - smoothstep(_EdgeFadeStart, _EdgeFadeEnd, _GateOfDeny_RectEdgeDistance);
            clip(_GateOfDeny_RectEdgeMask - 0.001);
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float * _GateOfDeny_RectEdgeMask;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Replace
        }
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
        #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
        #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
        #define VARYINGS_NEED_SHADOW_COORD
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_GBUFFER
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        // #define _ALPHAPREMULTIPLY_ON 1
        #define USE_UNITY_CROSSFADE 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV : INTERP0;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV : INTERP1;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh : INTERP2;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
             float4 shadowCoord : INTERP3;
            #endif
             float4 tangentWS : INTERP4;
             float4 texCoord0 : INTERP5;
             float4 fogFactorAndVertexLight : INTERP6;
             float3 positionWS : INTERP7;
             float3 normalWS : INTERP8;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS.xyzw = input.tangentWS;
            output.texCoord0.xyzw = input.texCoord0;
            output.fogFactorAndVertexLight.xyzw = input.fogFactorAndVertexLight;
            output.positionWS.xyz = input.positionWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.sh;
            #endif
            #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
            output.shadowCoord = input.shadowCoord;
            #endif
            output.tangentWS = input.tangentWS.xyzw;
            output.texCoord0 = input.texCoord0.xyzw;
            output.fogFactorAndVertexLight = input.fogFactorAndVertexLight.xyzw;
            output.positionWS = input.positionWS.xyz;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_SampleGradientV1_float(Gradient Gradient, float Time, out float4 Out)
        {
            // convert to OkLab if we need perceptual color space.
            float3 color = lerp(Gradient.colors[0].rgb, LinearToOklab(Gradient.colors[0].rgb), Gradient.type == 2);
        
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                float3 color2 = lerp(Gradient.colors[c].rgb, LinearToOklab(Gradient.colors[c].rgb), Gradient.type == 2);
                color = lerp(color, color2, lerp(colorPos, step(0.01, colorPos), Gradient.type % 2)); // grad.type == 1 is fixed, 0 and 2 are blends.
            }
            color = lerp(color, OklabToLinear(color), Gradient.type == 2);
        
        #ifdef UNITY_COLORSPACE_GAMMA
            color = LinearToSRGB(color);
        #endif
        
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type % 2));
            }
        
            Out = float4(color, alpha);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        float Unity_SimpleNoise_ValueNoise_Deterministic_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0; Hash_Tchou_2_1_float(c0, r0);
            float r1; Hash_Tchou_2_1_float(c1, r1);
            float r2; Hash_Tchou_2_1_float(c2, r2);
            float r3; Hash_Tchou_2_1_float(c3, r3);
            float bottomOfGrid = lerp(r0, r1, f.x);
            float topOfGrid = lerp(r2, r3, f.x);
            float t = lerp(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        
        void Unity_SimpleNoise_Deterministic_float(float2 UV, float Scale, out float Out)
        {
            float freq, amp;
            Out = 0.0f;
            freq = pow(2.0, float(0));
            amp = pow(0.5, float(3-0));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 NormalTS;
            float3 Emission;
            float Metallic;
            float Smoothness;
            float Occlusion;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            Gradient _Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient = _ColorGradient;
            float4 _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4 = IN.uv0;
            float _Split_4ef266625d014cc0ba426547b31d0c8b_R_1_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[0];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[1];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_B_3_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[2];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_A_4_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[3];
            float _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float = _Gradient_Offset;
            float _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float;
            Unity_Add_float(_Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float, _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float);
            float4 _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float, _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4);
            Gradient _Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient = _ContrastGradient;
            float3 _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpacePosition, _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3);
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[0];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_G_2_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[1];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[2];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_A_4_Float = 0;
            float2 _Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2 = float2(_Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float, _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float);
            float2 _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2 = _Tiling;
            float2 _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2 = _ScrollSpeed;
            float2 _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2);
            float2 _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2;
            Unity_TilingAndOffset_float(_Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2, _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2, _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2);
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_R_1_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[0];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[1];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_B_3_Float = 0;
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_A_4_Float = 0;
            float4 _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient, _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float, _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4);
            float4 _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4;
            Unity_Absolute_float4(_SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4, _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4);
            float _Property_415c963f9fd64887ad28506dae40c290_Out_0_Float = _AuroraPower;
            float _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float;
            Unity_Absolute_float(_Property_415c963f9fd64887ad28506dae40c290_Out_0_Float, _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float);
            float4 _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4;
            Unity_Power_float4(_Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4, (_Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float.xxxx), _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4);
            float2 _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 0.02), float2 (0, 0), _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2);
            float _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float = _NoiseScale;
            float _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float;
            Unity_SimpleNoise_Deterministic_float(_TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2, _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float, _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float);
            float _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float = _NoiseIntensity;
            float _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float;
            Unity_Multiply_float_float(_SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float, _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float, _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float);
            float4 _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4, (_Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float.xxxx), _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4);
            float4 _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4, _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4, _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4);
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.BaseColor = (_Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4.xyz);
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Emission = float3(0, 0, 0);
            surface.Metallic = float(0);
            surface.Smoothness = float(0.5);
            surface.Occlusion = float(1);
            float2 _GateOfDeny_UV = IN.uv0.xy;
            float2 _GateOfDeny_CenteredUV = abs(_GateOfDeny_UV - float2(0.5, 0.5)) * 2.0;
            float _GateOfDeny_RectEdgeDistance = max(_GateOfDeny_CenteredUV.x, _GateOfDeny_CenteredUV.y);
            float _GateOfDeny_RectEdgeMask = 1.0 - smoothstep(_EdgeFadeStart, _EdgeFadeEnd, _GateOfDeny_RectEdgeDistance);
            clip(_GateOfDeny_RectEdgeMask - 0.001);
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float * _GateOfDeny_RectEdgeMask;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        ColorMask R
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define USE_UNITY_CROSSFADE 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags
            {
                "LightMode" = "DepthNormals"
            }
        
        // Render State
        Cull Off
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LOD_FADE_CROSSFADE
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALS
        #define USE_UNITY_CROSSFADE 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/LODCrossFade.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 tangentWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 TangentSpaceNormal;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 tangentWS : INTERP0;
             float3 normalWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.tangentWS.xyzw = input.tangentWS;
            output.normalWS.xyz = input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.tangentWS = input.tangentWS.xyzw;
            output.normalWS = input.normalWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 NormalTS;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.NormalTS = IN.TangentSpaceNormal;
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
            output.TangentSpaceNormal = float3(0.0f, 0.0f, 1.0f);
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Meta"
            Tags
            {
                "LightMode" = "Meta"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma shader_feature _ EDITOR_VISUALIZATION
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define ATTRIBUTES_NEED_TEXCOORD2
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define VARYINGS_NEED_TEXCOORD1
        #define VARYINGS_NEED_TEXCOORD2
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_META
        #define _FOG_FRAGMENT 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
             float4 texCoord1;
             float4 texCoord2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpacePosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float4 texCoord1 : INTERP1;
             float4 texCoord2 : INTERP2;
             float3 positionWS : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.texCoord1.xyzw = input.texCoord1;
            output.texCoord2.xyzw = input.texCoord2;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.texCoord1 = input.texCoord1.xyzw;
            output.texCoord2 = input.texCoord2.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_SampleGradientV1_float(Gradient Gradient, float Time, out float4 Out)
        {
            // convert to OkLab if we need perceptual color space.
            float3 color = lerp(Gradient.colors[0].rgb, LinearToOklab(Gradient.colors[0].rgb), Gradient.type == 2);
        
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                float3 color2 = lerp(Gradient.colors[c].rgb, LinearToOklab(Gradient.colors[c].rgb), Gradient.type == 2);
                color = lerp(color, color2, lerp(colorPos, step(0.01, colorPos), Gradient.type % 2)); // grad.type == 1 is fixed, 0 and 2 are blends.
            }
            color = lerp(color, OklabToLinear(color), Gradient.type == 2);
        
        #ifdef UNITY_COLORSPACE_GAMMA
            color = LinearToSRGB(color);
        #endif
        
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type % 2));
            }
        
            Out = float4(color, alpha);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        float Unity_SimpleNoise_ValueNoise_Deterministic_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0; Hash_Tchou_2_1_float(c0, r0);
            float r1; Hash_Tchou_2_1_float(c1, r1);
            float r2; Hash_Tchou_2_1_float(c2, r2);
            float r3; Hash_Tchou_2_1_float(c3, r3);
            float bottomOfGrid = lerp(r0, r1, f.x);
            float topOfGrid = lerp(r2, r3, f.x);
            float t = lerp(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        
        void Unity_SimpleNoise_Deterministic_float(float2 UV, float Scale, out float Out)
        {
            float freq, amp;
            Out = 0.0f;
            freq = pow(2.0, float(0));
            amp = pow(0.5, float(3-0));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float3 Emission;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            Gradient _Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient = _ColorGradient;
            float4 _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4 = IN.uv0;
            float _Split_4ef266625d014cc0ba426547b31d0c8b_R_1_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[0];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[1];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_B_3_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[2];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_A_4_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[3];
            float _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float = _Gradient_Offset;
            float _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float;
            Unity_Add_float(_Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float, _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float);
            float4 _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float, _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4);
            Gradient _Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient = _ContrastGradient;
            float3 _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpacePosition, _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3);
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[0];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_G_2_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[1];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[2];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_A_4_Float = 0;
            float2 _Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2 = float2(_Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float, _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float);
            float2 _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2 = _Tiling;
            float2 _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2 = _ScrollSpeed;
            float2 _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2);
            float2 _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2;
            Unity_TilingAndOffset_float(_Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2, _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2, _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2);
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_R_1_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[0];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[1];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_B_3_Float = 0;
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_A_4_Float = 0;
            float4 _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient, _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float, _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4);
            float4 _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4;
            Unity_Absolute_float4(_SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4, _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4);
            float _Property_415c963f9fd64887ad28506dae40c290_Out_0_Float = _AuroraPower;
            float _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float;
            Unity_Absolute_float(_Property_415c963f9fd64887ad28506dae40c290_Out_0_Float, _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float);
            float4 _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4;
            Unity_Power_float4(_Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4, (_Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float.xxxx), _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4);
            float2 _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 0.02), float2 (0, 0), _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2);
            float _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float = _NoiseScale;
            float _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float;
            Unity_SimpleNoise_Deterministic_float(_TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2, _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float, _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float);
            float _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float = _NoiseIntensity;
            float _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float;
            Unity_Multiply_float_float(_SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float, _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float, _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float);
            float4 _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4, (_Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float.xxxx), _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4);
            float4 _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4, _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4, _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4);
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.BaseColor = (_Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4.xyz);
            surface.Emission = float3(0, 0, 0);
            float2 _GateOfDeny_UV = IN.uv0.xy;
            float2 _GateOfDeny_CenteredUV = abs(_GateOfDeny_UV - float2(0.5, 0.5)) * 2.0;
            float _GateOfDeny_RectEdgeDistance = max(_GateOfDeny_CenteredUV.x, _GateOfDeny_CenteredUV.y);
            float _GateOfDeny_RectEdgeMask = 1.0 - smoothstep(_EdgeFadeStart, _EdgeFadeEnd, _GateOfDeny_RectEdgeDistance);
            clip(_GateOfDeny_RectEdgeMask - 0.001);
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float * _GateOfDeny_RectEdgeMask;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "Universal 2D"
            Tags
            {
                "LightMode" = "Universal2D"
            }
        
        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On

        Stencil
        {
            Ref 1
            Comp NotEqual
            Pass Replace
        }
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define _NORMALMAP 1
        #define _NORMAL_DROPOFF_TS 1
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_2D
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include_with_pragmas "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRenderingKeywords.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/FoveatedRendering.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpacePosition;
             float4 uv0;
             float3 TimeParameters;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
             float3 WorldSpacePosition;
             float3 TimeParameters;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float4 texCoord0 : INTERP0;
             float3 positionWS : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.texCoord0.xyzw = input.texCoord0;
            output.positionWS.xyz = input.positionWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.texCoord0 = input.texCoord0.xyzw;
            output.positionWS = input.positionWS.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float _WaveSpeed;
        float _WaveStrength;
        float4 _Base_Color;
        float _WaveDensity;
        float _Gradient_Offset;
        float _AuroraPower;
        float _Alpha;
        float _EdgeFadeStart;
        float _EdgeFadeEnd;
        float _NoiseScale;
        float _NoiseIntensity;
        float2 _ScrollSpeed;
        float2 _Tiling;
        CBUFFER_END
        
        
        // Object and Global properties
        static Gradient _ColorGradient = {0,6,2,{float4(0.4621003,0.9716981,0.2704254,0),float4(0.2196078,0.7843137,0.5980723,0.197055),float4(0.1647059,0.4588737,0.5843138,0.3411765),float4(0.2488971,0.1647059,0.5843138,0.4941176),float4(0.1647059,0.3159231,0.5843138,0.7000076),float4(0.4403841,0.9549171,0.2997675,1),float4(0,0,0,0),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        static Gradient _ContrastGradient = {0,7,2,{float4(1,1,1,0),float4(0,0,0,0.1764706),float4(1,1,1,0.4088197),float4(0.1698113,0.1698113,0.1698113,0.6588235),float4(1,1,1,0.8382391),float4(0.7844777,0.7844777,0.7844777,0.9264668),float4(1,1,1,1),float4(0,0,0,0)},{float2(1,0),float2(1,1),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0),float2(0,0)}};
        
        
        // Graph Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Hashes.hlsl"
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Add_float(float A, float B, out float Out)
        {
            Out = A + B;
        }
        
        void Unity_Multiply_float_float(float A, float B, out float Out)
        {
            Out = A * B;
        }
        
        void Unity_Sine_float(float In, out float Out)
        {
            Out = sin(In);
        }
        
        void Unity_Add_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A + B;
        }
        
        void Unity_SampleGradientV1_float(Gradient Gradient, float Time, out float4 Out)
        {
            // convert to OkLab if we need perceptual color space.
            float3 color = lerp(Gradient.colors[0].rgb, LinearToOklab(Gradient.colors[0].rgb), Gradient.type == 2);
        
            [unroll]
            for (int c = 1; c < Gradient.colorsLength; c++)
            {
                float colorPos = saturate((Time - Gradient.colors[c - 1].w) / (Gradient.colors[c].w - Gradient.colors[c - 1].w)) * step(c, Gradient.colorsLength - 1);
                float3 color2 = lerp(Gradient.colors[c].rgb, LinearToOklab(Gradient.colors[c].rgb), Gradient.type == 2);
                color = lerp(color, color2, lerp(colorPos, step(0.01, colorPos), Gradient.type % 2)); // grad.type == 1 is fixed, 0 and 2 are blends.
            }
            color = lerp(color, OklabToLinear(color), Gradient.type == 2);
        
        #ifdef UNITY_COLORSPACE_GAMMA
            color = LinearToSRGB(color);
        #endif
        
            float alpha = Gradient.alphas[0].x;
            [unroll]
            for (int a = 1; a < Gradient.alphasLength; a++)
            {
                float alphaPos = saturate((Time - Gradient.alphas[a - 1].y) / (Gradient.alphas[a].y - Gradient.alphas[a - 1].y)) * step(a, Gradient.alphasLength - 1);
                alpha = lerp(alpha, Gradient.alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), Gradient.type % 2));
            }
        
            Out = float4(color, alpha);
        }
        
        void Unity_Normalize_float3(float3 In, out float3 Out)
        {
            Out = normalize(In);
        }
        
        void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
        {
            Out = A * B;
        }
        
        void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Absolute_float4(float4 In, out float4 Out)
        {
            Out = abs(In);
        }
        
        void Unity_Absolute_float(float In, out float Out)
        {
            Out = abs(In);
        }
        
        void Unity_Power_float4(float4 A, float4 B, out float4 Out)
        {
            Out = pow(A, B);
        }
        
        float Unity_SimpleNoise_ValueNoise_Deterministic_float (float2 uv)
        {
            float2 i = floor(uv);
            float2 f = frac(uv);
            f = f * f * (3.0 - 2.0 * f);
            uv = abs(frac(uv) - 0.5);
            float2 c0 = i + float2(0.0, 0.0);
            float2 c1 = i + float2(1.0, 0.0);
            float2 c2 = i + float2(0.0, 1.0);
            float2 c3 = i + float2(1.0, 1.0);
            float r0; Hash_Tchou_2_1_float(c0, r0);
            float r1; Hash_Tchou_2_1_float(c1, r1);
            float r2; Hash_Tchou_2_1_float(c2, r2);
            float r3; Hash_Tchou_2_1_float(c3, r3);
            float bottomOfGrid = lerp(r0, r1, f.x);
            float topOfGrid = lerp(r2, r3, f.x);
            float t = lerp(bottomOfGrid, topOfGrid, f.y);
            return t;
        }
        
        void Unity_SimpleNoise_Deterministic_float(float2 UV, float Scale, out float Out)
        {
            float freq, amp;
            Out = 0.0f;
            freq = pow(2.0, float(0));
            amp = pow(0.5, float(3-0));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(1));
            amp = pow(0.5, float(3-1));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
            freq = pow(2.0, float(2));
            amp = pow(0.5, float(3-2));
            Out += Unity_SimpleNoise_ValueNoise_Deterministic_float(float2(UV.xy*(Scale/freq)))*amp;
        }
        
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            float3 Position;
            float3 Normal;
            float3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            float _Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float = _WaveDensity;
            float _Split_603cf2626ae84357933114adbf2513a7_R_1_Float = IN.WorldSpacePosition[0];
            float _Split_603cf2626ae84357933114adbf2513a7_G_2_Float = IN.WorldSpacePosition[1];
            float _Split_603cf2626ae84357933114adbf2513a7_B_3_Float = IN.WorldSpacePosition[2];
            float _Split_603cf2626ae84357933114adbf2513a7_A_4_Float = 0;
            float _Add_3809baa03386403ea0baba2693a09625_Out_2_Float;
            Unity_Add_float(_Split_603cf2626ae84357933114adbf2513a7_R_1_Float, _Split_603cf2626ae84357933114adbf2513a7_B_3_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float);
            float _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float;
            Unity_Multiply_float_float(_Property_52abba6ad0d04787860434a0253e1b25_Out_0_Float, _Add_3809baa03386403ea0baba2693a09625_Out_2_Float, _Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float);
            float _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float = _WaveSpeed;
            float _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float;
            Unity_Multiply_float_float(IN.TimeParameters.x, _Property_5ed8cd01b0ee4b97aa8f1314acebcf7a_Out_0_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float);
            float _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float;
            Unity_Add_float(_Multiply_b2fdcc384c64421e99d480617190cdc5_Out_2_Float, _Multiply_61e34288d6cc4f0a8c31531fcc488d08_Out_2_Float, _Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float);
            float _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float;
            Unity_Sine_float(_Add_9c7d9f30cf4746fb8cfb4bf3511f3371_Out_2_Float, _Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float);
            float _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float = _WaveStrength;
            float _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float;
            Unity_Multiply_float_float(_Sine_e6d3e9a5b0cb4b2eae5e20394640202a_Out_1_Float, _Property_110f223dedbb4fb3b61f2dff71bc3ef7_Out_0_Float, _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float);
            float3 _Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3 = float3(float(0), _Multiply_9d774c5ed4f94df89980fadccab74250_Out_2_Float, float(0));
            float3 _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            Unity_Add_float3(_Vector3_2c57638b50bd42b5b4379cb1615f5e75_Out_0_Vector3, IN.ObjectSpacePosition, _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3);
            description.Position = _Add_a6d2cb64a131477ca39c4430a98eeff0_Out_2_Vector3;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            Gradient _Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient = _ColorGradient;
            float4 _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4 = IN.uv0;
            float _Split_4ef266625d014cc0ba426547b31d0c8b_R_1_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[0];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[1];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_B_3_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[2];
            float _Split_4ef266625d014cc0ba426547b31d0c8b_A_4_Float = _UV_2d73accf38df4534a910cc82cb57353d_Out_0_Vector4[3];
            float _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float = _Gradient_Offset;
            float _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float;
            Unity_Add_float(_Split_4ef266625d014cc0ba426547b31d0c8b_G_2_Float, _Property_997ce9bc31af4591a87583166e015ee6_Out_0_Float, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float);
            float4 _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_19ee99317ab242ae8debe05c92a15548_Out_0_Gradient, _Add_a11e3dca03464923bc879409cd78a63f_Out_2_Float, _SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4);
            Gradient _Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient = _ContrastGradient;
            float3 _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3;
            Unity_Normalize_float3(IN.WorldSpacePosition, _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3);
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[0];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_G_2_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[1];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float = _Normalize_d31ccc91d17e4d87889defe707a45637_Out_1_Vector3[2];
            float _Split_8b1e8fb564cc49bb9dfdc38bc7987302_A_4_Float = 0;
            float2 _Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2 = float2(_Split_8b1e8fb564cc49bb9dfdc38bc7987302_R_1_Float, _Split_8b1e8fb564cc49bb9dfdc38bc7987302_B_3_Float);
            float2 _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2 = _Tiling;
            float2 _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2 = _ScrollSpeed;
            float2 _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2;
            Unity_Multiply_float2_float2((IN.TimeParameters.x.xx), _Property_9a0093d886da40378406be0160a5505b_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2);
            float2 _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2;
            Unity_TilingAndOffset_float(_Vector2_7b0712293be343cba34459da0a78e23e_Out_0_Vector2, _Property_da90eadaab004bf08a8ef95b52528e03_Out_0_Vector2, _Multiply_b377009c8fd8472384c5e5bc019e8712_Out_2_Vector2, _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2);
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_R_1_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[0];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float = _TilingAndOffset_de397b1593aa458aaee8ed488c126c48_Out_3_Vector2[1];
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_B_3_Float = 0;
            float _Split_2ea873a6cfe14c18bd806da2c14a17ca_A_4_Float = 0;
            float4 _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4;
            Unity_SampleGradientV1_float(_Property_9f79fd0af10e4bef863b1edaea084c8a_Out_0_Gradient, _Split_2ea873a6cfe14c18bd806da2c14a17ca_G_2_Float, _SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4);
            float4 _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4;
            Unity_Absolute_float4(_SampleGradient_19cd1b8e5beb451e89632859bd94cc44_Out_2_Vector4, _Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4);
            float _Property_415c963f9fd64887ad28506dae40c290_Out_0_Float = _AuroraPower;
            float _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float;
            Unity_Absolute_float(_Property_415c963f9fd64887ad28506dae40c290_Out_0_Float, _Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float);
            float4 _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4;
            Unity_Power_float4(_Absolute_48755a56a4a14aeeb395555d66f7b2fa_Out_1_Vector4, (_Absolute_ceaf3562896c4559961020c16e8b0283_Out_1_Float.xxxx), _Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4);
            float2 _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2;
            Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 0.02), float2 (0, 0), _TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2);
            float _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float = _NoiseScale;
            float _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float;
            Unity_SimpleNoise_Deterministic_float(_TilingAndOffset_b1974c6c0f184723baa711afa5a26bb5_Out_3_Vector2, _Property_581a9c00b716437aa9dca6ccb7e6170d_Out_0_Float, _SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float);
            float _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float = _NoiseIntensity;
            float _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float;
            Unity_Multiply_float_float(_SimpleNoise_8e166dbc7b924e77bfd55a26a1a210e0_Out_2_Float, _Property_0119b06ebbd64f9ebdf680573b0c414a_Out_0_Float, _Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float);
            float4 _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4;
            Unity_Multiply_float4_float4(_Power_3cd2acb76ada42e3b67ebbbc9d089070_Out_2_Vector4, (_Multiply_903bcc68098a480c9bbb2f8c72f0503e_Out_2_Float.xxxx), _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4);
            float4 _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4;
            Unity_Multiply_float4_float4(_SampleGradient_eed746c8eee04454a80e4f3555b7493f_Out_2_Vector4, _Multiply_846df1da9b0443deb9c65365b977c7bf_Out_2_Vector4, _Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4);
            float _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float = _Alpha;
            surface.BaseColor = (_Multiply_2b05eeac9c9f4534ae41f99363e52d5b_Out_2_Vector4.xyz);
            float2 _GateOfDeny_UV = IN.uv0.xy;
            float2 _GateOfDeny_CenteredUV = abs(_GateOfDeny_UV - float2(0.5, 0.5)) * 2.0;
            float _GateOfDeny_RectEdgeDistance = max(_GateOfDeny_CenteredUV.x, _GateOfDeny_CenteredUV.y);
            float _GateOfDeny_RectEdgeMask = 1.0 - smoothstep(_EdgeFadeStart, _EdgeFadeEnd, _GateOfDeny_RectEdgeDistance);
            clip(_GateOfDeny_RectEdgeMask - 0.001);
            surface.Alpha = _Property_f145c12e997e43ffadf540391fb6ffe5_Out_0_Float * _GateOfDeny_RectEdgeMask;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
            output.WorldSpacePosition =                         TransformObjectToWorld(input.positionOS);
            output.TimeParameters =                             _TimeParameters.xyz;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
        #if VFX_USE_GRAPH_VALUES
            uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
            /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
        #endif
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
            output.WorldSpacePosition = input.positionWS;
        
            #if UNITY_UV_STARTS_AT_TOP
            #else
            #endif
        
        
            output.uv0 = input.texCoord0;
            output.TimeParameters = _TimeParameters.xyz; // This is mainly for LW as HD overwrite this value
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphLitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}