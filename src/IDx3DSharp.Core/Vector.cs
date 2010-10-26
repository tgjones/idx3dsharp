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
    /// <summary>
    /// Defines a 3D vector.
    /// </summary>
    public class Vector : ICloneable
    {
#region Properties

        public float X = 0;      //Cartesian (default)
        public float Y = 0;      //Cartesian (default)
        public float Z = 0;      //Cartesian (default),Cylindric
        public float R = 0;      //Cylindric
        public float Theta = 0;  //Cylindric

#endregion
        // C O N S T R U C T O R S

        public Vector()
        {
        }

        public Vector(float xpos, float ypos, float zpos)
        {
            X = xpos;
            Y = ypos;
            Z = zpos;
        }

        // P U B L I C   M E T H O D S

        public Vector Normalize()
        // Normalizes the vector
        {
            float dist = Length();
            if (dist == 0) return this;
            float invdist = 1 / dist;
            X *= invdist;
            Y *= invdist;
            Z *= invdist;
            return this;
        }

        /// <summary>
        /// Reverses the vector.
        /// </summary>
        /// <returns></returns>
        public Vector Reverse()
        {
            X = -X;
            Y = -Y;
            Z = -Z;
            return this;
        }

        /// <summary>
        /// Length of this vector.
        /// </summary>
        /// <returns></returns>
        public float Length()
        {
            return (float) Math.Sqrt(X*X + Y*Y + Z*Z);
        }

        /// <summary>
        /// Modifies the vector by matrix m.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public Vector Transform(Matrix m)
        {
            float newx = X * m.m00 + Y * m.m01 + Z * m.m02 + m.m03;
            float newy = X * m.m10 + Y * m.m11 + Z * m.m12 + m.m13;
            float newz = X * m.m20 + Y * m.m21 + Z * m.m22 + m.m23;
            return new Vector(newx, newy, newz);
        }

        /// <summary>
        /// Builds the cylindric coordinates out of the given cartesian coordinates.
        /// </summary>
        public void BuildCylindric()
        {
            R = (float)Math.Sqrt(X * X + Y * Y);
            Theta = (float)Math.Atan2(X, Y);
        }

        /// <summary>
        /// Builds the cartesian coordinates out of the given cylindric coordinates.
        /// </summary>
        public void BuildCartesian()
        {
            X = R * MathUtility.Cos(Theta);
            Y = R * MathUtility.Sin(Theta);
        }

        /// <summary>
        /// returns the normal vector of the plane defined by the two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector GetNormal(Vector a, Vector b)
        {
            return VectorProduct(a, b).Normalize();
        }

        /// <summary>
        /// returns the normal vector of the plane defined by the two vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector GetNormal(Vector a, Vector b, Vector c)
        {
            return VectorProduct(a, b, c).Normalize();
        }

        /// <summary>
        /// Returns a x b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector VectorProduct(Vector a, Vector b)
        {
            return new Vector(a.Y * b.Z - b.Y * a.Z, a.Z * b.X - b.Z * a.X, a.X * b.Y - b.X * a.Y);
        }

        /// <summary>
        /// Returns (b-a) x (c-a).
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Vector VectorProduct(Vector a, Vector b, Vector c)
        {
            return VectorProduct(Subtract(b, a), Subtract(c, a));
        }

        /// <summary>
        /// Returns the angle between 2 vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Angle(Vector a, Vector b)
        {
            a.Normalize();
            b.Normalize();
            return (a.X * b.X + a.Y * b.Y + a.Z * b.Z);
        }

        /// <summary>
        /// Adds 2 vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector Add(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        /// <summary>
        /// Subtracts 2 vectors.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vector Subtract(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        /// <summary>
        /// Scales a vector by the specified amount.
        /// </summary>
        /// <param name="f"></param>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Vector Scale(float f, Vector a)
        {
            return new Vector(f*a.X, f*a.Y, f*a.Z);
        }

        /// <summary>
        /// Returns the length of a vector.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Length(Vector a)
        {
            return (float) Math.Sqrt(a.X*a.X + a.Y*a.Y + a.Z*a.Z);
        }

        /// <summary>
        /// Returns a random vector.
        /// </summary>
        /// <param name="fact"></param>
        /// <returns></returns>
        public static Vector Random(float fact)
        {
            return new Vector(fact * MathUtility.Random(), fact * MathUtility.Random(), fact * MathUtility.Random());
        }

        public override string ToString()
        {
            return "<vector x=" + X + " y=" + Y + " z=" + Z + ">\r\n";
        }

        public Vector Clone()
        {
            return new Vector(X, Y, Z);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}