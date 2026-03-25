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
        }

        public void Draw(SpriteBatch spriteBatch, Player player)
        {
            CurrentFase.Draw(spriteBatch, player);
        }

        // Restart the current phase using the factory
        public void ReplayCurrentFase()
        {
            var replay = PhaseFactory.CreateFase(game, currentIndex);
            if (replay == null)
                return;

            fases[currentIndex] = replay;
            fases[currentIndex].LoadContent(game.Content);
        }

        // Advance to the next phase using the factory
        public bool TryAdvanceToNextFase()
        {
            int nextIndex = currentIndex + 1;
            var next = PhaseFactory.CreateFase(game, nextIndex);
            if (next == null)
                return false;

            if (nextIndex < fases.Count)
                fases[nextIndex] = next;
            else
                fases.Add(next);

            currentIndex = nextIndex;
            next.LoadContent(game.Content);
            return true;
        }

        public void SwitchToNextFase(IFase next)
        {
            if (next == null)
                return;

            int nextIndex = currentIndex + 1;
            if (nextIndex < fases.Count)
                fases[nextIndex] = next;
            else
                fases.Add(next);

            currentIndex = nextIndex;
            next.LoadContent(game.Content);
        }
    }
}
