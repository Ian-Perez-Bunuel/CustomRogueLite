using NUnit.Framework;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [SerializeField] ComputeShader clearRoomCompute;
    [SerializeField] MarchingCubesCompute world;
    [SerializeField] BaseRoomGenerator roomNoiseGenerator;
    static Vector3 chunkDimensions;

    [SerializeField] int amountOfRooms;

    Dictionary<List<Chunk>, Vector3Int> roomsWithSize = new Dictionary<List<Chunk>, Vector3Int>();

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
        // Holds all the chunks in the room
        List<Chunk> room = new List<Chunk>();

        // Starting chunk is the min chunk
        Vector3 startingChunkPos = GetRandomWorldPos();
        Chunk startingChunk = world.GetChunkFromWorldPos(startingChunkPos);

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
                    room.Add(c);
                    ClearChunk(c);
                }
            }
        }

        Vector3 roomOrigin = startingChunk.GetOrigin(world.worldSettings.boundsSize);

        // Generate base room noise
        roomNoiseGenerator.AddNoiseToRoom(room, roomDimensions, roomOrigin);

        // Add to list of rooms
        roomsWithSize.Add(room, roomDimensions);
    }

    private void ClearChunk(Chunk chunk)
    {
        world.EditChunk(clearRoomCompute, chunk);
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
