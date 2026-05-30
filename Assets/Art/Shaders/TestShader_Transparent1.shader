Shader "Custom/SpriteRoundedCorners"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // X, Y 곡률 파라미터 (0~1 사이 값 권장)
        _CornerRadius ("Corner Radius (X, Y)", Vector) = (0.2, 0.2, 0, 0)
    }
    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        // 스프라이트 렌더러의 기본 블렌딩 방식입니다.
        Blend One OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            float2 _CornerRadius;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                // 스프라이트 렌더러의 Color(Tint) 값을 곱해줍니다.
                OUT.color = IN.color * _Color; 
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                // 텍스처 색상과 스프라이트 렌더러의 색상을 결합합니다.
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                
                // 스프라이트 특성상 Premultiplied Alpha 처리를 해줍니다.
                c.rgb *= c.a; 

                // --- 0~1 (Zero-one) UV 기준 네 귀퉁이 투명도 계산 ---
                float2 p = abs(IN.texcoord - 0.5) * 2.0;
                float2 radius = max(_CornerRadius, 0.0001);
                float2 circleCenter = 1.0 - radius;
                
                float alphaMask = 1.0;

                if (p.x > circleCenter.x && p.y > circleCenter.y)
                {
                    float2 dist = (p - circleCenter) / radius;
                    if (length(dist) > 1.0)
                    {
                        alphaMask = 0.0;
                    }
                }

                // 알파 마스크를 적용합니다.
                c.a *= alphaMask;
                c.rgb *= alphaMask; 

                return c;
            }
            ENDCG
        }
    }
}