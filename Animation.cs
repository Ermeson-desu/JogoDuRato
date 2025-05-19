// Developed by Danilo Borges
// 22/03/2019
// danilo.bsto@gmail.com
// Essa é uma classe de animação referente ao meu projeto piloto Mono.Helper.
// O código está incompleto.
// Pode-se usar a vontade em outros projetos.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System;

namespace Mono.Helper
{
    public class Animation : IDisposable
    {        
        private Game game;
        private SpriteBatch spriteBatch;
        private float elapsedTime;
        private Loader load;
        private float parallax = 1f;

        public bool Disposed { get; private set; }        
        
        public bool Enabled { get; set; }        
        public bool Visible { get; set; }     
        
        public List<Sprite> Sprites { get; set; }  
        public float Time { get; set; }        
        public ushort Index { get; private set; }        
        public ushort FrameIndex { get; private set; }
        public Sprite Current { get; protected set; }         
        public Vector2 Position { get; set; }
        public float Parallax { get { return parallax; } set { if (value == 0) value = 1; parallax = value; } }        
        public Vector2 Origin { get; set; }
        public Vector2 GetOriginCorrection { get { return Current != null ? Current.OriginCorrection : Vector2.Zero; } }        
        public Rectangle Bounds { get; set; }
        public Point Size { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public Color Color { get; set; }
        public SpriteEffects Effects { get; set; }
        
        public event Action<GameTime, Animation> OnUpdate;        
        public event Action<GameTime, Animation> OnDraw;
        public event Action<GameTime, Animation> OnChangeIndex;
        
        public Animation(Game game, float time)
        {
            spriteBatch = game.Services.GetService<SpriteBatch>();
            elapsedTime = 0f;
            load = new Loader(game, true);
            this.game = game;
            Index = 0;
            FrameIndex = 0;
            Time = time;
            Position = Vector2.Zero;
            Origin = Vector2.Zero;
            Parallax = 1;
            Sprites = new List<Sprite>();
            Rotation = 0f;
            Scale = new Vector2(1, 1);
            Color = Color.White;
            Effects = SpriteEffects.None;            
            Enabled = true;
            Visible = true;
            Size = Point.Zero;
            Bounds = Rectangle.Empty;
            Current = null;            
        }

        internal Animation(Animation animation)
        {
            spriteBatch = animation.game.Services.GetService<SpriteBatch>();
            elapsedTime = 0f;
            load = animation.load;
            game = animation.game;
            Index = 0;
            FrameIndex = 0;
            Position = animation.Position;
            Origin = animation.Origin;
            Parallax = animation.Parallax;
            Sprites = animation.Sprites;            
            Rotation = animation.Rotation;
            Scale = animation.Scale;
            Color = animation.Color;
            Effects = animation.Effects;
            Enabled = animation.Enabled;
            Visible = animation.Visible;
            Current = animation.Current;
            Time = animation.Time;
            Size = animation.Size;
            Bounds = animation.Bounds;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!Enabled)
                return;

            if(Sprites.Count > 0)
            {
                elapsedTime += gameTime.ElapsedGameTime.Milliseconds;

                if(elapsedTime > Time)
                {
                    //Verifica se o index do frame atual, é maior que a quantidade de frames do sprite ativo.
                    if (FrameIndex >= Sprites[Index].TextureFrames.Count - 1)
                    {
                        //Se sim, é hora de pular de sprite ou voltar para o primeiro frame,
                        //Caso só tenhamos uma sprite

                        if (Index >= Sprites.Count - 1)
                            Index = 0;
                        else
                            Index++;

                        FrameIndex = 0;
                    }                        
                    else
                        FrameIndex++;

                    OnChangeIndex?.Invoke(gameTime, this);

                    elapsedTime = 0;
                    Current = Sprites[Index];
                }
            }

            Size = Current.TextureFrames[FrameIndex].Size;
            Bounds = Current.TextureFrames[FrameIndex];
            
            OnUpdate?.Invoke(gameTime, this);
        }
        
        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            if (Current != null)
            {
                var pos = Position * parallax;
                var source = Current.TextureFrames[FrameIndex];

                var color = Color;
                if (color == Color.White && Current.Color != Color.White)
                    color = Current.Color;

                var rotation = Rotation;
                var origin = Origin;
                var scale = Scale;
                var eft = Effects;

                spriteBatch.Draw(
                        texture: Current.Texture,
                        position: pos,
                        sourceRectangle: source,
                        color: color,
                        rotation: rotation,
                        origin: origin,
                        scale: scale,
                        effects: eft,
                        layerDepth: 0
                        );
            }

            OnDraw?.Invoke(gameTime, this);
        } 

        public void AddSprite(params Sprite[] sprites)
        {
            if (sprites != null)
                Sprites.AddRange(sprites);

            if (Current == null)
            {
                Current = Sprites[0];
                Size = Current.TextureFrames[FrameIndex].Size;
                Bounds = Current.TextureFrames[FrameIndex];
            }                
        }
        
        public void AddSprite(params string[] sources)
        {
            Loader loader = new Loader(game);
            List<Sprite> tmpSprites = new List<Sprite>();

            foreach(string s in sources)
            {
                Sprite temp = new Sprite(loader.Texture2D(s));
                tmpSprites.Add(temp);
            }

            AddSprite(tmpSprites.ToArray());
        }

        public void AddSprite(string source, params Rectangle[] frames)
        {
            Loader loader = new Loader(game);
            Sprite tempSprite = new Sprite(loader.Texture2D(source));
            tempSprite.TextureFrames.Clear();
            tempSprite.TextureFrames.AddRange(frames);            

            AddSprite(tempSprite);
        }        
        
        public void Show()
        {
            Enabled = true;
            Visible = true;            
        }        
        
        public void Hide()
        {
            Enabled = false;
            Visible = false;
        }       
       
        public Animation Clone()
        {
            Animation animation = new Animation(this);
            return animation;
        }

        #region DISPOSE

        /// <summary>Releases the resources of this instance</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Releases the resources of this instance</summary>
        /// <param name="disposing">If true, releases the disposables resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                game.Dispose();

                foreach (Sprite s in Sprites)
                    s.Dispose();

                Current.Dispose();
            }

            Disposed = true;
        }

        #endregion
    }
}