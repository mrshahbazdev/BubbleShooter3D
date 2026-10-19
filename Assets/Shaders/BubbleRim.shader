Shader "Custom/BubbleRim"
{
    // Fresnel rim-light bubble shader: glossy candy sphere with a glowing
    // edge rim so bubbles read as truly 3D even in a dark arcade scene.
    // Falls back gracefully — BubblePalette uses Standard if this is missing.
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,1)
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0
        _RimStrength ("Rim Strength", Range(0.0, 3.0)) = 1.2
        _Gloss ("Smoothness", Range(0.0, 1.0)) = 0.9
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        fixed4 _Color;
        fixed4 _RimColor;
        half _RimPower;
        half _RimStrength;
        half _Gloss;
        half _Metallic;

        struct Input
        {
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Gloss;
            // Fresnel rim: bright glowing edge, dark center — classic bubble look.
            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            o.Emission = _RimColor.rgb * pow(rim, _RimPower) * _RimStrength;
        }
        ENDCG
    }
    FallBack "Standard"
}
