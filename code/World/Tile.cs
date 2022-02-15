using Sandbox;
using System;

public partial class Tile : BaseNetworkable
{
	[Net] private short _height { get; set; } = 0;
	[Net] public float Height { get { return (float)(_height) / 2f; } set { _height = (short)(value * 2); } }
	[Net] public WallState Walls { get; set; }
}
[Flags]
public enum WallState : ushort
{
	X    = 0b_0000,
	N    = 0b_0001,
	E    = 0b_0010,
	S    = 0b_0100,
	W    = 0b_1000
}


