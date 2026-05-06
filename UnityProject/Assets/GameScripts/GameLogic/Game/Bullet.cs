using System;
using System.Collections;
using System.Collections.Generic;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed;

        private bool _isUp = false;
        private Vector3 _cachePos = Vector3.zero;
        private bool _isRecycled;

        public void SetDirect(bool isUp, Vector3 startPos)
        {
            _isRecycled = false;
            _isUp = isUp;
            _cachePos = startPos;
            transform.position = _cachePos;
        }

        private void Update()
        {
            if (_isRecycled)
            {
                return;
            }

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
                _isRecycled = true;
                GameManager.Instance.HideGo(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_isRecycled)
            {
                return;
            }

            Log.Info("OnTriggerEnter2D Bullet");
            if (other.collider.CompareTag(Constant.Layer.EnemyLayerName))
            {
                Log.Info("Bullet Constant.Layer.EnemyLayerName");
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

            Log.Info("OnTriggerEnter2D Bullet");
            if (other.CompareTag(Constant.Layer.EnemyLayerName))
            {
                Log.Info("Bullet Constant.Layer.EnemyLayerName");
                _isRecycled = true;
                GameManager.Instance.HideGo(gameObject);
            }
        }
    }
}
