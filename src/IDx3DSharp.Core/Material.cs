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
using System.Diagnostics;
using System.IO;

namespace IDx3DSharp
{
	public sealed class Material
	// Material description object
	{
		public uint color = 0;
		public uint transparency = 0;
		public uint reflectivity = 255;
		public Texture texture = null;
		public Texture envmap = null;
		public bool flat = false;
		public bool wireframe = false;
		bool opaque = true;
		string texturePath = null;
		string envmapPath = null;
		public TextureSettings textureSettings = null;
		public TextureSettings envmapSettings = null;

		// Constructor

		public Material()
		{
		}

		public Material(uint color)
		{
			setColor(color);
		}

		public Material(Texture t)
		{
			setTexture(t);
			reflectivity = 255;
		}

		/*public Material(URL docURL, String filename)
		// Call from Applet
		{			
			int pos=0;
			String temp=docURL.toString();
			while (temp.indexOf("/",pos)>0) pos=temp.indexOf("/",pos)+1;
			temp=temp.substring(0,pos)+filename;
			while (temp.indexOf("/",pos)>0) pos=temp.indexOf("/",pos)+1;
			String file=temp.substring(pos);
			String base=temp.substring(0,pos);
			
			try{
				importFromStream(new java.net.URL(base+file).openStream(),new URL(base));
			}
			catch (Exception e){Debug.WriteLine(e+"");}
		}*/

		public Material(string filename)
		// Call from Application
		{
			string @base = filename.Substring(0, filename.Length - (Path.GetFileName(filename).Length));
			try
			{
				importFromStream(File.OpenRead(filename), @base);
			}
			catch (Exception e) { Debug.WriteLine(e + ""); }
		}


		// Setters

		public void setTexture(Texture t)
		{
			texture = t;
			if (texture != null) texture.Resize();
		}

		public void setEnvmap(Texture env)
		{
			envmap = env;
			env.resize(256, 256);
		}

		public void setColor(uint c)
		{
			color = c;
		}

		public void setTransparency(uint factor)
		{
			transparency = MathUtility.Crop(factor, 0, 255);
			opaque = (transparency == 0);
		}

		public void setReflectivity(uint factor)
		{
			reflectivity = MathUtility.Crop(factor, 0, 255);
		}

		public void setFlat(bool flat)
		{
			this.flat = flat;
		}

		public void setWireframe(bool wireframe)
		{
			this.wireframe = wireframe;
		}

		// Getters

		public Texture getTexture()
		{
			return texture;
		}

		public Texture getEnvmap()
		{
			return envmap;
		}

		public uint getColor()
		{
			return color;
		}

		public uint getTransparency()
		{
			return transparency;
		}

		public uint getReflectivity()
		{
			return reflectivity;
		}

		public bool isFlat()
		{
			return flat;
		}

		public bool isWireframe()
		{
			return wireframe;
		}

		// Material import

		private void importFromStream(Stream inStream, Object baseURL)
		{
			BinaryReader input = new BinaryReader(inStream);
			readSettings(input);
			readTexture(input, true);
			readTexture(input, false);
		}

		private void readSettings(BinaryReader inStream)
		{
			this.setColor((uint) this.readInt(inStream));
			this.setTransparency(inStream.ReadByte());
			this.setReflectivity(inStream.ReadByte());
			this.setFlat(inStream.ReadBoolean());
		}

		private int readInt(BinaryReader inStream)
		{
			return ((((inStream.ReadByte() << 0x18) | (inStream.ReadByte() << 0x10)) | (inStream.ReadByte() << 8)) | inStream.ReadByte());
		}

		private string readString(BinaryReader inStream)
		{
			byte num;
			string str = "";
			while ((num = inStream.ReadByte()) != 60)
			{
				str = str + ((char) num);
			}
			return str;
		}

		private void readTexture(BinaryReader inStream, bool textureId)
		{
			Texture t = null;
			switch (inStream.ReadSByte())
			{
				case 1:
					t = new Texture(readString(inStream));
					if ((t != null) && textureId)
					{
						this.texturePath = t.path;
						this.textureSettings = null;
						this.setTexture(t);
					}
					if (!((t == null) || textureId))
					{
						this.envmapPath = t.path;
						this.envmapSettings = null;
						this.setEnvmap(t);
					}
					break;

				case 2:
					{
						int w = this.readInt(inStream);
						int h = this.readInt(inStream);
						int num4 = inStream.ReadSByte();
						float persistency = this.readInt(inStream);
						float density = this.readInt(inStream);
						persistency = 0.5f;
						density = 0.5f;
						int samples = inStream.ReadByte();
						int num8 = inStream.ReadByte();
						uint[] colors = new uint[num8];
						for (int i = 0; i < num8; i++)
						{
							colors[i] = (uint) this.readInt(inStream);
						}
						switch (num4)
						{
							case 1:
								t = TextureFactory.PERLIN(w, h, persistency, density, samples, 0x400).colorize(ColorUtility.makeGradient(colors, 0x400));
								break;

							case 2:
								t = TextureFactory.WAVE(w, h, persistency, density, samples, 0x400).colorize(ColorUtility.makeGradient(colors, 0x400));
								break;

							case 3:
								t = TextureFactory.GRAIN(w, h, persistency, density, samples, 20, 0x400).colorize(ColorUtility.makeGradient(colors, 0x400));
								break;
						}
						if (textureId)
						{
							this.texturePath = null;
							this.textureSettings = new TextureSettings(t, w, h, num4, persistency, density, samples, colors);
							this.setTexture(t);
						}
						else
						{
							this.envmapPath = null;
							this.envmapSettings = new TextureSettings(t, w, h, num4, persistency, density, samples, colors);
							this.setEnvmap(t);
						}
						return;
					}
			}
		}
	}
}