using Sandbox;
using System;

public partial class Pawn : ModelEntity
{
	[Net] public Game CurrentGame { get; set; }
	[Net] private Vector2 WishPos { get; set; }
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

			var hit = GridWorld.WorldToGrid( tr.Value );
			WishPos = hit;
		}

		if ( !IsServer ) return;
		
		if ( Moving )
		{
			Position = Position.WithZ( 0 );
			if ( Position.Distance( CurDestPos ) < 0.1 * GridWorld.GRID_SCALE )
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
			var PosOnGrid = GridWorld.WorldToGrid( Position );
			if ( PosOnGrid != WishPos )
			{
				PathfindToWish(PosOnGrid);
				Moving = true;
			}
		}
		
		Position = Position.WithZ( CurrentGame.CurrentGridWorld.GetHeightFromWorld(Position.x, Position.y) );
		base.Simulate( cl );
	}
	private void PathfindToWish(Vector2 PosOnGrid)
	{
		/*
		var p = new Pathfind( CurrentGame.CurrentGridWorld ).FromPosition( PosOnGrid ).ToPosition( WishPos ).Path(63);
		
		if(!p.CanPathfind) { WishPos = PosOnGrid; return; }

		CurDestPos = GridWorld.GridToWorld( (p.NextPos + PosOnGrid).SnapToGrid( 1 ) );
		*/
	}	
	public override void FrameSimulate( Client cl )
	{
		
		
		var GS = GridWorld.GRID_SCALE;
		var gridPlane = new Plane( Position.WithZ( 0 ), Vector3.Up );

		if ( (Local.Hud as HUD).MouseEnabled )
		{
			var tr = gridPlane.Trace( Input.Cursor );
			if ( tr == null ) return;
			var hit = tr.Value.SnapToGrid( GS );

			//hit.z = WorldEntity.GetHeightFromWorld( hit.x, hit.y ) * ChunkEntity.GRID_SCALE;
			hit.z += 2;
			DebugOverlay.Box( hit, hit, Color.Yellow, 0, false );
			DebugOverlay.Box( hit.WithZ(2), hit.WithZ( 2 ), Color.Yellow, 0, false );
		}
		if ( WishPos != GridWorld.WorldToGrid(Position) )
		{
			var W_WishPos = GridWorld.GridToWorld( WishPos );
			//W_WishPos.z = CurrentGame.CurrentGridWorld.GetHeightFromGrid( (int)WishPos.x, (int)WishPos.y ) * GridWorld.GRID_SCALE;
			W_WishPos.z += 2;
			DebugOverlay.Box( W_WishPos, W_WishPos, Color.Red, 0, true );
		}
		
		
		base.FrameSimulate( cl );
	}
}
