using Sandbox;
using System;

public partial class Pawn : ModelEntity
{
	[Net] public Game CurrentGame { get; set; }
	[Net] private Vector3 WishPos { get; set; }
	private bool Moving;	
	private Vector3 CurDestPos;
	public override void Spawn()
	{
		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		EnableDrawing = true;

		base.Spawn();
	}
	int cx = 0;
	public override void Simulate( Client cl )
	{
		if ( IsServer && Input.Pressed( InputButton.Jump ) )
		{
			
			CurrentGame.CurrentGridWorld.GetTileChunk( cx % 7 - 3, cx / 7 - 3 );					
			cx++;
		}
		
		var gridPlane = new Plane( Vector3.Zero, Vector3.Up );

		if ( IsServer && Input.Pressed( InputButton.Attack1 ) )
		{
			var tr = gridPlane.Trace( Input.Cursor );
			if ( tr == null ) return;
			WishPos = tr.Value;
		}
		if ( IsServer && Input.Pressed( InputButton.Slot3 ) )
		{
			Log.Info( Position );
			Log.Info( GridWorld.GridToWorld( GridWorld.WorldToGrid( Position ) ) );
			Log.Info( GridWorld.WorldToGrid( Position ) );
			Log.Info( "---" );
			Log.Info( GridWorld.GridToChunk( GridWorld.WorldToGrid( Position ) ) );
			Log.Info( CurrentGame.CurrentGridWorld.GetHeightFromWorld( Position ) );
		}
		if (IsServer && Input.Pressed(InputButton.Slot4))
		{
			Position = Vector3.Zero;
			WishPos = Position;
			CurDestPos = Position;
			Moving = false;
		}

		if ( !IsServer ) return;
		
		if ( Moving )
		{
			Position = Position.WithZ( 0 );
			if ( Position.Distance( CurDestPos.WithZ(0) ) < 0.1 * GridWorld.GRID_SCALE )
			{
				Position = CurDestPos;
				Moving = false;
			}
			else
			{
				Position = Position.LerpTo( CurDestPos, Time.Delta * 10 );
			}			
		}
		else
		{
			if ( Position != WishPos )
			{
				PathfindToWish();
				Moving = true;
			}
		}
		
		Position = Position.WithZ( CurrentGame.CurrentGridWorld.GetHeightFromWorld(Position) * GridWorld.GRID_SCALE );		
		base.Simulate( cl );
	}
	private void PathfindToWish()
	{
		var p = new Pathfind( CurrentGame.CurrentGridWorld ).FromPosition( Position ).ToPosition( WishPos ).Path(63);
		
		if(!p.CanPathfind) { WishPos = Position; return; }

		CurDestPos = (Position + GridWorld.GridToWorld( (p.NextPos) ).SnapToGrid( GridWorld.GRID_SCALE ));
		
	}	
	public override void FrameSimulate( Client cl )
	{
				
		var GS = GridWorld.GRID_SCALE;
		var gridPlane = new Plane( Position.WithZ(2), Vector3.Up );
		var GB = new Vector3( GS, GS, 0);
		GB /= 2;

		//if ( (Local.Hud as HUD).MouseEnabled )
		{
			var tr = gridPlane.Trace( Input.Cursor );
			if ( tr == null ) return;
			var hit = (tr.Value + GB / 2).SnapToGrid( GS );

			//hit.z = WorldEntity.GetHeightFromWorld( hit.x, hit.y ) * ChunkEntity.GRID_SCALE;
			hit.z += 2;
			DebugOverlay.Box( hit + GB, hit - GB, Color.Yellow, 0, false );
			//DebugOverlay.Box( hit.WithZ(2), hit.WithZ( 2 ), Color.Yellow, 0, false );
		}
		if ( WishPos != Position )
		{
			var W_WishPos = WishPos;
			//W_WishPos.z = CurrentGame.CurrentGridWorld.GetHeightFromGrid( (int)WishPos.x, (int)WishPos.y ) * GridWorld.GRID_SCALE;
			W_WishPos.z += 2;
			DebugOverlay.Box( W_WishPos, W_WishPos, Color.Red, 0, true );
		}
		
		
		base.FrameSimulate( cl );
	}
}
