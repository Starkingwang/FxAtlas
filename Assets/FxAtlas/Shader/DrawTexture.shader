Shader "Hidden/DrawTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }


    SubShader

    {
        Tags { "RenderType"="Opaque" }
        LOD 100

		CGINCLUDE
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

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

		ENDCG

        Pass
        {
			Cull Off ZWrite Off ZTest Always
			ColorMask RGB
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return fixed4(col.rgb, 1);
			}

            ENDCG
        }

		Pass
        {
			Cull Off ZWrite Off ZTest Always
			ColorMask A
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return fixed4(0, 0, 0, col.a);
			}

            ENDCG
        }
    }
}
