using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
	private static MapGenerator instance;
	public static MapGenerator Instance
	{
		get { return instance; }
	}

	[SerializeField]
	private GameObject floorPrefab;

	[SerializeField]
	private GameObject wallPrefab;

	[SerializeField]
	private GameObject enemyPrefab;

	[SerializeField]
	private GameObject healthPrefab;

	[SerializeField]
	private GameObject keyPrefab;

	[SerializeField]
	private Texture2D alternateWall;

	private Room[] rooms;
	private TileType[,] map;
	public TileType[,] Map
	{
		get { return map; }
	}

	private int sizeX;
	public int SizeX
	{
		get { return sizeX;}
	}
	private int sizeY;
	public int SizeY
	{
		get { return sizeY;}
	}

	private int totalCells;
	public int TotalCells
	{
		get { return totalCells;}
	}

	private Room spawnRoom;

	public MapGenerator()
	{
		instance = this;
	}

	void Start ()
	{
		GenMap (50, 25);
	}

	void GenMap(int width, int height)
	{
		sizeX = width * 2;
		sizeY = height * 2;
		map = new TileType[sizeX, sizeY];

		rooms = GenRooms ();

		// Generate map
		ProcessRooms ();

		GenCorridors ();
		CleanCorridors ();
		CheckMap ();

		ProcessMapTiles ();
		AddWalls ();
		PlacePlayer ();
		SpawnEnemies (100);
		SpawnObjects (PlayerController.MAX_KEY, keyPrefab);
		SpawnObjects (15, healthPrefab);
	}

	Room[] GenRooms()
	{
		// Calculate rooms amount
		int mapAir = sizeX * sizeY;
		int roomAmount = (int) Random.Range(mapAir * 0.01F, mapAir * 0.015F);

		// Generate rooms
		Room[] rooms = new Room[roomAmount];
		for (int i = 0; i < roomAmount; i++)
		{
			Room room;
			bool valid;
			int tries = 0;
			do
			{
				valid = true;

				// Try to create a new room with margin
				int x = Random.Range(0, sizeX / 2) * 2 + 1;
				int y = Random.Range(0, sizeY / 2) * 2 + 1;
				int width = x + Random.Range(3, 5) * 2 - 1;
				int height = y + Random.Range(3, 5) * 2 - 1;
				room = new Room(x, y , width, height);
				if(OutsideMap(room.xMax, room.yMax))
				{
					valid = false;
					continue;
				}

				for(int j = 0; j < i; j++)
				{
					// Check room is not outside map / intersect another one
					if(room.Overlap(rooms[j], 0))
					{
						valid = false;
						tries++;
						break;
					}
				}

			} while(!valid && tries < 10);

			// Failed to generate room
			if (tries >= 10)
			{
				i--;
				roomAmount--;
			}
			else
			{
				rooms [i] = room;
			}
		}
		System.Array.Resize (ref rooms, roomAmount);

		return rooms;
	}

	void ProcessRooms()
	{
		foreach(Room room in rooms)
		{
			for (int x = (int) room.xMin; x < room.xMax; x++)
			{
				for (int y = (int) room.yMin; y < room.yMax; y++)
				{
					map[x, y] = TileType.ROOM;
				}
			}
		}
	}

	void GenCorridors()
	{
		// Generate corridors, multiple maze may be generated if rooms create a closed area
		for (int x = 1; x < map.GetLength(0) - 1; x+=2)
		{
			for (int y = 1; y < map.GetLength(1) - 1; y+=2)
			{
				if (map [x, y] == TileType.WALL)
				{
					new PrimMaze().Generate (this, new Cell(x, y));
				}
			}
		}
	}
		
	void CleanCorridors()
	{
		for (int x = 1; x < map.GetLength(0) - 1; x+=2)
		{
			for (int y = 1; y < map.GetLength(1) - 1; y+=2)
			{
				if (map [x, y] == TileType.CORRIDOR)
				{
					ClearCorridorSegment (new Cell (x, y));
				}
			}
		}
	}

	void ClearCorridorSegment (Cell cell)
	{
		Direction lastDirection = Direction.NORTH;
		int neighboorsCorridors = 0;
		Cell nearCell;
		Cell farCell;

		//Find corridor direction 
		Direction[] directions = (Direction[]) System.Enum.GetValues(typeof(Direction));
		for (int i = 0; i < directions.Length; i++)
		{
			nearCell = cell.Offset (directions[i], 1);
			farCell = cell.Offset (directions[i], 2);

			if (map[nearCell.x, nearCell.y] != TileType.WALL)
			{
				lastDirection = directions [i];
				neighboorsCorridors++;
			}
		}

		nearCell = cell.Offset (lastDirection, 1);
		farCell = cell.Offset (lastDirection, 2);

		// Clean useless paths
		if(neighboorsCorridors == 1)
		{
			map [cell.x, cell.y] = map[nearCell.x, nearCell.y] = TileType.WALL;
			ClearCorridorSegment (farCell);
		}
	}

	void CheckMap()
	{
		// Invalid map -> Regen map -> lazy solution
		foreach (Room room in rooms)
		{
			if (room.doors.Count == 0)
			{
				GenMap (sizeX / 2, sizeY / 2);
				break;
			}
		}
	}

	void ProcessMapTiles()
	{
		// Generate rooms
		foreach(Room room in rooms)
		{
			int roomWidth = room.xMax - room.xMin;
			int roomHeight = room.yMax - room.yMin;
			GameObject roomFloor = Instantiate (floorPrefab, new Vector3((room.xMin + roomWidth / 2) * Cell.CELL_SIZE, 0.0F, (room.yMin + roomHeight / 2) * Cell.CELL_SIZE), Quaternion.Euler(90.0F, 0.0F, 0.0F));
			roomFloor.transform.localScale = new Vector3 (roomWidth * Cell.CELL_SIZE, roomHeight * Cell.CELL_SIZE, 1.0F);
			roomFloor.name = "Room";
			totalCells += roomWidth * roomHeight;
		}

		for (int x = 0; x < map.GetLength(0); x++)
		{
			for (int y = 0; y < map.GetLength(1); y++)
			{
				if (map[x, y] == TileType.CORRIDOR)
				{
					GameObject g = Instantiate (floorPrefab, Cell.AsVector3(x, y), Quaternion.Euler(90.0F, 0.0F, 0.0F));
					g.name = "Corridor";
					totalCells++;
				}
			}
		}
	}

	void AddWalls()
	{
		Direction[] directions = (Direction[]) System.Enum.GetValues(typeof(Direction));
		for (int x = 0; x < map.GetLength(0); x++)
		{
			for (int y = 0; y < map.GetLength(1); y++)
			{
				if (map[x, y] == TileType.WALL)
				{
					foreach (Direction dir in directions)
					{	
						float angle = dir.GetDirectionAngle ();

						Cell nearCell = new Cell (x, y).Offset(dir, 1);
						if (!OutsideMap(nearCell.x, nearCell.y) && map[nearCell.x, nearCell.y] != TileType.WALL)
						{
							GameObject g = Instantiate (wallPrefab, new Vector3 (x * Cell.CELL_SIZE - (x - nearCell.x) * Cell.CELL_SIZE / 2.0F, Cell.CELL_SIZE / 2.0F, y * Cell.CELL_SIZE - (y - nearCell.y) * Cell.CELL_SIZE / 2.0F), Quaternion.Euler (0.0F, angle, 0.0F));
							g.name = "Wall";

							if (Random.value < 0.2F)
							{
								g.GetComponent<Renderer> ().material.mainTexture = alternateWall;
							}
						}
					}
				}
			}
		}
	}

	void PlacePlayer()
	{
		spawnRoom = rooms [Random.Range (0, rooms.Length)];
		float x = Random.Range (spawnRoom.xMin, spawnRoom.xMax) * Cell.CELL_SIZE;
		float y = Random.Range (spawnRoom.yMin, spawnRoom.yMax) * Cell.CELL_SIZE;

		// Warp player
		GameObject.FindGameObjectsWithTag ("Player")[0].transform.position = new Vector3(x, 1.0F, y);
	}

	void SpawnEnemies(int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			// Don't spawn enemies in player room
			Room room = rooms [Random.Range (0, rooms.Length)];
			if (spawnRoom != room)
			{
				SpawnEnemy (room);
			}
			else
			{
				i--;
			}
		}
	}

	void SpawnObjects(int amount, GameObject obj)
	{
		for (int i = 0; i < amount; i++)
		{
			// Don't spawn enemies in player room
			Room room = rooms [Random.Range (0, rooms.Length)];
			if (spawnRoom != room)
			{
				Instantiate (obj, room.RandomCellInRoom ().AsVector3 (1.0F), Quaternion.identity);
			}
			else
			{
				i--;
			}
		}
	}

	public void SpawnEnemy(Room room = null)
	{
		if (room != null)
		{
			Instantiate (enemyPrefab, room.RandomCellInRoom ().AsVector3 (1.0F), Quaternion.identity);
		}
		else
		{
			// Pick a random room
			room = rooms [Random.Range (0, rooms.Length)];
			Instantiate (enemyPrefab, room.RandomCellInRoom ().AsVector3 (1.0F), Quaternion.identity);
		}
	}

	// Check point is in map
	bool OutsideMap(int x, int y)
	{
		return x < 0 || y < 0 || x >= sizeX || y >= sizeY;
	}

	// Check point is in map with margin around edges
	bool OutsideMapWithMargin(int x, int y, int margin)
	{
		return x < margin || y < margin || x >= sizeX - margin || y >= sizeY - margin;
	}

	// Find a room at a certain position
	public Room GetRoomAt(int x, int y)
	{
		foreach (Room room in rooms)
		{
			if (room.Contains(x, y))
			{
				return room;
			}
		}

		return null;
	}

	public TileType SafeTileAt(Cell cell)
	{
		if (!OutsideMap (cell.x, cell.y))
		{
			return map [cell.x, cell.y];
		}
		return TileType.WALL;
	}
}

