// | -----------------------------------------------------------------
// | idx3d III is (c)1999/2000 by Peter Walser
// | -----------------------------------------------------------------
// | idx3d is a 3d engine written in 100% pure Java (1.1 compatible)
// | and provides a fast and flexible API for software 3d rendering
// | on the Java platform.
// |
// | Feel free to use the idx3d API / classes / source code for
// | non-commercial purposes (of course on your own risk).
// | If you intend to use idx3d for commercial purposes, please
// | contact me with an e-mail [proxima@active.ch].
// |
// | Thanx & greetinx go to:
// | * Wilfred L. Guerin, 	for testing, bug report, and tons 
// |			of brilliant suggestions
// | * Sandy McArthur,	for reverse loops
// | * Dr. Douglas Lyons,	for mentioning idx3d1 in his book
// | * Hugo Elias,		for maintaining his great page
// | * the comp.graphics.algorithms people, 
// | 			for scientific concerns
// | * Tobias Hill,		for inspiration and awakening my
// |			interest in java gfx coding
// | * Kai Krause,		for inspiration and hope
// | * Incarom & Parisienne,	for keeping me awake during the 
// |			long coding nights
// | * Doris Langhard,	for being the sweetest girl on earth
// | * Etnica, Infinity Project, X-Dream and "Space Night"@BR3
// | 			for great sound while coding
// | and all coderz & scenerz out there (keep up the good work, ppl :)
// |
// | Peter Walser
// | proxima@active.ch
// | http://www2.active.ch/~proxima
// | "On the eigth day, God started debugging"
// | -----------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace IDx3DSharp
{
	/// <summary>
	/// Defines a triangle vertex.
	/// </summary>
	public class Vertex : ICloneable
	{
		// F I E L D S

		public SceneObject parent;

		public Vector pos = new Vector();   //(x,y,z) Coordinate of vertex
		public Vector pos2;  //Transformed vertex coordinate
		public Vector n = new Vector();   //Normal Vector at vertex
		public Vector n2;  //Transformed normal vector (camera space)

		public int X;  //Projected x coordinate
		public int Y;  //Projected y coordinate
		public int Z;  //Projected z coordinate for z-Buffer

		public float Tu = 0; // Texture x-coordinate (relative)
		public float Tv = 0; // Texture y-coordinate (relative)

		public int nx = 0; // Normal x-coordinate for envmapping
		public int ny = 0; // Normal y-coordinate for envmapping
		public int tx = 0; // Texture x-coordinate (absolute)
		public int ty = 0; // Texture y-coordinate (absolute)


		public bool visible = true;  //visibility tag for clipping
		public int clipcode = 0;
		public int id; // Vertex index

		private float fact;
		private List<Triangle> neighbor = new List<Triangle>(); //Neighbor triangles of vertex


		#region Constructors

		public Vertex()
		{
			pos = new Vector(0f, 0f, 0f);
		}

		public Vertex(float xpos, float ypos, float zpos)
		{
			pos = new Vector(xpos, ypos, zpos);
		}

		public Vertex(float xpos, float ypos, float zpos, float u, float v)
		{
			pos = new Vector(xpos, ypos, zpos);
			this.Tu = u;
			this.Tv = v;
		}

		public Vertex(Vector ppos)
		{
			pos = ppos.Clone();
		}

		public Vertex(Vector ppos, float u, float v)
		{
			pos = ppos.Clone();
			this.Tu = u;
			this.Tv = v;
		}

		#endregion

		// P U B L I C   M E T H O D S

		/// <summary>
		/// Projects this vertex into camera space
		/// </summary>
		/// <param name="vertexProjection"></param>
		/// <param name="normalProjection"></param>
		/// <param name="camera"></param>
		public void Project(Matrix vertexProjection, Matrix normalProjection, Camera camera)
		{
			pos2 = pos.Transform(vertexProjection);
			n2 = n.Transform(normalProjection);

			fact = camera.screenscale / camera.fovfact / ((pos2.Z > 0.1) ? pos2.Z : 0.1f);
			X = (int) (pos2.X * fact + (camera.screenwidth >> 1));
			Y = (int) (-pos2.Y * fact + (camera.screenheight >> 1));
			Z = (int) (65536f * pos2.Z);
			nx = (int) (n2.X * 127 + 127);
			ny = (int) (n2.Y * 127 + 127);
			if (parent.material == null) return;
			if (parent.material.texture == null) return;
			tx = (int) ((float) parent.material.texture.width * Tu);
			ty = (int) ((float) parent.material.texture.height * Tv);
		}

		public void setUV(float u, float v)
		{
			this.Tu = u;
			this.Tv = v;
		}

		public void clipFrustrum(int w, int h)
		{
			// View plane clipping
			clipcode = 0;
			if (X < 0) clipcode |= 1;
			if (X >= w) clipcode |= 2;
			if (Y < 0) clipcode |= 4;
			if (Y >= h) clipcode |= 8;
			if (pos2.Z < 0) clipcode |= 16;
			visible = (clipcode == 0);
		}

		/// <summary>
		/// Registers a neighbor triangle
		/// </summary>
		/// <param name="triangle"></param>
		public void registerNeighbor(Triangle triangle)
		{
			if (!neighbor.Contains(triangle)) neighbor.Add(triangle);
		}

		public void resetNeighbors()
		// resets the neighbors
		{
			neighbor.Clear();
		}

		public void regenerateNormal()
		// recalculates the vertex normal
		{
			float nx = 0;
			float ny = 0;
			float nz = 0;
			foreach (Triangle tri in neighbor)
			{
				Vector wn = tri.getWeightedNormal();
				nx += wn.X;
				ny += wn.Y;
				nz += wn.Z;
			}
			n = new Vector(nx, ny, nz).Normalize();
		}

		public void scaleTextureCoordinates(float fx, float fy)
		{
			Tu *= fx;
			Tv *= fy;
		}

		public Vertex Clone()
		{
			Vertex newVertex = new Vertex();
			newVertex.pos = pos.Clone();
			newVertex.n = n.Clone();
			newVertex.Tu = Tu;
			newVertex.Tv = Tv;
			return newVertex;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		public override string ToString()
		{
			return "<vertex  x=" + pos.X + " y=" + pos.Y + " z=" + pos.Z + " u=" + Tu + " v=" + Tv + ">\r\n";
		}

		public bool equals(Vertex v)
		{
			return ((pos.X == v.pos.X) && (pos.Y == v.pos.Y) && (pos.Z == v.pos.Z));
		}

		public bool equals(Vertex v, float tolerance)
		{
			return Math.Abs(Vector.Subtract(pos, v.pos).Length()) < tolerance;
		}
	}
}