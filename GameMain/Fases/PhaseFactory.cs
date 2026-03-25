using Microsoft.Xna.Framework;

namespace GameDuMouse.GameMain.Fases
{
    public static class PhaseFactory
    {
        public static IFase CreateFase(Game game, int index)
        {
            return index switch
            {
                0 => new Fase01(game),
                // 1 => new Fase02(game), // adicione conforme novas fases
                // 2 => new Fase03(game),
                _ => null
            };
        }

        public static bool HasFase(int index)
        {
            return index switch
            {
                0 => true,
                // 1 => true,
                // 2 => true,
                _ => false
            };
        }

        public static IFase CreateDynamicFase(Game game, string mapName)
        {
            var data = GameDuMouse.GameMain.Core.MapDataManager.LoadByName(mapName);
            if (data == null)
                return null;

            return new DynamicFase(game, data);
        }
    }
}
