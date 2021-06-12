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

namespace WindowsGame2
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
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
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        /// 
        /// ajustar el modelo 3d par adibujar 
        /// 

        Model myModel;

        SoundEffect soundEngine;
        SoundEffectInstance soundEngineInstance;
        SoundEffect soundHyperspaceActivation;


        float AspectRation;


        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            myModel = Content.Load<Model>("Models\\p1_wedge.fbx");
            soundEngine = Content.Load<SoundEffect>("Audio\\Waves\\engine_2");
            soundEngineInstance = soundEngine.CreateInstance();
            soundHyperspaceActivation = Content.Load<SoundEffect>("Audio\\Waves\\hypersapace_activate");
            AspectRation = graphics.GraphicsDevice.Viewport.AspectRatio;

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
        Vector3 modelVelocity = Vector3.Zero;
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();
           //utilizar el metodo para revisar actulaizaciones que han pasado
            UpdateInput();
            //agregar velocidad a la pososicion actual
            modelPosition += modelVelocity;
            //perdidia de la velocidad conforme pasas el tiempo 
            modelVelocity *= 0.95f;
            base.Update(gameTime);
        }

        protected void UpdateInput()
        {
            //leer el estado del gamepad y el estado del teclado
            GamePadState currentState = GamePad.GetState(PlayerIndex.One);
            KeyboardState currentKeyState = Keyboard.GetState();

            if (currentKeyState.IsKeyDown(Keys.A))
                modelRotation += 0.10f;
            else if (currentKeyState.IsKeyDown(Keys.D))
                modelRotation -= 0.10f;
            else modelRotation -= currentState.ThumbSticks.Left.X * 0.10f;
            //crear alguna velocidad si se esta presionando el disporador
            Vector3 modelVelocityAdd = Vector3.Zero;
            //encuantra en que direccion debemos dirigirnos
            modelVelocityAdd.X = -(float)Math.Sin(modelRotation);
            modelVelocityAdd.Z = -(float)Math.Cos(modelRotation);

            if (currentKeyState.IsKeyDown(Keys.W))
                modelVelocityAdd *= 1;
            else
                //ahora vamos a escalar nuestra direccion dependiendo de que tan fuerte estemos presionando el disparador
                modelVelocityAdd *= currentState.Triggers.Right;

            //finalmente vamos agregar este vector a nuestra velovcidad
            modelVelocity += modelVelocityAdd;

            GamePad.SetVibration(PlayerIndex.One, currentState.Triggers.Right, currentState.Triggers.Right);
            if (currentKeyState.Triggers.Right > 0 || currentKeyState.IsKeyDown(Keys.D)
                || currentKeyState.IsKeyDown(Keys.A))
            {
                if (soundEngineInstance.State == SoundState.Stopped)
                {
                    soundEngineInstance.Volume = .75f;
                    soundEngineInstance.IsLooped = true;
                    soundEngineInstance.Play();
                }
                else
                    soundEngineInstance.Resume();
            }
            else if (currentKeyState.Triggers.Right == 0 || currentKeyState.IsKeyUp(Keys.D) || currentKeyState.IsKeyUp(Keys.A))
                    {
                if (soundEngineInstance.State == SoundState.Playing)
                    soundEngineInstance.Pause();
                    }

            
            //en caso que te pierdas presionando A crearemos un portal para el centro 
            if (currentState.Buttons.A == ButtonState.Pressed || currentKeyState.IsKeyDown(Keys.Enter))
            {
                modelPosition = Vector3.Zero;
                modelVelocity = Vector3.Zero;
                modelRotation = 0.0f;
                soundHyperspaceActivation.Play();
            }
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// 

        //ajusta la pisisicon del modelo en el espacio del mundo y ajusta la rotacion

        Vector3 modelPosition = Vector3.Zero;
        float modelRotation = 0.0f;
        Vector3 cameraPosition = new Vector3(0.0f,50.0f,5000.0f);  
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);


            // copiar todas las tranformaciones de los padres 

            Matrix[] transforms = new Matrix[myModel.Bones.Count];
            myModel.CopyAbsoluteBoneTransformsTo(transforms);

            //dibujar el modelo un modelo puede contener multiples mallas asi que lo ciclaremos
            foreach (ModelMesh mesh in myModel.Meshes) 
            {
                //aqui es donde se ajusta la orientacion de la malla asi como la de la camara y proyeccion
                foreach (BasicEffect effect in mesh.Effects) 
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] *
                        Matrix.CreateRotationY (modelRotation)
                        * Matrix.CreateTranslation(modelPosition);
                    effect.View = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
                    effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45.0f), AspectRation, 1.0f, 10000.0f);                  
                }
                //A continuacion Dibuja la malla
                mesh.Draw();
            }

                base.Draw(gameTime);
        }
    }
}
