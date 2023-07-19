HEADER
{
	CompileTargets = ( IS_SM_50 && ( PC || VULKAN ) );
	Description = "VHS Actual";
}

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    VrForward();                                                    // Indicates this shader will be used for main rendering
    
    ToolsWireframe( "vr_tools_wireframe.vfx" );                     // Allows for mat_wireframe to work
    ToolsShadingComplexity( "vr_tools_shading_complexity.vfx" );     // Shows how expensive drawing is in debug view
    Default();
}

//=========================================================================================================================
COMMON
{
	#include "system.fxc"
    #include "common.fxc"

    #define VertexInput VS_INPUT
    #define PixelInput PS_INPUT

    #define VertexOutput VS_OUTPUT
    #define PixelOutput PS_OUTPUT

    float g_vAberrationAmount < UiGroup("Feature"); UiType(Slider); Default(0.0f); Range(0.0f, 0.01f); >;
    FloatAttribute(g_vAberrationAmount, true);


    float g_vRange < UiGroup("Feature"); UiType(Slider); Default(0.05f); Range(0.0f, 0.1f); >;
    FloatAttribute(g_vRange, true);

    float g_vNoiseQuality < UiGroup("Feature"); UiType(Slider); Default(250.0f); Range(1.0f, 500.0f); >;
    FloatAttribute(g_vNoiseQuality, true);

    float g_vNoiseIntensity < UiGroup("Feature"); UiType(Slider); Default(0.0088f); Range(0.0f, 0.05f); >;
    FloatAttribute(g_vNoiseIntensity, true);

    float g_vOffsetIntensity < UiGroup("Feature"); UiType(Slider); Default(0.02f); Range(0.0f, 0.1f); >;
    FloatAttribute(g_vOffsetIntensity, true);

    float g_vColorOffsetIntensity < UiGroup("Feature"); UiType(Slider); Default(1.3f); Range(0.0f, 1.5f); >;
    FloatAttribute(g_vColorOffsetIntensity, true);
	
	float g_vEdgeFuzz < UiGroup("Feature"); UiType(Slider); Default(0.0f); Range(0.0f, 5.5f); >;
    FloatAttribute( g_vEdgeFuzz, true);


}

//=========================================================================================================================

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;    
};

//=========================================================================================================================

struct PixelInput
{
    float4 vPositionPs : SV_Position;
    float2 vTexCoord : TEXCOORD0;
};

//=========================================================================================================================

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        o.vPositionPs = float4(i.vPositionOs.xyz, 1.0f);
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}

//=========================================================================================================================

