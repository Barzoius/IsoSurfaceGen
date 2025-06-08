Shader "Custom/TriplanarMapping"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Tiling("Tiling", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        sampler2D _MainTex;
        float _Tiling;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 blending = abs(normalize(IN.worldNormal));
            blending /= dot(blending, float3(1.0, 1.0, 1.0));

            float2 xUV = IN.worldPos.zy * _Tiling;
            float2 yUV = IN.worldPos.xz * _Tiling;
            float2 zUV = IN.worldPos.xy * _Tiling;

            float4 xTex = tex2D(_MainTex, xUV);
            float4 yTex = tex2D(_MainTex, yUV);
            float4 zTex = tex2D(_MainTex, zUV);

            o.Albedo = xTex.rgb * blending.x + yTex.rgb * blending.y + zTex.rgb * blending.z;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
