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
	public static Vector2 WorldToGrid(Vector3 world )
	{
		return world.SnapToGrid( GRID_SCALE ) / GRID_SCALE;
	}
	public static Vector3 GridToWorld(Vector2 grid)
	{
		return grid * GRID_SCALE;
	}
	public static Vector4 GridToChunk(Vector2 grid ) // x y chunkx chunky
	{
		var x = (grid.x) % CHUNK_SIZE;
		var y = (grid.y) % CHUNK_SIZE;
		x = Math.Abs(x >= 0 ? x : CHUNK_SIZE + x);
		y = Math.Abs(y >= 0 ? y : CHUNK_SIZE + y);
		var cx = (grid.x / CHUNK_SIZE).SnapToGrid( 1 );
		var cy = (grid.y / CHUNK_SIZE).SnapToGrid( 1 );
		return new Vector4( x, y, cx, cy );
	}
	public static Vector2 ChunkToGrid( Vector4 chunk )
	{
		return new Vector2( chunk.x + (chunk.z * GRID_SCALE), chunk.y + (chunk.w * GRID_SCALE) );
	}
	public ChunkEntity GetTileChunk(int cx, int cy)
	{
		if ( !TileChunkMap.ContainsKey( "X" + cx + "Y" + cy ) )
		{
			var TileChunk = new ChunkEntity();
			TileChunk.ChunkX = cx;
			TileChunk.ChunkY = cy;
			TileChunk.Position = new Vector3( cx * GRID_SCALE * CHUNK_SIZE - GRID_SCALE / 2, cy * GRID_SCALE * CHUNK_SIZE - GRID_SCALE / 2, 1 );
			TileChunk.SetChunkMDL( new MeshBuilder().Build( this, cx, cy ) );
			TileChunkMap.Add( "X" + cx + "Y" + cy, TileChunk );
			return TileChunk;
		}

		return TileChunkMap["X" + cx + "Y" + cy];
	}
	public List<Tile> GenerateChunk(int cx, int cy)
	{
		var b = new int[CHUNK_SIZE, CHUNK_SIZE];
		List<Tile> TileChunk = new();

		/*
		for ( int x = 0; x < CHUNK_SIZE; x++ )
			for ( int y = 0; y < CHUNK_SIZE; y++ )
				b[x, y] = Rand.Int( 0, 2 );
		*/
		if ( cx == 0 && cy == 0 )
		{
			b[0, 0] = 1;
			b[2, 0] = 2;
			b[0, 2] = 3;
		}
		
		if( cx == -1 && cy == -1 )
		{
			b[0, 0] = 1;
			b[2, 0] = 2;
			b[0, 2] = 3;
		}



		// Load Heightmap		
		for ( int y = 0; y < CHUNK_SIZE; y++ )
			for ( int x = 0; x < CHUNK_SIZE; x++ )
			{
				var t = new Tile();
				t.Height = (float)(b[x, y]) / 2f;
				TileChunk.Add( t );
			}
		return TileChunk;
	}
	public List<Tile> GetChunk(int cx, int cy)
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
		var TileChunk = GetChunk( cx, cy );
		//Log.Info( x + " | " + y + " " + (x + y * CHUNK_SIZE) );
		return TileChunk[x + y * CHUNK_SIZE];
	}
	public float GetHeightFromWorld( Vector3 world )
	{
		return GetHeightFromChunk( GridToChunk( WorldToGrid( world ) ) );
	}
	public float GetHeightFromGrid( Vector2 v1 )
	{
		return GetHeightFromChunk( GridToChunk( v1 ) );
	}
	public float GetHeightFromChunk( Vector4 chunk)
	{
		return GetTile( chunk ).Height;
	}
}
