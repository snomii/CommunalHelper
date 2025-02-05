﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.CommunalHelper.Entities {
    [Tracked]
    class DreamJellyfishRenderer : Entity {

        private List<DreamJellyfish> jellies = new List<DreamJellyfish>();

        private VirtualRenderTarget renderTarget;
        private BlendState alphaMaskBlendState = new BlendState() {
            ColorSourceBlend = Blend.Zero,
            ColorBlendFunction = BlendFunction.Add,
            ColorDestinationBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add,
            AlphaDestinationBlend = Blend.Zero,
        };

        private float animTimer;

        public DreamJellyfishRenderer() {
            Depth = Depths.Player - 4;
            Tag = Tags.Global | Tags.TransitionUpdate;
            /* 
             * Okay, so this was the only way I could imagine doing this.
             * I was told that using a render target for masking stuff was a bit brutal, but I couldn't find another way.
             */
            renderTarget = VirtualContent.CreateRenderTarget("communalhelper-dreamjellyfishrenderer", 48, 42);
        }

        public void Track(DreamJellyfish jelly) {
            jellies.Add(jelly);
        }

        public void Untrack(DreamJellyfish jelly) {
            jellies.Remove(jelly);
        }

        public override void Removed(Scene scene) {
            Dispose();
            base.Removed(scene);
        }

        public override void SceneEnd(Scene scene) {
            Dispose();
            base.SceneEnd(scene);
        }

        private void Dispose() {
            if (renderTarget != null)
                renderTarget.Dispose();
            renderTarget = null;
        }

        public override void Update() {
            base.Update();
            animTimer += 6f * Engine.DeltaTime;
        }

        public override void Render() {
            Camera camera = SceneAs<Level>().Camera;

            foreach (DreamJellyfish jelly in jellies) {

                Rectangle jellyBounds = jelly.ParticleBounds;
                Vector2 pos = jelly.Position + new Vector2(jellyBounds.X, jellyBounds.Y);

                float left = pos.X;
                float right = pos.X + jellyBounds.Width;
                float top = pos.Y;
                float bottom = pos.Y + jellyBounds.Height;
                if (right < camera.Left || left > camera.Right || bottom < camera.Top || top > camera.Bottom)
                    continue; // Skip jelly rendering if it's not on screen.

                MTexture frame = jelly.Sprite.Texture;

                // Outline
                frame.DrawCentered(jelly.Position + new Vector2(0, -3), Color.White, jelly.Sprite.Scale, jelly.Sprite.Rotation);
                frame.DrawCentered(jelly.Position + new Vector2(0, -5), Color.White, jelly.Sprite.Scale, jelly.Sprite.Rotation);
                frame.DrawCentered(jelly.Position + new Vector2(-1, -4), Color.White, jelly.Sprite.Scale, jelly.Sprite.Rotation);
                frame.DrawCentered(jelly.Position + new Vector2(1, -4), Color.White, jelly.Sprite.Scale, jelly.Sprite.Rotation);

                GameplayRenderer.End();
                // Here we start drawing on a virtual texture.
                Engine.Graphics.GraphicsDevice.SetRenderTarget(renderTarget);
                Engine.Graphics.GraphicsDevice.Clear(Color.Lerp(Color.Black, Color.White, jelly.Flash));

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect);
                for (int i = 0; i < jelly.Particles.Length; i++) {
                    int layer = jelly.Particles[i].Layer;

                    Vector2 particlePos = jelly.Particles[i].Position;
                    particlePos += camera.Position * (0.3f + 0.25f * layer);
                    while (particlePos.X < left) {
                        particlePos.X += jellyBounds.Width;
                    }
                    while (particlePos.X > right) {
                        particlePos.X -= jellyBounds.Width;
                    }
                    while (particlePos.Y < top) {
                        particlePos.Y += jellyBounds.Height;
                    }
                    while (particlePos.Y > bottom) {
                        particlePos.Y -= jellyBounds.Height;
                    }

                    Color color = jelly.AllowDreamDash ? jelly.Particles[i].EnabledColor : jelly.Particles[i].DisabledColor;
                    MTexture mTexture;
                    switch (layer) {
                        case 0: {
                            int num2 = (int) ((jelly.Particles[i].TimeOffset * 4f + animTimer) % 4f);
                            mTexture = DreamJellyfish.ParticleTextures[3 - num2];
                            break;
                        }
                        case 1: {
                            int num = (int) ((jelly.Particles[i].TimeOffset * 2f + animTimer) % 2f);
                            mTexture = DreamJellyfish.ParticleTextures[1 + num];
                            break;
                        }
                        default:
                            mTexture = DreamJellyfish.ParticleTextures[2];
                            break;
                    }
                    mTexture.DrawCentered(particlePos - pos, color);
                }
                Draw.SpriteBatch.End();

                // We have drawn the dream block background and the stars, and we want to mask it using an alpha mask.
                // The alpha masks are the same images that Gliders have, only with alpha information, no color, so they overlap the region in which we want the background to be visible.

                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, alphaMaskBlendState, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, ColorGrade.Effect);
                frame.DrawCentered(
                    new Vector2(-1, 10) + new Vector2(jellyBounds.Width, jellyBounds.Height) / 2f,
                    Color.White,
                    jelly.Sprite.Scale,
                    jelly.Sprite.Rotation
                );
                Draw.SpriteBatch.End();

                // Mask is drawn, we'll switch back to where the game was drawing entities previously.
                Engine.Graphics.GraphicsDevice.SetRenderTarget(GameplayBuffers.Gameplay);
                GameplayRenderer.Begin();
                // Draw Virtual Texture
                Draw.SpriteBatch.Draw(renderTarget, pos, Color.White);
            }
        }

        #region Hooks

        internal static void Load() {
            On.Celeste.LevelLoader.LoadingThread += LevelLoader_LoadingThread;
        }

        internal static void Unload() {
            On.Celeste.LevelLoader.LoadingThread -= LevelLoader_LoadingThread;
        }

        private static void LevelLoader_LoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self) {
            self.Level.Add(new DreamJellyfishRenderer()); // must add before calling orig, has shown to possibly crash otherwise.
            orig(self);
        }

        #endregion
    }
}
