using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class NoiseDensity : DensityGenerator
{
    [Header("Noise")]
    public int seed;
    public int numOctaves = 4;          // how many layers of noise you stack.
    public float lacunarity = 2;        // how much the frequency of noise increases each octave.
    public float persistence = .5f;     // how much the amplitude(strength) of noise decreases each octave.
    public float noiseScale = 1;        // overall scale(zoom) of your noise. (Also the frequency after / by 100)
    public float noiseWeight = 1;       // how strongly noise affects your final density.
    public float weightMultiplier = 1;  // used in the fancy weight logic inside the loop.
    public float verticalBiasStrength;  // strength of the vertical split of density

    public Vector4 shaderParams;

    public override ComputeBuffer Generate(ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 roomBounds, Vector3 roomOrigin, Vector3 chunkCentre, Vector3 offset, float spacing)
    {
        int kernel = densityShader.FindKernel("Density");
        buffersToRelease = new List<ComputeBuffer>();

        // Noise parameters
        var prng = new System.Random(seed);
        var offsets = new Vector3[numOctaves];
        float offsetRange = 1000;
        for (int i = 0; i < numOctaves; i++)
        {
            offsets[i] = new Vector3((float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1, (float)prng.NextDouble() * 2 - 1) * offsetRange;
        }

        var offsetsBuffer = new ComputeBuffer(offsets.Length, sizeof(float) * 3); // an array of float3 values, one for each noise octave(more on octaves in a moment).
        offsetsBuffer.SetData(offsets);
        buffersToRelease.Add(offsetsBuffer);

        densityShader.SetVector("chunkCentre", new Vector4(chunkCentre.x, chunkCentre.y, chunkCentre.z));

        // Noise
        densityShader.SetInt("octaves", Mathf.Max(1, numOctaves));
        densityShader.SetFloat("lacunarity", lacunarity);
        densityShader.SetFloat("persistence", persistence);
        densityShader.SetFloat("noiseScale", noiseScale);
        densityShader.SetFloat("noiseWeight", noiseWeight);
        densityShader.SetFloat("weightMultiplier", weightMultiplier);

        densityShader.SetBuffer(kernel, "offsets", offsetsBuffer);

        return base.Generate(pointsBuffer, numPointsPerAxis, boundsSize, roomBounds, roomOrigin, chunkCentre, offset, spacing);
    }
}