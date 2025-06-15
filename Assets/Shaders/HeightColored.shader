Shader "Custom/HeightColored"
{
    Properties
    {
        _MinHeight("Min Height", Float) = 5
        _MidHeight("Mid Height", Float) = 70
        _MaxHeight("Max Height", Float) = 90

        _LowColor("Low Color", Color) = (0.231, 0.502, 0.114, 1)     // Green
        _MidColor("Mid Color", Color) = (0.4, 0.3, 0.25, 1)          // Brown/Gray
        _HighColor("High Color", Color) = (1, 1, 1, 1)               // White

        _Metallic("Metallic", Range(0,1)) = 0.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        struct Input
        {
            float3 worldPos;
        };

        float _MinHeight;
        float _MidHeight;
        float _MaxHeight;
        fixed4 _LowColor;
        fixed4 _MidColor;
        fixed4 _HighColor;
        half _Metallic;
        half _Smoothness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float height = IN.worldPos.y;

            fixed3 color;
            if (height < _MidHeight)
            {
                float t = saturate((height - _MinHeight) / (_MidHeight - _MinHeight));
                color = lerp(_LowColor.rgb, _MidColor.rgb, t);
            }
            else
            {
                float t = saturate((height - _MidHeight) / (_MaxHeight - _MidHeight));
                color = lerp(_MidColor.rgb, _HighColor.rgb, t);
            }

            o.Albedo = color;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }

    FallBack "Standard"
}
