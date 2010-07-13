namespace IDx3DSharp
{
	public class TextureSettings
	{
		public Texture texture;
		public int width;
		public int height;
		public int type;
		public float persistency;
		public float density;
		public int samples;
		public int numColors;
		public uint[] colors;

		public TextureSettings(Texture tex, int w, int h, int t, float p, float d, int s, uint[] c)
		{
			texture = tex;
			width = w;
			height = h;
			type = t;
			persistency = p;
			density = d;
			samples = s;
			colors = c;
		}
	}
}