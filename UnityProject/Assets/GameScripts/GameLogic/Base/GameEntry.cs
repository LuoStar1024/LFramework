using System;
using System.Collections;
using LFramework;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 游戏入口。依靠AOT那边通过创建预制体，然后Unity自动调用生命周期函数。
    /// </summary>
    public partial class GameEntry : MonoBehaviour
    {
        private void Awake()
        {
            Log.Info("=======Hello, HybridCLR 看到此条日志代表你成功运行了示例项目的热更新代码=======");
            Log.Info("<color=green> GameEntry.Awake </color>");
        }

        private IEnumerator Start()
        {
            // 重置流程组件，初始化热更新流程。
            var fsm = LFrameworkEntry.GetModule<IFsmManager>();
            fsm.DestroyFsm<IProcedureManager>();
            var procedureManager = LFrameworkEntry.GetModule<IProcedureManager>();
            ProcedureBase[] procedures =
            {
                new ProcedureChangeScene(),
                new ProcedureGame(),
                new ProcedureMenu(),
                new ProcedureLogin(),
                new ProcedureGameLogicLaunch(),
            };
            procedureManager.Initialize(LFrameworkEntry.GetModule<IFsmManager>(), procedures);

            yield return new WaitForEndOfFrame();

            InitComponents();

            EventHelper.OnInit();

            procedureManager.StartProcedure<ProcedureGameLogicLaunch>();
        }

        private void OnDestroy()
        {
            EventHelper.OnDestroy();
        }

        private static void InitComponents()
        {
            InitBuiltinComponents();
            InitCustomComponents();
        }
    }
}