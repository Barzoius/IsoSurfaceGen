Shader "Unlit/tth"
{
    Properties
    {
        _MinHeight("Min Height", Float) = 5
        _MidHeight("Mid Height", Float) = 25
        _MaxHeight("Max Height", Float) = 30

        _Tex1("Low Texture", 2D) = "white" {}
        _Tex2("Mid Texture", 2D) = "white" {}
        _Tex3("High Texture", 2D) = "white" {}

        _Tiling("Tiling", Float) = 1
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _Tex1, _Tex2, _Tex3;
        float _Tiling;

        float _MinHeight;
        float _MidHeight;
        float _MaxHeight;
        half _Metallic;
        half _Smoothness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {

            float2 xUV = IN.worldPos.zy * _Tiling;
            float2 yUV = IN.worldPos.xz * _Tiling;
            float2 zUV = IN.worldPos.xy * _Tiling;

   
            float3 blend = abs(normalize(IN.worldNormal));
            blend /= (blend.x + blend.y + blend.z); // normalize weights


            float4 lowTex = tex2D(_Tex1, xUV) * blend.x +
                            tex2D(_Tex1, yUV) * blend.y +
                            tex2D(_Tex1, zUV) * blend.z;

            float4 midTex = tex2D(_Tex2, xUV) * blend.x +
                            tex2D(_Tex2, yUV) * blend.y +
                            tex2D(_Tex2, zUV) * blend.z;

            float4 highTex = tex2D(_Tex3, xUV) * blend.x +
                             tex2D(_Tex3, yUV) * blend.y +
                             tex2D(_Tex3, zUV) * blend.z;

   
            float height = IN.worldPos.y;
            float3 finalColor;

            if (height < _MidHeight)
            {
                float t = saturate((height - _MinHeight) / (_MidHeight - _MinHeight));
                finalColor = lerp(lowTex.rgb, midTex.rgb, t);
            }
            else
            {
                float t = saturate((height - _MidHeight) / (_MaxHeight - _MidHeight));
                finalColor = lerp(midTex.rgb, highTex.rgb, t);
            }

            o.Albedo = finalColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
