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
				var verts = Floor( x, y, t.Height, GridWorld.GRID_SCALE, t.FloorTI );
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
					var verts = FloorWall( x, y, T.Height, (TN.Height - T.Height), 0, T.FloorWallTI );
					foreach ( var v in verts )
						fVerts.Add( v );
				}				
				else if (TN.Height < T.Height)
				{
					var verts = FloorWall( x, y + 1, TN.Height, (T.Height - TN.Height), 1, TN.FloorWallTI );
					foreach ( var v in verts )
						fVerts.Add( v );
				}
				
				if ( TE.Height > T.Height )
				{
					var verts = FloorWall( x, y, T.Height, (TE.Height - T.Height), 1.5f, T.FloorWallTI );
					foreach ( var v in verts )
						fVerts.Add( v );
				}
				else if ( TE.Height < T.Height )
				{
					var verts = FloorWall( x + 1, y, TE.Height, (T.Height - TE.Height), 0.5f, TE.FloorWallTI );
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

				var TN = GetTile( x, y + 1 );
				var TS = GetTile( x, y - 1 );
				var TE = GetTile( x + 1, y );
				var TW = GetTile( x - 1, y );

				var TNW = GetTile( x - 1, y + 1 );
				var TNE = GetTile( x + 1, y + 1 );
				var TSW = GetTile( x - 1, y - 1 );
				var TSE = GetTile( x + 1, y - 1 );

				List<TristVertex> verts;

				if ( N )
				{
					// These are all from the perspective of the facing wall being the "north" wall
					// North of Facing has Wall Facing Tile, ect...
					var FN = TN.SouthWall;
					var FE = TE.WestWall;
					var FW = TW.EastWall;
					// Check if there's a corner NW, ect.
					var cNWE = TNW.EastWall;
					var cNWS = TNW.SouthWall;
					var cNEW = TNE.WestWall;
					var cNES = TNE.SouthWall;

					verts = WallSeg( x, y, T.Height, N, E, W, FN, FE, FW, cNWE, cNWS, cNEW, cNES, 0, T.WallTI );
					foreach ( var v in verts )
						wVerts.Add( v );
				}				
				if ( S )
				{
					var FN = TS.NorthWall;
					var FE = TE.WestWall;
					var FW = TW.EastWall;

					var cNWE = TSE.WestWall;
					var cNWS = TSE.NorthWall;
					var cNEW = TSW.EastWall;
					var cNES = TSW.NorthWall;

					verts = WallSeg( x, y, T.Height, S, W, E, FN, FW, FE, cNWE, cNWS, cNEW, cNES, 1, T.WallTI );
					foreach ( var v in verts )
						wVerts.Add( v );
				}
				if ( E )
				{
					var FN = TE.WestWall;
					var FE = TS.NorthWall;
					var FW = TN.SouthWall;					

					var cNWE = TNE.SouthWall;
					var cNWS = TNE.WestWall;
					var cNEW = TSE.NorthWall;
					var cNES = TSE.WestWall;

					verts = WallSeg( x, y, T.Height, E, S, N, FN, FE, FW, cNWE, cNWS, cNEW, cNES, 1.5f, T.WallTI );
					foreach ( var v in verts )
						wVerts.Add( v );
				}
				if ( W )
				{
					var FN = TW.EastWall;
					var FE = TN.SouthWall;
					var FW = TS.NorthWall;					

					var cNWE = TSW.NorthWall;
					var cNWS = TSW.EastWall;
					var cNEW = TNW.SouthWall;
					var cNES = TNW.EastWall;

					verts = WallSeg( x, y, T.Height, W, N, S, FN, FE, FW, cNWE, cNWS, cNEW, cNES, 0.5f, T.WallTI );
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
	private TristVertex[] Floor( float iX, float iY, float iZ, float s, uint tI = 0 )
	{
		var x = iX * s;
		var y = iY * s;
		var z = iZ * s;
		CheckMinMax( x, y, z );

		var v = GetQuad(
			new Vector3( x, y, z ),
			new Vector3( x, y + s, z ),
			new Vector3( x + s, y, z ),
			new Vector3( x + s, y + s, z ),
			tI
		);
		
		return v;
	}
	private TristVertex[] FloorWall ( float x, float y, float z, float h, float r, uint tI = 0)
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
	private List<TristVertex> WallSeg( int x, int y, float h, bool n, bool e, bool w, bool fN, bool fE, bool fW, bool cNWE, bool cNWS, bool cNEW, bool cNES, float r, uint tI = 0 )
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

		if ( n )
		{
			//Inside Wall
			{
				float x1 = -HGS;
				float x2 = HGS;
				float y1 = GS * SL;
				if ( e ) x2 -= GS * SS;
				if ( w ) x1 += GS * SS;
				if ( !fW & !w & (cNWE & !cNWS) ) x1 -= GS * SS;
				if ( !fE & !e & (cNEW & !cNES) ) x2 += GS * SS;
				var a = Rotate( new Vector3( x1, y1, z1 ), r );
				var b = Rotate( new Vector3( x1, y1, z2 ), r );
				var c = Rotate( new Vector3( x2, y1, z1 ), r );
				var d = Rotate( new Vector3( x2, y1, z2 ), r );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, tI, (x2 - x1) / GS, 1, (x1 + HGS) / GS );//, (x2 - x1) / GS, 1, (x1 / GS - x) );
				foreach ( var v in verts )
					wVerts.Add( v );
			}

			// TOP
			{
				float x1 = -HGS;
				float x2 = HGS;
				float y1 = GS * SL;
				float y2 = HGS;
				if ( w ) x1 += GS * SS;
				//if ( !fW & !w & (cNWE & !cNWS) ) x1 -= GS * SS;
				if ( !fE & !e & (cNEW & !cNES) ) x2 += GS * SS;
				var a = Rotate( new Vector3( x1, y1, z2 ), r );
				var b = Rotate( new Vector3( x1, y2, z2 ), r );
				var c = Rotate( new Vector3( x2, y1, z2 ), r );
				var d = Rotate( new Vector3( x2, y2, z2 ), r );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, tI, (x2 - x1) / GS, SS, (x1 + HGS) / GS );


				foreach ( var v in verts )
					wVerts.Add( v );
			}

			// BACK
			if ( !fN )
			{
				float x1 = HGS;
				float x2 = -HGS;
				float y1 = HGS;
				var a = Rotate( new Vector3( x1, y1, z1 ), r );
				var b = Rotate( new Vector3( x1, y1, z2 ), r );
				var c = Rotate( new Vector3( x2, y1, z1 ), r );
				var d = Rotate( new Vector3( x2, y1, z2 ), r );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, tI );
				foreach ( var v in verts )
					wVerts.Add( v );
			}

			if ( !fE & !e & !(cNEW & !cNES) ) // East Cap
			{
				float x1 = HGS;
				float y1 = HGS - SS * GS;
				float y2 = HGS;
				var a = Rotate( new Vector3( x1, y1, z1 ), r );
				var b = Rotate( new Vector3( x1, y1, z2 ), r );
				var c = Rotate( new Vector3( x1, y2, z1 ), r );
				var d = Rotate( new Vector3( x1, y2, z2 ), r );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, tI, SS, 1, 1 - SS );
				foreach ( var v in verts )
					wVerts.Add( v );
			}
			if ( !fW & !w & !( cNWE & !cNWS ) ) // West Cap
			{
				float x1 = -HGS;
				float y1 = HGS;
				float y2 = HGS - SS * GS;
				var a = Rotate( new Vector3( x1, y1, z1 ), r );
				var b = Rotate( new Vector3( x1, y1, z2 ), r );
				var c = Rotate( new Vector3( x1, y2, z1 ), r );
				var d = Rotate( new Vector3( x1, y2, z2 ), r );
				a += off;
				b += off;
				c += off;
				d += off;

				verts = GetQuad( a, b, c, d, tI, SS );
				foreach ( var v in verts )
					wVerts.Add( v );
			}

		}

		return wVerts;
	}
}
