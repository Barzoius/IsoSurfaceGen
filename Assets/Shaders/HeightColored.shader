Shader "Custom/HeightColored"
{
     Properties
    {
        _MinHeight("Min Height", Float) = 10
        _MaxHeight("Max Height", Float) = 30
        _LowColor("Low Color", Color) = (0.231, 0.502, 0.114)
        _HighColor("High Color", Color) = (0.62, 0.408, 0.149, 1)
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
        float _MaxHeight;
        fixed4 _LowColor;
        fixed4 _HighColor;
        half _Metallic;
        half _Smoothness;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float height = IN.worldPos.y;
            float t = saturate((height - _MinHeight) / (_MaxHeight - _MinHeight));
            o.Albedo = lerp(_LowColor.rgb, _HighColor.rgb, t);
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }

    FallBack "Standard"
}
