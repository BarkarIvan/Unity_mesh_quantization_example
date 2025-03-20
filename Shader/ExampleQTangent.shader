Shader "Hidden/Pack Example QTangent"
{
    Properties
    {
        _NormalMap("NormalMap", 2D) = "white"{}
    }

    SubShader
    {

        Tags
        {
            "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "meshCompression.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 qTangent : NORMAL; //snorm16 -> float (fetcher) -> float (by obj2world)
                half2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD1;
                half2 uv : TEXCOORD0;
                half3 normalWS : NORMAL;
                half3 tangentWS : TANGENT;
                half3 bitangentWS : BITANGENT;
                half3 color : COLOR;
            };

            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            float4 _NormalMap_ST;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 pos;
                unpackPositionSNorm16(IN.positionOS, pos);
                OUT.positionHCS = TransformObjectToHClip(pos);
                half3 n;
                half3 t;
                half3 b;

                toTangentFrame(normalize(IN.qTangent), n, t, b);

                OUT.normalWS = TransformObjectToWorldNormal(n);
                OUT.tangentWS = mul((float3x3)GetObjectToWorldMatrix(), t);
                OUT.bitangentWS = mul((float3x3)GetObjectToWorldMatrix(), b);
                OUT.positionWS = mul(GetObjectToWorldMatrix(), float4(pos, 1));
                OUT.uv = TRANSFORM_TEX(IN.uv, _NormalMap);
                OUT.color = (OUT.normalWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv));
                half3x3 tangentToWorld = half3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS),
                     normalize(IN.normalWS));
                half3 normalWS = mul(normalTS, tangentToWorld);
                half c = dot(_MainLightPosition, normalWS)*0.5+0.5;
                half4 customColor = half4(c,c,c, 1);
                return customColor;
            }
            ENDHLSL
        }
    }
}