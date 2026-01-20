Shader "UI/Custom/PlasmaBall_PixelArt"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        
        [Header(Pixel Art Settings)]
        _PixelRes ("Pixel Resolution", Float) = 64.0 // 픽셀 해상도 (낮을수록 도트가 커짐)

        [Header(Colors)]
        [HDR] _CoreColor ("Core Color", Color) = (2.5, 2.0, 2.5, 1)
        _GlowColor ("Glow Color", Color) = (0.8, 0.1, 0.8, 1)
        
        [Header(Lightning Settings)]
        _Speed ("Speed", Range(0.1, 5.0)) = 1.5
        _NoiseScale ("ZigZag Frequency", Range(1, 30)) = 10.0
        _NoiseStrength ("ZigZag Strength", Range(0.0, 0.5)) = 0.15
        _Bolts ("Bolt Count", Int) = 6
        _BeamWidth ("Beam Width", Range(0.01, 0.1)) = 0.02 // 픽셀 아트에선 약간 두꺼워야 잘 보임
        
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
            float _PixelRes;
            float4 _CoreColor;
            float4 _GlowColor;
            float _Speed;
            float _NoiseScale;
            float _NoiseStrength;
            int _Bolts;
            float _BeamWidth;
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
                // [픽셀 아트 핵심 로직]
                // UV 좌표를 해상도(_PixelRes) 단위로 끊어버림(floor)
                // 이렇게 하면 계산이 "칸" 단위로 이루어져 모자이크 효과가 남
                float2 pixelUV = floor(IN.texcoord * _PixelRes) / _PixelRes;
                
                // 1. 중심점 맞추기
                float2 baseUV = pixelUV - 0.5;
                float dist = length(baseUV);
                
                // 원형 클리핑 (도트 느낌을 위해 테두리도 끊어지게 처리)
                if(dist > 0.5) discard;

                float3 finalColor = float3(0,0,0);
                float time = _Time.y * _Speed;

                // A. 배경 글로우 (밴딩 현상이 생기는데, 픽셀아트에선 오히려 좋음)
                float glow = 1.0 - smoothstep(0.0, 0.5, dist);
                finalColor += _GlowColor.rgb * (glow * 0.4);

                // B. 번개 줄기 생성 (Loop)
                for(int i=0; i < _Bolts; i++)
                {
                    float seed = float(i) * 123.45;
                    
                    // Domain Warping (공간 왜곡)
                    float2 noiseInput = baseUV * _NoiseScale + float2(0, time * 0.5) + seed;
                    float2 warp = (fbm2(noiseInput) - 0.5) * 2.0; 
                    float2 warpedUV = baseUV + warp * _NoiseStrength * smoothstep(0.0, 1.0, dist * 2.0);

                    float angleToPixel = atan2(warpedUV.y, warpedUV.x);
                    
                    float angle = (float(i) / float(_Bolts)) * 6.2831; 
                    
                    if(_InteractPower > 0.0 && i == 0) {
                        float2 dirToTarget = _TargetPos.xy - 0.5;
                        dirToTarget += (fbm2(dirToTarget * 5.0 + time) - 0.5) * 0.1; 
                        angle = atan2(dirToTarget.y, dirToTarget.x);
                    } else {
                        angle += time * 0.1 + sin(time * 0.5 + i * 10.0) * 0.5; 
                    }

                    float angleDiff = abs(angle - angleToPixel);
                    if(angleDiff > 3.14159) angleDiff = 6.28318 - angleDiff;

                    // 두께 계산 (픽셀 아트라 너무 얇으면 사라지므로 _BeamWidth로 조절)
                    float thicknessNoise = fbm(warpedUV * 5.0 - time * 3.0);
                    float beamWidth = _BeamWidth + (_BeamWidth * 3.0) * thicknessNoise; 

                    float beam = beamWidth / (abs(angleDiff) * length(warpedUV) + 0.001);
                    beam *= smoothstep(0.5, 0.35, dist);

                    finalColor += _CoreColor.rgb * beam;
                }

                // C. 중심부 핵
                float core = 1.0 / (dist * 20.0 + 0.2);
                finalColor += _CoreColor.rgb * core * 1.5;

                // 인터랙션 하이라이트
                if(_InteractPower > 0.0)
                {
                    float2 targetUV = _TargetPos.xy - 0.5;
                    // 타겟 위치도 픽셀 단위로 스냅
                    targetUV = floor((_TargetPos.xy) * _PixelRes) / _PixelRes - 0.5;
                    
                    float distToTouch = length(baseUV - targetUV);
                    finalColor += _CoreColor.rgb * (0.01 / (distToTouch + 0.01)) * _InteractPower;
                }

                fixed4 col = fixed4(finalColor, 1.0);
                fixed4 texColor = tex2D(_MainTex, IN.texcoord);
                col.a = texColor.a; 

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