PS
{
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

    #include "common/eo_math.shader"

    CreateTexture2D( g_FrameBuffer )< Attribute( "ColorBuffer" );  	SrgbRead( true ); Filter( MIN_MAG_LINEAR_MIP_POINT ); AddressU( MIRROR ); AddressV( MIRROR ); >; 
    struct PixelOutput
    {
        float4 vColor : SV_Target0;
    };

    float verticalBar(float pos, float uvY, float offset)
    {
        float edge0 = (pos - g_vRange);
        float edge1 = (pos + g_vRange);

        float x = pow(smoothstep(edge0, pos, uvY),20.0) * offset;
        x -= pow(smoothstep(pos, edge1, uvY),20.0) * offset;
        return x;
    }
	
	float3 DoBackgroundBlur( float3 color, float2 uv, float2 size )
	{
		float Pi = 6.28318530718; // Pi*2
        float Directions = 16.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
        float Quality = 4.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
        float taps = 1;

        // Blur calculations
        [unroll]
        for( float d=0.0; d<Pi; d+=Pi/Directions)
        {
            [unroll]
            for(float j=1.0/Quality; j<=1.0; j+=1.0/Quality)
            {
                taps += 1;
                color += Tex2D( g_FrameBuffer, uv + float2( cos(d), sin(d) ) * size * j ).rgb;    
            }
        }
        
        // Output to screen
        color /= taps;

        return color;
	}
	
	//random hash
	float4 hash42B(float2 p){
		
		float4 p4 = frac(float4(p.xyxy) * float4(443.8975,397.2973, 491.1871, 470.7827));
		p4 += dot(p4.wzxy, p4+19.19);
		return frac(float4(p4.x * p4.y, p4.x*p4.z, p4.y*p4.w, p4.x*p4.w));
	}
	
	


	float hash( float n ){
		return frac(sin(n)*43758.5453123);
	}

	// 3d noise function (iq's)
	float n( in float3 x ){
		float3 p = floor(x);
		float3 f = frac(x);
		f = f*f*(3.0-2.0*f);
		float n = p.x + p.y*57.0 + 113.0*p.z;
		float res = lerp(lerp(lerp( hash(n+  0.0), hash(n+  1.0),f.x),
							lerp( hash(n+ 57.0), hash(n+ 58.0),f.x),f.y),
						lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
							lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
		return res;
	}
	
	float nn(float2 p){


    float y = p.y;
    float s = 2.;
    
    float v = (n( float3(5, 			1., 1.0) ) + .0);
    //v*= n( float3( (fragCoord.xy + float2(s,0.))*100.,1.0) );
   	v*= hash42B(   float2(p.x +g_flTime*0.01, p.y) ).x +.3 ;

    
    v = pow(v+.3, 1.);
	if(v<.7) v = 0.;  //threshold
    return v;
	}
	

    PixelOutput MainPs( PixelInput i )
    {
        float2 uv = i.vPositionPs.xy * g_vInvGBufferSize.xy;

        for (float i = 0.0; i < 0.3; i += 0.1313)
        {
            float d = (g_flTime * i) % 1.7;
            float o = 1;
            o *= g_vOffsetIntensity;
            uv.x += verticalBar(d, uv.y, o);
        }
        
        float uvY = uv.y;
        uvY *= g_vNoiseQuality;
        uvY = float(int(uvY)) * (1.0 / g_vNoiseQuality);
        float noise = hash12(float2(g_flTime * 0.00001, uvY));
        uv.x += noise * g_vNoiseIntensity;

        float2 offsetR = float2(0.004, 0.0) * g_vColorOffsetIntensity;
        float2 offsetG = float2(0.008 , 0.0) * g_vColorOffsetIntensity;
        
        float r = DoBackgroundBlur(float3(0.0,0.0,0.0), uv + offsetR, float2(g_vEdgeFuzz/1000.0,g_vEdgeFuzz/1000.0) ).r;
        float g = DoBackgroundBlur(float3(0.0,0.0,0.0), uv + offsetG, float2(g_vEdgeFuzz/1000.0,g_vEdgeFuzz/1000.0) ).g;
        float b = Tex2D(g_FrameBuffer, uv).b;
		
		
		float3 f = Tex2D( g_FrameBuffer, uv).rgb;
		
        float4 tex = float4(r, g, b, 1.0);
        PixelOutput o;
        o.vColor.rgba = tex;

        /*

        float2 t = uv * 100 + g_flTime;
        float a = noise4(t);
		
		

        PixelOutput o;
        o.vColor.rgb = 1 - Tex2D(g_FrameBuffer, uv.xy).rgb; // invert screen color
        float amountS = g_vAberrationAmount * ((sin(g_flTime * 2) + 1) / 2);
        float amount = amountS * a;
        float colr = Tex2D(g_FrameBuffer, float2(uv.x - amount, uv.y - amount)).r;
        float colg = Tex2D(g_FrameBuffer, float2(uv.x + amount, uv.y + amount)).g;
        float colb = Tex2D(g_FrameBuffer, uv.xy).b;
        o.vColor.a = 1.0f;

        o.vColor.rgb = float3(colr, colg, colb);
        */


        return o;
    }
}