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

namespace IDx3DSharp
{
	/// <summary>
	/// Defines a 3D triangle.
	/// </summary>
	public class Triangle
	{
		#region Fields

		public SceneObject parent;  // the object which obtains this triangle
		public bool visible = true;  //visibility tag for clipping
		public bool outOfFrustrum = false;  //visibility tag for frustrum clipping

		public Vertex p1;  // first  vertex
		public Vertex p2;  // second vertex 
		public Vertex p3;  // third  vertex 

		public Vector n;  // Normal vector of flat triangle
		public Vector n2; // Projected Normal vector

		private int minx, maxx, miny, maxy; // for clipping
		private Vector triangleCenter = new Vector();
		public float dist = 0;

		public uint id = 0;

		#endregion

		#region Constructor

		public Triangle(Vertex a, Vertex b, Vertex c)
		{
			p1 = a;
			p2 = b;
			p3 = c;
		}

		#endregion

		#region Properties

		public Vector Center
		{
			get
			{
				float cx = (p1.pos.X + p2.pos.X + p3.pos.X) / 3;
				float cy = (p1.pos.Y + p2.pos.Y + p3.pos.Y) / 3;
				float cz = (p1.pos.Z + p2.pos.Z + p3.pos.Z) / 3;
				return new Vector(cx, cy, cz);
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Backface culling and frustrum clipping.
		/// </summary>
		/// <param name="w"></param>
		/// <param name="h"></param>
		public void ClipFrustrum(int w, int h)
		{
			if (parent.material == null) { visible = false; return; }
			outOfFrustrum = (p1.clipcode & p2.clipcode & p3.clipcode) != 0;
			if (outOfFrustrum) { visible = false; return; }
			if (n2.Z > 0.5) { visible = true; return; }

			triangleCenter.X = (p1.pos2.X + p2.pos2.X + p3.pos2.X);
			triangleCenter.Y = (p1.pos2.Y + p2.pos2.Y + p3.pos2.Y);
			triangleCenter.Z = (p1.pos2.Z + p2.pos2.Z + p3.pos2.Z);
			visible = Vector.Angle(triangleCenter, n2) > 0;

		}

		public void Project(Matrix normalProjection)
		{
			n2 = n.Transform(normalProjection);
			dist = getDist();
		}

		public void regenerateNormal()
		{
			n = Vector.GetNormal(p1.pos, p2.pos, p3.pos);
		}

		public Vector getWeightedNormal()
		{
			return Vector.VectorProduct(p1.pos, p2.pos, p3.pos);
		}

		public Vertex getMedium()
		{
			float cx = (p1.pos.X + p2.pos.X + p3.pos.X) / 3;
			float cy = (p1.pos.Y + p2.pos.Y + p3.pos.Y) / 3;
			float cz = (p1.pos.Z + p2.pos.Z + p3.pos.Z) / 3;
			float cu = (p1.Tu + p2.Tu + p3.Tu) / 3;
			float cv = (p1.Tv + p2.Tv + p3.Tv) / 3;
			return new Vertex(cx, cy, cz, cu, cv);
		}

		public float getDist()
		{
			return p1.Z + p2.Z + p3.Z;
		}


		public bool degenerated()
		{
			return p1.equals(p2) || p2.equals(p3) || p3.equals(p1);
		}

		public Triangle Clone()
		{
			return new Triangle(p1, p2, p3);
		}

		#endregion
	}
}