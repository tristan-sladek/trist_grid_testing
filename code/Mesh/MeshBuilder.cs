using Sandbox;
using System;
using System.Collections.Generic;
public class MeshBuilder
{
	float MAX = float.MinValue;
	float MIN = float.MaxValue;
	private Vertex[] GetQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float w = 1, float h = 1, float ox = 0, float oy = 0 )
	{
		Vector3 N = (v1 - v2).Cross( v1 - v3 ) * -1;
		Vector3 T = (v2 - v1).Normal * -1;
		
		Vertex a = new Vertex( v1, N, T, new Vector2( 0 + ox, h + oy ) );
		a.Color = Color.Blue;
		Vertex b = new Vertex( v2, N, T, new Vector2( 0 + ox, 0 + oy ) );
		b.Color = Color.Red;
		Vertex c = new Vertex( v3, N, T, new Vector2( w + ox, h + oy ) );
		c.Color = Color.Green;
		Vertex d = new Vertex( v4, N, T, new Vector2( w + ox, 0 - oy ) );
		d.Color = Color.White;

		return new Vertex[]{ a, c, b, d, b, c };
	}
	private void CheckMinMax(params float[] v)
	{
		foreach(var val in v)
		{
			if ( val < MIN ) MIN = val;
			if ( val > MAX ) MAX = val;
		}		
	}
	private List<Vertex> FloorPass()
	{
		List<Vertex> fVerts = new();
		for ( int x = 0; x < GridWorld.CHUNK_SIZE; x++ )
			for ( int y = 0; y < GridWorld.CHUNK_SIZE; y++ )
			{
				var t = getTile(x,y);
				var verts = Floor( x, y, t.Height, GridWorld.GRID_SCALE );
				if ( verts != null && verts.Length > 0 )
					foreach ( var v in verts )
						fVerts.Add( v );
			}
				
		return fVerts;
	}
	private List<Vertex> HorizontalPass()
	{
		List<Vertex> hVerts = new();
		for ( int y = 0; y < GridWorld.CHUNK_SIZE; y++)
			for (int x = 0;x < GridWorld.CHUNK_SIZE; x++ )
			{
				var TA = getTile(x,y);
				var TB = getTile(x + 1,y);
				if ( TA.Height == TB.Height ) continue;
				float x1 = GridWorld.GRID_SCALE;
				float y1 = GridWorld.GRID_SCALE;
				float y2 = GridWorld.GRID_SCALE;
				float z1 = GridWorld.GRID_SCALE;
				float z2 = GridWorld.GRID_SCALE;
				if ( TA.Height > TB.Height )
				{
					x1 *= x + 1;
					y1 *= y;
					y2 *= y + 1;
					z1 *= TB.Height;
					z2 *= TA.Height;
				}
				else
				{
					x1 *= x + 1;
					y1 *= y + 1;
					y2 *= y;
					z1 *= TA.Height;
					z2 *= TB.Height;
				}				
				var verts = GetQuad(
					new Vector3( x1, y1, z1 ),
					new Vector3( x1, y1, z2 ),
					new Vector3( x1, y2, z1 ),
					new Vector3( x1, y2, z2 ),
					1,
					Math.Abs( TA.Height - TB.Height )
				);
				foreach(var v in verts)
				hVerts.Add( v );
			}		
		return hVerts;
	}
	private List<Vertex> VerticalPass()
	{
		List<Vertex> vVerts = new();
		for ( int y = 0; y < GridWorld.CHUNK_SIZE; y++ )
			for ( int x = 0; x < GridWorld.CHUNK_SIZE; x++ )
			{
				var TA = getTile( x, y );
				var TB = getTile( x, y + 1 );
				if ( TA.Height == TB.Height ) continue;				
				float x1 = GridWorld.GRID_SCALE;
				float x2 = GridWorld.GRID_SCALE;
				float y1 = GridWorld.GRID_SCALE;
				float z1 = GridWorld.GRID_SCALE;
				float z2 = GridWorld.GRID_SCALE;
				if ( TA.Height > TB.Height )
				{					
					x1 *= x + 1;
					x2 *= x;
					y1 *= y + 1;
					z1 *= TB.Height;
					z2 *= TA.Height;
				}
				else
				{					
					x1 *= x;
					x2 *= x + 1;
					y1 *= y + 1;
					z1 *= TA.Height;
					z2 *= TB.Height;
				}
				var verts = GetQuad(
					new Vector3( x1, y1, z1 ),
					new Vector3( x1, y1, z2 ),
					new Vector3( x2, y1, z1 ),
					new Vector3( x2, y1, z2 ),
					1,
					Math.Abs( TA.Height - TB.Height )
				);
				foreach ( var v in verts )
					vVerts.Add( v );
			}
		return vVerts;
	}
	private Vector3 Rotate(Vector3 vec, float ang)
	{
		return new Vector3(
			vec.x * MathF.Cos( ang * MathF.PI ) - vec.y * MathF.Sin( ang * MathF.PI ),
			vec.y * MathF.Cos( ang * MathF.PI ) + vec.x * MathF.Sin( ang * MathF.PI ),
			vec.z
			);
	}
	private List<Vertex> WallSeg(int x, int y, float h, bool N, bool E, bool W, bool NW, bool EW, bool WW, float R)
	{
		float GS = GridWorld.GRID_SCALE;
		float HGS = GS / 2;
		List<Vertex> wVerts = new();

		float z1 = h * GS;
		float z2 = (h + 1) * GS;		
		float SL = 0.375f; //Scale Large (how many units off the wall?)
		float SS = 0.5f - SL; //Scale Small (inverse of this)
		Vertex[] verts;

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


				verts = GetQuad( a, b, c, d );//, (x2 - x1) / GS, 1, (x1 / GS - x) );
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

				verts = GetQuad( a, b, c, d , (x2 - x1) / GS, SS, (x1 + HGS) );
				
				
				foreach ( var v in verts )
					wVerts.Add( v );
			}

			// BACK
			if( !NW )
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

				verts = GetQuad( a, b, c, d );
				foreach ( var v in verts )
					wVerts.Add( v );
			}

			if( !EW )
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

				verts = GetQuad( a, b, c, d, SS, 1, 1 - SS );
				foreach ( var v in verts )
					wVerts.Add( v );
			}
			if ( !WW )
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

				verts = GetQuad( a, b, c, d, SS );
				foreach ( var v in verts )
					wVerts.Add( v );
			}


		}

		return wVerts;
	}
	private List<Vertex> WallPass()
	{
		List<Vertex> wVerts = new();

		for ( int y = 0; y < GridWorld.CHUNK_SIZE; y++ )
			for ( int x = 0; x < GridWorld.CHUNK_SIZE; x++ )
			{
				var T = getTile( x, y );
				bool N = (T.Walls & 0b1000) > 0;
				bool S = (T.Walls & 0b0100) > 0;
				bool E = (T.Walls & 0b0010) > 0;
				bool W = (T.Walls & 0b0001) > 0;
				bool NW = (getTile( x, y + 1 ).Walls & 0b0100) > 0; // north tile has south wall, ect..
				bool SW = (getTile( x, y - 1 ).Walls & 0b1000) > 0;
				bool EW = (getTile( x + 1, y ).Walls & 0b0001) > 0;
				bool WW = (getTile( x - 1, y ).Walls & 0b1010) > 0;

				List<Vertex> verts;

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
	private Tile getTile( int x, int y )
	{
		int cx = ChunkX;
		int cy = ChunkY;
		if ( x < 0 ) { cx--; x += GridWorld.CHUNK_SIZE; }
		if ( y < 0 ) { cy--; y += GridWorld.CHUNK_SIZE; }
		if ( x >= GridWorld.CHUNK_SIZE ) { cx++; x -= GridWorld.CHUNK_SIZE; }
		if ( y >= GridWorld.CHUNK_SIZE ) { cy++; y -= GridWorld.CHUNK_SIZE; }
		return CurrentGridWorld.GetTile( new Vector4( x, y, cx, cy) );
	}
	private GridWorld CurrentGridWorld;
	private int ChunkX;
	private int ChunkY;
	public Model Build( GridWorld currentGridWorld, int chunkX, int chunkY )
	{
		CurrentGridWorld = currentGridWorld;
		ChunkX = chunkX;
		ChunkY = chunkY;

		var wall_verts = HorizontalPass();
		var temp = VerticalPass();
		foreach ( var v in temp )
			wall_verts.Add( v );

		var m1 = Material.Load( "dev/helper/testgrid.vmat" );
		var m2 = Material.Load( "materials/dev/dev_measuregeneric01.vmat" );
		var m3 = Material.Load( "materials/dev/dev_measuregeneric01b.vmat" );
		var m4 = Material.Load( "materials/dev/debug_wireframe.vmat" );

		return new ModelBuilder()
			.AddMesh( CreateMesh( FloorPass(), m2 ) )
			.AddMesh( CreateMesh( wall_verts, m3 ) )
			.AddMesh( CreateMesh( WallPass(), m1 ) )
			.Create();
	}
	private Mesh CreateMesh( List<Vertex> verts, Material mat)
	{
		Mesh mesh = null;
		if ( verts.Count > 0 )
		{
			mesh = new Mesh( mat ) { Bounds = new BBox( MIN, MAX ) };
			mesh.CreateVertexBuffer<Vertex>( verts.Count, Vertex.Layout, verts );
		}
		return mesh;
	}
	public Vertex[] Floor( float ix, float iy, float iz, float s )
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
		);
		
		return v;
	}
}
