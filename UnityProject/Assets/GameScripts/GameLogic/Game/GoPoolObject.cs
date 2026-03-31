using LFramework;
using UnityEngine;

namespace GameLogic
{
    public class GoPoolObject : ObjectBase
    {
        public static GoPoolObject Create(string name, object target)
        {
            GoPoolObject goPoolObject = ReferencePool.Acquire<GoPoolObject>();
            goPoolObject.Initialize(name, target);
            return goPoolObject;
        }
        
        protected override void Release(bool isShutdown)
        {
            GameObject go = (GameObject)Target;
            if (go != null)
            {
                Object.Destroy(go);
            }
        }

        protected override void OnSpawn()
        {
            GameObject go = (GameObject)Target;
            go.SetActive(true);
        }

        protected override void OnUnspawn()
        {
            GameObject go = (GameObject)Target;
            go.SetActive(false);
        }
    }
}