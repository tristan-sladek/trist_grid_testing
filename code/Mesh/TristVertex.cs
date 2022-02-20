namespace Sandbox
{
	public struct TristVertex
	{
		public Vector3 position;
		public Vector3 normal;
		public Vector3 tangent;
		public Vector2 texcoord;
		public uint data;

		public static readonly VertexAttribute[] Layout = new VertexAttribute[5]
		{
			new VertexAttribute(VertexAttributeType.Position, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.Normal, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.Tangent, VertexAttributeFormat.Float32),
			new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.Float32, 2),
			new VertexAttribute(VertexAttributeType.TexCoord, VertexAttributeFormat.UInt32, 1, 10)
		};

		public TristVertex( Vector3 position, Vector3 normal, Vector3 tangent, Vector2 texcoord, uint data )
		{
			this.position = position;
			this.normal = normal;
			this.tangent = tangent;
			this.texcoord = texcoord;
			this.data = data;
		}
	}
}
