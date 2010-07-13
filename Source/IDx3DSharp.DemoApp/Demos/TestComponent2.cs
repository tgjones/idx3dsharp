using System;
using System.IO;

namespace IDx3DSharp.DemoApp.Demos
{
	public class TestComponent2 : BaseDemo
	{
		public override void PopulateScene(Scene scene)
		{
			scene.addMaterial("Stone1", new Material(new Texture("stone1.jpg")));
			scene.addMaterial("Stone2", new Material(new Texture("stone2.jpg")));
			scene.addMaterial("Stone3", new Material(new Texture("stone3.jpg")));
			scene.addMaterial("Stone4", new Material(new Texture("stone4.jpg")));

			scene.addLight("Light1", new Light(new Vector(0.2f, 0.2f, 1f), 0xFFFFFF, 144, 120));
			scene.addLight("Light2", new Light(new Vector(-1f, -1f, 1f), 0x332211, 100, 40));
			scene.addLight("Light3", new Light(new Vector(-1f, -1f, 1f), 0x666666, 200, 120));

			try
			{
				new Importer3ds().importFromStream(File.OpenRead("wobble.3ds"), scene);
			}
			catch (Exception e) { System.Console.WriteLine(e + ""); }

			scene.rebuild();
			for (int i = 0; i < scene.objects; i++)
				TextureProjector.ProjectFrontal(scene._object[i]);

			scene.Object("Sphere1").setMaterial(scene.material("Stone1"));
			scene.Object("Wobble1").setMaterial(scene.material("Stone2"));
			scene.Object("Wobble2").setMaterial(scene.material("Stone3"));
			scene.Object("Wobble3").setMaterial(scene.material("Stone4"));
			scene.normalize();
		}
	}
}