using Sandbox;
using System;
using System.Collections.Generic;
public partial class ChunkEntity : ModelEntity
{
	public int ChunkX { get; set; }
	public int ChunkY { get; set; }
	
	[ClientRpc]
	public void SetChunkMDL(Model m)
	{
		Model = m;
	}
}
