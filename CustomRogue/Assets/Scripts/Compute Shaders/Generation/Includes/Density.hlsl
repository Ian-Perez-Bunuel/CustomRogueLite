static const int numThreads = 8;

RWStructuredBuffer<float4> points;
int numPointsPerAxis;
float boundsSize;
float3 chunkCentre;
float3 roomDimensions;
float3 offset;
float spacing;

int indexFromCoord(uint x, uint y, uint z)
{
    return z * numPointsPerAxis * numPointsPerAxis + y * numPointsPerAxis + x;
}