using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ship_Game
{
	public sealed class HaloTestScreen : GameScreen
	{
		private Effect effect;

		private Model Mesh;

		private Matrix View;

		private Matrix Projection;

		public HaloTestScreen(GameScreen parent) : base(parent)
		{
		}

		public override void Draw(GameTime gameTime)
		{
			Matrix world = Matrix.CreateScale(4.05f);
			Matrix view = (world * this.View) * this.Projection;
			this.effect.Parameters["World"].SetValue(world);
			this.effect.Parameters["Projection"].SetValue(this.Projection);
			this.effect.Parameters["View"].SetValue(this.View);
			this.effect.Parameters["CameraPosition"].SetValue(new Vector3(0f, 0f, 1500f));
			this.effect.Parameters["DiffuseLightDirection"].SetValue(new Vector3(-0.98f, 0.425f, -0.4f));
			this.effect.CurrentTechnique = this.effect.Techniques["Planet"];
			this.effect.Begin();
			foreach (EffectPass pass in this.effect.CurrentTechnique.Passes)
			{
				pass.Begin();
				this.Mesh.Meshes[0].Draw();
				pass.End();
			}
			this.effect.End();
			base.ScreenManager.GraphicsDevice.RenderState.AlphaBlendEnable = false;
			base.ScreenManager.GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
		}


		public override void LoadContent()
		{
			this.Mesh = TransientContent.Load<Model>("Model/sphere");
			this.effect = TransientContent.Load<Effect>("Effects/PlanetHalo");
			float width = (float)base.Viewport.Width;
			Viewport viewport = base.Viewport;
			float aspectRatio = width / (float)viewport.Height;
			Vector3 camPos = new Vector3(0f, 0f, 1500f) * new Vector3(-1f, 1f, 1f);
			this.View = ((Matrix.CreateTranslation(0f, 0f, 0f) 
                * Matrix.CreateRotationY(180f.ToRadians())) 
                * Matrix.CreateRotationX(0f.ToRadians())) 
                * Matrix.CreateLookAt(camPos, new Vector3(camPos.X, camPos.Y, 0f), new Vector3(0f, -1f, 0f));
			this.Projection = Matrix.CreatePerspectiveFieldOfView(0.7853982f, aspectRatio, 1f, 10000f);
			base.LoadContent();
		}
	}
}