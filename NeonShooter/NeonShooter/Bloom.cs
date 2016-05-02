using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BloomPostprocess
{
    public class Bloom
    {
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        Effect bloomExtractEffect;
        Effect bloomCombineEffect;
        Effect gaussianBlurEffect;
        RenderTarget2D renderTarget1;
        RenderTarget2D renderTarget2;
        public BloomSettings Settings { get { return settings; } set { settings = value; } }
        BloomSettings settings = BloomSettings.PresetSettings[0];

        public enum IntermediateBuffer { PreBloom, BlurredHorizontally, BlurredBothWays, FinalResult, }
        public IntermediateBuffer ShowBuffer { get { return showBuffer; } set { showBuffer = value; } }
        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;


        // C O N S T R U C T
        public Bloom(GraphicsDevice graphics, SpriteBatch passedSpriteBatch)
        {
            device = graphics;
            spriteBatch = passedSpriteBatch;
        }

        // L O A D 
        public void LoadContent(ContentManager Content, PresentationParameters pp)
        {
            bloomExtractEffect = Content.Load<Effect>("Shaders/BloomExtract");
            bloomCombineEffect = Content.Load<Effect>("Shaders/BloomCombine");
            gaussianBlurEffect = Content.Load<Effect>("Shaders/GaussianBlur");

            int width = pp.BackBufferWidth, height = pp.BackBufferHeight;
            SurfaceFormat format = pp.BackBufferFormat;

            // Create two rendertargets for the bloom processing. These are half the size of the backbuffer, in order to minimize fillrate costs. Reducing
            // the resolution in this way doesn't hurt quality, because we are going to be blurring the bloom images in any case.
            width /= 2;
            height /= 2;

            renderTarget1 = new RenderTarget2D(device, width, height, false, format, DepthFormat.None);
            renderTarget2 = new RenderTarget2D(device, width, height, false, format, DepthFormat.None);
        }
        public void UnloadContent()
        {
            renderTarget1.Dispose();
            renderTarget2.Dispose();
        }


        // D R A W
        public void Draw(RenderTarget2D sourceRenderTarget, RenderTarget2D destRenderTarget)
        {
            // Pass 1: draw the scene into rendertarget 1, using a shader that extracts only the brightest parts of the image.
            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(Settings.BloomThreshold);
            DrawFullscreenQuad(sourceRenderTarget, renderTarget1, bloomExtractEffect, IntermediateBuffer.PreBloom);

            // Pass 2: draw from rendertarget 1 into rendertarget 2, using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)renderTarget1.Width, 0);
            DrawFullscreenQuad(renderTarget1, renderTarget2, gaussianBlurEffect, IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1, using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)renderTarget1.Height);
            DrawFullscreenQuad(renderTarget2, renderTarget1, gaussianBlurEffect, IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            device.SetRenderTarget(destRenderTarget);

            EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(Settings.BloomIntensity);
            parameters["BaseIntensity"].SetValue(Settings.BaseIntensity);
            parameters["BloomSaturation"].SetValue(Settings.BloomSaturation);
            parameters["BaseSaturation"].SetValue(Settings.BaseSaturation);

            bloomCombineEffect.Parameters["BaseTexture"].SetValue(sourceRenderTarget);

            Viewport viewport = device.Viewport;

            DrawFullscreenQuad(renderTarget1, viewport.Width, viewport.Height, bloomCombineEffect, IntermediateBuffer.FinalResult);
            //now destRenderTarget has the result rendered into it
        }


        // DRAW FULL SCREEN QUAD 1
        void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget, Effect effect, IntermediateBuffer currentBuffer)
        {
            device.SetRenderTarget(renderTarget);
            DrawFullscreenQuad(texture, renderTarget.Width, renderTarget.Height, effect, currentBuffer);
        }

        // DRAW FULL SCREEN QUAD 2
        void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect, IntermediateBuffer currentBuffer)
        {
            // If the user has selected one of the show intermediate buffer options, we still draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer < currentBuffer)
            {
                effect = null;
            }
            device.Clear(Color.TransparentBlack); //<-- MUST do this for each target if using transparent target. 
            spriteBatch.Begin(0, BlendState.AlphaBlend, null, null, null, effect);
            spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
            spriteBatch.End();
        }


        // SET BLUR EFFECT PARAMETERS
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up the sample weight and offset effect parameters.
            EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one. This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            weightsParameter.SetValue(sampleWeights);  // Tell the effect about our new filter settings.
            offsetsParameter.SetValue(sampleOffsets);
        }


        // C O M P U T E   G A U S S I A N 
        float ComputeGaussian(float n)
        {
            float theta = Settings.BlurAmount;
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                            Math.Exp(-(n * n) / (2 * theta * theta)));
        }

    }
}
