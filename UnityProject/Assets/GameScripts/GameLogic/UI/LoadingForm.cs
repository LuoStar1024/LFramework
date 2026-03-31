using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    public class LoadingForm : UguiForm
    {
        [SerializeField] 
        private Slider progress;
        
        public void SetProgress(float value)
        {
            // if (value > 0.9f)
            // {
            //     value = 0.9f;
            // }

            progress.value = value;
        }
        
        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            progress.value = 0;
        }
    }
}