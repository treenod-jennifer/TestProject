// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Poko/UnlitTransparentTwoT"
{
		Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaTex ("AlphaTex", 2D) = "black" {}
		_Color ("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
	
	Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}
		

		// No culling or depth
		//Cull Off ZWrite Off ZTest Always
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
		//Cull Off
		Lighting Off

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
			sampler2D _AlphaTex;
			float4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				//float4 col = tex2D(_MainTex, i.uv) *  _Color;
				float3 finalColor = tex2D(_MainTex, i.uv) *  _Color;
				float finalAlpha = (tex2D(_AlphaTex,i.uv).a * _Color.a) ;

				return fixed4(finalColor, finalAlpha);
			//	return col;
			}
			ENDCG
		}
	}
}