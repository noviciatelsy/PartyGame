Shader "Unlit/ScrollingGradient_Smooth"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0.9686, 0.8118, 0.5059, 1) // #F7CF81
        _Color2 ("Color 2", Color) = (1, 0.7451, 0.8824, 1)    // #FFBEE1
        _ScrollSpeed ("Scroll Speed", Float) = 0.5
        _Frequency ("Frequency", Float) = 1.0 // 控制颜色循环的密度
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float4 _Color1;
            float4 _Color2;
            float _ScrollSpeed;
            float _Frequency;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. 计算随时间变化的偏移
                float timeOffset = _Time.y * _ScrollSpeed;
                
                // 2. 将 UV 坐标进行偏移和频率缩放
                // 使用 _Frequency 可以让你控制条纹的宽窄
                float yPos = i.uv.y * _Frequency - timeOffset;

                // 3. 核心改进：使用 pingpong（三角波）逻辑
                // frac(yPos) 会得到 0 -> 1, 0 -> 1 的循环
                // frac(yPos) * 2 - 1 会得到 -1 -> 1, -1 -> 1
                // abs(...) 之后，就会得到 1 -> 0 -> 1 的连续平滑曲线
                float smoothT = abs(frac(yPos) * 2.0 - 1.0);

                // 4. 根据平滑的 smoothT 进行插值
                // 这样颜色的变化路径是：Color2 -> Color1 -> Color2
                fixed4 col = lerp(_Color1, _Color2, smoothT);

                return col;
            }
            ENDCG
        }
    }
}