using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class Enemy : MonoBehaviour
    {
        private const int WeaponSoundId = 10002;
        private const int ExplosionSoundId = 10003;

        private enum EnemyFireMode
        {
            None,
            Down,
            AimAtPlayer
        }

        [SerializeField] private float speed;
        [SerializeField] private float fireInterval = 1f;

        private Vector3 _cachePos = Vector3.zero;
        private bool _isRecycled;
        private EnemyFireMode _fireMode = EnemyFireMode.None;
        private float _fireTimer;

        public void SetStartPos(Vector3 startPos)
        {
            SetStartPos(startPos, gameObject.name);
        }

        public void SetStartPos(Vector3 startPos, string enemyName)
        {
            _isRecycled = false;
            _fireTimer = 0f;
            _cachePos = startPos;
            _fireMode = GetFireMode(enemyName);
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
                Recycle();
                return;
            }

            UpdateFire();
        }

        private void UpdateFire()
        {
            if (_fireMode == EnemyFireMode.None || fireInterval <= 0f)
            {
                return;
            }

            _fireTimer += Time.deltaTime;
            while (_fireTimer >= fireInterval && !_isRecycled)
            {
                _fireTimer -= fireInterval;
                Fire();
            }
        }

        private void Fire()
        {
            GameObject go = GameManager.Instance.GetEnemyBullet();
            if (go == null)
            {
                return;
            }

            var bullet = go.GetComponent<Bullet>();
            if (bullet == null)
            {
                GameManager.Instance.HideGo(go);
                return;
            }

            var firePos = transform.position;
            Vector3 direction;
            if (_fireMode == EnemyFireMode.Down)
            {
                direction = Vector3.down;
            }
            else
            {
                if (!GameManager.Instance.TryGetPlayerPosition(out var playerPos))
                {
                    GameManager.Instance.HideGo(go);
                    return;
                }

                direction = playerPos - firePos;
                if (direction.sqrMagnitude <= Vector3.kEpsilon)
                {
                    GameManager.Instance.HideGo(go);
                    return;
                }
            }

            go.transform.position = firePos;
            bullet.SetDirect(firePos, direction, Constant.Layer.PlayerLayerName);
            GameEntry.Audio.PlaySound(WeaponSoundId, transform);
        }

        private static EnemyFireMode GetFireMode(string enemyName)
        {
            switch (enemyName)
            {
                case "Enemy_2":
                    return EnemyFireMode.Down;
                case "Enemy_Boss":
                    return EnemyFireMode.AimAtPlayer;
                default:
                    return EnemyFireMode.None;
            }
        }

        private void Recycle()
        {
            _isRecycled = true;
            GameManager.Instance.HideGo(gameObject);
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
                GameEntry.Audio.PlaySound(ExplosionSoundId, transform);
                Recycle();
                GameManager.Instance.AddScore();
            }
        }
    }
}
