Shader "Hidden/ConfigureMaterialPrototype"
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,0)
		_SpecColor("Color", Color) = (0,0,0,0)
		_EmissionColor("Color", Color) = (0,0,0,0)
		_MainTex("Texture", 2D) = "white" {}
		_BumpMap("Normal", 2D) = "white" {}
		_EmissionMap("Emission", 2D) = "white" {}
		_OcclusionMap("Occlusion", 2D) = "white" {}
		_SpecGlossMap("Specular Smoothness", 2D) = "white" {}
	}

		FallBack "Diffuse"

}