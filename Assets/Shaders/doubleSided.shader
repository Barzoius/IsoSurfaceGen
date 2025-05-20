Shader "Custom/doubleSided"
{
Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = tex.rgb;
            o.Alpha = tex.a;

            // Flip normal for backfaces
            #ifdef UNITY_FRONT_FACING
            if (!unity_FrontFacing)
                o.Normal = -o.Normal;
            #endif
        }
        ENDCG
    }

    FallBack "Diffuse"
}
