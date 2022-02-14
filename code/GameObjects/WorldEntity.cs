﻿using Sandbox;
using System.Collections.Generic;
public partial class WorldEntity : ModelEntity
{
	public const float GRID_SCALE = 32f;
	public const int radius = 16;
	public const int maxR = radius * 2 + 1;
	[Net] public IList<float> heightMap { get; set; } = new List<float>();
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
	public float GetHeight(int x, int y)
	{
		if(InBounds( x ) && InBounds( y ) )
			return heightMap[x + y * maxR];
		return 1;
	}
	private void CreateHeightMap()
	{
		var b = new float[maxR, maxR];

		for ( int x = 0; x < maxR; x++ )
			for ( int y = 0; y < maxR; y++ )
				if ( x == 0 || y == 0 || x == maxR - 1 || y == maxR - 1 )
					b[x, y] = 4;
		for ( int i = 0; i < radius * radius; i++ )
		{
			float f = Rand.Int( 1, 4 );
			b[Rand.Int( 1, radius * 2 - 1 ), Rand.Int( 1, radius * 2 - 1 )] = f /2;
		}
			
		b[0, 0] = 0;

		for ( int y = 0; y < maxR; y++ )
			for ( int x = 0; x < maxR; x++ )
				heightMap.Add( b[x, y] );
	}
	
}
