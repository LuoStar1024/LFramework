using System;
using UnityEngine;

namespace GameLogic
{
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private float speed;

        private Vector3 _cachePos = Vector3.zero;
        
        public void SetStartPos(Vector3 startPos)
        {
            _cachePos = startPos;
            transform.position = _cachePos;
        }

        private void Update()
        {
            _cachePos.y -= speed * Time.deltaTime;
            transform.position = _cachePos;
            if (_cachePos.y < -7)
            {
                GameManager.Instance.HideGo(gameObject);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constant.Layer.PlayerLayerName))
            {
                GameManager.Instance.HideGo(gameObject);
                GameManager.Instance.AddScore();
            }
        }
    }
}