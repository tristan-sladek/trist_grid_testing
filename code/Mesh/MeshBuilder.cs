using Sandbox;
using System;
using System.Collections.Generic;
public class MeshBuilder
{
	float MAX = float.MinValue;
	float MIN = float.MaxValue;
	private GridWorld CurrentGridWorld;
	private int ChunkX;
	private int ChunkY;
	public Model Build( GridWorld currentGridWorld, int chunkX, int chunkY )
	{
		CurrentGridWorld = currentGridWorld;
		ChunkX = chunkX;
		ChunkY = chunkY;

		var m1 = Material.Load( "dev/helper/testgrid.vmat" );
		var m2 = Material.Load( "materials/dev/dev_measuregeneric01.vmat" );
		var m3 = Material.Load( "materials/dev/dev_measuregeneric01b.vmat" );
		var m4 = Material.Load( "materials/dev/debug_wireframe.vmat" );
		var m5 = Material.Load( "materials/vertex_lookup.vmat" );

		return new ModelBuilder()
			.AddMesh( CreateMesh( FloorPass(), m5 ) )
			.AddMesh( CreateMesh( FloorWallPass(), m5 ) )
			.AddMesh( CreateMesh( WallPass(), m5 ) )
			.Create();
	}
	private Mesh CreateMesh( List<TristVertex> verts, Material mat )
	{
		Mesh mesh = null;
		if ( verts.Count > 0 )
		{
			mesh = new Mesh( mat ) { Bounds = new BBox( MIN, MAX ) };
			mesh.CreateVertexBuffer<TristVertex>( verts.Count, TristVertex.Layout, verts );
		}
		return mesh;
	}
	private void CheckMinMax( params float[] v )
	{
		foreach ( var val in v )
		{
			if ( val < MIN ) MIN = val;
			if ( val > MAX ) MAX = val;
		}
	}
	// Main quad function, handles generating Normals and Tangent from vectors;
	// Params: v1 - v4 are coordinates in world space, in order it's x,y | x + 1, y | x, y + 1 | x + 1, y + 1
	// TI : Texture Index, this value controls the ID of the texture being pulled from the Texture Atlas material
	// W, H, OX, OY : These are Width, Height, Offset X, Offset Y; they control the subset drawing of the UV map
	//					Note, setting W or H over 1 will loop it on materials, useful for repeated textures.
	private TristVertex[] GetQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, uint ti = 1, float w = 1, float h = 1, float ox = 0, float oy = 0 )
	{
		Vector3 N = (v1 - v2).Cross( v1 - v3 ) * -1;
		Vector3 T = (v2 - v1).Normal * -1;
		
		TristVertex a = new TristVertex( v1, N, T, new Vector2( 0 + ox, h + oy ), ti );		
		TristVertex b = new TristVertex( v2, N, T, new Vector2( 0 + ox, 0 + oy ), ti );		
		TristVertex c = new TristVertex( v3, N, T, new Vector2( w + ox, h + oy ), ti );		
		TristVertex d = new TristVertex( v4, N, T, new Vector2( w + ox, 0 - oy ), ti );
		
		return new TristVertex[]{ a, c, b, d, b, c };
	}
	// This loops over all tiles to generate the floor quads based upon their height.
	private List<TristVertex> FloorPass()
	{
		List<TristVertex> fVerts = new();
		for ( int x = 0; x < GridWorld.CHUNK_SIZE; x++ )
			for ( int y = 0; y < GridWorld.CHUNK_SIZE; y++ )
			{
				var t = GetTile(x,y);
				var verts = Floor( x, y, t.Height, GridWorld.GRID_SCALE );
				if ( verts != null && verts.Length > 0 )
					foreach ( var v in verts )
						fVerts.Add( v );
			}
				
		return fVerts;
	}
	// This generates all walls inbetween differring floor heights
	private List<TristVertex> FloorWallPass()
	{
		var CS = GridWorld.CHUNK_SIZE;
		List<TristVertex> fVerts = new();
		for(int y = 0; y < CS; y++ )
			for(int x = 0; x < CS; x++ )
			{
				var T = GetTile( x, y );		//Current Tile
				var TE = GetTile( x + 1, y );	// East Tile
				var TN = GetTile( x, y + 1 );	// North Tile

				// Can't use implicit else because quad drawn on TN.Height == T.Height
				if(TN.Height > T.Height)
				{
					// TODO: replace texture index with tile's attribute
					var verts = FloorWall( x, y, T.Height, (TN.Height - T.Height), 0, 1 );
					foreach ( var v in verts )
						fVerts.Add( v );
				}				
				else if (TN.Height < T.Height)
				{
					var verts = FloorWall( x, y + 1, TN.Height, (T.Height - TN.Height), 1, 1 );
					foreach ( var v in verts )
						fVerts.Add( v );
				}
				
				if ( TE.Height > T.Height )
				{
					var verts = FloorWall( x, y, T.Height, (TE.Height - T.Height), 1.5f, 1 );
					foreach ( var v in verts )
						fVerts.Add( v );
				}
				else if ( TE.Height < T.Height )
				{
					var verts = FloorWall( x + 1, y, TE.Height, (T.Height - TE.Height), 0.5f, 1 );
					foreach ( var v in verts )
						fVerts.Add( v );
				}
				

			}
		return fVerts;
	}
	// This generates walls based upon the tile's Wall Attribute. 0b1111 in order of walls is 0bNSEW
	private List<TristVertex> WallPass()
	{
		List<TristVertex> wVerts = new();

		for ( int y = 0; y < GridWorld.CHUNK_SIZE; y++ )
			for ( int x = 0; x < GridWorld.CHUNK_SIZE; x++ )
			{
				var T = GetTile( x, y );
				bool N = T.NorthWall;
				bool S = T.SouthWall;
				bool E = T.EastWall;
				bool W = T.WestWall;
				bool NW = GetTile( x, y + 1 ).SouthWall; // north tile has south wall, ect..
				bool SW = GetTile( x, y - 1 ).NorthWall;
				bool EW = GetTile( x + 1, y ).WestWall;
				bool WW = GetTile( x - 1, y ).EastWall;

				List<TristVertex> verts;

				if ( N )
				{
					verts = WallSeg( x, y, T.Height, N, E, W, NW, EW, WW, 0 );
					foreach ( var v in verts )
						wVerts.Add( v );
				}				
				if ( S )
				{
					verts = WallSeg( x, y, T.Height, S, W, E, SW, WW, EW, 1 );
					foreach ( var v in verts )
						wVerts.Add( v );
				}
				if ( E )
				{
					verts = WallSeg( x, y, T.Height, E, S, N, EW, SW, NW, 1.5f );
					foreach ( var v in verts )
						wVerts.Add( v );
				}
				if ( W )
				{
					verts = WallSeg( x, y, T.Height, W, N, S, WW, NW, SW, 0.5f );
					foreach ( var v in verts )
						wVerts.Add( v );
				}
				
			}

		return wVerts;
	}
	// Rotates a vector's X Y coordinates by 1 radian
	private Vector3 Rotate( Vector3 vec, float ang )
	{
		return new Vector3(
			vec.x * MathF.Cos( ang * MathF.PI ) - vec.y * MathF.Sin( ang * MathF.PI ),
			vec.y * MathF.Cos( ang * MathF.PI ) + vec.x * MathF.Sin( ang * MathF.PI ),
			vec.z
			);
	}
	// Retrieves a tile from the world using GRID SPACE coordinates
	private Tile GetTile( int x, int y )
	{
		int cx = ChunkX;
		int cy = ChunkY;
		if ( x < 0 ) { cx--; x += GridWorld.CHUNK_SIZE; }
		if ( y < 0 ) { cy--; y += GridWorld.CHUNK_SIZE; }
		if ( x >= GridWorld.CHUNK_SIZE ) { cx++; x -= GridWorld.CHUNK_SIZE; }
		if ( y >= GridWorld.CHUNK_SIZE ) { cy++; y -= GridWorld.CHUNK_SIZE; }
		return CurrentGridWorld.GetTile( new Vector4( x, y, cx, cy ) );
	}
	private TristVertex[] Floor( float ix, float iy, float iz, float s )
	{
		var x = ix * s;
		var y = iy * s;
		var z = iz * s;
		CheckMinMax( x, y, z );

		var v = GetQuad(
			new Vector3( x, y, z ),
			new Vector3( x, y + s, z ),
			new Vector3( x + s, y, z ),
			new Vector3( x + s, y + s, z )
			, 0
		);
		
		return v;
	}
	private TristVertex[] FloorWall ( float x, float y, float z, float h, float r, uint tI = 1)
	{
		// Provides functionality for building a quad on the south face of a tile,
		// allows for rotation to support E,W, and S tiles.

		var GS = GridWorld.GRID_SCALE;
		var HGS = GS / 2;

		var x1 = -HGS;
		var x2 = HGS;
		var y1 = HGS;
		var z2 = h * GS;

		// offset to move tile by when it is calculated
		var off = new Vector3( x * GS + HGS, y * GS + HGS, z * GS );

		// Vectors calculated from the perspective of 0,0 being the tile to rotate from
		var a = Rotate( new Vector3( x1, y1, 0 ), r);
		var b = Rotate( new Vector3( x1, y1, z2  ), r);
		var c = Rotate( new Vector3( x2, y1, 0 ), r);
		var d = Rotate( new Vector3( x2, y1, z2 ), r);
		
		// Local tile moved to sit at proper offset;
		a += off;
		b += off;
		c += off;
		d += off;

		return GetQuad( a, b, c, d, tI, 1, h );
	}
	private List<TristVertex> WallSeg( int x, int y, float h, bool N, bool E, bool W, bool NW, bool EW, bool WW, float R )
	{
		float GS = GridWorld.GRID_SCALE;
		float HGS = GS / 2;
		List<TristVertex> wVerts = new();

		float z1 = h * GS;
		float z2 = (h + 1) * GS;
		float SL = 0.375f; //Scale Large (how many units off the wall?)
		float SS = 0.5f - SL; //Scale Small (inverse of this)
		TristVertex[] verts;

		var off = new Vector3( x * GS + GS / 2, y * GS + GS / 2, 0 );

		if ( N )
		{
			//Inside Wall
			{
				float x1 = -HGS;
				float x2 = HGS;
				float y1 = GS * SL;
				if ( E ) x2 -= GS * SS;
				if ( W ) x1 += GS * SS;
				var a = Rotate( new Vector3( x1, y1, z1 ), R );
				var b = Rotate( new Vector3( x1, y1, z2 ), R );
				var c = Rotate( new Vector3( x2, y1, z1 ), R );
				var d = Rotate( new Vector3( x2, y1, z2 ), R );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, 2, (x2 - x1) / GS, 1, (x1 + HGS) / GS );//, (x2 - x1) / GS, 1, (x1 / GS - x) );
				foreach ( var v in verts )
					wVerts.Add( v );
			}

			// TOP
			{
				float x1 = -HGS;
				float x2 = HGS;
				float y1 = GS * SL;
				float y2 = HGS;
				if ( W ) x1 += GS * SS;
				var a = Rotate( new Vector3( x1, y1, z2 ), R );
				var b = Rotate( new Vector3( x1, y2, z2 ), R );
				var c = Rotate( new Vector3( x2, y1, z2 ), R );
				var d = Rotate( new Vector3( x2, y2, z2 ), R );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, 2, (x2 - x1) / GS, SS, (x1 + HGS) / GS );


				foreach ( var v in verts )
					wVerts.Add( v );
			}

			// BACK
			if ( !NW )
			{
				float x1 = HGS;
				float x2 = -HGS;
				float y1 = HGS;
				var a = Rotate( new Vector3( x1, y1, z1 ), R );
				var b = Rotate( new Vector3( x1, y1, z2 ), R );
				var c = Rotate( new Vector3( x2, y1, z1 ), R );
				var d = Rotate( new Vector3( x2, y1, z2 ), R );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, 2 );
				foreach ( var v in verts )
					wVerts.Add( v );
			}

			if ( !EW & !E )
			{
				float x1 = HGS;
				float y1 = HGS - SS * GS;
				float y2 = HGS;
				var a = Rotate( new Vector3( x1, y1, z1 ), R );
				var b = Rotate( new Vector3( x1, y1, z2 ), R );
				var c = Rotate( new Vector3( x1, y2, z1 ), R );
				var d = Rotate( new Vector3( x1, y2, z2 ), R );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, 2, SS, 1, 1 - SS );
				foreach ( var v in verts )
					wVerts.Add( v );
			}
			if ( !WW & !W )
			{
				float x1 = -HGS;
				float y1 = HGS;
				float y2 = HGS - SS * GS;
				var a = Rotate( new Vector3( x1, y1, z1 ), R );
				var b = Rotate( new Vector3( x1, y1, z2 ), R );
				var c = Rotate( new Vector3( x1, y2, z1 ), R );
				var d = Rotate( new Vector3( x1, y2, z2 ), R );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, 2, SS );
				foreach ( var v in verts )
					wVerts.Add( v );
			}


		}

		return wVerts;
	}
}
