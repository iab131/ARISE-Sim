Shader "Unlit/InfiniteGrid"
{
    Properties
    {
        _GridColor("Grid Color", Color) = (0.35, 0.35, 0.35, 1)
        _BgColor("Background", Color)   = (0.10, 0.10, 0.10, 1)
        _AxisXColor("X Axis", Color)    = (0.85, 0.25, 0.25, 1)
        _AxisZColor("Z Axis", Color)    = (0.25, 0.85, 0.25, 1)
        _CellSize("Cell Size", Float)   = 1
        _LineThickness("Line Thickness", Float) = 0.015
        _MajorEvery("Major Line Every N", Float) = 10
        _MajorMul("Major Line Multiplier", Float) = 1.0
        _AxisMul("XY Axis multiplier", Float) = 2
        _FadeStart("Fade Start", Float) = 50
        _FadeEnd("Fade End", Float)     = 150
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex:POSITION; };
            struct v2f {
                float4 pos:SV_POSITION;
                float3 worldPos:TEXCOORD0;
            };

            float4 _GridColor, _BgColor, _AxisXColor, _AxisZColor;
            float _CellSize, _LineThickness, _MajorEvery, _MajorMul, _AxisMul;
            float _FadeStart, _FadeEnd;

            v2f vert(appdata v){
                v2f o;
                float4 w = mul(unity_ObjectToWorld, v.vertex);
                o.worldPos = w.xyz;
                o.pos = mul(UNITY_MATRIX_VP, w);
                return o;
            }

            float gridLine(float coord, float cell, float thickness){
                float g = abs(frac(coord / cell) - 0.5) * 2.0; // distance to line center 0..1
                return smoothstep(thickness, 0.0, g);          // 1 near line, 0 away
            }

            float gridMajor(float coord, float cell, float every, float thickness, float mul){
                float c = coord / (cell * every);
                float g = abs(frac(c) - 0.5) * 2.0;
                return smoothstep(thickness * mul, 0.0, g);
            }

            fixed4 frag(v2f i):SV_Target
            {
                float3 wp = i.worldPos;

                // Base
                float4 col = _BgColor;

                // Regular grid masks
                float gx = gridLine(wp.x, _CellSize, _LineThickness);
                float gz = gridLine(wp.z, _CellSize, _LineThickness);
                float g  = max(gx, gz);

                // Major grid masks (thicker via _MajorMul internally)
                float gmx = gridMajor(wp.x, _CellSize, _MajorEvery, _LineThickness, _MajorMul);
                float gmz = gridMajor(wp.z, _CellSize, _MajorEvery, _LineThickness, _MajorMul);
                float gm  = max(gmx, gmz);

                // 👉 Remove the “outline”: suppress normal lines wherever a major line exists
                float gNoMajor = g * (1.0 - gm);

                // Axis thickness uses the SAME multiplier as major lines
                float axisThickness = _LineThickness * _AxisMul;
                float ax = smoothstep(axisThickness, 0.0, abs(wp.z)); // X axis (Z=0)
                float az = smoothstep(axisThickness, 0.0, abs(wp.x)); // Z axis (X=0)

                // Distance fade
                float fade = 1.0;
                #if defined(UNITY_MATRIX_V)
                float d = distance(_WorldSpaceCameraPos, float3(wp.x, 0, wp.z));
                fade = saturate(1.0 - smoothstep(_FadeStart, _FadeEnd, d));
                #endif

                // Draw: regular grid (no-major), then major, then axes (no double-blend halos)
                col = lerp(col, _GridColor, gNoMajor * 0.75 * fade);
                col = lerp(col, _GridColor, gm      * 0.95 * fade); // a touch stronger for majors
                col = lerp(col, _AxisXColor, ax * fade);
                col = lerp(col, _AxisZColor, az * fade);

                // Alpha from strongest feature (prevents “glow” outlines)
                float alpha = max(max(gNoMajor, gm), max(ax, az)) * fade;
                col.a = saturate(alpha + 0.05);
                return col;
            }
            ENDCG
        }
    }
}
