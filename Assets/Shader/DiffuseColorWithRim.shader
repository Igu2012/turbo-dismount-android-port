Shader "Dismount/DiffuseColorWithRim" {
Properties {
 _MainColor ("Main Color", Color) = (1,1,1,1)
 _RimColor ("Rim Color", Color) = (0.5,0.5,0.5,1)
 _RimPower ("Rim Power", Range(0,18)) = 3
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