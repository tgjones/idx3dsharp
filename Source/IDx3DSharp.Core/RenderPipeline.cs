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

using System.Collections.Generic;

namespace IDx3DSharp
{
	public class RenderPipeline
	{
		// F I E L D S

		public Screen screen;
		Scene scene;
		public Lightmap lightmap;

		private bool resizingRequested = false;
		private bool antialiasChangeRequested = false;
		private int requestedWidth;
		private int requestedHeight;
		private bool requestedAntialias;
		public bool useIdBuffer = false;

		Rasterizer rasterizer;
		List<Triangle> opaqueQueue = new List<Triangle>();
		List<Triangle> transparentQueue = new List<Triangle>();


		// Q U I C K  R E F E R E N C E S

		const int zFar = 0xFFFFFFF;

		// B U F F E R S

		public uint[] zBuffer;
		public uint[] idBuffer;


		// C O N S T R U C T O R S

		public RenderPipeline(Scene scene, int w, int h)
		{
			System.Drawing.Color.Black.ToArgb();
			this.scene = scene;
			screen = new Screen(w, h);
			zBuffer = new uint[screen.w * screen.h];
			rasterizer = new Rasterizer(this);
		}


		// P U B L I C   M E T H O D S

		public void setAntialias(bool antialias)
		{
			antialiasChangeRequested = true;
			requestedAntialias = antialias;
		}

		public float getFPS()
		{
			return (float) ((screen.FPS * 100)) / 100;
		}

		public void resize(int w, int h)
		{
			resizingRequested = true;
			requestedWidth = w;
			requestedHeight = h;
		}

		public void buildLightMap()
		{
			if (lightmap == null) lightmap = new Lightmap(scene);
			else lightmap.rebuildLightmap();
			rasterizer.loadLightmap(lightmap);
		}


		public void render(Camera cam)
		{
			// Resize if requested
			if (resizingRequested) performResizing();
			if (antialiasChangeRequested) performAntialiasChange();
			rasterizer.rebuildReferences(this);

			// Clear buffers	
			MathUtility.clearBuffer(zBuffer, zFar);
			if (useIdBuffer) MathUtility.clearBuffer(idBuffer, uint.MaxValue);
			if (scene.environment.background != null)
				screen.drawBackground(scene.environment.background, 0, 0, screen.w, screen.h);
			else screen.clear(scene.environment.bgcolor);

			// Prepare
			cam.setScreensize(screen.w, screen.h);
			scene.prepareForRendering();
			emptyQueues();

			// Project

			Matrix m = Matrix.multiply(cam.getMatrix(), scene.matrix);
			Matrix nm = Matrix.multiply(cam.getNormalMatrix(), scene.normalmatrix);
			Matrix vertexProjection, normalProjection;
			SceneObject obj;
			Triangle t;
			Vertex v;
			int w = screen.w;
			int h = screen.h;
			for (uint id = 0, length = scene.objects; id < length; id++)
			{
				obj = scene._object[id];
				if (obj.visible)
				{
					vertexProjection = obj.matrix.Clone();
					normalProjection = obj.normalmatrix.Clone();
					vertexProjection.transform(m);
					normalProjection.transform(nm);

					for (int i = 0, innerlength = obj.vertices; i < innerlength; i++)
					{
						v = obj.vertex[i];
						v.Project(vertexProjection, normalProjection, cam);
						v.clipFrustrum(w, h);
					}
					for (uint i = 0, innerlength = obj.triangles; i < innerlength; i++)
					{
						t = obj.triangle[i];
						t.Project(normalProjection);
						t.ClipFrustrum(w, h);
						enqueueTriangle(t);
					}
				}
			}

			Triangle[] tri = getOpaqueQueue();
			if (tri != null)
				for (int i = tri.Length - 1; i >= 0; i--)
				{
					rasterizer.loadMaterial(tri[i].parent.material);
					rasterizer.Render(tri[i]);
				}

			tri = getTransparentQueue();
			if (tri != null)
				for (int i = 0; i < tri.Length; i++)
				{
					rasterizer.loadMaterial(tri[i].parent.material);
					rasterizer.Render(tri[i]);
				}

			screen.render();

		}

		public void UseIdBuffer(bool useIdBuffer)
		{
			this.useIdBuffer = useIdBuffer;
			if (useIdBuffer) idBuffer = new uint[screen.w * screen.h];
			else idBuffer = null;
		}


		// P R I V A T E   M E T H O D S

		private void performResizing()
		{
			resizingRequested = false;
			screen.resize(requestedWidth, requestedHeight);
			zBuffer = new uint[screen.w * screen.h];
			if (useIdBuffer) idBuffer = new uint[screen.w * screen.h];
		}

		private void performAntialiasChange()
		{
			antialiasChangeRequested = false;
			screen.setAntialias(requestedAntialias);
			zBuffer = new uint[screen.w * screen.h];
			if (useIdBuffer) idBuffer = new uint[screen.w * screen.h];
		}

		// Triangle sorting

		private void emptyQueues()
		{
			opaqueQueue.Clear();
			transparentQueue.Clear();
		}

		private void enqueueTriangle(Triangle tri)
		{
			if (tri.parent.material == null) return;
			if (tri.visible == false) return;
			if ((tri.parent.material.transparency == 255) && (tri.parent.material.reflectivity == 0)) return;

			if (tri.parent.material.transparency > 0) transparentQueue.Add(tri);
			else opaqueQueue.Add(tri);
		}

		private Triangle[] getOpaqueQueue()
		{
			if (opaqueQueue.Count == 0) return null;
			Triangle[] tri = new Triangle[opaqueQueue.Count];
			opaqueQueue.CopyTo(tri);

			return sortTriangles(tri, 0, tri.Length - 1);
		}

		private Triangle[] getTransparentQueue()
		{
			if (transparentQueue.Count == 0) return null;
			Triangle[] tri = new Triangle[transparentQueue.Count];
			transparentQueue.CopyTo(tri);

			return sortTriangles(tri, 0, tri.Length - 1);
		}

		private Triangle[] sortTriangles(Triangle[] tri, int L, int R)
		{
			float m = (tri[L].dist + tri[R].dist) / 2;
			int i = L;
			int j = R;
			Triangle temp;

			do
			{
				while (tri[i].dist > m) i++;
				while (tri[j].dist < m) j--;

				if (i <= j)
				{
					temp = tri[i];
					tri[i] = tri[j];
					tri[j] = temp;
					i++;
					j--;
				}
			}
			while (j >= i);

			if (L < j) sortTriangles(tri, L, j);
			if (R > i) sortTriangles(tri, i, R);

			return tri;
		}
	}
}