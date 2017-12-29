using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * Generate a prim maze
 * Actually old java code of mine translated to C# and adapted to generate doors for rooms
 **/
public class PrimMaze
{
	private MapGenerator mapGenerator;
	private TileType[,] maze;
	private LinkedList<MazeFrontier> frontiers;
	private int width;
	private int height;

	public void Generate(MapGenerator mapGenerator, Cell cell)
	{
		// Init
		this.mapGenerator = mapGenerator;
		frontiers = new LinkedList<MazeFrontier>();

		maze = mapGenerator.Map;
		width = maze.GetLength (0);
		height = maze.GetLength (1);

		// Create entry point corridor
		CarveWallAt(cell);
		AddFrontiers(Direction.NORTH, cell);

		// Generate maze
		while(!ExploreFrontier()){}
	}

	private bool ExploreFrontier()
	{
		// No more frontiers, the maze is finished
		if(frontiers.Count == 0) return true;

		MazeFrontier frontier;

		// Chances to continue previous line
		if(Random.Range(0.0F, 1.0F) < 0.1F)
		{
			frontier = frontiers.First.Value;
			frontiers.RemoveFirst();
		}
		else
		{
			// Explore a new frontier
			int index = Random.Range (0, frontiers.Count);
			frontier = frontiers.ElementAt(index);
			DeleteNode(index);
		}

		// Create a new corridor
		Cell cell = GoToNextCell(frontier);
		if(cell != null)
		{
			//If this new pos is a wall (in case of room)
			if (IsWallAt (cell))
			{
				CarveWallAt (cell);
				AddFrontiers (frontier.direction, cell);
			}
		}

		return false;
	}

	private void AddFrontiers(Direction direction, Cell cell)
	{
		// Add frontier for each direction
		Direction[] directions = DirectionUtils.DIRECTIONS;
		for (int i = 0; i < directions.Length; i++)
		{
			Cell nearCell = cell.Offset (directions[i], 1);
			Cell farCell = cell.Offset (directions[i], 2);

			if (!IsOutsideMazeAt (farCell) && IsWallAt (nearCell))
			{
				if(direction == directions[i])
				{
					frontiers.AddFirst(new MazeFrontier(directions[i], nearCell));				
				}
				else
				{
					frontiers.AddLast(new MazeFrontier(directions[i], nearCell));
				}
			}
		}
	}

	private Cell GoToNextCell(MazeFrontier frontier)
	{
		Cell nextCell = frontier.Offset (1);

		// Check frontier can carve
		if (CanCarveAt (nextCell, frontier.direction))
		{
			CarveWallAt(frontier);
			return nextCell;
		}

		return null;
	}


	//Utils
	private void CarveWallAt(Cell cell)
	{
		maze[cell.x, cell.y] = TileType.CORRIDOR;
	}

	private bool IsWallAt(Cell cell)
	{
		return maze[cell.x, cell.y] == TileType.WALL;
	}

	private bool CanCarveAt(Cell cell, Direction direction)
	{
		if (maze [cell.x, cell.y] == TileType.WALL)
		{
			return true;
		}
		else if(maze [cell.x, cell.y] == TileType.ROOM)
		{
			Room roomAt = mapGenerator.GetRoomAt (cell.x, cell.y);
			return roomAt != null ? roomAt.CanOpenDoor(this, direction, cell.x, cell.y) : false;
		}

		return false;
	}

	private bool IsOutsideMazeAt(Cell cell)
	{
		return cell.x < 1 || cell.y < 1 || cell.x > width - 2 || cell.y > height - 2;
	}
		
	private void DeleteNode(int index)
	{
		var node = frontiers.First;
		int i = 0;
		while (node != null)
		{
			var nextNode = node.Next;
			if (index == i)
			{
				frontiers.Remove(node);
			}
			node = nextNode;
			i++;
		}
	}
}
