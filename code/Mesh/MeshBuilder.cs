using Sandbox;
using System;
using System.Collections.Generic;
public class MeshBuilder
{
	float MAX = float.MinValue;
	float MIN = float.MaxValue;
	private Vertex[] GetQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float w = 1, float h = 1 )
	{
		Vector3 N = (v1 - v2).Cross( v1 - v3 ) * -1;
		Vector3 T = (v2 - v1).Normal * -1;
		
		Vertex a = new Vertex( v1, N, T, new Vector2( 0, h ) );
		a.Color = Color.Blue;
		Vertex b = new Vertex( v2, N, T, new Vector2( 0, 0 ) );
		b.Color = Color.Red;
		Vertex c = new Vertex( v3, N, T, new Vector2( w, h ) );
		c.Color = Color.Green;
		Vertex d = new Vertex( v4, N, T, new Vector2( w, 0 ) );
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
		for ( int y = -1; y < GridWorld.CHUNK_SIZE; y++)
			for (int x = -1;x < GridWorld.CHUNK_SIZE; x++ )
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
		for ( int y = -1; y < GridWorld.CHUNK_SIZE; y++ )
			for ( int x = -1; x < GridWorld.CHUNK_SIZE; x++ )
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

		var verts = FloorPass();
		var hVerts = HorizontalPass();
		var vVerts = VerticalPass();

		foreach ( var v in hVerts )
			verts.Add( v );
		foreach ( var v in vVerts )
			verts.Add( v );

		Mesh mesh = null;		
		if ( verts.Count > 0 )
		{
			
			mesh = new Mesh( Material.Load( "dev/helper/testgrid.vmat" ) )
			//mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01.vmat" ) )
			//mesh = new Mesh( Material.Load( "materials/dev/debug_wireframe.vmat" ) )
			{
				Bounds = new BBox( MIN, MAX )
			};
			mesh.CreateVertexBuffer<Vertex>( verts.Count, Vertex.Layout, verts );
		}

		Log.Info("Mesh created with " + (verts.Count) + " verts.");
		
		return new ModelBuilder().AddMesh(mesh).Create();
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
