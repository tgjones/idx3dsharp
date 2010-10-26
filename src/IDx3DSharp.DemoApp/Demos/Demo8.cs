using System;
using System.IO;

namespace IDx3DSharp.DemoApp.Demos
{
	public class Demo8 : BaseDemo
	{
		public override void PopulateScene(Scene scene)
		{
			scene.addMaterial("Chrome", new Material("chrome.material"));

			try
			{
				new Importer3ds().importFromStream(File.OpenRead("mech.3ds"), scene);
			}
			catch (Exception e) { System.Console.WriteLine(e + ""); }

			scene.rebuild();
			for (int i = 0; i < scene.objects; i++)
				scene._object[i].setMaterial(scene.material("Chrome"));
			scene.normalize();
			scene.rotate(3.14159265f / 2, 3.14159265f / 2, 0f);
		}
	}
}