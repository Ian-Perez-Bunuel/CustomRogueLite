using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public List<Chunk> chunks;
    public Vector3Int dimensions;
    public Vector3 min;
    public Vector3 max;

    public Room(Vector3Int d)
    {
        dimensions = d;
        chunks = new List<Chunk>();
        min = Vector3.zero;
        max = Vector3.zero;
    }

    public void AddChunk(Chunk chunk)
    {
        chunks.Add(chunk);
    }
}
