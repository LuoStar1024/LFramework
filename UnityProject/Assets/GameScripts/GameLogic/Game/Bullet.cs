using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed;
        [SerializeField] private Vector2 visualForwardDirection = Vector2.up;

        private Vector3 _cachePos = Vector3.zero;
        private Vector3 _direction = Vector3.up;
        private string _targetTag = Constant.Layer.EnemyLayerName;
        private bool _isRecycled;

        public void SetDirect(bool isUp, Vector3 startPos)
        {
            SetDirect(startPos, isUp ? Vector3.up : Vector3.down, Constant.Layer.EnemyLayerName);
        }

        public void SetDirect(Vector3 startPos, Vector3 direction, string targetTag)
        {
            if (direction.sqrMagnitude <= Vector3.kEpsilon)
            {
                Log.Warning("Bullet direction is zero.");
                _isRecycled = true;
                return;
            }

            _isRecycled = false;
            _cachePos = startPos;
            _direction = direction.normalized;
            _targetTag = targetTag;
            transform.position = _cachePos;
            ApplyVisualDirection(_direction);
        }

        private void Update()
        {
            if (_isRecycled)
            {
                return;
            }

            _cachePos += _direction * speed * Time.deltaTime;
            transform.position = _cachePos;
            if (Mathf.Abs(_cachePos.x) > 5 || Mathf.Abs(_cachePos.y) > 6)
            {
                Recycle();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_isRecycled)
            {
                return;
            }

            TryHit(other.collider);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_isRecycled)
            {
                return;
            }

            TryHit(other);
        }

        private void TryHit(Collider2D other)
        {
            Log.Info("OnTriggerEnter2D Bullet");
            var otherBullet = other.GetComponent<Bullet>();
            if (otherBullet != null)
            {
                if (!other.CompareTag(gameObject.tag))
                {
                    otherBullet.Recycle();
                    Recycle();
                }

                return;
            }

            if (!other.CompareTag(_targetTag))
            {
                return;
            }

            Log.Info($"Bullet hit {_targetTag}");
            if (_targetTag == Constant.Layer.PlayerLayerName)
            {
                var player = other.GetComponent<Player>();
                if (player != null)
                {
                    player.Hit();
                }
            }

            Recycle();
        }

        private void ApplyVisualDirection(Vector3 direction)
        {
            Vector2 from = visualForwardDirection.sqrMagnitude > Vector3.kEpsilon
                ? visualForwardDirection.normalized
                : Vector2.up;
            Vector2 to = direction.sqrMagnitude > Vector3.kEpsilon
                ? new Vector2(direction.x, direction.y).normalized
                : from;
            float angle = Vector2.SignedAngle(from, to);
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void Recycle()
        {
            if (_isRecycled)
            {
                return;
            }

            _isRecycled = true;
            GameManager.Instance.HideGo(gameObject);
        }
    }
}
