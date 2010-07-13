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
using System.Collections.Generic;
using System.Text;

namespace IDx3DSharp
{
public class SceneObject : CoreObject
{
	// F I E L D S
	
		public object userData=null;	// Can be freely used
		public string user=null; 	// Can be freely used

        public List<Vertex> vertexData = new List<Vertex>();
        public List<Triangle> triangleData = new List<Triangle>();
		
		public uint id;  // This object's index
		public string name="";  // This object's name
		public bool visible=true; // Visibility tag
		
		public Scene parent=null;
		private bool dirty=true;  // Flag for dirty handling
		
		public Vertex[] vertex;
		public Triangle[] triangle;
		
		public int vertices=0;
		public uint triangles=0;
		
		public Material material=null; 

	// C O N S T R U C T O R S

		public SceneObject()
		{
		}

	// D A T A  S T R U C T U R E S
	
		public Vertex Vertex(int id)
		{
			return (Vertex) vertexData[id];
		}
		
		public Triangle Triangle(int id)
		{
			return (Triangle) triangleData[id];
		}		

		public void addVertex(Vertex newVertex)
		{
			newVertex.parent=this;
			vertexData.Add(newVertex);
			dirty=true;
		}

		public void addTriangle(Triangle newTriangle)
		{
			newTriangle.parent=this;
			triangleData.Add(newTriangle);
			dirty=true;
		}
		
		public void addTriangle(int v1, int v2, int v3)
		{
			addTriangle(Vertex(v1),Vertex(v2),Vertex(v3));
		}
		
		public void removeVertex(Vertex v)
		{
			vertexData.Remove(v);
		}
		
		public void removeTriangle(Triangle t)
		{
			triangleData.Remove(t);
		}
		
		public void removeVertexAt(int pos)
		{
			vertexData.RemoveAt(pos);
		}
		
		public void removeTriangleAt(int pos)
		{
			triangleData.RemoveAt(pos);
		}
		
		
		public void setMaterial(Material m)
		{
			material=m;
		}
		
		public void rebuild()
		{
			if (!dirty) return;
			dirty=false;
			
			// Generate faster structure for vertices
			vertices=vertexData.Count;
			vertex=new Vertex[vertices];
			vertexData.CopyTo(vertex);
			
			// Generate faster structure for triangles
			triangles=(uint) triangleData.Count;
			triangle=new Triangle[triangles];
			triangleData.CopyTo(triangle);
			for (uint i=0, length = triangles;i<length;i++)
			{
				triangle[i].id=i;
			}
			
			for (int i=0, length = vertices;i<length;i++)
			{
				vertex[i].id=i;
				vertex[i].resetNeighbors();
			}
			
			Triangle tri;
			for (uint i=0, length = triangles;i<length;i++)
			{
				tri=triangle[i];
				tri.p1.registerNeighbor(tri);
				tri.p2.registerNeighbor(tri);
				tri.p3.registerNeighbor(tri);
			}

			regenerate();
		}

		public void addVertex(float x, float y, float z)
		{
			addVertex(new Vertex(x,y,z));
		}
		
		
		public void addVertex(float x, float y, float z, float u, float v)
		{
			Vertex vert=new Vertex(x,y,z);
			vert.setUV(u,v);
			addVertex(vert);
		}

		public void addTriangle(Vertex a, Vertex b, Vertex c)
		{
			addTriangle(new Triangle(a,b,c));
		}

		public void regenerate()
		// Regenerates the vertex normals
		{
			for (int i=0;i<triangles;i++) triangle[i].regenerateNormal();
			for (int i=0;i<vertices;i++) vertex[i].regenerateNormal();
		}

        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append("<object id=" + name + ">\r\n");
            for (int i = 0; i < vertices; i++) buffer.Append(vertex[i].ToString());
            return buffer.ToString();
        }
		
		public void scaleTextureCoordinates(float fu, float fv)
		{
			rebuild();
			for (int i=0;i<vertices;i++) vertex[i].scaleTextureCoordinates(fu,fv);
		}
		
		public void tilt(float fact)
		{
			rebuild();
			for (int i=0;i<vertices;i++)
				vertex[i].pos=Vector.Add(vertex[i].pos,Vector.Random(fact));
			regenerate();
		}
			
		public Vector Min()
		{
			if (vertices==0) return new Vector(0f,0f,0f);
			float minX=vertex[0].pos.X;
			float minY=vertex[0].pos.Y;
			float minZ=vertex[0].pos.Z;
			for (int i=1; i<vertices; i++) 
			{
				if(vertex[i].pos.X<minX) minX=vertex[i].pos.X;
				if(vertex[i].pos.Y<minY) minY=vertex[i].pos.Y;
				if(vertex[i].pos.Z<minZ) minZ=vertex[i].pos.Z;
			}
			return new Vector(minX,minY,minZ);
		}
		
		public Vector Max()
		{
			if (vertices==0) return new Vector(0f,0f,0f);
			float maxX=vertex[0].pos.X;
			float maxY=vertex[0].pos.Y;
			float maxZ=vertex[0].pos.Z;
			for (int i=1; i<vertices; i++) 
			{
				if(vertex[i].pos.X>maxX) maxX=vertex[i].pos.X;
				if(vertex[i].pos.Y>maxY) maxY=vertex[i].pos.Y;
				if(vertex[i].pos.Z>maxZ) maxZ=vertex[i].pos.Z;
			}
			return new Vector(maxX,maxY,maxZ);
		}
		
		
		public void detach()
		// Centers the object in its coordinate system
		// The offset from origin to object center will be transfered to the matrix,
		// so your object actually does not move.
		// Usefull if you want prepare objects for self rotation.
		{
			Vector center=getCenter();
			
			for (int i=0;i<vertices;i++)
			{
				vertex[i].pos.X-=center.X;	
				vertex[i].pos.Y-=center.Y;	
				vertex[i].pos.Z-=center.Z;	
			}
			shift(center);
		}
		
