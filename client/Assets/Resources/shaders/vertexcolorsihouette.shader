// [rebuild] viết lại từ GLSL GLES3 bản gốc, thay DummyShaderTextExporter
Shader "FQDev/VertexColorSihouette" {
	Properties {
		_BaseColor ("Base Color", Vector) = (1,1,1,1)
		_RimColor ("Rim Color", Vector) = (0,0,0,0)
		_RimPower ("Rim Power", Range(0.1, 8)) = 4
	}
	SubShader {
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass {
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			fixed4 _BaseColor;
			fixed4 _RimColor;
			float _RimPower;

			struct appdata {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				fixed4 color : COLOR;
			};

			struct v2f {
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				fixed4 color : TEXCOORD2;
				float3 ambient : TEXCOORD3;
			};

			v2f vert(appdata v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.color = v.color;
				o.ambient = ShadeSH9(float4(o.worldPos, 1.0));
				return o;
			}

			fixed4 frag(v2f i) : SV_Target {
				half3 worldNormal = normalize(i.worldNormal);
				half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
				half rim = pow(1.0 - saturate(dot(worldNormal, viewDir)), _RimPower);
				half3 rimCol = rim * _RimColor.rgb * _RimColor.a;

				half3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				half NdotL = saturate(dot(worldNormal, lightDir));
				half3 diffAmb = _LightColor0.rgb * NdotL + i.ambient;

				fixed4 col = i.color * _BaseColor;
				col.rgb = col.rgb * diffAmb + rimCol;

				return col;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}