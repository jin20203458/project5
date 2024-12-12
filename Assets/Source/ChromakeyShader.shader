Shader "Unlit/KeepTextAndWhite"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TextColor("Text Color", Color) = (0.0, 0.0, 0.0, 1.0) // 글씨 색상 (기본값: 검은색)
        _WhiteThreshold("White Threshold", Range(0, 1)) = 0.9 // 흰색 감도
        _ColorThreshold("Color Threshold", Range(0, 1)) = 0.1 // 글씨 감도
    }
        SubShader
        {
            Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
            LOD 100

            Pass
            {
                ZWrite Off
                Blend SrcAlpha OneMinusSrcAlpha

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"

                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                };

                sampler2D _MainTex;
                float4 _TextColor;
                float _WhiteThreshold;
                float _ColorThreshold;

                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = v.uv;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    fixed4 texColor = tex2D(_MainTex, i.uv);

                // 글씨 색상과의 거리 계산
                float textDiff = distance(texColor.rgb, _TextColor.rgb);

                // 흰색 감지 (R, G, B 값이 모두 높은 경우)
                float whiteLevel = (texColor.r + texColor.g + texColor.b) / 3.0;
                bool isWhite = whiteLevel > _WhiteThreshold;

                // 글씨 또는 흰색이면 유지, 그렇지 않으면 투명
                if (textDiff < _ColorThreshold || isWhite)
                {
                    texColor.a = 1.0; // 글씨와 흰색은 불투명
                }
                else
                {
                    texColor.a = 0.0; // 배경은 투명
                }

                return texColor;
            }
            ENDCG
        }
        }
}