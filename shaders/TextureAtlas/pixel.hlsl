#ifndef COMMON_PIXEL_H
#define COMMON_PIXEL_H


//Combos -------------------------------------------------------------------------------------------------------------------------------------------------
StaticCombo( S_BAKED_SELF_ILLUM, F_BAKED_SELF_ILLUM, Sys( ALL ) );
StaticCombo( S_BAKED_EMISSIVE, F_BAKED_EMISSIVE, Sys( ALL ) );

//Includes -----------------------------------------------------------------------------------------------------------------------------------------------
//Custom TextureFiltering. Set to NEAREST Currently. maybe there is a combination of Filter that you don't need to Pad the Original UV idk.
SamplerState TextureFiltering < Filter( POINT ); MaxAniso( 8 ); >;

//Define Alpha Test if not defined yet
#ifndef S_ALPHA_TEST
    #define S_ALPHA_TEST 0
#endif
//Define TRANSLUCENT if not defined yet
#ifndef S_TRANSLUCENT
    #define S_TRANSLUCENT 0
#endif

//Include the Minimal Variant for Materials.
#include "common/pixel.material.minimal.hlsl"
//Include our Custom Material Inputs. you can remove and replace stuff inside it. or Rename them to fit this shader.
#include "TextureAtlas/material.input.hlsl"


//Include Secret sauce. idk whats inside of this... but it probably includes all the Secret Methods and such. like MaterialToCombinerInput.
#include "sbox_pixel.fxc"

//-----------------------------------------------------------------------------
//
// Compose the final color with lighting from material parameters
//
//-----------------------------------------------------------------------------

//Basically the same as the original just without Reflection Support. Replace this with the Original again if you want those.
PixelOutput FinalizePixelMaterial( PixelInput i, Material m )
{
    CombinerInput o = MaterialToCombinerInput( i, m );
    
    return FinalizePixel( o );
}


#endif // COMMON_PIXEL_H