﻿#ifndef MLABRP_LIGHTING_INCLUDED
#define MLABRP_LIGHTING_INCLUDED

// 使用时请直接注释掉：
//      仅用于编辑时不报错，防止嵌套式依赖关系，使代码变得难以维护
//      但也意味着，使用该库，必须包含以下依赖库
#include "SurfaceData.hlsl"
#include "Light.hlsl"

float3 IncomingLight (SurfaceData surface, Light light) {
    float3 diffuse = dot(surface.normal, light.direction);
    diffuse = max(0.0, diffuse);  // 0 ~ ∞
    //diffuse = saturate(diffuse);  // 0 ~ 1
    return diffuse * light.color;
}

float3 GetLighting(SurfaceData surface, Light light)
{
    return IncomingLight(surface, light) * surface.color;
}

float3 GetLighting (SurfaceData surface) {
    Light light = GetDirectionalLight();
    return GetLighting(surface, light);
}

#endif