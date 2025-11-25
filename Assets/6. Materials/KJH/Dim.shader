Shader "MyShader/Dim"
{
    Properties
    {
        _MainTex ("Ink Mask (Grayscale)", 2D) = "white" {}
        _Threshold ("Threshold", Range(0,1)) = 0
        _Value ("Value", Range(0.0, 1)) = 0.05
        _Color ("Color", Color) = (0,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Threshold;
            float _Value;
            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float mask = tex2D(_MainTex, i.uv).r;

                // 아주 작은 값에서는 step으로 전환
                float alpha;
                if (_Value < 1e-4)
                {
                    alpha = step(_Threshold, mask); // 마스크가 threshold 이상이면 alpha = 1
                }
                else
                {
                    alpha = smoothstep(_Threshold - _Value, _Threshold + _Value, mask);
                }

                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
