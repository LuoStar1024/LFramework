using System;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed;

        private Vector3 _cachePos = Vector3.zero;
        private bool _isRecycled;

        public void SetStartPos(Vector3 startPos)
        {
            _isRecycled = false;
            _cachePos = startPos;
            transform.position = _cachePos;
        }

        private void Update()
        {
            if (_isRecycled)
            {
                return;
            }

            _cachePos.y -= speed * Time.deltaTime;
            transform.position = _cachePos;
            if (_cachePos.y < -7)
            {
                _isRecycled = true;
                GameManager.Instance.HideGo(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isRecycled)
            {
                return;
            }

            Log.Info("OnTriggerEnter2D Enemy");
            if (other.CompareTag(Constant.Layer.PlayerLayerName))
            {
                Log.Info("Enemy Constant.Layer.PlayerLayerName");
                _isRecycled = true;
                GameManager.Instance.HideGo(gameObject);
                GameManager.Instance.AddScore();
            }
        }
    }
}
