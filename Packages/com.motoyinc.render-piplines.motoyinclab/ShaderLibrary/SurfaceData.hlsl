#ifndef MLABRP_SURFACE_DATA_INCLUDED
#define MLABRP_SURFACE_DATA_INCLUDED

struct SurfaceData
{
    float3 normal;
    float3 color;
    float alpha;
};

#if defined(_GLOBAL_DEBUG)
int _debugFlag;
float4 DebugSurface(SurfaceData surface)
{
    switch (_debugFlag)
    {
    case 0:
        return float4(1, 0, 1, 1);
    case 1:
        return float4(surface.color, 1);
    case 2:
        return float4(surface.alpha, surface.alpha, surface.alpha, surface.alpha);
    case 3:
        return float4(surface.normal, 1);
    case 4:
        // 我们检查normal每个位置的向量大小，会法线长度有问题，这样的normal会导致光照出错，所以需要归一化向量
        return abs(length(surface.normal)-1)*10;
    default:
        return float4(1, 0, 1, 1);
    }
}
#endif

#endif
