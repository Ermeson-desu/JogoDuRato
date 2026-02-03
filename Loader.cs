using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;

namespace Mono.Helper
{
    /// <summary>Provides methods to load files in Content folder.</summary>
    public class Loader
    {
        //PROPERTIES

        /// <summary>The Game instance.</summary>
        public Game Game { get; private set; }
        /// <summary>If true, the methods will load files using FromStream  method (if possible).</summary>
        public bool UseFromStream { get; set; }

        //CONSTRUCT

        /// <summary>Initialize a new instance of Loader.</summary>
        /// <param name="game">The Game instance.</param>
        /// <param name="useContentToLoadFiles">If true, the methods will load files using FromStream method (if possible).</param>
        public Loader(Game game, bool useFromStream = false)
        {
            Game = game;
            UseFromStream = useFromStream;
        }

        //METHODS

        /// <summary>Use to load Textures2D (can be load via FromStream method).</summary>
        /// <param name="assetName">The asset name.</param>
        /// <returns>A Texture2D object.</returns>
        public Texture2D Texture2D(string assetName)
        {
            if (UseFromStream)
            {
                string path = Path.Combine(Game.Content.RootDirectory, assetName);
                using (FileStream stream = new FileStream(path, FileMode.Open))
                    return Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(Game.GraphicsDevice, stream);
            }
            else
                return Game.Content.Load<Texture2D>(assetName);
        }

        public static Texture2D Texture2D(Game game, string assetName) => Texture2D(game, assetName, false);
        public static Texture2D Texture2D(Game game, string assetName, bool useFromStream)
        {
            if (useFromStream)
            {
                string path = Path.Combine(game.Content.RootDirectory, assetName);
                using (FileStream stream = new FileStream(path, FileMode.Open))
                    return Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(game.GraphicsDevice, stream);
            }
            else
                return game.Content.Load<Texture2D>(assetName);
        }

        /// <summary>Use to load SoundEffect (can be load via FromStream method).</summary>
        /// <param name="assetName">The asset name.</param>
        /// <returns>A SoundEffect object.</returns>
        public SoundEffect SoundEffect(string assetName)
        {
            if (UseFromStream)
            {
                string path = Path.Combine(Game.Content.RootDirectory, assetName);
                using (FileStream stream = new FileStream(path, FileMode.Open))
                    return Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(stream);
            }
            else
                return Game.Content.Load<SoundEffect>(assetName);
        }

        public static SoundEffect SoundEffect(Game game, string assetName) => SoundEffect(game, assetName, false);
        public static SoundEffect SoundEffect(Game game, string assetName, bool useFromStream)
        {
            if (useFromStream)
            {
                string path = Path.Combine(game.Content.RootDirectory, assetName);
                using (FileStream stream = new FileStream(path, FileMode.Open))
                    return Microsoft.Xna.Framework.Audio.SoundEffect.FromStream(stream);
            }
            else
                return game.Content.Load<SoundEffect>(assetName);
        }

        /// <summary>Use to load Song (can be load via FromStream method).</summary>
        /// <param name="assetName">The asset name.</param>
        /// <returns>A Song object.</returns>
        public Song Song(string assetName)
        {
            if (UseFromStream)
            {
                string path = Path.Combine(Game.Content.RootDirectory, assetName);
                return Microsoft.Xna.Framework.Media.Song.FromUri(path, new Uri(path, UriKind.Relative));
            }
            else
                return Game.Content.Load<Song>(assetName);
        }

        public static Song Song(Game game, string assetName) => Song(game, assetName, true);
        public static Song Song(Game game, string assetName, bool useFromStream)
        {
            if (useFromStream)
            {
                string path = Path.Combine(game.Content.RootDirectory, assetName);
                return Microsoft.Xna.Framework.Media.Song.FromUri(path, new Uri(path, UriKind.Relative));
            }
            else
                return game.Content.Load<Song>(assetName);
        }

        /// <summary>Use to load Video (load via Content.Load<Video>).</summary>
        /// <param name="assetName">The asset name.</param>
        /// <returns>A Video object.</returns>
        public Video Video(string assetName)
        {            
            return Game.Content.Load<Video>(assetName);
        }

        public static Video Video(Game game, string assetName)
        {
            return game.Content.Load<Video>(assetName);
        }

        /// <summary>Use to load SpriteFont (load via Content.Load<SpriteFont>).</summary>
        /// <param name="assetName">The asset name.</param>
        /// <returns>A SpriteFont object.</returns>
        public SpriteFont SpriteFont(string assetName)
        {            
            return Game.Content.Load<SpriteFont>(assetName);
        }

        public static SpriteFont SpriteFont(Game game, string assetName)
        {
            return game.Content.Load<SpriteFont>(assetName);
        }

        /// <summary>Use to load generic type (using Content.Load<T>).</summary>
        /// <param name="type">Object type.</param>
        /// <param name="assetName">The asset name.</param>
        /// <returns>A specified object type.</returns>
        public T Load<T>(T type, string assetName)
        {
            return Game.Content.Load<T>(assetName);            
        }

        public static T Load<T>(Game game, T type, string assetName)
        {
            return game.Content.Load<T>(assetName);
        }

        /// <summary>Use to load a object instance in Game.Services.GetServices<T>().</summary>
        /// <typeparam name="T">The instance type.</typeparam>
        /// <returns>the object in Service.</returns>
        public T Service<T>() where T : class
        {            
            return  Game.Services.GetService<T>();
        }

        public static T Service<T>(Game game) where T : class
        {
            return game.Services.GetService<T>();
        }
    }    
}