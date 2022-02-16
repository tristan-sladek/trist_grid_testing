using Sandbox;
using System;

public partial class Tile : BaseNetworkable
{
	[Net] private short _height { get; set; } = 0;
	[Net] public float Height { get { return (float)(_height) / 2f; } set { _height = (short)(value * 2); } }
	[Net] public ushort Walls { get; set; } //NSEW 0b1111
}

