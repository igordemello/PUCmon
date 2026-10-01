// Invisible wall: writes depth but no color, so the camera video stays visible while anything virtual behind it is
// hidden. Drawn before the rest of the scene.
Shader "PUCmon/DepthMask"
{
    SubShader
    {
        Tags { "Queue" = "Geometry-10" "RenderType" = "Opaque" }
        ColorMask 0
        ZWrite On
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert (float4 vertex : POSITION) : SV_POSITION { return UnityObjectToClipPos(vertex); }
            fixed4 frag () : SV_Target { return 0; }
            ENDCG
        }
    }
}
