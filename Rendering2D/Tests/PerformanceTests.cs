﻿using DeltaEngine.Content;
using DeltaEngine.Core;
using DeltaEngine.Datatypes;
using DeltaEngine.Entities;
using DeltaEngine.Platforms;
using DeltaEngine.Rendering2D.Fonts;
using NUnit.Framework;

namespace DeltaEngine.Rendering2D.Tests
{
	[Category("Slow")]
	public class PerformanceTests : TestWithMocksOrVisually
	{
		//ncrunch: no coverage start
		/// <summary>
		/// Draws 100*100 sprites each frame to test rendering performance (millions of polygons/s).
		/// </summary>
		[Test]
		public void Draw10000SpritesPerFrame()
		{
			var material = new Material(Shader.Position2DUV, "DeltaEngineLogo");
			for (int y = 0; y < 100; y++)
				for (int x = 0; x < 100; x++)
					new Sprite(material, new Rectangle(x / 100.0f, y / 100.0f, 0.01f, 0.01f));
			new FpsDisplay();
		}

		public class FpsDisplay : FontText, Updateable
		{
			public FpsDisplay()
				: base(Font.Default, "", Rectangle.FromCenter(0.5f, 0.25f, 0.2f, 0.2f)) {}

			public void Update()
			{
				Text = "Fps = " + GlobalTime.Current.Fps;
			}

			public bool IsPauseable { get { return true; } }
		}
	}
}
