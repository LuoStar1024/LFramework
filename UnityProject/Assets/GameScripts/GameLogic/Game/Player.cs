using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class Player : MonoBehaviour
    {
        private const int WeaponSoundId = 10001;
        private const int ExplosionSoundId = 10004;

        [SerializeField] private float speed;

        [SerializeField] private int hp;

        [SerializeField] private float fireInterval;

        [SerializeField] private Transform[] transWeapons;

        [SerializeField] private GameObject prefabBullet;

        private Rect _playerMoveBoundary = default(Rect);
        private Vector3 _targetPosition = Vector3.zero;
        private Vector3 _cachePos = Vector3.zero;
        private float _timer = 0;
        private bool _isDead;

        private void Start()
        {
            Background sceneBackground = FindObjectOfType<Background>();
            if (sceneBackground == null)
            {
                Log.Warning("Can not find scene background.");
                return;
            }

            _playerMoveBoundary = new Rect(sceneBackground.PlayerMoveBoundary.bounds.min.x,
                sceneBackground.PlayerMoveBoundary.bounds.min.y,
                sceneBackground.PlayerMoveBoundary.bounds.size.x, sceneBackground.PlayerMoveBoundary.bounds.size.y);
            _cachePos = transform.localPosition;
            _targetPosition = transform.localPosition;
            _timer = fireInterval;
        }

        private void Update()
        {
            if (_isDead)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= fireInterval)
            {
                _timer -= fireInterval;
                GameEntry.Audio.PlaySound(WeaponSoundId, transform);
                for (int i = 0; i < transWeapons.Length; i++)
                {
                    Fire(transWeapons[i].position);
                }
            }

            if (Input.GetMouseButton(0))
            {
                Vector3 point = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                _targetPosition = new Vector3(point.x, point.y, 0f);
            }

            Vector3 direction = _targetPosition - _cachePos;
            if (direction.sqrMagnitude <= Vector3.kEpsilon)
            {
                return;
            }

            Vector3 distance =
                Vector3.ClampMagnitude(direction.normalized * speed * Time.deltaTime, direction.magnitude);
            _cachePos.x = Mathf.Clamp(_cachePos.x + distance.x, _playerMoveBoundary.xMin, _playerMoveBoundary.xMax);
            _cachePos.y = Mathf.Clamp(_cachePos.y + distance.y, _playerMoveBoundary.yMin, _playerMoveBoundary.yMax);
            transform.localPosition = _cachePos;
        }

        // private void OnCollisionEnter2D(Collision2D other)
        // {
        //     if (other.collider.CompareTag(Constant.Layer.EnemyLayerName))
        //     {
        //         Destroy(gameObject);
        //         GameManager.Instance.GameOver();
        //     }
        // }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constant.Layer.EnemyLayerName))
            {
                Hit();
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.CompareTag(Constant.Layer.EnemyLayerName))
            {
                Hit();
            }
        }

        public void Hit()
        {
            if (_isDead)
            {
                return;
            }

            _isDead = true;
            GameEntry.Audio.PlaySound(ExplosionSoundId, transform);
            GameManager.Instance.ClearPlayer(gameObject);
            Destroy(gameObject);
            GameEntry.Timer.AddTimer(0.5f, OnGameOverDelayComplete);
        }

        private static void OnGameOverDelayComplete()
        {
            GameManager.Instance.GameOver();
        }

        private void Fire(Vector3 firePos)
        {
            var go = GameManager.Instance.GetBullet();
            if (go == null)
            {
                return;
            }

            go.transform.position = firePos;
            go.GetComponent<Bullet>().SetDirect(true, firePos);
        }
    }
}
