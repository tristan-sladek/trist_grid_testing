using Sandbox;
using System.Collections.Generic;
public partial class WorldEntity : ModelEntity
{
	public const float GRID_SCALE = 32f;
	public const int radius = 64;
	public const int maxR = radius * 2 + 1;
	[Net] public IList<bool> boolList { get; set; } = new List<bool>();	
	public override void Spawn()
	{
		CreateBoolList();
	}
	public override void ClientSpawn()
	{
		var m = new MeshBuilder();

		for ( int x = 0; x < maxR; x++ )
			for ( int y = 0; y < maxR; y++ )
			{
				float xc = (float) x - radius - 0.5f;
				float yc = (float) y - radius - 0.5f;
				bool high = !canWalk( x, y );

				m.AddFloor( xc, yc, high ? 1 : 0, GRID_SCALE );

				if ( high )
				{
					if ( canWalk( x + 1 , y ) )
					{
						m.AddWallN( xc, yc, 0, GRID_SCALE );
					}

					if ( canWalk( x, y + 1 ) )
					{
						m.AddWallW( xc, yc, 0, GRID_SCALE );
					}
				}
				else
				{
					if ( !canWalk( x + 1, y ) )
					{
						m.AddWallS( xc + 1, yc, 0, GRID_SCALE );
					}

					if ( !canWalk( x, y + 1 ) )
					{
						m.AddWallE( xc, yc + 1, 0, GRID_SCALE );
					}
				}

			}

		Model = m.Build();

		base.ClientSpawn();
	}
	public bool inBounds( int v )
	{
		if ( v < 0 || v > maxR - 1 )
			return false;
		return true;
	}
	public bool canWalk(int x, int y)
	{
		if ( inBounds( x ) && inBounds( y ) )
			return !boolList[x + y * maxR];
		// Log.Info( "Out of Bounds " + x + " | " + y );
		return false;
	}
	private void CreateBoolList()
	{
		var b = new bool[maxR, maxR];
		
		for ( int x = 0; x < maxR; x++ )
			for ( int y = 0; y < maxR; y++ )
				if ( x == 0 || y == 0 || x == maxR - 1 || y == maxR - 1 )
					b[x, y] = true;
		for ( int i = 0; i < radius * radius; i++ )
			b[Rand.Int( 1, radius * 2 ), Rand.Int( 1, radius * 2 )] = true;
		//b[0, 0] = false;

		for ( int y = 0; y < maxR; y++ ) 
			for (int x = 0; x < maxR; x++ )
				boolList.Add( b[ x , y ]);
	}
}
