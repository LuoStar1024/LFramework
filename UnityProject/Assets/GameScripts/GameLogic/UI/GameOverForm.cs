using UnityEngine;

namespace GameLogic
{
    public class GameOverForm : UguiForm
    {
        public void OnRestartBtnClick()
        {
            PlayUISound(1001);
            GameManager.Instance.RestartGame();
            Close();
        }

        public void OnReturnBtnClick()
        {
            PlayUISound(1001);
            GameEntry.Event.FireGroup<EventGroupUI>().ReturnMenu();
            Close();
        }
    }
}