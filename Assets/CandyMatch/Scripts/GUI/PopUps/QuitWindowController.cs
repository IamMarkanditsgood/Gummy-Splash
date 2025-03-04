
namespace Mkey
{
    public class QuitWindowController : PopUpsController
    {
        public override void RefreshWindow()
        {
            base.RefreshWindow();
        }

        public void No_Click()
        {
            CloseWindow();
        }

        public void Yes_Click()
        {
            CloseWindow();
            SceneLoader.Instance.LoadScene(0);
            //  Application.Quit();
        }
    }
}