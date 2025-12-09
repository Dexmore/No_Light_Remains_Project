Shader "Custom/Universal_UIDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        
        // 0 = 랜덤, 1 = 위에서 아래로
        _VerticalBias ("Vertical Bias (0=Random, 1=TopDown)", Range(0, 1)) = 0.5 
        
        _BurnSize ("Burn Size", Range(0.0, 0.2)) = 0.05
        [HDR] _BurnColor ("Burn Color", Color) = (1, 0.4, 0, 1)

        // UI 마스킹 필수 속성
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _WriteMask ("Stencil Write Mask", Float) = 255
        _ReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "DissolvePass"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

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
                float2 texcoord  : TEXCOORD0;
                float2 noiseUV   : TEXCOORD1;
                float4 worldPosition : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;

            fixed _DissolveAmount;
            fixed _BurnSize;
            fixed4 _BurnColor;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            
            fixed _VerticalBias; 

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.noiseUV = TRANSFORM_TEX(v.texcoord, _NoiseTex);

                OUT.color = v.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                half noiseValue = tex2D(_NoiseTex, IN.noiseUV).r;

                half combinedValue = noiseValue + (IN.texcoord.y * _VerticalBias);
                
                half adjustedCutoff = _DissolveAmount * (1 + _VerticalBias);

                // 1. 투명화 처리
                if (combinedValue <= adjustedCutoff)
                {
                    discard;
                }

                // 2. 불타는 테두리 처리 (여기가 수정됨!)
                // 기존: _DissolveAmount > 0 (0보다만 크면 그려라 -> 1일 때도 그려짐)
                // 수정: _DissolveAmount > 0 && _DissolveAmount < 0.99
                // (완전히 사라진 상태(1.0) 근처에서는 테두리도 그리지 마라!)
                if (combinedValue < adjustedCutoff + _BurnSize 
                    && _DissolveAmount > 0.01 
                    && _DissolveAmount < 0.99) 
                {
                    color = _BurnColor;
                }

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}