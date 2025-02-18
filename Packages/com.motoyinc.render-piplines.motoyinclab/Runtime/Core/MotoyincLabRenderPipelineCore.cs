﻿using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
namespace UnityEngine.Rendering.MotoyincLab
{
    static class NativeArrayExtensions
    {
        /// <summary>
        /// IMPORTANT: Make sure you do not write to the value! There are no checks for this!
        /// 重要：请不要往里面写入值，该方法没有做安全检查。
        /// </summary>
        public static unsafe ref T UnsafeElementAt<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
        }

        public static unsafe ref T UnsafeElementAtMutable<T>(this NativeArray<T> array, int index) where T : struct
        {
            return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
        }
    }

    

    internal enum MLRPProfileId
    {
        MotoyincLabRenderTotal,
        RenderCameraStack,
        MainLightShadow
    }
    
}