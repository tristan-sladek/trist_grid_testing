using Sandbox;
using System;
using System.Collections.Generic;
internal class PathNode
{
	public int X, Y;
	public PathNode( int x, int y) { X = x; Y = y; }
	public PathNode( float x, float y) { X = (int)x; Y = (int)y; }
}

public partial class Pathfind
{
	GridWorld CurrentGridWorld { get; set; }
	public bool CanPathfind { get; set; }
	public Vector2 NextPos { get; set; } = Vector3.Zero;
	private Vector3 Start { get; set; }
	private Vector3 End { get; set; }
	public Pathfind( GridWorld currentGridWorld )
	{
		CurrentGridWorld = currentGridWorld;
	}
	public Pathfind FromPosition(Vector3 InStart )
	{
		Start = InStart;
		Start.SnapToGrid( 1 ); //make sure everything is clean
		return this;
	}
	public Pathfind ToPosition(Vector3 InEnd )
	{
		End = InEnd;
		End.SnapToGrid( 1 ); //make sure everything is clean
		return this;
	}
	private int[,] localPathList;
	private int Depth;
	private int MaxDepth;
	private Vector3 localStart;
	private Vector3 localEnd;
	public Pathfind Path( int depth = 16 )
	{
		Depth = depth;
		MaxDepth = (depth * 2) + 1; //Maximum Depth Index, Depth is the radius + 1 for the center square.
		localPathList = new int[MaxDepth, MaxDepth];
		for ( int x = 0; x < MaxDepth; x++ )
			for ( int y = 0; y < MaxDepth; y++ )
				localPathList[x, y] = -1; //default to unchecked;
		
		localStart = WorldToLocal( Start );
		localEnd = WorldToLocal( End );
		// Log.Info( "MAP: " + Start + " | " + End + " : LOCAL: " + localStart + " | " + localEnd );
		if ( localStart == localEnd ) return this; //Don't even try to pathfind to self
		
		var localDist = (localEnd - localStart); 
		if ( Math.Abs(localDist.x) > Depth || Math.Abs( localDist.y ) > Depth ) //distance in square
			return this; //quick cancel on distance beyond reach

		//BFS to find path
		List<PathNode> Queue = new();
		Queue.Add( new PathNode(localStart.x,localStart.y) );
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
			int d = localPathList[x,y];

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
			GenerateRoute();
		}
		
		//PrintDepthTree();

		return this;
	}
	private void GenerateRoute()
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
			Route.Add( new PathNode( x, y ) );
			if ( ++s > MaxDepth )
			{
				CanPathfind = false;
				//Log.Error( "Force Break! " + x + " | " + y ); PrintDepthTree();
				break;
			}

			int minX = x;
			int minY = y;
			int minD = d;

			if ( GetDSafe( x - 1, y ) < minD && LocalCanWalkTo(x - 1, y, x, y ) )
			{
				minD = GetDSafe( x - 1, y );
				minX = x - 1;
				minY = y;
			}
			if ( GetDSafe( x + 1, y ) < minD && LocalCanWalkTo( x + 1, y, x, y ) )
			{
				minD = GetDSafe( x + 1, y );
				minX = x + 1;
				minY = y;
			}
			if ( GetDSafe( x, y - 1 ) < minD && LocalCanWalkTo( x, y - 1, x, y ) )
			{
				minD = GetDSafe( x, y - 1 );
				minX = x;
				minY = y - 1;
			}
			if ( GetDSafe( x, y + 1 ) < minD && LocalCanWalkTo( x, y + 1, x, y ) )
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
	private void CheckDSafe(int x, int y, List<PathNode> Queue, int lx, int ly, int ld)
	{ //Used for checking unvisited neighbors
		if ( !(x < MaxDepth && x >= 0 && y < MaxDepth && y >= 0) ) return; // In Range
		
		var v = localPathList[x , y]; //Current Distance from start

		if ( v < 0 ) //-1, unvisited
		{
			if ( LocalCanWalkTo( lx , ly, x, y ) ) //Check if this is blocked via world ent
			{
				Queue.Add( new PathNode( x , y ) );
				localPathList[x , y] = ld + 1;
			}
			else
			{
				localPathList[x , y] = MaxDepth + 1; //MaxDepth + 1, farther than anything
			}
		}
		else if ( v < ld && LocalCanWalkTo( lx, ly, x, y ) && LocalCanWalkTo( x, y, lx, ly ) ) //more efficient path found from prev stuff
		{
			ld = v + 1;
			localPathList[lx, ly] = ld; //just update to make things smoother
		}
		else if( v == MaxDepth + 1 && LocalCanWalkTo( lx,ly,x,y) ) //marked as wall from one side, but I can walk to it.
		{
			Queue.Insert(0, new PathNode( x, y ) );
			localPathList[x, y] = ld + 1;
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
		Log.Info( "-----------------------------------------------" );
		for(int y = MaxDepth - 1; y >= 0 ; y--)
		{
			string s = ("Y" + y + ": ").PadRight(5);
			for ( int x = 0; x < MaxDepth; x++ )
			{
				if ( x > 0 ) s += " | ";
				if ( localStart.x == x && localStart.y == y )
					s += " S";
				else if ( localEnd.x == x && localEnd.y == y )
					s += " E";
				else
					s += (localPathList[x, y] >= 0) ? localPathList[x, y].ToString().PadLeft(2) : "  ";
			}
			Log.Info( s );
		}
	}
	public Vector3 LocalToWorld( Vector2 local )
	{
		return GridWorld.GridToWorld( GridWorld.WorldToGrid( Start ) + local - new Vector2( Depth, Depth ) );
	}
	public Vector2 WorldToLocal( Vector3 world )
	{
		return GridWorld.WorldToGrid( world - Start ) + new Vector2( Depth  , Depth);
	}
	private bool LocalCanWalkTo(int xb, int yb, int xe, int ye)
	{
		var ha = CurrentGridWorld.GetHeightFromWorld( LocalToWorld( new Vector2( xb, yb ) ) );
		var hb = CurrentGridWorld.GetHeightFromWorld( LocalToWorld( new Vector2( xe, ye ) ) );
		return (hb - ha) <= 0.5;
	}	
}

