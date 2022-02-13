
using Sandbox;
public partial class Game : Sandbox.Game
{
	public HoverCamera HoverCamera { get; set; } = new();
	public HUD HUD { get; private set; }
	[Net] public WorldEntity World { get; set; }
	public Game()
	{
		if(IsClient)
		{
			HUD = new HUD();
			Local.Hud = HUD;
		}
		if( IsServer)
		{
			if ( !World.IsValid() ) CreateWorldEntity();
		}		
	}
	public void CreateWorldEntity()
	{
		if ( World.IsValid() ) World.Delete();
		World = new WorldEntity();
		World.Position = new Vector3( 0, 0, 1 );
	}
	public override void ClientJoined( Client cl )
	{
		var pawn = new Pawn();
		pawn.Game = this;
		cl.Pawn = pawn;
		cl.Camera = HoverCamera;
		
		base.ClientJoined( cl );
	}
	public override void BuildInput( InputBuilder input )
	{
		HUD.MouseEnabled = !input.Down( InputButton.Attack2 );
		base.BuildInput( input );
	}
}

