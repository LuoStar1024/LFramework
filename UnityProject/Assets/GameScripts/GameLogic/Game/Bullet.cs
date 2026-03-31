using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLogic
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed;

        private bool _isUp = false;
        private Vector3 _cachePos = Vector3.zero;
        
        public void SetDirect(bool isUp, Vector3 startPos)
        {
            _isUp = isUp;
            _cachePos = startPos;
            transform.position = _cachePos;
        }

        private void Update()
        {
            if (_isUp)
            {
                _cachePos.y += speed * Time.deltaTime;
            }
            else
            {
                _cachePos.y -= speed * Time.deltaTime;
            }
            transform.position = _cachePos;
            if (MathF.Abs(_cachePos.y) > 5)
            {
                GameManager.Instance.HideGo(gameObject);
            }
        }
        
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag(Constant.Layer.EnemyLayerName))
            {
                GameManager.Instance.HideGo(gameObject);
            }
        }
    }
}
