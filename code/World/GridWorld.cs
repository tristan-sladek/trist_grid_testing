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
	public static Vector2 WorldToGrid(Vector3 world)
	{
		return world.SnapToGrid( GRID_SCALE ) / GRID_SCALE;
	}
	public static Vector3 GridToWorld(Vector2 grid)
	{
		return grid * GRID_SCALE;
	}
	public static Vector4 GridToChunk(Vector2 grid ) // x y chunkx chunky
	{
		return new Vector4(grid.x % CHUNK_SIZE , grid.y % CHUNK_SIZE , (int)(grid.x / CHUNK_SIZE), (int)(grid.y / CHUNK_SIZE) );
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
			TileChunk.Position = new Vector3( cx * GRID_SCALE * CHUNK_SIZE, cy * GRID_SCALE * CHUNK_SIZE, 1 );
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

		for ( int x = 0; x < CHUNK_SIZE; x++ )
			for ( int y = 0; y < CHUNK_SIZE; y++ )
				b[x, y] = Rand.Int( 0, 2 );

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

		var TileChunk = GetChunk( cx, cy );
		return TileChunk[x + y * CHUNK_SIZE];
	}
	public float GetHeightFromWorld( float x, float y )
	{
		return GetHeightFromChunk( GridToChunk( WorldToGrid( new Vector2( x, y ) ) ) );
	}
	public float GetHeightFromGrid( int x, int y )
	{
		return GetHeightFromChunk( GridToChunk( new Vector2( x, y ) ) );
	}
	public float GetHeightFromChunk( Vector4 chunk)
	{
		return GetTile( chunk ).Height;
	}
	public bool GetCanWalkAtoBGrid( int ax, int ay, int bx, int by )
	{
		var ha = GetHeightFromGrid( ax, ay );
		var hb = GetHeightFromGrid( bx, by );
		return (hb - ha) <= 0.5;
	}
}
