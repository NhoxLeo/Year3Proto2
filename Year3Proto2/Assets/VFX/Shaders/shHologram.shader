Shader "Custom/hologram"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Pattern("Pattern", 2D) = "white" {}
		_Mask("Mask", 2D) = "white" {}
		_SpeedXPatt("Speed X Pattern", Float) = 0.0
		_SpeedYPatt("Speed Y Pattern", Float) = 0.0
		_SpeedXMask("Speed X Mask", Float) = 0.0
		_SpeedYMask("Speed Y Mask", Float) = 0.0
		_Value("Value",Range(0.0,128.0)) = 1.0

	}
		SubShader
		{
			Tags
		{
			"RenderType" = "Transparent" 
			"Queue" = "Transparent"
		}
			Blend One One
			LOD 100
			Zwrite Off
			Cull off

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
					float4 color : COLOR;
				};

				struct v2f
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float2 uv : TEXCOORD0;
					float2 patternUV : TEXCOORD1;
					float2 maskUV : TEXCOORD2;
				};

				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _Pattern;
				float4 _Pattern_ST;
				sampler2D _Mask;
				float4 _Mask_ST;
				float _Value;

				v2f vert(appdata v)
				{
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					o.patternUV = TRANSFORM_TEX(v.uv, _Pattern);
					o.maskUV = TRANSFORM_TEX(v.uv, _Mask);
					o.color = v.color;
					return o;
				}

				float _SpeedXPatt;
				float _SpeedYPatt;
				float _SpeedXMask;
				float _SpeedYMask;

				fixed4 frag(v2f i) : SV_Target
				{
					float4 col = tex2D(_MainTex, i.uv);
					float2 offsetPattern = frac(_Time.y * float2(_SpeedXPatt, _SpeedYPatt));
					float4 pattern = tex2D(_Pattern, i.patternUV + offsetPattern);
					float2 offsetMask = frac(_Time.y * float2(_SpeedXMask, _SpeedYMask));
					float4 mask = tex2D(_Mask, i.maskUV + offsetMask);

					col.rgb *= i.color.rgb;
					col.a *= i.color.a;

					return col * pattern * mask * _Value * i.color.a;
				}
				ENDCG
			}
		}
}
