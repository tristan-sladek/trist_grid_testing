using Sandbox;
using System;
using System.Collections.Generic;

public partial class GridWorld
{
	public const float GRID_SCALE = 32f;
	public const int CHUNK_SIZE = 16;
	public const int CHUNK_SIZE_SQUARED = 256;
	Dictionary<string, List<Tile>> ChunkMap = new();
	Dictionary<string, ChunkEntity> TileChunkMap = new();
	public static Vector2 WorldToGrid( Vector3 world )
	{
		return world.SnapToGrid( GRID_SCALE ) / GRID_SCALE;
	}
	public static Vector3 GridToWorld( Vector2 grid )
	{
		return grid * GRID_SCALE;
	}
	public static Vector4 GridToChunk( Vector2 grid ) // x y chunkx chunky
	{
		var x = (grid.x) % CHUNK_SIZE;
		var y = (grid.y) % CHUNK_SIZE;
		x = Math.Abs( x >= 0 ? x : CHUNK_SIZE + x );
		y = Math.Abs( y >= 0 ? y : CHUNK_SIZE + y );
		var cx = (grid.x / CHUNK_SIZE).Floor();
		var cy = (grid.y / CHUNK_SIZE).Floor();
		return new Vector4( x, y, cx, cy );
	}
	public static Vector2 ChunkToGrid( Vector4 chunk )
	{
		return new Vector2( chunk.x + (chunk.z * GRID_SCALE), chunk.y + (chunk.w * GRID_SCALE) );
	}
	public ChunkEntity GetTileChunk( int cx, int cy )
	{
		if ( !TileChunkMap.ContainsKey( "X" + cx + "Y" + cy ) )
		{
			var TileChunk = new ChunkEntity();
			TileChunk.ChunkX = cx;
			TileChunk.ChunkY = cy;
			TileChunk.Position = new Vector3( cx * GRID_SCALE * CHUNK_SIZE - GRID_SCALE / 2, cy * GRID_SCALE * CHUNK_SIZE - GRID_SCALE / 2, 1 );
			TileChunkMap.Add( "X" + cx + "Y" + cy, TileChunk );
			QueueTileChunk( TileChunk );			
			return TileChunk;
		}
		return TileChunkMap["X" + cx + "Y" + cy];
	}
	public List<Tile> GenerateChunk( int cx, int cy )
	{
		var b = new Tile[CHUNK_SIZE, CHUNK_SIZE];
		List<Tile> TileChunk = new();

		for ( int y = 0; y < CHUNK_SIZE; y++ )
			for ( int x = 0; x < CHUNK_SIZE; x++ )
				b[x, y] = new();

				/*
				for ( int x = 0; x < CHUNK_SIZE; x++ )
					for ( int y = 0; y < CHUNK_SIZE; y++ )
						b[x, y] = Rand.Int( 0, 2 );
				*/

		if ( cx == 0 && cy == 0 )
		{
			b[0, 0].Height = 0.5f;
			b[0, 0].Walls = 0b0101;
			b[2, 0].Height = 1;
			b[0, 2].Height = 1.5f;

			b[4, 4].Walls = 0b1111;
			b[5, 4].Walls = 0b1011;
			b[6, 4].Walls = 0b1001;
		}
		if( cx == 1 && cy == 1)
		{
			var m = new MazeGen().GenerateMaze( 16 );
			foreach( Tile t in m)
				TileChunk.Add( t );
			return TileChunk;
		}
				
		var d = 3;
		if(cx > d || cy > d || cx < -d || cy < -d)
			for ( int x = 0; x < CHUNK_SIZE; x++ )
				for ( int y = 0; y < CHUNK_SIZE; y++ )
					b[x, y].Height = 30;



		// Load Heightmap		
		for ( int y = 0; y < CHUNK_SIZE; y++ )
			for ( int x = 0; x < CHUNK_SIZE; x++ )
			{
				TileChunk.Add( b[x,y] );
			}
		return TileChunk;
	}
	public List<Tile> GetChunk( int cx, int cy )
	{
		if ( !ChunkMap.ContainsKey( "X" + cx + "Y" + cy ) )
		{
			var TileChunk = GenerateChunk( cx, cy );
			ChunkMap.Add( "X" + cx + "Y" + cy, TileChunk );
			return TileChunk;
		}

		return ChunkMap["X" + cx + "Y" + cy];
	}
	public Tile GetTile( Vector4 chunk )
	{
		int x = (int)chunk.x;
		int y = (int)chunk.y;
		int cx = (int)chunk.z;
		int cy = (int)chunk.w;
		/*
		while ( x < 0 ) { cx--; x += CHUNK_SIZE; }
		while ( y < 0 ) { cy--; y += CHUNK_SIZE; }
		while ( x >= CHUNK_SIZE ) { cx++; x -= CHUNK_SIZE; }
		while ( y >= CHUNK_SIZE ) { cy++; y -= CHUNK_SIZE; }
		*/
		var ChunkMap = GetChunk( cx, cy );
		//Log.Info( x + " | " + y + " " + (x + y * CHUNK_SIZE) );
		return ChunkMap[x + y * CHUNK_SIZE];
	}
	public float GetHeightFromWorld( Vector3 world )
	{
		return GetHeightFromChunk( GridToChunk( WorldToGrid( world ) ) );
	}
	public float GetHeightFromGrid( Vector2 v1 )
	{
		return GetHeightFromChunk( GridToChunk( v1 ) );
	}
	public float GetHeightFromChunk( Vector4 chunk )
	{
		return GetTile( chunk ).Height;
	}
	List<ChunkEntity> ModelQueue = new();
	long lastGen = 0;
	public void QueueTileChunk(ChunkEntity TileChunk)
	{
		ModelQueue.Add( TileChunk );
	}
	public void Simulate()
	{
		long currGen = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		if ( ModelQueue.Count > 0 && currGen - lastGen > 100)
		{
			ChunkEntity c = ModelQueue[0];
			ModelQueue.RemoveAt( 0 );
			c.SetChunkMDL( new MeshBuilder().Build( this, c.ChunkX, c.ChunkY ) );
			lastGen = currGen;
		}
			
	}
}