		public Vector getCenter()
		// Returns the center of this object
		{
			Vector max=Max();
			Vector min=Min();
			return new Vector((max.X+min.X)/2,(max.Y+min.Y)/2,(max.Z+min.Z)/2);
		}
		
		public Vector getDimension()
		// Returns the x,y,z - Dimension of this object
		{
			Vector max=Max();
			Vector min=Min();
			return new Vector(max.X-min.X,max.Y-min.Y,max.Z-min.Z);
		}			
		
		public void matrixMeltdown()
		// Applies the transformations in the matrix to all vertices
		// and resets the matrix to untransformed.
		{
			rebuild();
			for (int i=vertices-1;i>=0;i--)
				vertex[i].pos=vertex[i].pos.Transform(matrix);
			regenerate();
			matrix.reset();
			normalmatrix.reset();
		}
		
		public SceneObject Clone()
		{
			SceneObject obj=new SceneObject();
			rebuild();
			for(int i=0;i<vertices;i++) obj.addVertex(vertex[i].Clone());
			for(int i=0;i<triangles;i++) obj.addTriangle(triangle[i].Clone());
			obj.name=name+" [cloned]";
			obj.material=material;
			obj.matrix=matrix.Clone();
			obj.normalmatrix=normalmatrix.Clone();
			obj.rebuild();
			return obj;
		}
		
		public void removeDuplicateVertices()
		{
			rebuild();
            List<Edge> edgesToCollapse = new List<Edge>();
			for (int i=0;i<vertices;i++)
				for (int j=i+1;j<vertices;j++)
					if (vertex[i].equals(vertex[j],0.0001f))
						edgesToCollapse.Add(new Edge(vertex[i],vertex[j]));
            foreach (Edge edge in edgesToCollapse)
			    edgeCollapse(edge);
		
			removeDegeneratedTriangles();
		}
		
		public void removeDegeneratedTriangles()
		{
			rebuild();
			for (int i=0;i<triangles;i++)
				if (triangle[i].degenerated()) removeTriangleAt(i);
			
			dirty=true;
			rebuild();			
		}
		
		public void meshSmooth()
		{				
			rebuild();
			Triangle tri;
			float u,v;
			Vertex a,b,c,d,e,f,temp;
			Vector ab,bc,ca,nab,nbc,nca,center;
			float sab,sbc,sca,rab,rbc,rca;
			float uab,vab,ubc,vbc,uca,vca;
			float sqrt3=(float)Math.Sqrt(3f);
			
			for (int i=0;i<triangles;i++)
			{
				tri=Triangle(i);
				a=tri.p1;
				b=tri.p2;
				c=tri.p3;
				ab=Vector.Scale(0.5f,Vector.Add(b.pos,a.pos));
                bc = Vector.Scale(0.5f, Vector.Add(c.pos, b.pos));
                ca = Vector.Scale(0.5f, Vector.Add(a.pos, c.pos));
				rab=Vector.Subtract(ab,a.pos).Length();
                rbc = Vector.Subtract(bc, b.pos).Length();
                rca = Vector.Subtract(ca, c.pos).Length();
				
				nab=Vector.Scale(0.5f,Vector.Add(a.n,b.n));
                nbc = Vector.Scale(0.5f, Vector.Add(b.n, c.n));
                nca = Vector.Scale(0.5f, Vector.Add(c.n, a.n));
				uab=0.5f*(a.Tu+b.Tu);
				vab=0.5f*(a.Tv+b.Tv);
				ubc=0.5f*(b.Tu+c.Tu);
				vbc=0.5f*(b.Tv+c.Tv);
				uca=0.5f*(c.Tu+a.Tu);
				vca=0.5f*(c.Tv+a.Tv);
				sab=1f-nab.Length();
                sbc = 1f - nbc.Length();
                sca = 1f - nca.Length();
				nab.Normalize();
                nbc.Normalize();
                nca.Normalize();
				
				d=new Vertex(Vector.Subtract(ab,Vector.Scale(rab*sab,nab)),uab,vab);
                e = new Vertex(Vector.Subtract(bc, Vector.Scale(rbc * sbc, nbc)), ubc, vbc);
                f = new Vertex(Vector.Subtract(ca, Vector.Scale(rca * sca, nca)), uca, vca);
				
				addVertex(d);
				addVertex(e);
				addVertex(f);
				tri.p2=d;
				tri.p3=f;
				addTriangle(b,e,d);
				addTriangle(c,f,e);
				addTriangle(d,e,f);
			}
			removeDuplicateVertices();			
		}
		

	// P R I V A T E   M E T H O D S

		private void edgeCollapse(Edge edge)
		// Collapses the edge [u,v] by replacing v by u
		{
			Vertex u=edge.start();
			Vertex v=edge.end();
			if (!vertexData.Contains(u)) return;
            if (!vertexData.Contains(v)) return;
			rebuild();
			Triangle tri;
			for (int i=0; i<triangles; i++)
			{
				tri=Triangle(i);
				if (tri.p1==v) tri.p1=u;
				if (tri.p2==v) tri.p2=u;
				if (tri.p3==v) tri.p3=u;
			}
			vertexData.Remove(v);
		}
}
}