using Sandbox;
using System.Collections.Generic;
public class MeshBuilder
{
	List<Vertex> vertices = new();
	float MAX = float.MinValue;
	float MIN = float.MaxValue;
	private void AddQuad( Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4 )
	{
		Vector3 N = (v1 - v2).Cross( v1 - v3 ) * -1;
		Vector3 T = (v2 - v1).Normal * -1;
		
		Vertex a = new Vertex( v1, N, T, new Vector2( 0, 0 ) );
		a.Color = Color.Blue;
		Vertex b = new Vertex( v2, N, T, new Vector2( 0, 1 ) );
		b.Color = Color.Red;
		Vertex c = new Vertex( v3, N, T, new Vector2( 1, 0 ) );
		c.Color = Color.Green;
		Vertex d = new Vertex( v4, N, T, new Vector2( 1, 1 ) );
		d.Color = Color.White;

		AddVerts( a, c, b, d, b, c );

	}
	private void AddVerts(params Vertex[] v)
	{
		foreach ( var vertex in v )
		{
			vertices.Add( vertex );
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
		Mesh mesh = null;		
		if ( vertices.Count > 0 )
		{
			mesh = new Mesh( Material.Load( "materials/dev/dev_measuregeneric01.vmat" ) )
			//mesh = new Mesh( Material.Load( "materials/dev/debug_wireframe.vmat" ) )
			{
				Bounds = new BBox( MIN, MAX )
			};
			mesh.CreateVertexBuffer<Vertex>( vertices.Count, Vertex.Layout, vertices.ToArray() );
		}

		Log.Info("Mesh created with " + (vertices.Count) + " verts.");
		
		return new ModelBuilder().AddMesh(mesh).Create();
	}
	public MeshBuilder AddFloor( float x, float y, float z, float s )
	{
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );
		AddQuad(
			new Vector3( x, y, z ),
			new Vector3( x, y + s, z ),
			new Vector3( x + s, y, z ),
			new Vector3( x + s, y + s, z )
		);
		
		return this;
	}
	public MeshBuilder AddWallE( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		x += s;
		var y2 = y + s * w;
		var z2 = z + s * h;
		CheckMinMax( x, y, z );
		AddQuad(
			new Vector3( x, y, z ),
			new Vector3( x, y, z2 ),
			new Vector3( x, y2, z ),
			new Vector3( x, y2, z2 )
		);
		
		return this;
	}
	public MeshBuilder AddWallW( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );

		var y2 = y + s * w;
		var z2 = z + s * h;

		AddQuad(
			new Vector3( x, y, z ),
			new Vector3( x, y2, z ),
			new Vector3( x, y, z2 ),
			new Vector3( x, y2, z2 )
		);
		
		return this;
	}
	public MeshBuilder AddWallN( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );

		y += s;
		var x2 = x + s * w;
		var z2 = z + s * h;

		AddQuad(
			new Vector3( x2, y, z ),
			new Vector3( x2, y, z2 ),
			new Vector3( x, y, z ),
			new Vector3( x, y, z2 )
		);
		return this;
	}
	public MeshBuilder AddWallS( float x, float y, float z = 0, float s = 16, float w = 1, float h = 1 )
	{
		// all grid values scaled by grid scale
		x *= s;
		y *= s;
		z *= s;
		CheckMinMax( x, y, z );

		var x2 = x + s * w;
		var z2 = z + s * h;

		AddQuad(
			new Vector3( x, y, z ),
			new Vector3( x, y, z2 ),
			new Vector3( x2, y, z ),
			new Vector3( x2, y, z2 )
		);
		
		return this;
	}
}
