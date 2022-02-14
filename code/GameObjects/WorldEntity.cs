using Sandbox;
using System;
using System.Collections.Generic;
public partial class WorldEntity : ModelEntity
{
	public const float GRID_SCALE = 32f;
	public const int radius = 64;
	public int maxR { get { return radius * 2 + 1; }  }
	[Net] public IList<short> heightMap { get; set; } = new List<short>();
	public override void Spawn()
	{
		//CreateBoolList();
		CreateHeightMap();
	}
	public override void ClientSpawn()
	{
		var m = new MeshBuilder();

		for ( int x = 0; x < maxR; x++ )
			for ( int y = 0; y < maxR; y++ )
			{
				float cX = (float) x - radius - 0.5f; //current x
				float cY = (float) y - radius - 0.5f; //current y
				float cH = GetHeight( x, y ); //current height
				
				m.AddFloor( cX, cY, cH, GRID_SCALE );

				float eH = GetHeight( x + 1, y ); //east Height
				float nH = GetHeight( x, y + 1 ); //north Height

				if ( InBounds( y + 1 ) ) //compare north height
				{
					// no walls on ==
					if ( cH < nH )
						m.AddWallS( cX, cY + 1, cH, GRID_SCALE, 1, nH - cH );
					else if ( cH > nH )
						m.AddWallN( cX, cY, nH, GRID_SCALE, 1, cH - nH );
				}
				if ( InBounds( x + 1 ) ) //compare east height
				{
					// no walls on ==
					if ( cH < eH )
						m.AddWallW( cX + 1, cY, cH, GRID_SCALE, 1, eH - cH );
					else if ( cH > eH )
						m.AddWallE( cX, cY, eH, GRID_SCALE, 1, cH - eH );
				}
			}

		Model = m.Build();

		base.ClientSpawn();
	}
	public bool InBounds( int v )
	{
		if ( v < 0 || v > maxR - 1 )
			return false;
		return true;
	}
	public bool CanWalk(int x, int y)
	{
		if ( InBounds( x ) && InBounds( y ) )
			return GetHeight( x, y ) == 0;
		return false;
	}
	public bool CanWalkTo(int x1, int y1, int x2, int y2)
	{
		if ( InBounds( x1 ) && InBounds( x2 ) && InBounds( y1 ) && InBounds( y2 ) )
		{
			var h1 = GetHeight( x1, y1 );
			var h2 = GetHeight( x2, y2 );
			return ( h2 - h1 <= 0.5);
		}
		return false;
	}
	public float GetHeight(int x, int y)
	{
		int index = x + y * maxR;
		if ( index < heightMap.Count && InBounds( x ) && InBounds( y ) )
			return (float) (heightMap[index]) / 2;
		return 0;
	}
	public float GetHeightFromWorld(float x, float y)
	{
		int ix = (int)(x / GRID_SCALE) + radius;
		int iy = (int)(y / GRID_SCALE) + radius;
		return GetHeight( ix, iy );
	}
	private void CreateHeightMap()
	{
		var b = new short[maxR, maxR];

		// Put shell around walls
		for ( int x = 0; x < maxR; x++ )
			for ( int y = 0; y < maxR; y++ )
				if ( x == 0 || y == 0 || x == maxR - 1 || y == maxR - 1 )
					b[x, y] = 64;
		
		// Random Shit
		for ( int i = 0; i < radius * radius; i++ )
		{
			short f = (short) Rand.Int( 1, 4 );
			b[Rand.Int( 1, radius * 2 - 1 ), Rand.Int( 1, radius * 2 - 1 )] = f;
		}
		
		// Flatten Middle
		var cSize = 14;
		for(int x = radius - cSize; x < radius + cSize; x++ )
			for ( int y = radius - cSize; y < radius + cSize; y++ )
			{
				b[x, y] = 0;
			}
		
		for(int x = 1; x < 15; x++)
		{
			b[radius + x - 8, radius + 6] = (short) x;
			b[radius + x - 8, radius - 7] = (short) (15 - x);
		}
		for ( int y = 2; y < 14; y++ )
		{
			b[radius + 6, radius + y - 8] = (short)y;
			b[radius - 7, radius + y - 8] = (short)(15 - y);
		}
		for (int y = 0; y < 12; y++ )
			for ( int x = 0; x < 12; x++ )
			{
				b[radius + x - 6, radius + y - 6] = 14;
			}
		// Load Heightmap
		for ( int y = 0; y < maxR; y++ )
			for ( int x = 0; x < maxR; x++ )
				heightMap.Add( b[x, y] );
	}
}
