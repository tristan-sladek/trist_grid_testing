﻿using Sandbox;
using System;

public partial class Tile : BaseNetworkable
{
	[Net] public short HeightNW { get; set; } = 0;
	[Net] public short HeightNE { get; set; } = 0;
	[Net] public short HeightSE { get; set; } = 0;
	[Net] public short HeightSW { get; set; } = 0;
	public float Height
	{ 
		get { 
			return (HeightNW + HeightNE + HeightSE + HeightSW) / 8f; 
		} 
		set {
			HeightNW = (short)(value * 2);
			HeightNE = HeightNW;
			HeightSE = HeightNW;
			HeightSW = HeightNW;
		}
	}
	public ushort Walls { get; set; } //NSEW 0b1111
	public uint WallTI { get; set; }
	public uint FloorTI { get; set; }
	public uint FloorWallTI { get; set; }
	public bool NorthWall { get { return (Walls & (ushort)0b1000) > 0; } }
	public bool SouthWall { get { return (Walls & (ushort)0b0100) > 0; } }
	public bool EastWall { get { return (Walls & (ushort)0b0010) > 0; } }
	public bool WestWall { get { return (Walls & (ushort)0b0001) > 0; } }
}

