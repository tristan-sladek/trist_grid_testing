using Sandbox;
using System.Collections.Generic;
public class MeshBuilder
{
	List<Vertex> floor_vertices = new();
	List<Vertex> wall_vertices = new();
	float MAX = float.MinValue;
	float MIN = float.MaxValue;
	private void AddVerts(bool floor, params Vertex[] v)
	{
		if(floor)
			foreach ( var vertex in v )
			{
				var V = vertex;
				V.Color = Color.Blue;
				floor_vertices.Add( V );
			}
				
		else
			foreach ( var vertex in v )
			{
				var V = vertex;
				V.Color = Color.Red;
				wall_vertices.Add( V );
			}
				
	}
	private void CheckMinMax(params float[] v)
	{
		foreach(var val in v)
		{
			if ( val < MIN ) MIN = val;
			if ( val > MAX ) MAX = val;
		}		
	}
	public Model Build()
	{
		Mesh floor_mesh = null;
		Mesh wall_mesh = null;
		if ( floor_vertices.Count > 0 )
		{
			//floor_mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01.vmat" ) )
			floor_mesh = new Mesh( Material.Load( "materials/dev/debug_wireframe.vmat" ) )
			{
				Bounds = new BBox( MIN, MAX )
			};
			floor_mesh.CreateVertexBuffer<Vertex>( floor_vertices.Count, Vertex.Layout, floor_vertices.ToArray() );
		}

		if ( wall_vertices.Count > 0 )
		{
			//wall_mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01b.vmat" ) )
			wall_mesh = new Mesh( Material.Load( "materials/dev/debug_wireframe.vmat" ) )
			{
				Bounds = new BBox( MIN, MAX )
			};
			wall_mesh.CreateVertexBuffer<Vertex>( wall_vertices.Count, Vertex.Layout, wall_vertices.ToArray() );
		}
		Log.Info("Mesh created with " + (floor_vertices.Count + wall_vertices.Count) + " verts.");
		return new ModelBuilder().AddMesh( floor_mesh ).AddMesh(wall_mesh).Create();
	}
	public MeshBuilder AddFloor( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Up;
		var T = Vector3.Forward;

		var a = new Vertex( new Vector3( x, y, z ), N, T, new Vector2( 0, 0 ) );
		var b = new Vertex( new Vector3( x + s, y, z ), N, T, new Vector2( 1, 0 ) );
		var c = new Vertex( new Vector3( x + s, y + s, z ), N, T, new Vector2( 1, 1 ) );
		var d = new Vertex( new Vector3( x, y + s, z ), N, T, new Vector2( 0, 1 ) );

		AddVerts( true, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallE( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Forward; // normal
		var T = Vector3.Down;    // tangent

		var of = x + s; // x is offset by grid_scale
		//square is on y * z plane
		var v1 = y;
		var v2 = y + s * w;
		var v3 = z;
		var v4 = z + s * h;

		var a = new Vertex( new Vector3( of, v1, v3 ), N, T, new Vector2( 0, 0 ) );
		var b = new Vertex( new Vector3( of, v2, v3 ), N, T, new Vector2( w, 0 ) );
		var c = new Vertex( new Vector3( of, v2, v4 ), N, T, new Vector2( w, h ) );
		var d = new Vertex( new Vector3( of, v1, v4 ), N, T, new Vector2( 0, h ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallW( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Backward; // normal
		var T = Vector3.Up;		  // tangent

		//square is on y * z plane
		var v1 = y;
		var v2 = y + s * w;
		var v3 = z;
		var v4 = z + s * h;

		var a = new Vertex( new Vector3( x, v1, v3 ), N, T, new Vector2( 0, 0 ) );
		var b = new Vertex( new Vector3( x, v1, v4 ), N, T, new Vector2( h, 0 ) );
		var c = new Vertex( new Vector3( x, v2, v4 ), N, T, new Vector2( h, w ) );
		var d = new Vertex( new Vector3( x, v2, v3 ), N, T, new Vector2( 0, w ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallS( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Right; // normal
		var T = Vector3.Down;  // tangent

		//square is on x * z plane
		var v1 = x;
		var v2 = x + s * w;
		var v3 = z;
		var v4 = z + s * h;

		var a = new Vertex( new Vector3( v1, y, v3 ), N, T, new Vector2( 0, 0 ) );
		var b = new Vertex( new Vector3( v2, y, v3 ), N, T, new Vector2( w, 0 ) );
		var c = new Vertex( new Vector3( v2, y, v4 ), N, T, new Vector2( w, h ) );
		var d = new Vertex( new Vector3( v1, y, v4 ), N, T, new Vector2( 0, h ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallN( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Left; // normal
		var T = Vector3.Down; // tangent
		
		var of = y + s; // y is offset by grid_scale
		//square is on x * z plane
		var v1 = x;
		var v2 = x + s * w;
		var v3 = z;
		var v4 = z + s * h;

		var a = new Vertex( new Vector3( v1, of, v3 ), N, T, new Vector2( 0, 0 ) );
		var b = new Vertex( new Vector3( v1, of, v4 ), N, T, new Vector2( h, 0 ) );
		var c = new Vertex( new Vector3( v2, of, v4 ), N, T, new Vector2( h, w ) );
		var d = new Vertex( new Vector3( v2, of, v3 ), N, T, new Vector2( 0, w ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
}
