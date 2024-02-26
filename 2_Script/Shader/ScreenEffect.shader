// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Poko/ScreenEffect"
{
	Properties
	{
		_MainColor ("Main Color", Color) = (0,0,0,1)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag		
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			float4 _MainColor;

			fixed4 frag (v2f i) : SV_Target
			{
			
				float4 col = _MainColor;//lerp(col.rgb, lerp(_MaskColor.rgb, col.rgb, weight), _MaskColor.a);
				return col;
			}
			ENDCG
		}
	}
}
