using System;
using System.Collections.Generic;
using Sandbox;

internal class Cell
{
	public Cell(int x, int y)
	{
		X = x;
		Y = y;
	}
	public int X { get; set; }
	public int Y { get; set; }
	public bool[] Walls { get; set; } = { true, true, true, true };
	public bool Visited { get; set; } = false;
}
public class MazeGen
{
	private Cell[,] cells;
	private int Size { get; set; }
	public List<Tile> GenerateMaze(int size)
	{
		Size = size;
		cells = new Cell[size, size];
		for ( int x = 0; x < size; x++ )
			for ( int y = 0; y < size; y++ )
				cells[x, y] = new Cell( x, y );

		
		List<Cell> Stack = new();
		Stack.Add( GetCell( 0, 0 ) );
		while ( Stack.Count > 0 )
		{
			Cell Current = Stack[Stack.Count - 1];
			Current.Visited = true;
			Stack.RemoveAt( Stack.Count - 1 );
			
			var next = CheckNeighbors( Current );
			if ( next != null )
			{				
				Stack.Add( Current );
				if ( Current.Y < next.Y ) // N
				{
					Current.Walls[0] = false;
					next.Walls[1] = false;
				}
				else if ( Current.Y > next.Y ) // S
				{
					Current.Walls[1] = false;
					next.Walls[0] = false;
				}
				else if ( Current.X < next.X ) // E
				{
					Current.Walls[2] = false;
					next.Walls[3] = false;
				}
				else if ( Current.X > next.X ) // W
				{
					Current.Walls[3] = false;
					next.Walls[2] = false;
				}
				next.Visited = true;
				Stack.Add( next );
			}
		}

		List<Tile> tiles = new();
		for ( int y = 0; y < size; y++ )
			for(int x = 0; x < size; x++ )
			{
				Cell c = GetCell( x, y );
				Tile t = new();
				ushort N = ((c.Walls[0] == true) ? (ushort)0b1000 : (ushort)0b0000);
				ushort S = ((c.Walls[1] == true) ? (ushort)0b0100 : (ushort)0b0000);
				ushort E = ((c.Walls[2] == true) ? (ushort)0b0010 : (ushort)0b0000);
				ushort W = ((c.Walls[3] == true) ? (ushort)0b0001 : (ushort)0b0000);
				t.Walls = (ushort)(N | S | E | W);
				tiles.Add( t );
			}

		return tiles;
	}
	private Cell CheckNeighbors(Cell cell)
	{
		int x = cell.X;
		int y = cell.Y;
		var a = GetCell( x, y + 1 );
		var b = GetCell( x + 1, y );
		var c = GetCell( x, y - 1 );
		var d = GetCell( x - 1, y );

		List<Cell> neighbors = new();

		if ( a != null && !a.Visited ) neighbors.Add( a );
		if ( b != null && !b.Visited ) neighbors.Add( b );
		if ( c != null && !c.Visited ) neighbors.Add( c );
		if ( d != null && !d.Visited ) neighbors.Add( d );

		if(neighbors.Count > 0)
		{
			return neighbors[Rand.Int( 0, neighbors.Count - 1 )];
		}
		return null;
	}
	private Cell GetCell(int x, int y)
	{
		if(x >= 0 && y >= 0 && x < Size && y < Size)
			return cells[x,y];
		return null;
	}
	
}
