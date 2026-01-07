Shader "UI/CRT_Effect"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        // 이 Color가 '형광색'을 결정합니다 (녹색, 호박색 등)
        _Color ("Tint (Phosphor Color)", Color) = (0, 1, 0, 1) 
        
        _ScanlineSize ("Scanline Size", Float) = 200.0
        _ScanlineIntensity ("Scanline Intensity", Range(0,1)) = 0.2
        _Curvature ("Curvature", Range(0, 0.1)) = 0.05
        
        // [신규] 모노톤 강도 (1이면 완전 흑백+틴트, 0이면 원래 색)
        _Monochrome ("Monochrome Strength", Range(0, 1)) = 1.0 

        // UI 마스킹 필수 속성
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _ScanlineSize;
            float _ScanlineIntensity;
            float _Curvature;
            float _Monochrome; // [신규]

            v2f vert(appdata_t v)
            {
                v2f OUT;
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                // 버텍스 컬러(UI 컴포넌트 색상)는 나중에 곱해줍니다.
                OUT.color = v.color; 
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. 화면 왜곡
                float2 uv = IN.texcoord;
                float2 center = float2(0.5, 0.5);
                float2 off = uv - center;
                float2 off2 = off * off;
                uv = center + off * (1.0 + _Curvature * (off2.x + off2.y));

                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                    return fixed4(0,0,0,1);

                // 2. 텍스처 색상 가져오기
                fixed4 texColor = tex2D(_MainTex, uv);

                // 3. [핵심] 모노톤 변환 로직
                // 3-1. 현재 픽셀의 밝기(Luminance) 구하기
                float gray = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
                
                // 3-2. 밝기 값에 우리가 원하는 형광색(_Color)을 입힘
                float3 monoColor = float3(gray, gray, gray) * _Color.rgb;
                
                // 3-3. 원래 색 vs 모노톤 색 섞기 (Monochrome 변수에 따라)
                texColor.rgb = lerp(texColor.rgb, monoColor, _Monochrome);
                
                // 4. 알파값 및 스캔라인 적용
                texColor.a *= IN.color.a; // 투명도 유지
                
                float scanline = sin(uv.y * _ScanlineSize * 3.14159);
                texColor.rgb -= (scanline * 0.5 + 0.5) * _ScanlineIntensity;

                return texColor;
            }
            ENDCG
        }
    }
}