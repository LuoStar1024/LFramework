using LFramework;
using UnityEngine;
using ProcedureOwner = LFramework.IFsm<LFramework.IProcedureManager>;

namespace Launcher
{
    /// <summary>
    /// 启动游戏入口预制体，正式把控制权从启动器流程交给热更游戏逻辑。
    /// </summary>
    public class ProcedureStartGame : ProcedureBase
    {
        public override bool UseNativeDialog { get; }

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            // 当前项目通过实例化 GameEntry 预制体触发 Awake/Start，进入热更侧流程。
            LoadGameEntry();
        }

        private async void LoadGameEntry()
        {
            var resComponent = LFrameworkEntry.GetModule<IResourceManager>();
            var gameEntryPrefab = await resComponent.LoadAsset<GameObject>("Assets/GameResRaw/GameEntry/GameEntry", 10);
            var go = Object.Instantiate(gameEntryPrefab, RootComponent.Instance.transform, true);
            go.transform.position = Vector3.zero;
            go.transform.rotation = new Quaternion(0, 0, 0, 0);

            // 游戏入口接管后，启动器界面即可整体隐藏。
            LauncherMgr.HideAll();
        }
    }
}