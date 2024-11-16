using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule;
namespace UnityEngine.Rendering.MotoyincLab
{
    public class RenderingUtils
    {
        internal static bool IsMRT(RTHandle[] colorBuffers)
        {
            return GetValidColorBufferCount(colorBuffers) > 1;
        }
        
        internal static uint GetValidColorBufferCount(RTHandle[] colorBuffers)
        {
            uint nonNullColorBuffers = 0;
            if (colorBuffers != null)
            {
                foreach (var identifier in colorBuffers)
                {
                    if (identifier != null && identifier.nameID != 0)
                        ++nonNullColorBuffers;
                }
            }
            return nonNullColorBuffers;
        }
        
        
    }
}