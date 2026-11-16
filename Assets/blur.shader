Shader "Custom/blur" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0.0, 10.0)) = 2.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

#include "UnityCG.cginc"

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

sampler2D _MainTex;
float _BlurSize;

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);
    fixed4 blur = fixed4(0, 0, 0, 0);

    int iterations = 5;
    float step = _BlurSize / _ScreenParams.y;

    for (int j = -iterations; j <= iterations; j++)
    {
        float offset = j * step;
        blur += tex2D(_MainTex, i.uv + float2(offset, 0));
    }

    blur /= iterations * 2 + 1;
    col = lerp(col, blur, _BlurSize);

    return col;
}
            ENDCG
        }
    }
}
