Shader "Dismount/BumperCarPole" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
 _Radius ("Radius", Float) = 0.1
 _Node0 ("Node 0", Vector) = (0,0,0,0)
 _Node1 ("Node 1", Vector) = (0,1,0,0)
 _Node2 ("Node 2", Vector) = (0,2,-0.5,0)
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