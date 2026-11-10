Shader "HexaBit/SpriteOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,0,0,1)
        
        // 1 divided by texture width/height (calculated automatically by script or manually set)
        _TexelSize ("Texel Size", Vector) = (0.01, 0.01, 0, 0)
        _OutlineThickness ("Outline Thickness", Range(0, 5)) = 1
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
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                half4 color     : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half4 _Color;
                half4 _OutlineColor;
                float _OutlineThickness;
            CBUFFER_END

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(v2f IN) : SV_Target
            {
                // Read the center pixel
                half4 centerColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord);
                
                // If the pixel is already opaque, just return it
                if (centerColor.a > 0.1) {
                    centerColor.rgb *= centerColor.a; // Premultiply alpha
                    return centerColor;
                }

                // Check neighbors for the outline
                float2 size = _MainTex_TexelSize.xy * _OutlineThickness;
                float alpha = 0;

                // Sample Up, Down, Left, Right
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord + float2(0, size.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord - float2(0, size.y)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord + float2(size.x, 0)).a;
                alpha += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.texcoord - float2(size.x, 0)).a;

                // If any neighbor is opaque, draw the outline color
                if (alpha > 0.1) {
                    half4 outlineCol = _OutlineColor;
                    outlineCol.rgb *= outlineCol.a; // Premultiply alpha
                    return outlineCol;
                }

                // Otherwise, return transparent
                return half4(0,0,0,0);
            }
            ENDHLSL
        }
    }
}