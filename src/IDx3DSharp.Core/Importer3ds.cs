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
using System.IO;
using System.Net;

namespace IDx3DSharp
{
	public class Importer3ds
	// Imports a scene from a 3ds (3d Studio Max) Ressource
	{
		// F I E L D S

		private int currentJunkId;
		private int nextJunkOffset;

		private Scene scene;
		private string currentObjectName = null;
		private SceneObject currentObject = null;
		private bool endOfStream = false;


		// P U B L I C   M E T H O D S

		public void importFromURL(Uri url, Scene targetscene)
		{
			if (url.Scheme == "http")
			{
				importFromStream(WebRequest.Create(url).GetResponse().GetResponseStream(), targetscene);
			}
			else
			{
				importFromStream(File.OpenRead(url.ToString()), targetscene);
			}
		}

		public void importFromStream(Stream inStream, Scene targetscene)
		{
			System.Console.WriteLine(">> Importing scene from 3ds stream ...");
			scene = targetscene;
			BinaryReader input = new BinaryReader(inStream);
			readJunkHeader(input);
			if (currentJunkId != 0x4D4D)
			{
				System.Console.WriteLine("Error: This is no valid 3ds file.");
				return;
			}
			while (!endOfStream) readNextJunk(input);
			inStream.Close();
		}


		// P R I V A T E   M E T H O D S

		private string readString(BinaryReader inStream)
		{
			byte num;
			string str = "";
			while ((num = inStream.ReadByte()) != 0)
				str = str + ((char) num);
			return str;
		}

		private int readInt(BinaryReader inStream)
		{
			return (((inStream.ReadByte() | (inStream.ReadByte() << 8)) | (inStream.ReadByte() << 0x10)) | (inStream.ReadByte() << 0x18));
		}

		private int readShort(BinaryReader inStream)
		{
			return (inStream.ReadByte() | (inStream.ReadByte() << 8));
		}

		private float readFloat(BinaryReader input)
		{
			int num = this.readInt(input);
			int num2 = ((num >> 0x1f) == 0) ? 1 : -1;
			int num3 = (num >> 0x17) & 0xff;
			int num4 = (num3 == 0) ? ((num & 0x7fffff) << 1) : ((num & 0x7fffff) | 0x800000);
			double num5 = (num2 * num4) * Math.Pow(2.0, (double) (num3 - 150));
			return (float) num5;
		}


		private void readJunkHeader(BinaryReader input)
		{
			currentJunkId = readShort(input);
			nextJunkOffset = readInt(input);
			endOfStream = currentJunkId < 0;
		}

		private void readNextJunk(BinaryReader input)
		{
			readJunkHeader(input);

			if (currentJunkId == 0x3D3D) return; // Mesh block
			if (currentJunkId == 0x4000) // Object block
			{
				currentObjectName = readString(input);
				System.Console.WriteLine(">> Importing object: " + currentObjectName);
				return;
			}
			if (currentJunkId == 0x4100)  // Triangular polygon object
			{
				currentObject = new SceneObject();
				scene.addObject(currentObjectName, currentObject);
				return;
			}
			if (currentJunkId == 0x4110) // Vertex list
			{
				readVertexList(input);
				return;
			}
			if (currentJunkId == 0x4120) // Point list
			{
				readPointList(input);
				return;
			}
			if (currentJunkId == 0x4140) // Mapping coordinates
			{
				readMappingCoordinates(input);
				return;
			}

			skipJunk(input);
		}

		private void skipJunk(BinaryReader inStream)
		{
			try
			{
				for (int i = 0; (i < (this.nextJunkOffset - 6)) && !this.endOfStream; i++)
				{
					this.endOfStream = inStream.ReadByte() < 0;
				}
			}
			catch (Exception)
			{
				this.endOfStream = true;
			}
		}

		private void readVertexList(BinaryReader input)
		{
			float x, y, z;
			int vertices = readShort(input);
			for (int i = 0; i < vertices; i++)
			{
				x = readFloat(input);
				y = readFloat(input);
				z = readFloat(input);
				currentObject.addVertex(x, -y, z);
			}
		}

		private void readPointList(BinaryReader input)
		{
			int v1, v2, v3;
			int triangles = readShort(input);
			for (int i = 0; i < triangles; i++)
			{
				v1 = readShort(input);
				v2 = readShort(input);
				v3 = readShort(input);
				readShort(input);
				currentObject.addTriangle(
					currentObject.Vertex(v1),
					currentObject.Vertex(v2),
					currentObject.Vertex(v3));
			}
		}

		private void readMappingCoordinates(BinaryReader input)
		{
			int vertices = readShort(input);
			for (int i = 0; i < vertices; i++)
			{
				currentObject.Vertex(i).Tu = readFloat(input);
				currentObject.Vertex(i).Tv = readFloat(input);
			}
		}
	}
}