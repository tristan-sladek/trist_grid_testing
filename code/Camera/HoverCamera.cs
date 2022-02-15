using Sandbox;
public class HoverCamera : Camera
{
	Angles LookInput;
	Vector3 MoveInput;
	Vector3 BasePosition;
	public float CamDistance = 150f;
	private float MoveMult = 1;
	private static float MIN_CAM = 50f;
	private static float MAX_CAM = 400f;
	private bool follow = false;

	
	public override void Update()
	{
		if ( follow )
			BasePosition = Local.Pawn.Position;
		else
			BasePosition += MoveInput.Normal * 300 * MoveMult * RealTime.Delta * Rotation.From( LookInput.WithPitch( 0 ) );

		float adj = (CamDistance - MIN_CAM) / (MAX_CAM - MIN_CAM);
		Rotation = Rotation.From( LookInput.WithPitch(25 + adj * 20) );

		Position = BasePosition;
		Position += Rotation.Forward * -CamDistance; //Zoom
		Position += new Vector3( 0, 0, 50 ) + Rotation.From( LookInput.WithPitch( 0 ) ).Forward * -50f;
	}
	public override void BuildInput( InputBuilder input )
	{
		MoveInput = input.AnalogMove;

		MoveMult = input.Down( InputButton.Run ) ? 3 : 1;

		if ( input.Pressed( InputButton.Walk ) ) follow = !follow;

		if ( input.Down(InputButton.Attack2) )
			LookInput += input.AnalogLook.WithRoll( 0 );

		if ( input.Pressed( InputButton.Slot1 ) ) BasePosition = BasePosition.WithZ( BasePosition.z + GridWorld.GRID_SCALE * MoveMult );
		if ( input.Pressed( InputButton.Slot2 ) ) BasePosition = BasePosition.WithZ( BasePosition.z - GridWorld.GRID_SCALE * MoveMult );

		CamDistance -= input.MouseWheel * 10;
		CamDistance = CamDistance.Clamp( MIN_CAM, MAX_CAM );
		
		base.BuildInput( input );
	}	
}
