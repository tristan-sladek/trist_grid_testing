#ifndef COMMON_PIXEL_MATERIAL_INPUTS_H
#define COMMON_PIXEL_MATERIAL_INPUTS_H


//
//
// ----WARNING----
// I Commented out a lot of stuff here to speed up compile times. just uncomment the stuff you need.
// you can rename stuff here. but be sure to rename all occurances.
//
//

//-----------------------------------------------------------------------------
//
// Material
//
//-----------------------------------------------------------------------------
CreateInputTexture2D( TextureColor,            Srgb,   8, "",                 "_color",  "Material,10/10", Default3( 1.0, 1.0, 1.0 ) );
CreateInputTexture2D( TextureNormal,           Linear, 8, "NormalizeNormals", "_normal", "Material,10/20", Default3( 0.5, 0.5, 1.0 ) );
CreateInputTexture2D( TextureRoughness,        Linear, 8, "",                 "_rough",  "Material,10/30", Default( 0.5 ) );
CreateInputTexture2D( TextureMetalness,        Linear, 8, "",                 "_metal",  "Material,10/40", Default( 1.0 ) );
CreateInputTexture2D( TextureAmbientOcclusion, Linear, 8, "",                 "_ao",     "Material,10/50", Default( 1.0 ) );
CreateInputTexture2D( TextureBlendMask,        Linear, 8, "",                 "_blend",  "Material,10/60", Default( 1.0 ) );

// We internally encode the opacity and blend mask differently
//#if ( S_ALPHA_TEST || S_TRANSLUCENT )
//    CreateInputTexture2D( TextureTranslucency, Linear, 8, "",                 "_trans",  "Material,10/70", Default3( 1.0, 1.0, 1.0 ) );
//    #define COLOR_TEXTURE_CHANNELS Channel( RGB, AlphaWeighted( TextureColor, TextureTranslucency ), Srgb ); Channel( A, Box( TextureTranslucency ), Linear )
//#else
    CreateInputTexture2D( TextureTintMask,     Linear, 8, "",                 "_tint",   "Material,10/70", Default( 1.0 ) );
    #define COLOR_TEXTURE_CHANNELS Channel( RGB,  Box( TextureColor ), Srgb ); Channel( A, Box( TextureTintMask ), Linear )
//#endif

//#if( S_BAKED_SELF_ILLUM )
//    CreateInputTexture2D( TextureSelfIllum, Linear, 8, "", "_illum", "Material,10/80", Default3( 1.0, 1.0, 1.0 ) );
//    CreateTexture2D( g_tSelfIllum ) < Channel( RGB, Box( TextureSelfIllum ), Linear ); OutputFormat( DXT1 ); SrgbRead( false ); >;
//#endif

//#if ( S_SELF_ILLUM )
//	CreateInputTexture2D( TextureSelfIllumMask, Srgb, 8, "", "_selfillum", "Self Illum,60/10", Default3( 0.0, 0.0, 0.0 ) );
//	CreateTexture2DWithoutSampler( g_tSelfIllumMask ) < Channel( RGB, Box( TextureSelfIllumMask ), Srgb ); OutputFormat( DXT1 ); SrgbRead( true ); >;
//#endif

float3 g_flTintColor < UiType( Color ); Default3( 1.0, 1.0, 1.0 ); UiGroup( "Material,10/90" ); >;
float g_flSelfIllumScale < Default( 1.0 ); Range( 0.0, 16.0 ); UiGroup( "Self Illum" ); >;
float g_flEmissionScale< Default( 1.0 ); Range( 0.0, 16.0 ); UiGroup( "Self Illum" ); >;

//CreateTexture2DWithoutSampler( g_tColor )  < COLOR_TEXTURE_CHANNELS; AddressU(CLAMP); AddressV(CLAMP); OutputFormat( BC7 ); SrgbRead( true ); >;
//CreateTexture2DWithoutSampler( g_tNormal ) < Channel( RGBA, Box( TextureNormal ), Linear );AddressU(CLAMP); AddressV(CLAMP); OutputFormat( BC7 ); SrgbRead( false ); >;
//CreateTexture2DWithoutSampler( g_tRma )    < Channel( R,    Box( TextureRoughness ), Linear );AddressU(CLAMP); AddressV(CLAMP); Channel( G, Box( TextureMetalness ), Linear ); Channel( B, Box( TextureAmbientOcclusion ), Linear );  Channel( A, Box( TextureBlendMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;
CreateTexture2D( g_tColor )  < Filter( POINT ); COLOR_TEXTURE_CHANNELS; AddressU(CLAMP); AddressV(CLAMP); OutputFormat( BC7 ); SrgbRead( true ); >;
CreateTexture2D( g_tNormal ) < Channel( RGBA, Box( TextureNormal ), Linear );AddressU(CLAMP); AddressV(CLAMP); OutputFormat( BC7 ); SrgbRead( false ); >;
CreateTexture2D( g_tRma )    < Channel( R,    Box( TextureRoughness ), Linear );AddressU(CLAMP); AddressV(CLAMP); Channel( G, Box( TextureMetalness ), Linear ); Channel( B, Box( TextureAmbientOcclusion ), Linear );  Channel( A, Box( TextureBlendMask ), Linear ); OutputFormat( BC7 ); SrgbRead( false ); >;



BoolAttribute( SupportsLightmapping, ( F_MORPH_SUPPORTED ) ? false : true );
BoolAttribute( PerVertexLighting, ( F_MORPH_SUPPORTED ) ? false : true );

//#if( S_BAKED_SELF_ILLUM )
//    TextureAttribute( LightSim_SelfIllumTexture, g_tSelfIllum );
//    FloatAttribute( LightSim_SelfIllumScale, g_flSelfIllumScale );
//#endif

#endif //COMMON_PIXEL_MATERIAL_INPUTS_H