public enum TileType
{
	WALL,
	ROOM,
	CORRIDOR
}

public class Room
{
	public int xMin;
	public int yMin;
	public int xMax;
	public int yMax;
	public List<Cell> doors;

	private PrimMaze lastMaze;
	private Direction lastDirection;

	public Room(int x, int y, int x2, int y2)
	{
		xMin = x;
		yMin = y;
		xMax = x2;
		yMax = y2;
		doors = new List<Cell> ();
	}

	public bool Overlap(Room r2, int margin)
	{
		Room margins = new Room (Mathf.Max(xMin - margin, 0), Mathf.Max(yMin - margin, 0), xMax + margin, yMax + margin);
		if (margins.xMax <= r2.xMin || margins.xMin >= r2.xMax || margins.yMax <= r2.yMin || margins.yMin >= r2.yMax)
		{
			return false;
		}

		return true;
	}

	public bool Contains(int x, int y)
	{
		return x >= xMin && x < xMax && y >= yMin && y < yMax;
	}

	public bool CanOpenDoor(PrimMaze maze, Direction direction, int x, int y)
	{
		float rate = lastDirection == direction ? 0.02F : Mathf.Max(0.9F - doors.Count * 0.45F, 0.0F);
		if (lastMaze != maze || Random.Range(0.0F, 1.0F) < rate)
		{
			doors.Add (FixDoorPos(direction, x, y));
			lastDirection = direction;
			lastMaze = maze;
			return true;
		}
		return false;
	}

	public Cell RandomCellInRoom()
	{
		int x = Random.Range (xMin, xMax);
		int y = Random.Range (yMin, yMax);

		return new Cell (x, y);
	}

	Cell FixDoorPos(Direction direction, int x, int y)
	{
		switch (direction)
		{
		case Direction.SOUTH:{return new Cell (x, y - 1);}
		case Direction.NORTH:{return new Cell (x, y + 1);}
		case Direction.EAST:{return new Cell (x - 1, y);}
		case Direction.WEST:{return new Cell (x + 1, y);}
		}
		return new Cell (x, y);
	}
}

