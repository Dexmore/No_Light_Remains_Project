Shader "UI/Custom/PlasmaBall_ZigZag"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        // --- 플라즈마 색상 (HDR 적용) ---
        [HDR] _CoreColor ("Core Color", Color) = (2.5, 2.0, 2.5, 1) // 더 밝게 빛나도록 강도 높임
        _GlowColor ("Glow Color", Color) = (0.8, 0.1, 0.8, 1)
        
        // --- 움직임 및 형태 제어 ---
        _Speed ("Speed", Range(0.1, 5.0)) = 1.5
        _NoiseScale ("ZigZag Frequency", Range(1, 30)) = 12.0  // 꺾이는 빈도 (높을수록 자잘하게 꺾임)
        _NoiseStrength ("ZigZag Strength", Range(0.0, 0.5)) = 0.15 // 꺾이는 강도 (높을수록 심하게 휨)
        _Bolts ("Bolt Count", Int) = 12
        
        // UI 필수 속성
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        // 인터랙션
        _TargetPos ("Target Position (UV)", Vector) = (0.5, 0.5, 0, 0)
        _InteractPower ("Interaction Power", Range(0, 1)) = 0.0
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

            // Custom Properties
            float4 _CoreColor;
            float4 _GlowColor;
            float _Speed;
            float _NoiseScale;
            float _NoiseStrength; // 추가됨
            int _Bolts;
            float4 _TargetPos;
            float _InteractPower;

            // --- 노이즈 함수들 ---
            float hash12(float2 p)
            {
                float3 p3  = frac(float3(p.xyx) * .1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f*f*(3.0-2.0*f);
                return lerp( lerp( hash12(i + float2(0.0,0.0)), hash12(i + float2(1.0,0.0)), u.x),
                             lerp( hash12(i + float2(0.0,1.0)), hash12(i + float2(1.0,1.0)), u.x), u.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amp = 0.5;
                float2 shift = float2(100.0, 100.0);
                float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.50));
                for (int i = 0; i < 4; i++) {
                    value += amp * noise(p);
                    p = mul(rot, p) * 2.0 + shift;
                    amp *= 0.5;
                }
                return value;
            }

            // 2D 벡터 노이즈 (UV 좌표 왜곡용)
            float2 fbm2(float2 p) {
                return float2(fbm(p), fbm(p + float2(12.3, 45.6)));
            }

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
                // 1. 기본 UV 및 거리 계산
                float2 baseUV = IN.texcoord - 0.5;
                float dist = length(baseUV);
                
                // 원형 클리핑
                if(dist > 0.5) discard;

                float3 finalColor = float3(0,0,0);
                float time = _Time.y * _Speed;

                // A. 배경 글로우
                float glow = 1.0 - smoothstep(0.0, 0.5, dist);
                finalColor += _GlowColor.rgb * (glow * 0.4);

                // B. 번개 줄기 생성 (Loop)
                for(int i=0; i < _Bolts; i++)
                {
                    // === [핵심 변경 사항: 공간 왜곡 (Domain Warping)] ===
                    // 각 번개마다(i) 다른 랜덤 시드 값을 가짐
                    float seed = float(i) * 123.45;
                    
                    // 시간에 따라 흐르는 노이즈 생성
                    float2 noiseInput = baseUV * _NoiseScale + float2(0, time * 0.5) + seed;
                    
                    // UV 좌표 자체를 휘게 만듦 (-1 ~ 1 범위로 변환)
                    float2 warp = (fbm2(noiseInput) - 0.5) * 2.0; 
                    
                    // 중심부는 덜 휘고, 외곽으로 갈수록 심하게 휘도록 (dist 곱함)
                    // _NoiseStrength로 휨 강도 조절
                    float2 warpedUV = baseUV + warp * _NoiseStrength * smoothstep(0.0, 1.0, dist * 2.0);

                    // ===============================================

                    // 휘어진 좌표(warpedUV)를 기준으로 각도 계산
                    float angleToPixel = atan2(warpedUV.y, warpedUV.x);
                    
                    // 번개 줄기의 기본 각도
                    float angle = (float(i) / float(_Bolts)) * 6.2831; // 기본 배치
                    
                    // 인터랙션 처리
                    if(_InteractPower > 0.0 && i == 0) {
                        float2 dirToTarget = _TargetPos.xy - 0.5;
                        // 타겟 방향도 약간 흔들리게
                        dirToTarget += (fbm2(dirToTarget * 5.0 + time) - 0.5) * 0.1; 
                        angle = atan2(dirToTarget.y, dirToTarget.x);
                    } else {
                        // 일반 줄기는 천천히 회전 + 불규칙한 움직임
                        angle += time * 0.1 + sin(time * 0.5 + i * 10.0) * 0.5; 
                    }

                    // 각도 차이 계산 (최단 거리)
                    float angleDiff = abs(angle - angleToPixel);
                    if(angleDiff > 3.14159) angleDiff = 6.28318 - angleDiff;

                    // 줄기 두께 (랜덤성 추가)
                    float thicknessNoise = fbm(warpedUV * 5.0 - time * 3.0);
                    float beamWidth = 0.002 + 0.015 * thicknessNoise; 

                    // 빛의 세기 계산
                    // warpedUV를 썼기 때문에 직선 거리 계산식에 넣어도 결과는 구불구불하게 나옴
                    float beam = beamWidth / (abs(angleDiff) * length(warpedUV) + 0.001);
                    
                    // 마스킹 (중심부 타지 않게, 외곽 부드럽게)
                    beam *= smoothstep(0.5, 0.35, dist);

                    finalColor += _CoreColor.rgb * beam;
                }

                // C. 중심부 핵 (원래 UV 사용 - 핵은 흔들리면 안됨)
                float core = 1.0 / (dist * 20.0 + 0.2);
                finalColor += _CoreColor.rgb * core * 1.5;

                // 인터랙션 하이라이트
                if(_InteractPower > 0.0)
                {
                    float2 targetUV = _TargetPos.xy - 0.5;
                    float distToTouch = length(baseUV - targetUV);
                    finalColor += _CoreColor.rgb * (0.01 / (distToTouch + 0.01)) * _InteractPower;
                }

                fixed4 col = fixed4(finalColor, 1.0);
                fixed4 texColor = tex2D(_MainTex, IN.texcoord); // 기본 텍스처(UI 스프라이트 등)
                col.a = texColor.a; 

                // UI 클리핑
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