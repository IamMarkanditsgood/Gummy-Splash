
namespace Mkey
{
    public class PauseWindowController : PopUpsController
    {
        private GameBoard MBoard { get { return GameBoard.Instance; } }
        public GuiController MGui { get { return GuiController.Instance; } }

        private GameConstructSet GCSet { get { return GameConstructSet.Instance; } }
        private LevelConstructSet LCSet { get { return GCSet.GetLevelConstructSet(GameLevelHolder.CurrentLevel); } }
        private GameObjectsSet GOSet { get { return GCSet.GOSet; } }

        public void Exit_Click()
        {
            MBoard.Pause();
            CloseWindow();
            MGui.ShowPopUpByDescription("quit");
        }

        public void Map_Click()
        {
            MBoard.Pause();
            CloseWindow();
            LifesHolder.Add(-1);
            foreach (var item in GOSet.BoosterObjects)
            {
                if (item.Use) item.ChangeUse();
            }
            SceneLoader.Instance.LoadScene(1);
        }

        public void Resume_Click()
        {
            MBoard.Pause();
            CloseWindow();
        }
    }
}
