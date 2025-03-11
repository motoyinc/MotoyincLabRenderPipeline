#ifndef MLABRP_SHADOW_PCSS_INCLUDED
#define MLABRP_SHADOW_PCSS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"


#define DISK_SAMPLE_COUNT 64
static const float2 fibonacciSpiralDirection[DISK_SAMPLE_COUNT] =
{
    float2 (1, 0),
    float2 (-0.7373688780783197, 0.6754902942615238),
    float2 (0.08742572471695988, -0.9961710408648278),
    float2 (0.6084388609788625, 0.793600751291696),
    float2 (-0.9847134853154288, -0.174181950379311),
    float2 (0.8437552948123969, -0.5367280526263233),
    float2 (-0.25960430490148884, 0.9657150743757782),
    float2 (-0.46090702471337114, -0.8874484292452536),
    float2 (0.9393212963241182, 0.3430386308741014),
    float2 (-0.924345556137805, 0.3815564084749356),
    float2 (0.423845995047909, -0.9057342725556143),
    float2 (0.29928386444487326, 0.9541641203078969),
    float2 (-0.8652112097532296, -0.501407581232427),
    float2 (0.9766757736281757, -0.21471942904125949),
    float2 (-0.5751294291397363, 0.8180624302199686),
    float2 (-0.12851068979899202, -0.9917081236973847),
    float2 (0.764648995456044, 0.6444469828838233),
    float2 (-0.9991460540072823, 0.04131782619737919),
    float2 (0.7088294143034162, -0.7053799411794157),
    float2 (-0.04619144594036213, 0.9989326054954552),
    float2 (-0.6407091449636957, -0.7677836880006569),
    float2 (0.9910694127331615, 0.1333469877603031),
    float2 (-0.8208583369658855, 0.5711318504807807),
    float2 (0.21948136924637865, -0.9756166914079191),
    float2 (0.4971808749652937, 0.8676469198750981),
    float2 (-0.952692777196691, -0.30393498034490235),
    float2 (0.9077911335843911, -0.4194225289437443),
    float2 (-0.38606108220444624, 0.9224732195609431),
    float2 (-0.338452279474802, -0.9409835569861519),
    float2 (0.8851894374032159, 0.4652307598491077),
    float2 (-0.9669700052147743, 0.25489019011123065),
    float2 (0.5408377383579945, -0.8411269468800827),
    float2 (0.16937617250387435, 0.9855514761735877),
    float2 (-0.7906231749427578, -0.6123030256690173),
    float2 (0.9965856744766464, -0.08256508601054027),
    float2 (-0.6790793464527829, 0.7340648753490806),
    float2 (0.0048782771634473775, -0.9999881011351668),
    float2 (0.6718851669348499, 0.7406553331023337),
    float2 (-0.9957327006438772, -0.09228428288961682),
    float2 (0.7965594417444921, -0.6045602168251754),
    float2 (-0.17898358311978044, 0.9838520605119474),
    float2 (-0.5326055939855515, -0.8463635632843003),
    float2 (0.9644371617105072, 0.26431224169867934),
    float2 (-0.8896863018294744, 0.4565723210368687),
    float2 (0.34761681873279826, -0.9376366819478048),
    float2 (0.3770426545691533, 0.9261958953890079),
    float2 (-0.9036558571074695, -0.4282593745796637),
    float2 (0.9556127564793071, -0.2946256262683552),
    float2 (-0.50562235513749, 0.8627549095688868),
    float2 (-0.2099523790012021, -0.9777116131824024),
    float2 (0.8152470554454873, 0.5791133210240138),
    float2 (-0.9923232342597708, 0.12367133357503751),
    float2 (0.6481694844288681, -0.7614961060013474),
    float2 (0.036443223183926, 0.9993357251114194),
    float2 (-0.7019136816142636, -0.7122620188966349),
    float2 (0.998695384655528, 0.05106396643179117),
    float2 (-0.7709001090366207, 0.6369560596205411),
    float2 (0.13818011236605823, -0.9904071165669719),
    float2 (0.5671206801804437, 0.8236347091470047),
    float2 (-0.9745343917253847, -0.22423808629319533),
    float2 (0.8700619819701214, -0.49294233692210304),
    float2 (-0.30857886328244405, 0.9511987621603146),
    float2 (-0.4149890815356195, -0.9098263912451776),
    float2 (0.9205789302157817, 0.3905565685566777)
};

real2 ComputeFibonacciSpiralDiskSampleClumped_Directional(const in int sampleIndex, const in real sampleCountInverse, const in real clumpExponent, out real sampleDistNorm)
{
    sampleDistNorm = (real)sampleIndex * sampleCountInverse;
    sampleDistNorm = PositivePow(sampleDistNorm, clumpExponent);
    return fibonacciSpiralDirection[sampleIndex] * sampleDistNorm;
}


