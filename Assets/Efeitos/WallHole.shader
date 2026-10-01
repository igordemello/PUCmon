// Broken-wall hole (WallHole.cs): unlit vertex colors, plus bands drifting down the tunnel so it feels endless.
Shader "PUCmon/WallHole"
{
    Properties
    {
        _Bands ("Bands strength", Range(0, 1)) = 0.35
        _Speed ("Bands speed", Float) = 0.12
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _Bands, _Speed;

            struct appdata { float4 vertex : POSITION; fixed4 color : COLOR; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; fixed4 color : COLOR; float depth : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.depth = v.uv.y;  // 0 at the wall, 1 at the far end of the tunnel
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float d = i.depth;
                float bands = 1 + _Bands * sin((d * 8 - _Time.y * _Speed) * 6.2831853) * d * (1 - d) * 4;
                return fixed4(i.color.rgb * bands, 1);
            }
            ENDCG
        }
    }
}
