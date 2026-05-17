using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class BaseRoomGenerator : MonoBehaviour
{
    [SerializeField] MarchingCubesCompute world;

    [Header("Generation Data")]
    public DensityGenerator densityGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void AddNoiseToRoom(List<Chunk> roomChunks, Vector3Int roomDimensions, Vector3 roomOrigin)
    {
        foreach (Chunk c in roomChunks)
        {
            AddNoiseToChunk(c, roomDimensions, roomOrigin);
        }
    }

    void AddNoiseToChunk(Chunk chunk, Vector3Int roomDimensions, Vector3 roomOrigin)
    {
        Vector3Int coord = chunk.GetCoords();
        Vector3 centre = world.CentreFromCoord(coord);

        // Put in build chunk
        float pointSpacing = world.worldSettings.boundsSize / (world.worldSettings.numPointsPerAxis - 1);

        densityGenerator.Generate(chunk.pointsBuffer, world.worldSettings.numPointsPerAxis, world.worldSettings.boundsSize, roomDimensions, roomOrigin, centre, world.worldSettings.offset, pointSpacing);

        chunk.valuesChanged = true;
    }
}