struct MainLightPCSSData
{
    // shadowFilterParams0
    float blockerSearchRadiusWS;
    int blockerSampleCount;
    int filterSampleCount;
    float blockerSamplingClumpExponent;

    float angularDiameter;
    
    // shadowBias
    float depthBias;
    float normalBias;

    // shadowmapSize
    float2 texelSize;
    float2 shadowmapSize;
    
    float2 shadowmapInAtlasScale;
};



real2 ComputeFibonacciSpiralDiskSampleUniform_Directional(const in int sampleIndex, const in real sampleCountInverse, const in real sampleBias, out real sampleDistNorm)
{
    // Samples biased away from the center, so that sample 0 doesn't fall at (0, 0), or it will not be affected by sample jitter and create a visible edge.
    sampleDistNorm = (real)sampleIndex * sampleCountInverse + sampleBias;

    // sqrt results in uniform distribution
    sampleDistNorm = sqrt(sampleDistNorm);

    return fibonacciSpiralDirection[sampleIndex] * sampleDistNorm;
}



bool Search_PCSS_Blocker(inout real averageBlocker,
    float2 shadowmapInAtlasScale,float2 shadowmapInAtlasOffset,
    TEXTURE2D_PARAM(ShadowMap, sampler_ShadowMap),float2 shadowCoord, float receiverDepth,
    float2 searchRadius,float sampleCount,float2 sampleJitter,float clumpExponent)
{
    float depthSum = 0.0;
    float depthCount = 0.0;
    float2 sampleCoord = float2(0.0, 0.0);
    float sampleDepth = 0.0;
    real sampleCountInverse = rcp((real)sampleCount);

    float2 minCoord = shadowmapInAtlasOffset;
    float2 maxCoord = shadowmapInAtlasOffset + shadowmapInAtlasScale;

    for(int i = 0.0; i < sampleCount; ++i)
    {
        real sampleDistNorm;
        real2 offset = 0.0f;
        offset = ComputeFibonacciSpiralDiskSampleClumped_Directional(i, sampleCountInverse, clumpExponent, sampleDistNorm);
        offset = real2(offset.x *  sampleJitter.y + offset.y * sampleJitter.x,
                       offset.x * -sampleJitter.x + offset.y * sampleJitter.y);
        
        sampleCoord = shadowCoord + offset * searchRadius;
        if(!any(sampleCoord < minCoord) || any(sampleCoord > maxCoord))
        {
            sampleDepth = SAMPLE_TEXTURE2D(ShadowMap, sampler_ShadowMap, sampleCoord).r;
        }

        if(sampleDepth > receiverDepth)
        {
            depthSum += sampleDepth;
            ++depthCount;
        }
    }
    if(depthCount > FLT_EPS)
    {
        averageBlocker = depthSum / depthCount;
        return true;
    }
    else
    {
        averageBlocker = 0;
        return false;
    }
    // return depthCount > FLT_EPS ? (depthSum / depthCount) : 0;
}


float Estimate_PCSS_Penumbra(float receiverDepth, float avgBlockerDepth, float lightRadius, float shadowMapSize)
{
    return shadowMapSize * (receiverDepth - avgBlockerDepth) * lightRadius / avgBlockerDepth;
}


float Compute_PCSS_PCF(
    float2 shadowmapInAtlasScale,float2 shadowmapInAtlasOffset,
    TEXTURE2D_SHADOW_PARAM(ShadowMap, sampler_ShadowMap), float3 shadowCoord,
    float filterRadius,float sampleCount,float2 sampleJitter,float clumpExponent
    )
{
    float shadowAttenuationSum = 0.0;
    float3 sampleCoord = float3(0.0, 0.0, shadowCoord.z);
    real sampleCountInverse = rcp((real)sampleCount);
    float2 minCoord = shadowmapInAtlasOffset;
    float2 maxCoord = shadowmapInAtlasOffset + shadowmapInAtlasScale;

    
    for(int i = 0; i < sampleCount; ++i)
    {
        real sampleDistNorm;
        real2 offset = 0.0f;
        offset = ComputeFibonacciSpiralDiskSampleClumped_Directional(i, sampleCountInverse, clumpExponent, sampleDistNorm);
        offset = real2(offset.x *  sampleJitter.y + offset.y * sampleJitter.x,
                       offset.x * -sampleJitter.x + offset.y * sampleJitter.y);
        sampleCoord.xy = shadowCoord.xy + offset * filterRadius;
        if(!any(sampleCoord < minCoord) || any(sampleCoord > maxCoord))
        {
            shadowAttenuationSum += SAMPLE_TEXTURE2D_SHADOW(ShadowMap, sampler_ShadowMap, sampleCoord.xyz);
        }
    }

    return shadowAttenuationSum / (float)sampleCount;
}

#endif