Shader "UI/RoundedCorners/RoundedCorners" {
    Properties {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}

        // --- Mask support ---
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        // ---
    }
    
    SubShader {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
        }

        // --- Mask support ---
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        Lighting Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]
        // ---
        
		// standared blanding
        Blend SrcAlpha OneMinusSrcAlpha
		// no zwrite
        ZWrite Off 

        Pass {
            CGPROGRAM
            
			// include unity and other functions
            #include "UnityCG.cginc"
            #include "SDFUtils.cginc"
            #include "ShaderSetup.cginc"
            
            #pragma vertex vert
            #pragma fragment frag
            
			// the main texture
            sampler2D _MainTex;

			// the fragment part of the shader
            fixed4 frag (v2f i) : SV_Target {
                float alpha = CalcAlpha(i.uv, i.uv1, i.uv2.x);
                return mixAlpha(tex2D(_MainTex, i.uv), i.color, alpha);
            }
            
            ENDCG
        }
    }
}
