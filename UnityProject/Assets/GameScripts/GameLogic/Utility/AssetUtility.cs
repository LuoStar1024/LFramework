using LFramework;

namespace GameLogic
{
    public static partial class AssetUtility
    {
        public static string GetActorEffectAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Actor/Effect/{0}", assetName);
        }
        
        public static string GetActorMapAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Actor/Map/{0}", assetName);
        }
        
        public static string GetActorRoleAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Actor/Role/{0}", assetName);
        }
        
        public static string GetAudioBgmAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Audio/Bgm/{0}", assetName);
        }

        public static string GetAudioSoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Audio/Sound/{0}", assetName);
        }
        
        public static string GetAudioUISoundAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Audio/UISound/{0}", assetName);
        }
        
        public static string GetDataTableAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/DataTable/{0}", assetName);
        }
        
        public static string GetDllAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Dll/{0}", assetName);
        }
        
        public static string GetFontAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Font/{0}", assetName);
        }

        public static string GetSceneAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Scene/{0}", assetName);
        }
        
        public static string GetSpriteMapAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Sprite/Map/{0}", assetName);
        }
        
        public static string GetSpriteRoleAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Sprite/Role/{0}", assetName);
        }
        
        public static string GetSpriteUIAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/Sprite/UI/{0}", assetName);
        }

        public static string GetUIFormAsset(string assetName)
        {
            return Utility.Text.Format("Assets/GameResRaw/UIForm/{0}", assetName);
        }
    }
}