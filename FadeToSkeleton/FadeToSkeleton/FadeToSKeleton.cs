using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Kinect;

namespace PullHeadOff
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class PullHeadOff : Microsoft.Xna.Framework.Game
    {

        private KinectSensor _KinectDevice;
      //  private Int32Rect _GreenScreenImageRect; This was in the chapter on greenscreen but it needs a reference

        private short[] _DepthPixelData;

        SpriteBatch spriteBatch;
        private CoordinateMapper coordinateMapper;


        private Skeleton[] _SkeletonData;

        Effect kinectColorVisualizer;
       // private DepthImagePoint rootDepthPoint;
        private Texture2D hat;
        float STANDARD_Z_DEPTH = 0.8f;
        Vector3 headPosition;
        Camera camera;
        GraphicsDeviceManager graphics;
        Texture2D colorVideo;
        Texture2D framesAnimation;
        // for spriteBatch draw 
        Texture2D texture;
        Vector2 position;

        Rectangle torsoPlaceRec;
        Rectangle leftArmPlaceRec;
        Rectangle rightArmPlaceRec;

        Rectangle destinationRectangle;       
        Rectangle headSourceRec;
        Rectangle leftEyeSourceRectangle;
        Color color;
        Vector2 origin;
        float rotation;
        Vector2 scale;
        SpriteEffects spriteEffects;
        float layerDepth;
        Vector2 resolution;
        Skeleton first;
        byte[] rawColorPixelData;
        byte[] colorPixelData;
        byte[] backGroundSnapShot;

        private Texture2D backGroundTexture;
        float startAnimationZ = 2;
        float endAnimationZ = 1.5f;

        int deviation = 0;
        private Rectangle animationSourceRectangle;
        private Vector3 shoulderCenterPosition;
        private Vector3 hipCenterPosition;
        private Vector3 handLeftPosition;
        private Vector3 shoulderLeftPosition;
        private Vector3 shoulderRightPosition;







        private Texture2D bark;
        private DepthStencilState depthState;

        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;
        SoundEffectInstance sound;
        private float pitch;
        private SoundEffectInstance soundForward;
        private SoundEffectInstance soundBackwards;


        public PullHeadOff()
              

        {
            graphics = new GraphicsDeviceManager(this);
            this.graphics.PreferredBackBufferWidth = 1280;
            this.graphics.PreferredBackBufferHeight = 960;

            
     
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
       
            headPosition = new Vector3(0, 0, STANDARD_Z_DEPTH);
            shoulderLeftPosition = new Vector3(0, 0, STANDARD_Z_DEPTH);
            shoulderRightPosition = new Vector3(0, 0, STANDARD_Z_DEPTH);
            shoulderCenterPosition  = new Vector3(0, 0, STANDARD_Z_DEPTH);
            handLeftPosition = new Vector3(0, 0, STANDARD_Z_DEPTH);

            hipCenterPosition  = new Vector3(0, 0, STANDARD_Z_DEPTH);
            position = new Vector2(0,0);
            destinationRectangle = new Rectangle(0, 0, 1280, 960);
            headSourceRec = new Rectangle();
            leftEyeSourceRectangle = new Rectangle();
            color = Color.White;
            origin = new Vector2(0,0);
            rotation = 0;
            scale = new Vector2(1f,1f);
            spriteEffects = SpriteEffects.None;
            layerDepth = 1;
            resolution = new Vector2(640, 480);
            camera = new Camera(this, new Vector3(0, 0, 5), Vector3.Zero, Vector3.Up);
            Components.Add(camera);
            _KinectDevice = KinectSensor.KinectSensors[0];
            _KinectDevice.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);
            _KinectDevice.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            var parameters = new TransformSmoothParameters

            {
               
            };
            _KinectDevice.SkeletonStream.Enable(parameters);

            _KinectDevice.AllFramesReady += kinect_AllFramesReady;


            DepthImageStream depthStream = this._KinectDevice.DepthStream;
            this._DepthPixelData = new short[depthStream.FramePixelDataLength];
            this._SkeletonData = new Skeleton[this._KinectDevice.SkeletonStream.FrameSkeletonArrayLength];



            _KinectDevice.Start();
            backGroundSnapShot = new byte[this._KinectDevice.ColorStream.FramePixelDataLength];
            coordinateMapper = new CoordinateMapper(_KinectDevice);
           colorVideo = new Texture2D(graphics.GraphicsDevice, _KinectDevice.ColorStream.FrameWidth, _KinectDevice.ColorStream.FrameHeight);
           texture = new Texture2D(graphics.GraphicsDevice, _KinectDevice.ColorStream.FrameWidth, _KinectDevice.ColorStream.FrameHeight);
            base.Initialize();
        }

       
   
           void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs allFrame)
        {
            if (null == this._KinectDevice)
            {
                return;
            }
 using (ColorImageFrame colorFrame = allFrame.OpenColorImageFrame())
                            {
                               rawColorPixelData = new byte[this._KinectDevice.ColorStream.FramePixelDataLength];

                   
                                if (null != colorFrame)
                                {   // Copy the pixel data from the image to a temporary array
                                    //colorFrame.CopyPixelDataTo(secondColorPixelData);
                                    colorFrame.CopyPixelDataTo(rawColorPixelData);
                         
                                      // texture = new Texture2D(graphics.GraphicsDevice, 1280, 960);
                                      //  texture.SetData(playerImage);
                                    

                                }
            using (SkeletonFrame skeletonFrame = allFrame.OpenSkeletonFrame())
            {
                if (null != skeletonFrame)
                {
                    skeletonFrame.CopySkeletonDataTo(this._SkeletonData);
                first = (from s in this._SkeletonData where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                    if (first == null)
                        return;
                    JointType joint = JointType.Head;
                    headPosition = new Vector3(first.Joints[joint].Position.X, first.Joints[joint].Position.Y, first.Joints[joint].Position.Z);
                    joint = JointType.ShoulderCenter;
                    shoulderCenterPosition = new Vector3(first.Joints[joint].Position.X, first.Joints[joint].Position.Y, first.Joints[joint].Position.Z);
                    joint = JointType.HipCenter;
                    hipCenterPosition = new Vector3(first.Joints[joint].Position.X, first.Joints[joint].Position.Y, first.Joints[joint].Position.Z);
                    joint = JointType.HandLeft;
                    handLeftPosition = new Vector3(first.Joints[joint].Position.X, first.Joints[joint].Position.Y, first.Joints[joint].Position.Z);

                    joint = JointType.ShoulderLeft;
                    shoulderLeftPosition = new Vector3(first.Joints[joint].Position.X, first.Joints[joint].Position.Y, first.Joints[joint].Position.Z);
                    joint = JointType.ShoulderRight;
                    shoulderRightPosition = new Vector3(first.Joints[joint].Position.X, first.Joints[joint].Position.Y, first.Joints[joint].Position.Z);
                    joint = JointType.HandRight;
                  
                
                    handRightPosition = coordinateMapper.MapSkeletonPointToColorPoint(first.Joints[joint].Position, ColorImageFormat.RgbResolution1280x960Fps12);  
                    using (DepthImageFrame depthFrame = allFrame.OpenDepthImageFrame())
                    {
                        if (null != depthFrame)
                        {
                            depthFrame.CopyPixelDataTo(this._DepthPixelData);
                           

    
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            SoundEffect soundEffect =  Content.Load<SoundEffect>(@"Poetry");
   
            sound = soundEffect.CreateInstance();
            soundEffect = Content.Load<SoundEffect>(@"PoetryBackwards");
            soundBackwards = soundEffect.CreateInstance();
            audioEngine = new AudioEngine(@"Content\Sound.xgs");
            waveBank = new WaveBank(audioEngine, @"Content\Wave Bank.xwb");
            soundBank = new SoundBank(audioEngine, @"Content\Sound Bank.xsb");
          // soundBank.PlayCue("07");          
           sound.Play();
           // mask = Content.Load<Texture2D>(@"singleWasp");
            bark = Content.Load<Texture2D>(@"bark5");
            redSpot = Content.Load<Texture2D>(@"RedSpot");
           framesAnimation = Content.Load<Texture2D>(@"frames0Wasp");
           backGroundTexture = Content.Load<Texture2D>(@"BackField");
          //  thisModel = new BasicModel(Content.Load<Model>(@"bark"));
           hat = Content.Load<Texture2D>(@"hat");
          //  texture = Content.Load<Texture2D>(@"transparent");
            this.kinectColorVisualizer = Content.Load<Effect>("KinectColorVisualizer");
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 


        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            int animationFrame;
            float startheadcutoff = 0;
            if (headPosition.Z >= startAnimationZ )
            {
                animationFrame = (int)((startAnimationZ - headPosition.Z) / ((startAnimationZ - endAnimationZ) / 48));
               // animationFrame = 0;
            }
            else
                if (headPosition.Z - endAnimationZ <= 0)
                {
                    animationFrame = (int)((startAnimationZ - headPosition.Z) / ((startAnimationZ - endAnimationZ) / 48));
                  //  animationFrame = 0;
                    startheadcutoff = headPosition.Z-endAnimationZ;
                }
                else
                {
                    animationFrame = (int)(( startAnimationZ - headPosition.Z) / ((startAnimationZ - endAnimationZ) / 48));
                }
            animationSourceRectangle = new Rectangle((animationFrame % 6)*150, (animationFrame % 8)*150, 150, 150);
         headSourceRec = new Rectangle((int)(550 + headPosition.X * (1280 * (STANDARD_Z_DEPTH / headPosition.Z))), (int)(400 + headPosition.Y * (-580 * (1.8 / headPosition.Z))), (int)(450 * (STANDARD_Z_DEPTH / headPosition.Z)), (int)(400 * (STANDARD_Z_DEPTH / headPosition.Z)));
                float torsoHeight = shoulderCenterPosition.Y - hipCenterPosition.Y ;
                float torsoWidth = handRightPosition.X - handLeftPosition.X;
                torsoPlaceRec = new Rectangle(0, 0, 1280, 960);
       

            //leftArmPlaceRec;
            // rightArmPlaceRec;
 
       
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                captureBackground();
           
            // setting source rectangles for eyes

             base.Update(gameTime);
            handRightRectangle = new Rectangle((int)handRightPosition.X, (int) handRightPosition.Y, 50, 50);
        }

        private void captureBackground()
        {
            backGroundSnapShot = rawColorPixelData;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 
        byte AlphaFade = 255;
        float colorFade = 1;
      //  private Texture2D redSpot;
        private Rectangle handRightRectangle;
        private ColorImagePoint handRightPosition;
        private Texture2D redSpot;
        protected override void Draw(GameTime gameTime)
        {
            if(colorFade > 0)
                colorFade -=.0039f;
            if (AlphaFade > 0)
                AlphaFade -= 1;
            // if the colorPixelData has been set, convert the colors to bgr and set it to the video
            if (rawColorPixelData != null)
            {

                colorPixelData = new byte[rawColorPixelData.Length];


                // convert RGBto BGR
          
                for (int i = 0; i < rawColorPixelData.Length; i += 4)
                {
                    colorPixelData[i] = (byte)(rawColorPixelData[i]*4/4 * colorFade);
                    colorPixelData[i + 1] =(byte)(rawColorPixelData[i + 1]*4/4*colorFade);
                    colorPixelData[i + 2] = (byte)(rawColorPixelData[i + 2]*4/4*colorFade);
                    colorPixelData[i + 3] = AlphaFade;
                }

                 // make cutout
                //int tolerance = 30;
                //int toleranceGreen = 20;
                //for (int i = 0; i < colorPixelData.Length; i += 4)
                //{
                //    int c = i % 5120;
                //    if (
                //        ((i <= (1280 * (headSourceRec.Y - 30) + headSourceRec.X) * 4 
                //        || i >= (1280 * (headSourceRec.Y + headSourceRec.Height) + headSourceRec.X + headSourceRec.Width) * 4 
                //        || (c >= (4 * (headSourceRec.X + headSourceRec.Width + 50))) || c <= (headSourceRec.X * 4))
                //        ) 
                //        && (
                //        i <= (1280 * (torsoPlaceRec.Y - 30) + torsoPlaceRec.X) * 4 
                //        || i >= (1280 * ( torsoPlaceRec.Y +  torsoPlaceRec.Height) +  torsoPlaceRec.X +  torsoPlaceRec.Width) * 4 
                //        || (c >= (4* ( torsoPlaceRec.X +  torsoPlaceRec.Width+50))) 
                //        || c <= ( torsoPlaceRec.X*4)
                //        )
 
                //        ||((rawColorPixelData[i + 1] > (rawColorPixelData[i] - tolerance) 
                //        && rawColorPixelData[i + 1] > (rawColorPixelData[i + 2]) + toleranceGreen) 
                //        || ((rawColorPixelData[i] <= backGroundSnapShot[i] + tolerance 
                //        && rawColorPixelData[i] >= backGroundSnapShot[i] - tolerance 
                //        && rawColorPixelData[i + 1] > backGroundSnapShot[i + 1] - toleranceGreen 
                //        && rawColorPixelData[i + 2] <= backGroundSnapShot[i + 2] + tolerance 
                //        && rawColorPixelData[i + 2] >= backGroundSnapShot[i + 2] - tolerance)))
                //        )
                //        //(rawColorPixelData[i] <= backGroundSnapShot[i] + tolerance && rawColorPixelData[i + 1] > backGroundSnapShot[i + 1] - toleranceGreen && rawColorPixelData[i + 2] <= backGroundSnapShot[i + 2] + tolerance)
                //        /*(rawColorPixelData[i] >= backGroundSnapShot[i] - tolerance && rawColorPixelData[i] <= backGroundSnapShot[i] + toleranceGreen
                //        && rawColorPixelData[i + 1] > backGroundSnapShot[i + 1]-toleranceGreen 
                //        && rawColorPixelData[i + 2] >= backGroundSnapShot[i + 2] - tolerance   && rawColorPixelData[i + 2] <= backGroundSnapShot[i + 2] + tolerance)*/
                //    //if (rawColorPixelData[i] >= backGroundSnapShot[i] - tolerance && rawColorPixelData[i] <= backGroundSnapShot[i] + toleranceGreen && rawColorPixelData[i + 1] >= backGroundSnapShot[i + 1] - toleranceGreen && rawColorPixelData[i + 1] <= backGroundSnapShot[i + 1] + tolerance && rawColorPixelData[i + 2] >= backGroundSnapShot[i + 2] - tolerance && rawColorPixelData[i + 2] <= backGroundSnapShot[i + 2] + tolerance)
                //    {
                         
                //     //   if (i <= (1280 * (torsoPlaceRec.Y - 30) + torsoPlaceRec.X) * 4 || i >= (1280 * (torsoPlaceRec.Y + torsoPlaceRec.Height) + torsoPlaceRec.X + torsoPlaceRec.Width) * 4 || (c >= (4 * (torsoPlaceRec.X + torsoPlaceRec.Width + 50))) || c <= (torsoPlaceRec.X * 4))
                //        {
                //        colorPixelData[i] = 0x00;
                //        colorPixelData[i + 1] = 0x00;
                //        colorPixelData[i + 2] = 0x00;
                //        colorPixelData[i + 3] = 0x00;
                //        }
                //    }
                //}
                // set to video
                      colorVideo = new Texture2D(graphics.GraphicsDevice, 1280, 960);
                    colorVideo.SetData(colorPixelData);

                    ////// -----------shift 



                
                backGroundTexture.SetData(backGroundSnapShot);
            }

           
            GraphicsDevice.Clear(Color.Black);
            depthState = new DepthStencilState();
            depthState.StencilEnable = true;
            depthState.StencilFunction = CompareFunction.Always;
            depthState.StencilPass = StencilOperation.Replace;
            depthState.ReferenceStencil = 1;
            //if ((headSourceRec.X + headSourceRec.Width) >= colorVideo.Width)
            //{
            //    headSourceRec = new Rectangle(colorVideo.Width - headSourceRec.Width - 5, headSourceRec.Y, headSourceRec.Width, headSourceRec.Height);
            //}
            spriteBatch.Begin(SpriteSortMode.BackToFront, null, null, depthState, null, this.kinectColorVisualizer);
            spriteBatch.Draw(backGroundTexture, Vector2.Zero, new Rectangle(0, 0, 1280, 960), color, rotation, origin, scale, SpriteEffects.None, layerDepth);
          //  spriteBatch.Draw(mask, headSourceRec, color);
            //spriteBatch.End();


            //depthState = new DepthStencilState();
            //depthState.StencilEnable = true;  
            //depthState.StencilFunction = CompareFunction.Equal;
            //depthState.ReferenceStencil = 1;
            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, depthState, null, this.kinectColorVisualizer);
           // depthState.StencilFunction = CompareFunction.Equal;
           // depthState.ReferenceStencil = 1;
      //      spriteBatch.Draw(colorVideo, leftEyeDestRectangle, leftEyeSourceRectangle, color);
    //        spriteBatch.Draw(colorVideo, rightEyeDestRectangle, rightEyeSourceRectangle, color);
     //       spriteBatch.Draw(colorVideo, mouthDestRectangle, mouthSourceRectangle, color, 0f, Vector2.Zero, spriteEffects, layerDepth);
       //   spriteBatch.Draw(bark, torsoPlaceRec, color);

           spriteBatch.Draw(colorVideo, torsoPlaceRec, torsoPlaceRec, color);
          spriteBatch.Draw(framesAnimation, headSourceRec, animationSourceRectangle, color);
          spriteBatch.Draw(hat, handRightRectangle, color);
// Head
         //   spriteBatch.Draw(hat, new Rectangle(headSourceRec.X, headSourceRec.Y,(int)( hat.Width * (STANDARD_Z_DEPTH / headPosition.Z)), (int)(hat.Height * (STANDARD_Z_DEPTH / headPosition.Z))), new Rectangle(0, 0, hat.Width, hat.Height), color);
    
          
         
            spriteBatch.End();
            
            //Texture2D colorVideo2 = new Texture2D(graphics.GraphicsDevice, 640, 480);
           

        //    thisModel.Draw(camera, Matrix.CreateWorld(new Vector3(rootDepthPoint.X/4-100, rootDepthPoint.Y/-10-66, -1*(rootDepthPoint.Depth/20)+50), new Vector3(90, 0, 0), Vector3.Up), colorVideo, graphics);

        //thisModel.Draw(camera, Matrix.CreateWorld(new Vector3(0,-100, -70), new Vector3(90,0,0), Vector3.Up),texture,graphics);
     

            base.Draw(gameTime);
        }
    }
}
