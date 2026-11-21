// [rebuild] viết lại từ GLSL GLES3 bản gốc, thay DummyShaderTextExporter
Shader "Hovl/Particles/Add_CenterGlow" {
	Properties {
		_MainTex ("MainTex", 2D) = "white" {}
		_Noise ("Noise", 2D) = "white" {}
		_Flow ("Flow", 2D) = "white" {}
		_Mask ("Mask", 2D) = "white" {}
		_SpeedMainTexUVNoiseZW ("Speed MainTex U/V + Noise Z/W", Vector) = (0,0,0,0)
		_DistortionSpeedXYPowerZ ("Distortion Speed XY Power Z", Vector) = (0,0,0,0)
		_Emission ("Emission", Float) = 2
		_Color ("Color", Vector) = (0.5,0.5,0.5,1)
		[Toggle] _Usecenterglow ("Use center glow?", Float) = 0
		[MaterialToggle] _Usedepth ("Use depth?", Float) = 0
		_Depthpower ("Depth power", Float) = 1
		[Enum(Cull Off,0, Cull Front,1, Cull Back,2)] _CullMode ("Culling", Float) = 0
		[Enum(One,1,OneMinuSrcAlpha,6)] _Blend2 ("Blend mode subset", Float) = 1
		[HideInInspector] _texcoord ("", 2D) = "white" {}
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
		}
		Pass {
			Blend One [_Blend2]
			Cull [_CullMode]
			ZWrite Off
			ZTest LEqual
			ColorMask RGB

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile _ SOFTPARTICLES_ON

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _Noise;
			float4 _Noise_ST;
			sampler2D _Flow;
			float4 _Flow_ST;
			sampler2D _Mask;
			float4 _Mask_ST;

			float4 _SpeedMainTexUVNoiseZW;
			float4 _DistortionSpeedXYPowerZ;
			float _Emission;
			float4 _Color;
			float _Usecenterglow;
			float _Usedepth;
			float _Depthpower;

			#ifdef SOFTPARTICLES_ON
			sampler2D_float _CameraDepthTexture;
			#endif

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD1;
				#endif
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_t v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				#ifdef SOFTPARTICLES_ON
				o.projPos = ComputeScreenPos(o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				#endif
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
				float partZ = i.projPos.z;
				float fade = saturate((sceneZ - partZ) / _Depthpower);
				fade = lerp(1.0, fade, _Usedepth);
				float colorAlpha = i.color.a * fade;
				#else
				float colorAlpha = i.color.a;
				#endif

				float2 mainUV = i.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw + _Time.yy * _SpeedMainTexUVNoiseZW.xy;
				float2 flowUV = i.texcoord.xy * _Flow_ST.xy + _Flow_ST.zw + _Time.yy * _DistortionSpeedXYPowerZ.xy;
				float2 flowTex = tex2D(_Flow, flowUV).xy;

				float2 maskUV = i.texcoord.xy * _Mask_ST.xy + _Mask_ST.zw;
				float4 maskTex = tex2D(_Mask, maskUV);

				float2 distort = flowTex * maskTex.xy;
				mainUV = mainUV - distort * _DistortionSpeedXYPowerZ.zz;
				float4 mainTex = tex2D(_MainTex, mainUV);

				float2 noiseUV = i.texcoord.xy * _Noise_ST.xy + _Noise_ST.zw + _Time.yy * _SpeedMainTexUVNoiseZW.zw;
				float4 noiseTex = tex2D(_Noise, noiseUV);

				float4 col = mainTex * noiseTex * _Color * float4(i.color.rgb, colorAlpha);
				float alphaFactor = mainTex.a * noiseTex.a * _Color.a * colorAlpha;
				col = col * alphaFactor;

				float4 centerGlow = saturate(maskTex * saturate(maskTex - (1.0 - i.texcoord.z)));
				col = lerp(col, col * centerGlow, _Usecenterglow);
				col *= _Emission;

				return col;
			}
			ENDCG
		}
	}
}