Shader "Unlit/TextureInward"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
//
//// For video playing inside a sphere
//// https://www.youtube.com/watch?v=E7g4lJGg4nw
//Shader "Flip Normals" {
//    Properties{
//        _MainTex("Base (RGB)", 2D) = "white" {}
//    }
//        SubShader{
//
//            Tags { "RenderType" = "Opaque" }
//
//            Cull Off
//
//            CGPROGRAM
//
//            #pragma surface surf Lambert vertex:vert
//            sampler2D _MainTex;
//
//            struct Input {
//                float2 uv_MainTex;
//                float4 color : COLOR;
//            };
//
//            void vert(inout appdata_full v) {
//                v.normal.xyz = v.normal * -1;
//            }
//
//            void surf(Input IN, inout SurfaceOutput o) {
//                 fixed3 result = tex2D(_MainTex, IN.uv_MainTex);
//                 o.Albedo = result.rgb;
//                 o.Alpha = 1;
//            }
//
//            ENDCG
//
//    }
//
//        Fallback "Diffuse"
//}