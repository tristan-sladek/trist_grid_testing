using Sandbox;
using System;

public partial class Tile : BaseNetworkable
{
	[Net] private short _heightNW { get; set; }
	[Net] private short _heightNE { get; set; }
	[Net] private short _heightSE { get; set; }
	[Net] private short _heightSW { get; set; }
	public float HeightNW { get { return ((float)_heightNW) / 2f; } set { _heightNW = (short)(value * 2); } }
	public float HeightNE { get { return ((float)_heightNE) / 2f; } set { _heightNE = (short)(value * 2); } }
	public float HeightSE { get { return ((float)_heightSE) / 2f; } set { _heightSE = (short)(value * 2); } }
	public float HeightSW { get { return ((float)_heightSW) / 2f; } set { _heightSW = (short)(value * 2); } }
	public float Height 
	{ 
		get { 
			return (_heightNW + _heightNE + _heightSE + _heightSW) / 8f; 
		} 
		set {
			_heightNW = (short)(value * 2);
			_heightNE = _heightNW;
			_heightSE = _heightNW;
			_heightSW = _heightNW;
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

