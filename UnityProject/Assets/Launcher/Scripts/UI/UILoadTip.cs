using System;
using UnityEngine;
using UnityEngine.UI;

namespace Launcher
{
    /// <summary>
    /// UI更新加载提示。
    /// </summary>
    public class UILoadTip : UIBase
    {
        [SerializeField] private Button btnConfirm;

        [SerializeField] private Button btnUpdate;

        [SerializeField] private Button btnCancel;

        [SerializeField] private Text textDesc;

        public Action OnConfirmClick { get; set; }
        public Action OnUpdateClick { get; set; }
        public Action OnCancelClick { get; set; }

        private void Start()
        {
            btnConfirm.onClick.AddListener(OnClickConfirmButton);
            btnUpdate.onClick.AddListener(OnClickUpdateButton);
            btnCancel.onClick.AddListener(OnClickCancelButton);
        }

        public override void OnEnter(object data)
        {
            base.OnEnter(data);
            OnConfirmClick = null;
            OnUpdateClick = null;
            OnCancelClick = null;

            btnConfirm.gameObject.SetActive(false);
            btnUpdate.gameObject.SetActive(false);
            btnCancel.gameObject.SetActive(false);
            textDesc.text = data?.ToString();
        }

        public void SetAllCallback(Action onConfirm, Action onUpdate, Action onCancel)
        {
            btnConfirm.gameObject.SetActive(false);
            btnUpdate.gameObject.SetActive(false);
            btnCancel.gameObject.SetActive(false);

            OnConfirmClick = onConfirm;
            OnUpdateClick = onUpdate;
            OnCancelClick = onCancel;

            if (onConfirm != null)
            {
                btnConfirm.gameObject.SetActive(true);
            }

            if (onUpdate != null)
            {
                btnUpdate.gameObject.SetActive(true);
            }

            if (onCancel != null)
            {
                btnCancel.gameObject.SetActive(true);
            }
        }

        private void OnClickConfirmButton()
        {
            OnConfirmClick?.Invoke();
            Close();
        }

        private void OnClickUpdateButton()
        {
            OnUpdateClick?.Invoke();
            Close();
        }

        private void OnClickCancelButton()
        {
            OnCancelClick?.Invoke();
            Close();
        }
    }
}