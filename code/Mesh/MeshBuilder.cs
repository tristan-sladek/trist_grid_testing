using Sandbox;
using System.Collections.Generic;
public class MeshBuilder
{
	List<SimpleVertex> floor_vertices = new();
	List<SimpleVertex> wall_vertices = new();
	float MAX = float.MinValue;
	float MIN = float.MaxValue;
	
	private void AddVerts(bool floor, params SimpleVertex[] v)
	{
		if(floor)
			foreach ( var vertex in v )
				floor_vertices.Add( vertex );
		else
			foreach ( var vertex in v )
				wall_vertices.Add( vertex );
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
			floor_mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01.vmat" ) )
			{
				Bounds = new BBox( MIN, MAX )
			};
			floor_mesh.CreateVertexBuffer<SimpleVertex>( floor_vertices.Count, SimpleVertex.Layout, floor_vertices.ToArray() );
		}

		if ( wall_vertices.Count > 0 )
		{
			wall_mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01b.vmat" ) )
			{
				Bounds = new BBox( MIN, MAX )
			};
			wall_mesh.CreateVertexBuffer<SimpleVertex>( wall_vertices.Count, SimpleVertex.Layout, wall_vertices.ToArray() );
		}
		Log.Info("Mesh created with " + (floor_vertices.Count + wall_vertices.Count) + " verts.");
		return new ModelBuilder().AddMesh( floor_mesh ).AddMesh(wall_mesh).Create();
	}
	
	//I am not proud of what I am about to do.
	public MeshBuilder AddFloor( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Up;
		var T = Vector3.Forward;

		var a = new SimpleVertex( new Vector3( x, y, z ), N, T, new Vector2( 0, 0 ) );
		var b = new SimpleVertex( new Vector3( x + s, y, z ), N, T, new Vector2( 1, 0 ) );
		var c = new SimpleVertex( new Vector3( x + s, y + s, z ), N, T, new Vector2( 1, 1 ) );
		var d = new SimpleVertex( new Vector3( x, y + s, z ), N, T, new Vector2( 0, 1 ) );

		AddVerts( true, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallN( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Forward;
		var T = Vector3.Down;

		var a = new SimpleVertex( new Vector3( x + s, y, z ), N, T, new Vector2( 0, 0 ) );
		var b = new SimpleVertex( new Vector3( x + s, y + s, z ), N, T, new Vector2( 1, 0 ) );
		var c = new SimpleVertex( new Vector3( x + s, y + s, z + s ), N, T, new Vector2( 1, 1 ) );
		var d = new SimpleVertex( new Vector3( x + s, y, z + s ), N, T, new Vector2( 0, 1 ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallS( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Backward;
		var T = Vector3.Up;

		var a = new SimpleVertex( new Vector3( x, y, z ), N, T, new Vector2( 0, 0 ) );
		var b = new SimpleVertex( new Vector3( x, y, z + s ), N, T, new Vector2( 1, 0 ) );
		var c = new SimpleVertex( new Vector3( x, y + s, z + s ), N, T, new Vector2( 1, 1 ) );
		var d = new SimpleVertex( new Vector3( x, y + s, z ), N, T, new Vector2( 0, 1 ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallE( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Right;
		var T = Vector3.Down;

		var a = new SimpleVertex( new Vector3( x, y, z ), N, T, new Vector2( 0, 0 ) );
		var b = new SimpleVertex( new Vector3( x + s, y, z ), N, T, new Vector2( 1, 0 ) );
		var c = new SimpleVertex( new Vector3( x + s, y, z + s ), N, T, new Vector2( 1, 1 ) );
		var d = new SimpleVertex( new Vector3( x, y, z + s ), N, T, new Vector2( 0, 1 ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
	public MeshBuilder AddWallW( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		var N = Vector3.Left;
		var T = Vector3.Down;

		var a = new SimpleVertex( new Vector3( x, y + s, z ), N, T, new Vector2( 0, 0 ) );
		var b = new SimpleVertex( new Vector3( x, y + s, z + s ), N, T, new Vector2( 1, 0 ) );
		var c = new SimpleVertex( new Vector3( x + s, y + s, z + s ), N, T, new Vector2( 1, 1 ) );
		var d = new SimpleVertex( new Vector3( x + s, y + s, z ), N, T, new Vector2( 0, 1 ) );

		AddVerts( false, a, b, c, c, d, a );

		return this;
	}
}
