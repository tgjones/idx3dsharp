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

namespace IDx3DSharp
{
	public static class MathUtility
	{
		#region Trigonometry

		private static float[] _sinus;
		private static float[] _cosinus;
		private static bool _trig;
		private static float _rad2Scale = 4096f / 3.14159265f / 2f;
		private static float _pad = 256 * 3.14159265f;

		public static float DegreesToRadians(float deg)
		{
			return deg * 0.0174532925194f;
		}

		public static float RadiansToDegrees(float rad)
		{
			return rad * 57.295779514719f;
		}

		public static float Sin(float angle)
		{
			if (!_trig) BuildTrig();
			return _sinus[(int) ((angle + _pad) * _rad2Scale) & 0xFFF];
		}

		public static float Cos(float angle)
		{
			if (!_trig) BuildTrig();
			return _cosinus[(int) ((angle + _pad) * _rad2Scale) & 0xFFF];
		}

		private static void BuildTrig()
		{
			System.Console.WriteLine(">> Building idx3d_Math LUT");
			_sinus = new float[4096];
			_cosinus = new float[4096];

			for (int i = 0; i < 4096; i++)
			{
				_sinus[i] = (float) Math.Sin((float) i / _rad2Scale);
				_cosinus[i] = (float) Math.Cos((float) i / _rad2Scale);
			}
			_trig = true;
		}

		public static float pythagoras(float a, float b)
		{
			return (float) Math.Sqrt(a * a + b * b);
		}

		public static int pythagoras(int a, int b)
		{
			return (int) Math.Sqrt(a * a + b * b);
		}

		#endregion

		#region Range tools

		public static uint Crop(uint num, uint min, uint max)
		{
			return (num < min) ? min : (num > max) ? max : num;
		}

		public static int Crop(int num, int min, int max)
		{
			return (num < min) ? min : (num > max) ? max : num;
		}

		public static float Crop(float num, float min, float max)
		{
			return (num < min) ? min : (num > max) ? max : num;
		}

		public static bool inrange(int num, int min, int max)
		{
			return ((num >= min) && (num < max));
		}

		#endregion

		#region Buffer operations

		public static void clearBuffer(uint[] buffer, uint value)
		{
			int size = buffer.Length - 1;
			int cleared = 1;
			int index = 1;
			buffer[0] = value;

			while (cleared < size)
			{
				Array.Copy(buffer, 0, buffer, index, cleared);
				size -= cleared;
				index += cleared;
				cleared <<= 1;
			}
			Array.Copy(buffer, 0, buffer, index, size);
		}

		public static void cropBuffer(int[] buffer, int min, int max)
		{
			for (int i = buffer.Length - 1; i >= 0; i--) buffer[i] = Crop(buffer[i], min, max);
		}

		public static void copyBuffer(uint[] source, uint[] target)
		{
			Array.Copy(source, 0, target, 0, Crop(source.Length, 0, target.Length));
		}

		#endregion

		public static float Random()
		{
			return (float) ((new Random().NextDouble() * 2.0) - 1.0);
		}

		public static float Random(float min, float max)
		{
			return (float) ((new Random().NextDouble() * (max - min)) + min);
		}

		public static float RandomWithDelta(float average, float delta)
		{
			return average + Random() * delta;
		}

		public static float Interpolate(float a, float b, float d)
		{
			float f = (1 - Cos(d * (float) Math.PI)) * 0.5f;
			return a + f * (b - a);
		}
	}
}