using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
	private int cellSize = 5;
	private bool[,] exploredMap;
	private Texture2D minimapTexture;
	private RectTransform rectTransform;
	private Color transparent = new Color(1.0F, 1.0F, 1.0F, 0.0F);
	private Color fillCell = new Color(0.0F, 0.0F, 1.0F, 0.25F);
	private Cell lastCell;
	private int exploredCells;
	private int exploredPercent = -1;
	public int ExploredPercent
	{
		get { return exploredPercent; }
		set
		{
			if (exploredPercent != value)
			{
				exploredPercent = value;
				exploreText.text = value + "%";
			}
		}
	}

	[SerializeField]
	private Text exploreText;

	// Use this for initialization
	void Start ()
	{
		minimapTexture = new Texture2D ((int) (MapGenerator.Instance.SizeX * cellSize), (int) (MapGenerator.Instance.SizeY * cellSize), TextureFormat.ARGB32, false);
		exploredMap = new bool[MapGenerator.Instance.SizeX, MapGenerator.Instance.SizeY];
		ExploredPercent = 0;
		// Set transparant background
		for (int x = 0; x < minimapTexture.width; x++)
		{
			for (int y = 0; y < minimapTexture.height; y++)
			{
				minimapTexture.SetPixel (x, y, transparent);
			}
		}

		Image image = GetComponent<Image> ();
		image.sprite = Sprite.Create(minimapTexture, new Rect(0, 0, minimapTexture.width, minimapTexture.height), new Vector2(0.5F, 0.5F));

		rectTransform = GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2 (minimapTexture.width, minimapTexture.height);
	}
	
	// Update is called once per frame
	void Update ()
	{
		Cell cell = Cell.CellAt (PlayerController.Instance.transform.position);
		if (lastCell != cell)
		{
			TileType tile = MapGenerator.Instance.Map [cell.x, cell.y];
			int minX = 0;
			int maxX = 0;
			int minY = 0;
			int maxY = 0;

			if (tile == TileType.CORRIDOR)
			{
				minX = cell.x - 1;
				minY = cell.y - 1;
				maxX = cell.x + 2;
				maxY = cell.y + 2;
			} else if (tile == TileType.ROOM)
			{
				Room room = MapGenerator.Instance.GetRoomAt (cell.x, cell.y);
				minX = room.xMin - 1;
				maxX = room.xMax + 1;
				minY = room.yMin - 1;
				maxY = room.yMax + 1;
			}

			for (int x = minX; x < maxX; x++)
			{
				for (int y = minY; y < maxY; y++)
				{
					if (MapGenerator.Instance.Map [x, y] != TileType.WALL)
					{
						DrawCell (new Cell (x, y));
					}
				}
			}

			//Draw player
			if (lastCell != null)
			{
				DrawPlayerAt (lastCell, fillCell);
			}
			DrawPlayerAt(cell, new Color(0.5F, 1.0F, 1.0F));
		}

		minimapTexture.Apply ();
		lastCell = cell;

		// Cheat code
		if (Input.GetKey(KeyCode.M) && Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.P))
		{
			for (int x = 0; x < MapGenerator.Instance.SizeX; x++)
			{
				for (int y = 0; y < MapGenerator.Instance.SizeY; y++)
				{
					DrawCell (new Cell (x, y));
				}
			}
		}
	}

	void DrawCell(Cell cell)
	{
		if (exploredMap [cell.x, cell.y] || MapGenerator.Instance.SafeTileAt(cell) == TileType.WALL)
		{
			return;
		}

		exploredCells++;
		ExploredPercent = (int) ((exploredCells / (float) MapGenerator.Instance.TotalCells) * 100);
		exploredMap [cell.x, cell.y] = true;
		bool[] corners = GetCellData (cell);
		int x = (int) (cell.x * cellSize);
		int y = (int) (cell.y * cellSize);

		// Fill cell
		for (int i = 0; i < cellSize - 0; i++)
		{
			for (int j = 0; j < cellSize -0; j++)
			{
				minimapTexture.SetPixel (cell.x * cellSize + i, cell.y * cellSize + j, fillCell);
			}
		}

		if (corners [1])
		{
			HorizontalLine (x, cellSize, y);
		}

		if (corners [7])
		{
			HorizontalLine (x, cellSize, y + cellSize - 1);
		}

		if (corners [3])
		{
			VerticalLine (y, cellSize, x);
		}

		if (corners [5])
		{
			VerticalLine (y, cellSize, x + cellSize - 1);
		}
	}

	void DrawPlayerAt(Cell cell, Color color)
	{
		for (int i = 1; i < cellSize - 1; i++)
		{
			for (int j = 1; j < cellSize -1; j++)
			{
				minimapTexture.SetPixel (cell.x * cellSize + i, cell.y * cellSize + j, color);
			}
		}
	}

	void HorizontalLine(int x, int width, int y)
	{
		for (int i = x; i < x + width; i++)
		{
			minimapTexture.SetPixel (i, y, Color.white);
		}
	}

	void VerticalLine(int y, int height, int x)
	{
		for (int i = y; i < y + height; i++)
		{
			minimapTexture.SetPixel (x, i, Color.white);
		}
	}

	bool[] GetCellData(Cell cell)
	{
		Direction[] directions = DirectionUtils.DIRECTIONS;
		bool[] possibleDirection = new bool[9];
		for(int i = 0; i < directions.Length; i++)
		{
			Cell nearCell = cell.Offset(directions[i], 1);
			if(MapGenerator.Instance.SafeTileAt(nearCell) == TileType.WALL)
			{
				possibleDirection [i * 2 + 1] = true;
			}
		}

		// Corners
		possibleDirection [0] = possibleDirection[1] || possibleDirection[3];
		possibleDirection [2] = possibleDirection[1] || possibleDirection[5];
		possibleDirection [6] = possibleDirection[3] || possibleDirection[7];
		possibleDirection [8] = possibleDirection[5] || possibleDirection[7];

		return possibleDirection;
	}
}
