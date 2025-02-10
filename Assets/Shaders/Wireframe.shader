Shader"Custom/Wireframe"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _WireColor ("Wire Color", Color) = (0, 1, 0, 1)
        _WireThickness ("Wire Thickness", Range(0.0, 0.01)) = 0.002
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
Cull Off

Lighting Off

ZWrite On

Blend SrcAlpha
OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom

#include "UnityCG.cginc"

struct appdata
{
    float4 vertex : POSITION;
};

struct v2g
{
    float4 pos : SV_POSITION;
};

struct g2f
{
    float4 pos : SV_POSITION;
    float thickness : TEXCOORD0;
};

v2g vert(appdata v)
{
    v2g o;
    o.pos = UnityObjectToClipPos(v.vertex);
    return o;
}

[maxvertexcount(3)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
{
    float thickness = 0.005;
                
    g2f o;
    for (int i = 0; i < 3; i++)
    {
        o.pos = input[i].pos;
        o.thickness = thickness;
        triStream.Append(o);
    }
}

fixed4 _Color;
fixed4 _WireColor;
float _WireThickness;

fixed4 frag(g2f i) : SV_Target
{
    float edgeFactor = fwidth(i.thickness) * _WireThickness;
    float alpha = smoothstep(0.5 - edgeFactor, 0.5 + edgeFactor, i.thickness);
                
    return lerp(_WireColor, _Color, alpha);
}
            ENDCG
        }
    }
}
