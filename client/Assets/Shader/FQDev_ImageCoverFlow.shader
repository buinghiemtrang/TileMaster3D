// [rebuild] viết lại từ GLSL GLES3 bản gốc, thay DummyShaderTextExporter
Shader "FQDev/ImageCoverFlow" {
	Properties {
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Vector) = (1,1,1,1)
		_GlowMask ("Glow Mask", 2D) = "black" {}
		_GlowColor ("Glow Color", Vector) = (1,1,1,1)
		_GlowParams ("Glow Params (Width X, Width Y, Speed X, Speed Y", Vector) = (100,100,1,1)
		_GlowGap ("Glow Gap (Gap X, Gap Y)", Vector) = (2,2,0,0)
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255
		_ColorMask ("Color Mask", Float) = 15
		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil {
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend One OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass {
			Name "Default"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile _ UNITY_UI_CLIP_RECT
			#pragma multi_compile _ UNITY_UI_ALPHACLIP

			struct appdata_t {
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float2 glowUV   : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			sampler2D _GlowMask;
			float4 _GlowMask_ST;
			fixed4 _GlowColor;
			float4 _GlowParams;
			float2 _GlowGap;

			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

			v2f vert(appdata_t v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.worldPosition = v.vertex;
				o.vertex = UnityObjectToClipPos(o.worldPosition);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;

				float2 glowCoord = (v.texcoord + _Time.y * _GlowParams.zw) / _GlowParams.xy;
				o.glowUV = glowCoord * _GlowMask_ST.xy + _GlowMask_ST.zw;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				float2 glowFloor = floor(i.glowUV);
				float2 glowGap = max(_GlowGap.xy, float2(1.0, 1.0));
				float2 denom = (glowFloor * glowGap >= 0.0) ? glowGap : -glowGap;
				float2 rem = denom * frac(glowFloor / denom);
				float2 check = 1.0 - ceil((rem / glowGap) * (rem / glowGap) - 0.001);
				float glowActive = check.x * check.y;

				fixed4 glowTex = tex2D(_GlowMask, i.glowUV) * _GlowColor;
				float glowStrength = glowActive * glowTex.a;

				fixed4 color = (tex2D(_MainTex, i.texcoord) + _TextureSampleAdd) * i.color;
				color.rgb += glowTex.rgb * glowStrength;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				color.rgb *= color.a;
				return color;
			}
			ENDCG
		}
	}
}