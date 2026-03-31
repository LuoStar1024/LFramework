using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using LFramework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameLogic
{
    public class GameManager : SingletonBehaviour<GameManager>
    {
        private float _enemyInterval = 1.5f;
        private float _timer;
        
        private ResourceContainer _resourceContainer = null;
        private IObjectPool<GoPoolObject> _goObjectPool = null;
        private Dictionary<string, GameObject> _prefabDict = new Dictionary<string, GameObject>();

        private bool _isGameOver = false;
        private int _score = 0;

        private void Awake()
        {
            _resourceContainer = ResourceContainer.Create(this);
        }

        // protected override void OnDestroy()
        // {
        //     ReferencePool.Release(_resourceContainer);
        //     _resourceContainer = null;
        //     
        //     base.OnDestroy();
        // }

        protected override void OnRelease()
        {
            ReferencePool.Release(_resourceContainer);
            _resourceContainer = null;
            _goObjectPool.ReleaseAllUnused();
            GameEntry.ObjectPool.DestroyObjectPool(_goObjectPool);
        }

        private void Start()
        {
            _goObjectPool = GameEntry.ObjectPool.CreateSingleSpawnObjectPool<GoPoolObject>("GoPool");

            // var table = GameEntry.DataTable.Tables;
            // var itemDatas = table.TbItem.Get(10000);
            // Log.Error($"{itemDatas.Name}");
            var x = GameEntry.DataTable.TbSound.Get(1);
            Debug.LogError(x.GroupName.ToString());
            
            _score = 0;
            _timer = -1000;
            InitManager().Forget();
        }

        private void Update()
        {
            if (_isGameOver)
            {
                return;
            }
            
            // 产生敌机
            _timer += Time.deltaTime;
            if (_timer >= _enemyInterval)
            {
                _timer -= _enemyInterval;
                CreateEnemy();
            }
        }

        private async UniTaskVoid InitManager()
        {
            var playerPrefab = await _resourceContainer.LoadAsset<GameObject>(AssetUtility.GetActorRoleAsset("Player"));
            _prefabDict.Add("Player", playerPrefab);
            
            var bulletPrefab = await _resourceContainer.LoadAsset<GameObject>(AssetUtility.GetActorRoleAsset("PlayerBullet"));
            _prefabDict.Add("PlayerBullet", bulletPrefab);
            
            var enemy1Prefab = await _resourceContainer.LoadAsset<GameObject>(AssetUtility.GetActorRoleAsset("Enemy_1"));
            _prefabDict.Add("Enemy_1", enemy1Prefab);
            
            var enemy2Prefab = await _resourceContainer.LoadAsset<GameObject>(AssetUtility.GetActorRoleAsset("Enemy_2"));
            _prefabDict.Add("Enemy_2", enemy2Prefab);
            
            var enemyBossPrefab = await _resourceContainer.LoadAsset<GameObject>(AssetUtility.GetActorRoleAsset("Enemy_Boss"));
            _prefabDict.Add("Enemy_Boss", enemyBossPrefab);
            
            // 产生玩家
            CreatePlayer(playerPrefab);

            _timer = 0;
        }
        
        private void CreatePlayer(GameObject playerPrefab)
        {
            var go = Instantiate(playerPrefab, transform, true);
            go.name = "Player";
            go.transform.position = new Vector3(0, -3.5f, 0);
        }

        private void CreateEnemy()
        {
            string enemyName = null;
            var idx = Random.Range(0, 100);
            if (idx < 20)
            {
                enemyName = "Enemy_Boss";
            }
            else if (idx < 60)
            {
                enemyName = "Enemy_2";
            }
            else
            {
                enemyName = "Enemy_1";
            }

            GameObject go = null;
            var goObject = _goObjectPool.Spawn(enemyName);
            if (goObject == null)
            {
                if(!_prefabDict.TryGetValue(enemyName, out var goPrefab))
                {
                    // 需要加载资源
                    Log.Error($"{enemyName} Asset is null");
                    return;
                }
                go = Instantiate(goPrefab, transform, true);
                _goObjectPool.Register(GoPoolObject.Create(enemyName, go), true);
            }
            else
            {
                go = (GameObject)goObject.Target;
            }
            go.GetComponent<Enemy>().SetStartPos(new Vector3(Random.Range(-2.5f, 2.5f), 7, 0));
        }

        public GameObject GetBullet()
        {
            var goObject = _goObjectPool.Spawn("PlayerBullet");
            if (goObject == null)
            {
                if(!_prefabDict.TryGetValue("PlayerBullet", out var goPrefab))
                {
                    // 需要加载资源
                    Log.Error("PlayerBullet Asset is null");
                    return null;
                }
                var go = Instantiate(goPrefab, transform, true);
                _goObjectPool.Register(GoPoolObject.Create("PlayerBullet", go), true);
                return go;
            }

            return (GameObject)goObject.Target;
        }

        public void HideGo(GameObject go)
        {
            _goObjectPool.Unspawn(go);
        }

        public void AddScore()
        {
            _score++;
            var forms = GameEntry.UI.GetUIForms(AssetUtility.GetUIFormAsset("GameInfoForm"));
            if (forms != null && forms.Length > 0)
            {
                var logic = (GameInfoForm)forms[0].Logic;
                logic.SetScore(_score);
            }
        }
        
        public void GameOver()
        {
            _isGameOver = true;
            var childer = transform.childCount;
            for (int i = 0; i < childer; i++)
            {
                var go = transform.GetChild(i).gameObject;
                if (go.name != "Player" && go.activeSelf)
                {
                    HideGo(go);
                }
            }

            GameEntry.UI.OpenUIForm(AssetUtility.GetUIFormAsset("GameOverForm"), Constant.Setting.UIGroupNormal);
        }

        public void RestartGame()
        {
            if(!_prefabDict.TryGetValue("Player", out var goPrefab))
            {
                // 需要加载资源
                Log.Error($"Player Asset is null");
                return;
            }

            _score = -1;
            _isGameOver = false;
            CreatePlayer(goPrefab);
            _timer = 0;
            AddScore();
        }
    }
}