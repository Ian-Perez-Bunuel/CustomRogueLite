using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public struct Room
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

    [SerializeField] ComputeShader clearRoomCompute;
    [SerializeField] MarchingCubesCompute world;
    [SerializeField] BaseRoomGenerator roomNoiseGenerator;
    [SerializeField] Transform playerTransform;

    static Vector3 chunkDimensions;

    [SerializeField] int amountOfRooms;

    List<Room> allRooms;


    enum Direction
    {
        Left,
        Right,
        Front,
        Back,
        Top,
        Bottom,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chunkDimensions = world.GetChunkDimensions();
        allRooms = new List<Room>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            int roomsMade = 0;
            while (roomsMade < amountOfRooms)
            {
                Debug.Log("ROOM MADE");
                Vector3Int randDimension = new Vector3Int(
                    Random.Range(1, 4),
                    Random.Range(1, 4),
                    Random.Range(1, 4));

                CreateRoom(randDimension);

                roomsMade++;
            }
        }
    }

    private void CreateRoom(Vector3Int roomDimensions)
    {
        Room room = new Room(roomDimensions);

        // Starting chunk is the min chunk
        Vector3 startingChunkPos = GetRandomWorldPos();
        Chunk startingChunk = world.GetChunkFromWorldPos(startingChunkPos);
        SetRoomMinMax(ref room, startingChunk);

        if (startingChunk == null)
        {
            Debug.LogError("CHUNK NOT FOUND: The start chunk isn't within the world");
            return;
        }

        // End chunk is the max chunk
        Vector3 endChunkPos = new Vector3(
            startingChunkPos.x + (chunkDimensions.x * roomDimensions.x),
            startingChunkPos.y + (chunkDimensions.y * roomDimensions.y),
            startingChunkPos.z + (chunkDimensions.z * roomDimensions.z));

        Chunk endChunk = world.GetChunkFromWorldPos(endChunkPos);
        if (endChunk == null)
        {
            Debug.LogError("CHUNK NOT FOUND: The end chunk isn't within the world");
            return;
        }

        // Clear all chunks
        for (int x = 0; x < roomDimensions.x; x++)
        {
            for (int y = 0; y < roomDimensions.y; y++)
            {
                for (int z = 0; z < roomDimensions.z; z++)
                {
                    // Clear first
                    ClearOpenAreas();

                    Vector3 chunkPos = new Vector3(
                        startingChunkPos.x + (chunkDimensions.x * x),
                        startingChunkPos.y + (chunkDimensions.y * y),
                        startingChunkPos.z + (chunkDimensions.z * z));
                    Chunk c = world.GetChunkFromWorldPos(chunkPos);

                    if (c == null)
                    {
                        Debug.LogError("CHUNK NOT FOUND: The current chunk wasn't found");
                        return;
                    }

                    // Figure out which walls should be open
                    SetOpenedArea(Direction.Left, x > 0);
                    SetOpenedArea(Direction.Right, x < roomDimensions.x - 1);

                    SetOpenedArea(Direction.Bottom, y > 0);
                    SetOpenedArea(Direction.Top, y < roomDimensions.y - 1);

                    SetOpenedArea(Direction.Back, z > 0);
                    SetOpenedArea(Direction.Front, z < roomDimensions.z - 1);

                    // Clear the chunk
                    room.AddChunk(c);
                    ClearChunk(c);
                }
            }
        }

        Vector3 roomOrigin = startingChunk.GetOrigin(world.worldSettings.boundsSize);

        // Generate base room noise
        roomNoiseGenerator.AddNoiseToRoom(room.chunks, roomDimensions, roomOrigin);

        // Add to list of rooms
        allRooms.Add(room);
    }

    private void ClearChunk(Chunk chunk)
    {
        world.EditChunk(clearRoomCompute, chunk);
    }
    private void SetRoomMinMax(ref Room room, Chunk startingChunk)
    {
        float boundsSize = world.worldSettings.boundsSize;

        room.min = startingChunk.GetOrigin(boundsSize);

        room.max = room.min + new Vector3(
            room.dimensions.x * boundsSize,
            room.dimensions.y * boundsSize,
            room.dimensions.z * boundsSize
        );
    }
    public static bool IsWithinBounds(Vector3 p, Vector3 min, Vector3 max)
    {
        return p.x >= min.x && p.x < max.x &&
               p.y >= min.y && p.y < max.y &&
               p.z >= min.z && p.z < max.z;
    }

    private int GetRoomAtPos(Vector3 pos)
    {
        for (int i = 0; i < allRooms.Count; i++)
        {
            if (IsWithinBounds(pos, allRooms[i].min, allRooms[i].max))
            {
                return i;
            }
        }

        return -1; // No room
    }

    Vector3Int WorldToChunkCoord(Vector3 pos)
    {
        float s = world.worldSettings.boundsSize;

        int x = Mathf.FloorToInt((pos.x + s * 0.5f) / s);
        int y = Mathf.FloorToInt((pos.y + s * 0.5f) / s);
        int z = Mathf.FloorToInt((pos.z + s * 0.5f) / s);

        return new Vector3Int(x, y, z);
    }

    private Vector3 GetRandomWorldPos()
    {
        Vector3 worldMin;
        worldMin.x = world.GetWorldCenter().x - (world.GetDimensions().x / 2.0f);
        worldMin.y = world.GetWorldCenter().y - (world.GetDimensions().y / 2.0f);
        worldMin.z = world.GetWorldCenter().z - (world.GetDimensions().z / 2.0f);

        Vector3 worldMax;
        worldMax.x = world.GetWorldCenter().x + (world.GetDimensions().x / 2.0f);
        worldMax.y = world.GetWorldCenter().y + (world.GetDimensions().y / 2.0f);
        worldMax.z = world.GetWorldCenter().z + (world.GetDimensions().z / 2.0f);

        Vector3 randPos;
        randPos.x = Random.Range(worldMin.x, worldMax.x);
        randPos.y = Random.Range(worldMin.y, worldMax.y);
        randPos.z = Random.Range(worldMin.z, worldMax.z);

        return randPos;
    }
    private void SetOpenedArea(Direction dir, bool b)
    {
        switch (dir)
        {
            case Direction.Left:
                clearRoomCompute.SetBool("leftOpen", b);
                break;

            case Direction.Right:
                clearRoomCompute.SetBool("rightOpen", b);
                break;

            case Direction.Front:
                clearRoomCompute.SetBool("frontOpen", b);
                break;

            case Direction.Back:
                clearRoomCompute.SetBool("backOpen", b);
                break;

            case Direction.Top:
                clearRoomCompute.SetBool("topOpen", b);
                break;

            case Direction.Bottom:
                clearRoomCompute.SetBool("bottomOpen", b);
                break;
        }
    }
    private void ClearOpenAreas()
    {
        clearRoomCompute.SetBool("leftOpen", false);
        clearRoomCompute.SetBool("rightOpen", false);
        clearRoomCompute.SetBool("frontOpen", false);
        clearRoomCompute.SetBool("backOpen", false);
        clearRoomCompute.SetBool("topOpen", false);
        clearRoomCompute.SetBool("bottomOpen", false);
    }
    private Direction ReturnInverseDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Left:
                return Direction.Right;

            case Direction.Right:
                return Direction.Left;

            case Direction.Front:
                return Direction.Back;

            case Direction.Back:
                return Direction.Front;

            case Direction.Top:
                return Direction.Bottom;

            case Direction.Bottom:
                return Direction.Top;

            default:
                return dir;
        }
    }
    private Vector3 DirectionToVector(Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                return Vector3.left;

            case Direction.Right:
                return Vector3.right;

            case Direction.Front:
                return Vector3.forward;

            case Direction.Back:
                return Vector3.back;

            case Direction.Top:
                return Vector3.up;

            case Direction.Bottom:
                return Vector3.down;

            default:
                return Vector3.zero;
        }
    }
}
