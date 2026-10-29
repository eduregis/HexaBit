Shader "HexaBit/EnemyHitEffect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        
        _HitFlash ("Hit Flash", Range(0, 1)) = 0
        _DissolveAmount ("Dissolve Amount", Range(0, 1.1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "SpriteUnlit"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                half4 color     : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color     : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                float _HitFlash;
                float _DissolveAmount;
            CBUFFER_END

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                // 1. Lê a cor original do sprite
                half4 spriteColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord) * IN.color;

                // 2. Aplica o Flash Branco
                half4 finalColor = lerp(spriteColor, half4(1, 1, 1, spriteColor.a), _HitFlash);

                // 3. Lógica de Dissolução
                float noise = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.texcoord).r;
                float dissolveMask = step(_DissolveAmount, noise);

                // Aplica a máscara no Alpha
                finalColor.a *= dissolveMask;

                // Pré-multiplica o alpha
                finalColor.rgb *= finalColor.a;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}