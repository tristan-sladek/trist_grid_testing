
using Sandbox;
public partial class Game : Sandbox.Game
{
	public HoverCamera HoverCamera { get; set; } = new();
	public HUD HUD { get; private set; }
	public GridWorld CurrentGridWorld { get; set; }
	public Game()
	{
		if(IsClient)
		{
			HUD = new HUD();
			Local.Hud = HUD;
		}
		if( IsServer)
		{
			CurrentGridWorld = new GridWorld();
			Log.Info( CurrentGridWorld );
		}		
	}
	public override void ClientJoined( Client cl )
	{
		var pawn = new Pawn();
		pawn.CurrentGame = this;
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

