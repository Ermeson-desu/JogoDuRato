using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using GameDuMouse.GameMain.Fases;
using GameDuMouse.GameMain.Entities;

namespace GameDuMouse.GameMain.Core
{
    public class LevelManager
    {
        private Game game;
        private List<IFase> fases;
        private int currentIndex;

        public IFase CurrentFase => fases[currentIndex];

        public LevelManager(Game game)
        {
            this.game = game;
            fases = new List<IFase>();
            currentIndex = 0;
        }

        public void AddFase(IFase fase)
        {
            fases.Add(fase);
        }

        public void LoadContent(Microsoft.Xna.Framework.Content.ContentManager content)
        {
            foreach (var fase in fases)
                fase.LoadContent(content);
        }

        public void Update(Player player)
        {
            CurrentFase.Update(player);

            // Se a fase atual terminou, avança para a próxima
            if (CurrentFase.HasWon && currentIndex < fases.Count - 1)
            {
                currentIndex++;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            CurrentFase.Draw(spriteBatch, player);
        }
    }
}