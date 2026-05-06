using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class GameInfoForm : UguiForm
    {
        [SerializeField] private Text txtScore;

        [SerializeField] private Image[] imgBombs;

        public void SetScore(int score)
        {
            txtScore.text = score.ToString();
        }

        public void SetBombNum(int num)
        {
            for (int i = 0, len = imgBombs.Length; i < len; i++)
            {
                imgBombs[i].gameObject.SetActive(i < num);
            }
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            txtScore.text = "0";
            for (int i = 0, len = imgBombs.Length; i < len; i++)
            {
                imgBombs[i].gameObject.SetActive(false);
            }
        }
    }
}