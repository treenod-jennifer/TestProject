// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Poko/UnlitTransparentFrame"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Main Color", Color) = (1,1,1,1)
		_MaxFrame ("Max Frame", float) = 4
		_Frame ("Frame", float) = 1

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

			float _MaxFrame;
			float _Frame;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				int maxframe = _MaxFrame*0.5f;

				o.uv = v.uv/(maxframe);
				
				o.uv.y += ceil((float)_Frame/maxframe)* 0.5f;
				o.uv.x += ((_Frame - 1)%maxframe)* 0.5f;
				return o;
			}
			
			sampler2D _MainTex;
			float4 _Color;

			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv) *  _Color;
				return col;
			}
			ENDCG
		}
	}
}
