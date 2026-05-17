using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DensityGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;
    public ComputeShader densityShader;

    protected List<ComputeBuffer> buffersToRelease;

    public virtual ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 roomBounds, Vector3 roomOrigin, Vector3 centre, Vector3 offset, float spacing)
    {
        int kernel = densityShader.FindKernel("Density");

        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / (float)threadGroupSize);
        // Points buffer is populated inside shader with pos (xyz) + density (w).
        // Set paramaters
        densityShader.SetBuffer(kernel, "points", pointsBuffer);
        densityShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        densityShader.SetFloat("boundsSize", boundsSize);
        densityShader.SetVector("centre", new Vector4(centre.x, centre.y, centre.z));
        densityShader.SetVector("offset", new Vector4(offset.x, offset.y, offset.z));
        densityShader.SetFloat("spacing", spacing);

        Vector3 roomDimensions = roomBounds * boundsSize;
        densityShader.SetVector("roomDimensions", roomDimensions);
        densityShader.SetVector("roomOrigin", roomOrigin);

        // Dispatch shader
        densityShader.Dispatch(kernel, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        if (buffersToRelease != null)
        {
            foreach (var b in buffersToRelease)
            {
                b.Release();
            }
        }

        // Return voxel data buffer so it can be used to generate mesh
        return pointsBuffer;
    }
}