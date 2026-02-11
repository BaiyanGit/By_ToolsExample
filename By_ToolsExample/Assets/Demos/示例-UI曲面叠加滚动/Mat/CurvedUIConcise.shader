// 曲面UI简洁版
Shader "BY/CurvedUIConcise"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("色调", Color) = (1,1,1,1)
        [PerRendererData] _TextureSampleAdd ("Texture Sample Add", Vector) = (0, 0, 0, 0)
        _RadiusX ("半径X（水平）", Float) = 1700
        _RadiusZ ("半径Z（深度）", Float) = 800
        _ArcAngle ("弧角", Float) = 50
        _UIWidth ("UI宽度", Float) = 2000
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
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB

        Pass
        {
            Name "UI_TEXT_COLOR"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2_f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 world_position : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _RadiusX;
            float _RadiusZ;
            float _ArcAngle;
            float _UIWidth;

            v2_f vert(appdata_t v)
            {
                v2_f o;
                o.world_position = v.vertex;

                float3 pos = v.vertex.xyz;

                // X 归一化到 [-1,1]
                const float half_width = _UIWidth * 0.5;
                const float t = pos.x / half_width;

                // 映射到角度（弧度）
                const float angle = t * (_ArcAngle * 0.5) * UNITY_PI / 180.0;
                const float k = smoothstep(0, 1e-6, abs(t));
                const float curved_x_full = sin(angle) * _RadiusX;
                const float curved_x = lerp(pos.x, curved_x_full, k);
                const float curved_z_full = cos(angle) * _RadiusZ - _RadiusZ;

                pos.x = curved_x;
                pos.z = curved_z_full;

                o.vertex = UnityObjectToClipPos(float4(pos, 1));
                o.texcoord = v.texcoord;

                // 应用顶点颜色和全局颜色
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2_f i) : SV_Target
            {
                // 主纹理采样
                fixed4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;

                // UI裁剪
                color.a *= UnityGet2DClipping(i.world_position.xy, _ClipRect);
                return color;
            }
            ENDCG
        }
    }

    FallBack "UI/Default"
    CustomEditor "UnityEditor.UI.UIShaderGUI"
}