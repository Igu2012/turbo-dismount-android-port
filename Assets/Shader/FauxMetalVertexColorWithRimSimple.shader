Shader "Test/FauxMetalVertexColorWithRimSimple" {
Properties {
 _DiffuseBleed ("Diffuse Bleed", Range(0,1)) = 0.15
 _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,1)
 _RimPower ("Rim Power", Range(0,18)) = 1.5
}
	//DummyShaderTextExporter
	
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Lambert
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutput o)
		{
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
}