// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ScreenFadeEffectMask"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MaskTex ("Mask Texture", 2D) = "white" {}
		_MaskValue ("Mask Value", Range(0,1)) = 0.5
		_MaskColor ("Mask Color", Color) = (0,0,0,1)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag		
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv     : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv     : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _MaskTex;
			float _MaskValue;
			float4 _MaskColor;
			float4 _Offsets;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				v2f o = i;
				o.uv.x = (o.uv.x+_Offsets.x)*_Offsets.z;
				o.uv.y = (o.uv.y+_Offsets.y)*_Offsets.z;

#if UNITY_UV_STARTS_AT_TOP
				o.uv.y = 1 - o.uv.y;
#endif
				float4 mask = tex2D(_MaskTex, o.uv);
				float alpha = mask.a * (1 - 1/255.0);
				float weight = step(_MaskValue, alpha);
				col.rgb = lerp(col.rgb, lerp(_MaskColor.rgb, col.rgb, weight), _MaskColor.a);
				return col;
			}
			ENDCG
		}
	}
}
