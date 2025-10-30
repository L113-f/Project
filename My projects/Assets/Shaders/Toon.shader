Shader "Custom/SimpleToonURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _BaseMap   ("Base Map", 2D) = "white" {}
        _LightColor ("Light Color (lit)", Color) = (1,1,1,1)
        _ShadowColor("Shadow Color (shade)", Color) = (0.7,0.7,0.7,1)
        _Threshold ("Threshold", Range(0,1)) = 0.5    // 明暗分界
        _Feather   ("Feather", Range(0.001,0.2)) = 0.05 // 过渡宽度
    }

    SubShader
    {
        Tags{ "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 150

        Pass
        {
            Name "ForwardLit"
            Tags{ "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor, _BaseMap_ST;
                float4 _LightColor, _ShadowColor;
                float  _Threshold, _Feather;
            CBUFFER_END

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };
            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 shadowCoord: TEXCOORD3;
                float  fogCoord   : TEXCOORD4;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs   n = GetVertexNormalInputs(v.normalOS);
                o.positionCS  = p.positionCS;
                o.positionWS  = p.positionWS;
                o.normalWS    = n.normalWS;
                o.uv          = TRANSFORM_TEX(v.uv, _BaseMap);
                o.shadowCoord = TransformWorldToShadowCoord(p.positionWS);
                o.fogCoord    = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float3 N = normalize(i.normalWS);

                // 主光+阴影
                Light L = GetMainLight(i.shadowCoord);
                float ndl = saturate(dot(N, L.direction));
                float atten = L.distanceAttenuation * L.shadowAttenuation;

                // 二值化（带羽化）
                float t = saturate( (ndl - _Threshold) / max(_Feather, 1e-4) );
                t = smoothstep(0,1,t); // 轻微平滑

                float4 baseTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                float3 lit   = baseTex.rgb * _LightColor.rgb * L.color * atten;
                float3 shade = baseTex.rgb * _ShadowColor.rgb;

                float3 col = lerp(shade, lit, t);

                col = MixFog(col, i.fogCoord);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    FallBack Off
}
