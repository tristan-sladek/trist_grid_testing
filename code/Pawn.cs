using Sandbox;
using System;

public partial class Pawn : ModelEntity
{
	[Net] public Game Game { get; set; }
	private WorldEntity WorldEntity { get { return Game.World; } }
	[Net] private Vector3 WishPos { get; set; }
	private bool Moving;	
	private Vector3 CurDestPos;
	public override void Spawn()
	{
		SetModel( "models/sbox_props/watermelon/watermelon.vmdl" );
		EnableDrawing = true;

		base.Spawn();
	}
	public override void Simulate( Client cl )
	{
		if ( IsServer && Input.Pressed( InputButton.Jump ) )
		{
			Game.CreateWorldEntity();
		}
		if ( !WorldEntity.IsValid() ) return;
		var gridPlane = new Plane( WorldEntity.Position, Vector3.Up );

		if ( IsServer && Input.Pressed( InputButton.Attack1 ) )
		{
			var tr = gridPlane.Trace( Input.Cursor );
			if ( tr == null ) return;
			var hit = tr.Value.SnapToGrid( WorldEntity.GRID_SCALE ) / WorldEntity.GRID_SCALE;
			int x = (int) hit.x + WorldEntity.radius;
			int y = (int) hit.y + WorldEntity.radius;
			
			//Log.Info( "x:" + x + " y:" + y + "| Can Walk: " + WorldEntity.canWalk( x, y ) +" | Relative to Center x:" + (int)hit.x + " y:" + (int)hit.y );
			if( WorldEntity.InBounds(x) && WorldEntity.InBounds(y) ) WishPos = hit;
		}

		if ( !IsServer ) return;
		if ( Moving )
		{
			Position = Position.WithZ( 0 );
			if ( Position.Distance( CurDestPos ) < 0.1 * WorldEntity.GRID_SCALE )
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
			Position = Position.SnapToGrid( WorldEntity.GRID_SCALE );
			var PosOnGrid = (Position / WorldEntity.GRID_SCALE).SnapToGrid(1);
			if ( PosOnGrid != WishPos )
			{
				PathfindToWish(PosOnGrid);
				Moving = true;
			}
		}
		//Log.Info( Moving + " " + Position + " " + CurDestPos );
		Position = Position.WithZ( WorldEntity.GetHeightFromWorld( Position.x, Position.y ) * WorldEntity.GRID_SCALE );
		base.Simulate( cl );
	}
	private void PathfindToWish(Vector3 PosOnGrid)
	{
		PosOnGrid = PosOnGrid.WithZ( 0 );
		var p = new Pathfind( WorldEntity ).FromPosition( PosOnGrid ).ToPosition( WishPos ).Path();
		
		if(!p.CanPathfind) { WishPos = PosOnGrid; return; }

		CurDestPos = (p.NextPos + PosOnGrid).SnapToGrid( 1 ) * WorldEntity.GRID_SCALE;
	}
	public override void FrameSimulate( Client cl )
	{
		
		if ( WorldEntity.IsValid() )
		{
			var GS = WorldEntity.GRID_SCALE;
			var gridPlane = new Plane( Position.WithZ( 0 ), Vector3.Up );
			var half = new Vector3( GS / 2, GS / 2, 0 );

			if ( (Local.Hud as HUD).MouseEnabled )
			{
				var tr = gridPlane.Trace( Input.Cursor );
				if ( tr == null ) return;
				var hit = tr.Value.SnapToGrid( GS );
				hit.z += 2;				
				DebugOverlay.Box( hit - half, hit + half, Color.Yellow, 0, false );
			}

			if ( WishPos != Position.WithZ(0) / GS )
			{
				var WishToWorld = WishPos * GS;
				WishToWorld.z = WorldEntity.GetHeightFromWorld( WishToWorld.x, WishToWorld.y ) * WorldEntity.GRID_SCALE;
				WishToWorld.z += 2;
				DebugOverlay.Box( WishToWorld - half, WishToWorld + half, Color.Red, 0, true );
			}
		}
		base.FrameSimulate( cl );
	}
}
