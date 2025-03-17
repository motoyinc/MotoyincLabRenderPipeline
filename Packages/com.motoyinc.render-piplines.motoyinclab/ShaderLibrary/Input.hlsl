#ifndef MLABRP_INPUT_INCLUDED
#define MLABRP_INPUT_INCLUDED

struct InputData
{
    float4  positionCS;
    float3  positionWS;
    float3  normalWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    half4   shadowMask;
    float2  positionSS;
    float3  bakedGI;
};

#endif