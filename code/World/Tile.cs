using Sandbox;
using System;

public partial class Tile : BaseNetworkable
{
	[Net] private short _height { get; set; } = 0;
	[Net] public float Height { get { return (float)(_height) / 2f; } set { _height = (short)(value * 2); } }
	[Net] public ushort Walls { get; set; } //NSEW 0b1111
	public bool NorthWall { get { return (Walls & (ushort)0b1000) > 0; } }
	public bool SouthWall { get { return (Walls & (ushort)0b0100) > 0; } }
	public bool EastWall { get { return (Walls & (ushort)0b0010) > 0; } }
	public bool WestWall { get { return (Walls & (ushort)0b0001) > 0; } }
}

