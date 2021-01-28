//the application data
struct appdata {
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
    float4 color : COLOR;  // set from Image component property
};

// from vertext to fragment data
struct v2f {
    float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
    float4 vertex : SV_POSITION;
    float4 color : COLOR;
};

// the vertex part
v2f vert (appdata v) {
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    o.uv = v.uv;
	o.uv1 = v.uv1;
	o.uv2 = v.uv2;
    o.color = v.color;
    return o;
}

// mixes the alpha
inline fixed4 mixAlpha(fixed4 mainTexColor, fixed4 color, float sdfAlpha){
    fixed4 col = mainTexColor * color;
    col.a = min(col.a, sdfAlpha);
    return col;
}