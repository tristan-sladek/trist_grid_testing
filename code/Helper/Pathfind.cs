using Sandbox;
using System;
using System.Collections.Generic;
internal class PathNode
{
	public int X, Y, D;
	public PathNode( int x, int y, int d) { X = x; Y = y; D = d; }
	public PathNode( float x, float y, int d) { X = (int)x; Y = (int)y; D = d; }
}
public partial class Pathfind
{
	WorldEntity WorldEntity { get; set; }
	public bool CanPathfind { get; set; }
	public Vector3 NextPos { get; set; } = Vector3.Zero;
	private Vector3 Start { get; set; }
	private Vector3 End { get; set; }
	public Pathfind( WorldEntity w)
	{
		WorldEntity = w;
	}
	public Pathfind FromPosition(Vector3 InStart, bool OnGrid = true )
	{
		if(!OnGrid)
		{
			InStart /= WorldEntity.GRID_SCALE;
			Start = InStart;
		}
		else Start = InStart;
		Start.SnapToGrid( 1 ); //make sure everything is clean
		return this;
	}
	public Pathfind ToPosition(Vector3 InEnd, bool OnGrid = true )
	{
		if ( !OnGrid )
		{
			InEnd /= WorldEntity.GRID_SCALE;
			End = InEnd;
		}
		else End = InEnd;
		End.SnapToGrid( 1 ); //make sure everything is clean
		return this;
	}
	private int[,] localPathList;
	private int Depth;
	private int MaxDepth;
	public Pathfind Path( int depth = 16 )
	{
		Depth = depth;
		MaxDepth = (depth * 2) + 1; //Maximum Depth Index, Depth is the radius + 1 for the center square.
		localPathList = new int[MaxDepth, MaxDepth];
		for ( int x = 0; x < MaxDepth; x++ )
			for ( int y = 0; y < MaxDepth; y++ )
				localPathList[x, y] = -1; //default to unchecked;

		var localStart = MapToLocal( Start );
		var localEnd = MapToLocal( End );
		// Log.Info( "MAP: " + Start + " | " + End + " : LOCAL: " + localStart + " | " + localEnd );

		var localDist = (localEnd - localStart); 
		if ( Math.Abs(localDist.x) > Depth || Math.Abs( localDist.y ) > Depth ) //distance in square
			return this; //quick cancel on distance beyond reach

		//BFS to find path
		List<PathNode> Queue = new();
		Queue.Add( new PathNode(localStart.x,localStart.y,0) );
		localPathList[(int)localStart.x, (int)localStart.y] = 0;

		int c = 0; 
		while(Queue.Count > 0)
		{
			if ( ++c > MaxDepth * MaxDepth ) { // Log.Error( "Had to force break" ); PrintDepthTree();
				break;
			};
			// Pop First Node
			var curNode = Queue[0];
			Queue.RemoveAt(0);

			int x = curNode.X;
			int y = curNode.Y;
			int d = curNode.D;

			// Final Node Found
			if ( new Vector3( x, y, 0) == localEnd )
			{
				CanPathfind = true;
				break;
			}

			// Find Unvisited Neighbors
			CheckDSafe( x + 1, y, Queue, x, y, d );
			CheckDSafe( x - 1, y, Queue, x, y, d );
			CheckDSafe( x, y + 1, Queue, x, y, d );
			CheckDSafe( x, y - 1, Queue, x, y, d );

		}
		if ( CanPathfind ) //path to end found, backtrack route
		{
			List<PathNode> Route = new();

			int x = (int)localEnd.x;
			int y = (int)localEnd.y;
			int ex = (int)localStart.x;
			int ey = (int)localStart.y;
			int d = GetDSafe( x, y );
			int s = 0;

			while ( x != ex || y != ey )
			{
				Route.Add( new PathNode( x, y, d ) );
				if ( ++s > MaxDepth ) {	// Log.Error( "Force Break! " + x + " | " + y ); PrintDepthTree();
					break;
				}

				int minX = x;
				int minY = y;
				int minD = d;

				if ( GetDSafe( x - 1, y ) < minD )
				{
					minD = GetDSafe( x - 1, y );
					minX = x - 1;
					minY = y;
				}
				if ( GetDSafe( x + 1, y ) < minD )
				{
					minD = GetDSafe( x + 1, y );
					minX = x + 1;
					minY = y;
				}
				if ( GetDSafe( x, y - 1 ) < minD )
				{
					minD = GetDSafe( x, y - 1 );
					minX = x;
					minY = y - 1;
				}
				if ( GetDSafe( x, y + 1 ) < minD )
				{
					minD = GetDSafe( x, y + 1 );
					minX = x;
					minY = y + 1;
				}

				d = minD;
				x = minX;
				y = minY;
			}
			// Log.Info( "Path Found! " + s );
			var LastPath = Route[Route.Count - 1];

			NextPos = new Vector3( LastPath.X - localStart.x, LastPath.Y - localStart.y, 0 );
		}
		
		//PrintDepthTree();

		return this;
	}
	private void CheckDSafe(int x, int y, List<PathNode> Queue, int lx, int ly, int ld)
	{ //Used for checking unvisited neighbors
		if ( x < MaxDepth && x >= 0 && y < MaxDepth && y >= 0) // In Range
		{
			var v = localPathList[x , y]; //Current Distance from start
			if ( v < 0 ) //-1, unvisited
			{
				if ( LocalCanWalk( x , y ) ) //Check if this is blocked via world ent
				{
					Queue.Add( new PathNode( x , y, ld + 1 ) );
					localPathList[x , y] = ld + 1;
				}
				else
				{
					localPathList[x , y] = MaxDepth + 1; //MaxDepth + 1, farther than anything
				}
			}
			else if ( v < ld ) //more efficient path found from prev stuff
			{
				ld = v + 1;
				localPathList[lx, ly] = ld; //just update to make things smoother
			}
		}
	}
	private int GetDSafe(int x, int y)
	{
		int val = -1;
		if ( x < MaxDepth && x >= 0 && y < MaxDepth && y >= 0 ) val = localPathList[x, y];
		return val >= 0 ? val : MaxDepth + 1; //put it farther than anything if invalid
	}
	private void PrintDepthTree()
	{
			for(int y = 0; y < MaxDepth; y++)
		{
			string s = ("Y" + y + ": ").PadRight(5);
			for ( int x = 0; x < MaxDepth; x++ )
			{
				if ( x > 0 ) s += " | ";
				s += (localPathList[x, y] >= 0) ? localPathList[x, y].ToString().PadLeft(2) : "  "; 
			}
			Log.Info( s );
		}
	}
	private Vector3 LocalToMap( Vector3 local )
	{
		return local + Start - new Vector3( Depth , Depth , 0 );
	}
	private Vector3 MapToLocal(Vector3 map)
	{
		return map - Start + new Vector3( Depth  , Depth , 0 );
	}
	private bool LocalCanWalk(int x, int y)
	{
		var v = LocalToMap( new Vector3( x, y ) );
		int bx = (int)v.x + WorldEntity.radius;
		int by = (int)v.y + WorldEntity.radius;
		return WorldEntity.CanWalk( bx, by );
	}
}
