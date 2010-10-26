using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IDx3DSharp.DemoApp.Demos;

namespace IDx3DSharp.DemoApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using (MainForm testApp = new MainForm(new TestComponent2()))
			{
				testApp.Show();
				while (testApp.Created)
				{
					testApp.UpdateScene();
					testApp.Invalidate();
					Application.DoEvents();
				}
			}
		}
	}
}
