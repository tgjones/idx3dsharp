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

    public abstract class CoreObject
    {
        // F I E L D S

        public Matrix matrix = new Matrix();
        public Matrix normalmatrix = new Matrix();

        // M A T R I X   O P E R A T I O N S

        public void transform(Matrix m)
        {
            matrix.transform(m);
            normalmatrix.transform(m);
        }

        public void shift(float dx, float dy, float dz)
        {
            matrix.shift(dx, dy, dz);
        }

        public void shift(Vector v)
        {
            matrix.shift(v.X, v.Y, v.Z);
        }

        public void scale(float d)
        {
            matrix.scale(d);
        }

        public void scale(float dx, float dy, float dz)
        {
            matrix.scale(dx, dy, dz);
        }

        public void scaleSelf(float d)
        {
            matrix.scaleSelf(d);
        }

        public void scaleSelf(float dx, float dy, float dz)
        {
            matrix.scaleSelf(dx, dy, dz);
        }

        public void rotate(Vector d)
        {
            rotateSelf(d.X, d.Y, d.Z);
        }

        public void rotateSelf(Vector d)
        {
            rotateSelf(d.X, d.Y, d.Z);
        }

        public void rotate(float dx, float dy, float dz)
        {
            matrix.rotate(dx, dy, dz);
            normalmatrix.rotate(dx, dy, dz);
        }

        public void rotateSelf(float dx, float dy, float dz)
        {
            matrix.rotateSelf(dx, dy, dz);
            normalmatrix.rotateSelf(dx, dy, dz);
        }

        public void setPos(float x, float y, float z)
        {
            matrix.m03 = x;
            matrix.m13 = y;
            matrix.m23 = z;
        }

        public void setPos(Vector v)
        {
            setPos(v.X, v.Y, v.Z);
        }

        public Vector getPos()
        {
            return new Vector(matrix.m03, matrix.m13, matrix.m23);
        }
    }
}