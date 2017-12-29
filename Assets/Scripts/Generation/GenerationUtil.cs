using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
	NORTH,
	WEST,
	EAST,
	SOUTH
}

public static class DirectionUtils
{
	public static Direction[] DIRECTIONS = (Direction[]) System.Enum.GetValues(typeof(Direction));

	// Return direction as Vector
	public static Vector3 GetDirectionVector(this Direction direction)
	{
		switch (direction)
		{
		case Direction.NORTH: {return new Vector3 (0.0F, 0.0F, -1.0F);}
		case Direction.SOUTH: {return new Vector3 (0.0F, 0.0F, 1.0F);}
		case Direction.WEST: {return new Vector3 (-1.0F, 0.0F, 0.0F);}
		default: {return new Vector3 (1.0F, 0.0F, 0.0F);}
		}
	}

	// Return direction angle
	public static float GetDirectionAngle(this Direction direction)
	{
		switch (direction)
		{
		case Direction.NORTH: {return 0.0F;}
		case Direction.SOUTH: {return 180.0F;}
		case Direction.WEST: {return 90.0F;}
		default: {return 270.0F;}
		}
	}

	// Return direction's opposite
	public static Direction GetOpposite(this Direction direction)
	{
		switch(direction)
		{
		case Direction.NORTH: {return Direction.SOUTH;}
		case Direction.SOUTH: {return Direction.NORTH;}
		case Direction.WEST: {return Direction.EAST;}
		default: {return Direction.WEST;}
		}
	}
}

public class Cell
{
	public const float CELL_SIZE = 4.0F;

	public int x;
	public int y;

	public Cell(int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	public Cell Offset(Direction direction, int offset)
	{
		int nX = x;
		int nY = y;

		switch (direction)
		{
		case Direction.NORTH:{ nY -= offset; break;}
		case Direction.SOUTH:{ nY += offset; break;}
		case Direction.WEST:{ nX -= offset; break;}
		case Direction.EAST:{ nX += offset; break;}
		}

		return new Cell (nX, nY);
	}

	public bool Equals(Cell other)
	{
		return other != null && other.x == x && other.y == y;
	}

	public Vector3 AsVector3(float height = 0.0F)
	{
		return Cell.AsVector3(x, y, height);
	}

	public static Vector3 AsVector3(int x, int y, float height = 0.0F)
	{
		return new Vector3 (x * CELL_SIZE, height, y * CELL_SIZE);
	}

	public static Cell CellAt(Vector3 pos)
	{
		int x = Mathf.RoundToInt (pos.x / 4.0F);
		int y = Mathf.RoundToInt(pos.z / 4.0F);

		return new Cell(x, y);
	}
}


public class MazeFrontier : Cell
{
	public Direction direction;

	public MazeFrontier(Direction direction, Cell cell) : base(cell.x, cell.y)
	{
		this.direction = direction;
	}

	public Cell Offset(int offset)
	{
		return base.Offset(direction, offset);
	}
}