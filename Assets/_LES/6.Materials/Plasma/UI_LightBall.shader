Shader "UI/Custom/PulsingOrb_Pixel"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Pixel Art Settings)]
        _PixelRes ("Pixel Resolution", Float) = 32.0 // 도트 해상도

        [Header(Colors)]
        [HDR] _CoreColor ("Core Color", Color) = (1.5, 2.5, 1.5, 1) // 중심부 색상 (HDR)
        _HaloColor ("Halo Color", Color) = (0.2, 0.8, 0.2, 1) // 외곽 퍼지는 색상
        
        [Header(Pulse Settings)]
        _Speed ("Pulse Speed", Range(0.1, 10.0)) = 2.0  // 깜빡이는 속도
        _MinSize ("Min Size", Range(0.0, 0.5)) = 0.1    // 가장 작아졌을 때 크기
        _MaxSize ("Max Size", Range(0.0, 0.5)) = 0.45   // 가장 커졌을 때 크기
        _Sharpness ("Pulse Sharpness", Range(1.0, 10.0)) = 1.0 // 1: 부드러움, 10: 깜빡임(Blink)

        // UI 필수 속성
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _ClipRect;

            float _PixelRes;
            float4 _CoreColor;
            float4 _HaloColor;
            float _Speed;
            float _MinSize;
            float _MaxSize;
            float _Sharpness;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 픽셀화 (Pixelation)
                // UV 좌표를 해상도 단위로 끊어줍니다.
                float2 pixelUV = floor(IN.texcoord * _PixelRes) / _PixelRes;
                
                // 중심점 기준 거리 계산
                float dist = length(pixelUV - 0.5);
                
                // 원형 마스크 (완전 바깥은 투명)
                if(dist > 0.5) discard;

                // 2. 펄스(Pulse) 파동 만들기
                // sin 함수: -1 ~ 1 사이를 오감
                // 0.5 + 0.5 * sin(...) : 0 ~ 1 사이로 변환
                float wave = sin(_Time.y * _Speed);
                
                // Sharpness를 적용해 파동의 모양을 변형
                // Sharpness가 1이면 부드러운 사인파, 높으면 급격하게 켜지는 파형
                float pulse = pow(0.5 + 0.5 * wave, _Sharpness);

                // 3. 반지름(Radius) 동적 변화
                // 펄스 값에 따라 최소 크기 ~ 최대 크기 사이를 움직임
                float currentRadius = lerp(_MinSize, _MaxSize, pulse);

                // 4. 색상 계산
                float3 finalColor = float3(0,0,0);

                // A. 코어 (중심부) - 항상 빛나지만 크기가 약간 변함
                // smoothstep을 사용하여 경계를 부드럽게(혹은 픽셀 느낌나게) 처리
                float coreRadius = currentRadius * 0.5;
                float coreAlpha = 1.0 - smoothstep(coreRadius, coreRadius + 0.05, dist);
                finalColor += _CoreColor.rgb * coreAlpha;

                // B. 헤일로 (퍼지는 빛) - 펄스에 맞춰 크기와 투명도가 변함
                // 외곽으로 갈수록 부드럽게 사라짐
                float haloAlpha = 1.0 - smoothstep(currentRadius * 0.8, currentRadius, dist);
                // 펄스가 약할 때(작을 때)는 빛도 어둡게 처리
                finalColor += _HaloColor.rgb * haloAlpha * (0.5 + 0.5 * pulse);

                // 5. 최종 합성 및 클리핑
                fixed4 col = fixed4(finalColor, 1.0);
                
                // 알파값: 코어와 헤일로 중 큰 값을 알파로 사용 (픽셀 느낌 살리기 위해)
                col.a = max(coreAlpha, haloAlpha);
                
                // 텍스처(스프라이트) 알파값 곱하기
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                col.a *= texColor.a;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col * IN.color;
            }
            ENDCG
        }
    }
}