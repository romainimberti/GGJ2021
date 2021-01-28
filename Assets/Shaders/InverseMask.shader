Shader "Unlit/InverseMask"
{
    Properties
    {
		// Texture given by the image
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		// color given by the image
		_Color("Tint", Color) = (1,1,1,1)
		// the mask image
		_MaskTex("Texture", 2D) = "white" {}
		// the mask uv
		_MaskUv("TextureUv", Vector) = (0,0,1,1)
    }
    SubShader
    { 
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "false"
		}

		// turns off culling 
		Cull Off
		// turns off lighting
		Lighting Off
		// turns off depth writing
		ZWrite Off
		// blends with normal transparency
		Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

			// app to vertex data
            struct appdata
            {
                float4 vertex : POSITION;
				float4 color: COLOR;
                float2 uv : TEXCOORD0;
            };
			
			// vertex to fragment data
            struct v2f
            {
                float2 uv : TEXCOORD0;
				float4 color: COLOR;
                float4 vertex : SV_POSITION;
            };

			// main texture
            sampler2D _MainTex;
			// mask texture
			sampler2D _MaskTex;
			// transform of the main texture
            float4 _MainTex_ST;
			// color of the object 
			float4 _Color;
			// uv's of the mask
			float4 _MaskUv;

			// the vertex shader 
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
                return o;
            }

			// the fragment shader
            fixed4 frag (v2f i) : SV_Target
            {
                // sample the textures and save the alpha
                fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 alphaImage = 1 - tex2D(_MaskTex, _MaskUv.xy + (i.uv * (_MaskUv.zw - _MaskUv.xy)));
				fixed alpha = col.a;

				//calculate final color
				col *= i.color;
				col.a = alpha * alphaImage.a * i.color.a;

                return col;
            }

            ENDCG
        }
    }
}
