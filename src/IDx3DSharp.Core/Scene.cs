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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IDx3DSharp
{
	public sealed class Scene : CoreObject
	{
		//Release Information

		public static string version = "3.1.001";
		public static string release = "29.05.2000";

		// F I E L D S		

		public RenderPipeline renderPipeline;
		public int width, height;

		public SceneEnvironment environment = new SceneEnvironment();
		public Camera defaultCamera = Camera.FRONT();

		public SceneObject[] _object;
		public Light[] _light;
		public uint objects = 0;
		public uint lights = 0;

		private bool objectsNeedRebuild = true;
		private bool lightsNeedRebuild = true;

		protected bool preparedForRendering = false;

		public Vector normalizedOffset = new Vector(0f, 0f, 0f);
		public float normalizedScale = 1f;
		private static bool instancesRunning = false;

		// D A T A   S T R U C T U R E S

		public Dictionary<string, SceneObject> objectData = new Dictionary<string, SceneObject>();
		public Dictionary<string, Light> lightData = new Dictionary<string, Light>();
		public Dictionary<string, Material> materialData = new Dictionary<string, Material>();
		public Dictionary<string, Camera> cameraData = new Dictionary<string, Camera>();


		// C O N S T R U C T O R S

		private Scene()
		{
		}

		public Scene(int w, int h)
		{
			showInfo(); width = w; height = h;
			renderPipeline = new RenderPipeline(this, w, h);
		}


		public void showInfo()
		{
			if (instancesRunning) return;
			System.Console.WriteLine();
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			System.Console.WriteLine(" idx3d Kernel " + version + " [Build " + release + "]");
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			System.Console.WriteLine(" (c)1999 by Peter Walser, all rights reserved.");
			System.Console.WriteLine(" http://www2.active.ch/~proxima/idx3d");
			System.Console.WriteLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
			instancesRunning = true;
		}


		// D A T A   M A N A G E M E N T

		public void removeAllObjects()
		{
			objectData = new Dictionary<string, SceneObject>();
			objectsNeedRebuild = true;
			rebuild();
		}

		public void rebuild()
		{
			if (objectsNeedRebuild)
			{
				objectsNeedRebuild = false;
				objects = (uint) objectData.Count;
				_object = new SceneObject[objects];
				objectData.Values.CopyTo(_object, 0);
				for (uint i = 0, length = objects; i < length; i++)
				{
					_object[i].id = i;
					_object[i].rebuild();
				}
			}

			if (lightsNeedRebuild)
			{
				lightsNeedRebuild = false;
				lights = (uint) lightData.Count;
				_light = new Light[lights];
				lightData.Values.CopyTo(_light, 0);
			}
		}

		// A C C E S S O R S

		public SceneObject Object(string key) { return (SceneObject) objectData[key]; }
		public Light Light(string key) { return (Light) lightData[key]; }
		public Material material(string key) { return (Material) materialData[key]; }
		public Camera camera(string key) { return (Camera) cameraData[key]; }

		// O B J E C T   M A N A G E M E N T

		public void addObject(string key, SceneObject obj) { obj.name = key; objectData.Add(key, obj); obj.parent = this; objectsNeedRebuild = true; }
		public void removeObject(string key) { objectData.Remove(key); objectsNeedRebuild = true; preparedForRendering = false; }

		public void addLight(string key, Light l) { lightData.Add(key, l); lightsNeedRebuild = true; }
		public void removeLight(string key) { lightData.Remove(key); lightsNeedRebuild = true; preparedForRendering = false; }

		public void addMaterial(string key, Material m) { materialData.Add(key, m); }
		public void removeMaterial(string key) { materialData.Remove(key); }

		public void addCamera(string key, Camera c) { cameraData.Add(key, c); }
		public void removeCamera(string key) { cameraData.Remove(key); }


		// R E N D E R I N G

		public void prepareForRendering()
		{
			if (preparedForRendering) return;
			preparedForRendering = true;

			System.Console.WriteLine(">> Preparing structures for realtime rendering ...   ");
			rebuild();
			renderPipeline.buildLightMap();
			printSceneInfo();
		}

		public void printSceneInfo()
		{
			System.Console.WriteLine(">> | Objects   : " + objects);
			System.Console.WriteLine(">> | Vertices  : " + countVertices());
			System.Console.WriteLine(">> | Triangles : " + countTriangles());
		}


		public void render(Camera cam)
		{
			renderPipeline.render(cam);
		}

		public void render()
		{
			renderPipeline.render(this.defaultCamera);
		}

		public Image getImage()
		{
			return renderPipeline.screen.getImage();
		}

		public void setAntialias(bool antialias)
		{
			renderPipeline.setAntialias(antialias);
		}

		public bool antialias()
		{
			return renderPipeline.screen.antialias;
		}

		public float getFPS()
		{
			return renderPipeline.getFPS();
		}

		public void useIdBuffer(bool useIdBuffer)
		// Enables / Disables idBuffering
		{
			renderPipeline.UseIdBuffer(useIdBuffer);
		}

		public Triangle identifyTriangleAt(int xpos, int ypos)
		{
			if (!renderPipeline.useIdBuffer) return null;
			if (xpos < 0 || xpos >= width) return null;
			if (ypos < 0 || ypos >= height) return null;

			int pos = xpos + renderPipeline.screen.w * ypos;
			if (renderPipeline.screen.antialias) pos *= 2;
			uint idCode = renderPipeline.idBuffer[pos];
			if (idCode == uint.MaxValue) return null;
			return _object[idCode >> 16].triangle[idCode & 0xFFFF];
		}

		public SceneObject identifyObjectAt(int xpos, int ypos)
		{
			Triangle tri = identifyTriangleAt(xpos, ypos);
			if (tri == null) return null;
			return tri.parent;
		}

		// P U B L I C   M E T H O D S

		public Size size()
		{
			return new Size(width, height);
		}

		public void resize(int w, int h)
		{
			if ((width == w) && (height == h)) return;
			width = w;
			height = h;
			renderPipeline.resize(w, h);
		}

		public void setBackgroundColor(uint bgcolor)
		{
			environment.bgcolor = bgcolor;
		}

		public void setBackground(Texture t)
		{
			environment.setBackground(t);
		}

		public void setAmbient(uint ambientcolor)
		{
			environment.ambient = ambientcolor;
		}

		public int countVertices()
		{
			int counter = 0;
			for (int i = 0; i < objects; i++) counter += _object[i].vertices;
			return counter;
		}

		public int countTriangles()
		{
			uint counter = 0;
			for (int i = 0; i < objects; i++) counter += _object[i].triangles;
			return (int) counter;
		}

		public override string ToString()
		{
			StringBuilder buffer = new StringBuilder();
			buffer.Append("<scene>\r\n");
			for (int i = 0; i < objects; i++) buffer.Append(_object[i].ToString());
			return buffer.ToString();
		}

		public void normalize()
		// useful if you can't find your objects on the screen ;)
		{
			objectsNeedRebuild = true;
			rebuild();

			Vector min, max, tempmax, tempmin;
			if (objects == 0) return;

			matrix = new Matrix();
			normalmatrix = new Matrix();

			max = _object[0].Max();
			min = _object[0].Min();

			for (int i = 0; i < objects; i++)
			{
				tempmax = _object[i].Max();
				tempmin = _object[i].Min();
				if (tempmax.X > max.X) max.X = tempmax.X;
				if (tempmax.Y > max.Y) max.Y = tempmax.Y;
				if (tempmax.Z > max.Z) max.Z = tempmax.Z;
				if (tempmin.X < min.X) min.X = tempmin.X;
				if (tempmin.Y < min.Y) min.Y = tempmin.Y;
				if (tempmin.Z < min.Z) min.Z = tempmin.Z;
			}
			float xdist = max.X - min.X;
			float ydist = max.Y - min.Y;
			float zdist = max.Z - min.Z;
			float xmed = (max.X + min.X) / 2;
			float ymed = (max.Y + min.Y) / 2;
			float zmed = (max.Z + min.Z) / 2;

			float diameter = (xdist > ydist) ? xdist : ydist;
			diameter = (zdist > diameter) ? zdist : diameter;

			normalizedOffset = new Vector(xmed, ymed, zmed);
			normalizedScale = 2 / diameter;

			shift(normalizedOffset.Reverse());
			scale(normalizedScale);
		}
	}
}