// [rebuild] viết lại từ GLSL GLES3 bản gốc, thay DummyShaderTextExporter
Shader "Custom/MagicCube" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = lerp(_Color.rgb, c.rgb, c.a);
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}