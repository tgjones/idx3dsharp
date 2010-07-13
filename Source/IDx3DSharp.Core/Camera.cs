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
    public class Camera
    {
        // F I E L D S

		public Matrix matrix=new Matrix();
		public Matrix normalmatrix=new Matrix();
	
		bool needsRebuild=true;   // Flag indicating changes on matrix

		// Camera settings
		public Vector pos=new Vector(0f,0f,0f);
		public Vector lookat=new Vector(0f,0f,0f);
		public float _roll=0f;
		
		public float fovfact;             // Field of View factor
		public int screenwidth;
		public int screenheight;
		public int screenscale;
		
	// C O N S T R U C T O R S

		public Camera()
		{
			setFov(90f);
		}

		public Camera(float fov)
		{
			setFov(fov);
		}

	// P U B L I C   M E T H O D S

    	public Matrix getMatrix()
		{
			rebuildMatrices();
			return matrix;
		}

    	public Matrix getNormalMatrix()
		{
			rebuildMatrices();
			return normalmatrix;
		}
		
		void rebuildMatrices()
		{
			if (!needsRebuild) return;
			needsRebuild=false;
		
			Vector forward,up,right;
			
			forward=Vector.Subtract(lookat,pos);
			up=new Vector(0f,1f,0f);
			right=Vector.GetNormal(up,forward);
			up=Vector.GetNormal(forward,right);
			
			forward.Normalize();
			up.Normalize();
			right.Normalize();
			
			normalmatrix=new Matrix(right,up,forward);
			normalmatrix.rotate(0,0,_roll);
			matrix=normalmatrix.Clone();
			matrix.shift(pos.X,pos.Y,pos.Z);
			
			normalmatrix=normalmatrix.inverse();
			matrix=matrix.inverse();
		}
		
		public void setFov(float fov)
		{
			fovfact=(float)Math.Tan(MathUtility.DegreesToRadians(fov)/2);
		}
		
		public void roll(float angle)
		{
			_roll+=angle;
			needsRebuild=true;
		}

		public void setPos(float px, float py, float pz)
		{
			pos=new Vector(px,py,pz);
			needsRebuild=true;
		}

		public void setPos(Vector p)
		{
			pos=p;
			needsRebuild=true;
		}
	
		public void lookAt(float px, float py, float pz)
		{
			lookat=new Vector(px,py,pz);
			needsRebuild=true;
		}

		public void lookAt(Vector p)
		{
			lookat=p;
			needsRebuild=true;
		}

		public void setScreensize(int w, int h)
		{
			screenwidth=w;
			screenheight=h;
			screenscale=(w<h)?w:h;
		}
		
	// MATRIX MODIFIERS
	
		public void shift(float dx, float dy, float dz)
		{
			pos=pos.Transform(Matrix.shiftMatrix(dx,dy,dz));
			lookat=lookat.Transform(Matrix.shiftMatrix(dx,dy,dz));
			needsRebuild=true;
			
		}
		
		public void shift(Vector v)
		{
			shift(v.X,v.Y,v.Z);

		}
	
		public void rotate(float dx, float dy, float dz)
		{
			pos=pos.Transform(Matrix.rotateMatrix(dx,dy,dz));
			needsRebuild=true;
		}
		
		public void rotate(Vector v)
		{
			rotate(v.X,v.Y,v.Z);
		}
		
		public static Camera FRONT()
		{
			Camera cam=new Camera();
			cam.setPos(0,0,-2f);
			return cam;
		}
		
		public static Camera LEFT()
		{
			Camera cam=new Camera();
			cam.setPos(2f,0,0);
			return cam;
		}
		
		public static Camera RIGHT()
		{
			Camera cam=new Camera();
			cam.setPos(-2f,0,0);
			return cam;
		}
		
		public static Camera TOP()
		{
			Camera cam=new Camera();
			cam.setPos(0,-2f,0);
			return cam;
		}
    }
}