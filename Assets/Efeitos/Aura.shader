// The G-HYDRA's miasma around a PUCmon: round particles in flat bands, like cartoon flames. Drawn after the wall hole
// (queue Geometry-5) and before the PUCmons (Geometry), without writing depth, so a PUCmon always shows in front of
// its own aura while the invisible wall (DepthMask) still hides the aura behind the poster.
Shader "PUCmon/Aura"
{
    Properties
    {
        [IntRange] _Bands ("Bands", Range(1, 8)) = 3
    }
    SubShader
    {
        Tags { "Queue" = "Geometry-2" "RenderType" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" }
        Blend One OneMinusSrcAlpha  // premultiplied, like the page's canvas
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Bands;

            struct appdata { float4 vertex : POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = length(i.uv - 0.5) * 2.0;  // 0 in the middle, 1 at the rim
                float a = ceil(saturate(1.0 - d) * _Bands) / _Bands * i.color.a;
                return fixed4(i.color.rgb * a, a);
            }
            ENDCG
        }
    }
}
