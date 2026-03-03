Shader "Custom/SplitTexture"
{
    Properties
    {
        _TexA ("Texture A", 2D) = "white" {}
        _TexB ("Texture B", 2D) = "white" {}

        _LineAngle ("Line Angle", Float) = 0
        _LineOffset ("Line Offset", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _TexA;
            sampler2D _TexB;

            float _LineAngle;
            float _LineOffset;

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
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 centeredUV = uv - 0.5;
                // 计算斜线
                float angle = radians(_LineAngle);
                float2 dir = float2(cos(angle), sin(angle));
                float linex = dot(centeredUV, dir);
                float offset = (_LineOffset - 0.5) ;
                // 斜线公式
                //float linex = uv.x * cos(angle) + uv.y * sin(angle);

                if (linex > offset)
                {
                    return tex2D(_TexA, uv);
                }
                else
                {
                    return tex2D(_TexB, uv);
                }
            }

            ENDCG
        }
    }
}