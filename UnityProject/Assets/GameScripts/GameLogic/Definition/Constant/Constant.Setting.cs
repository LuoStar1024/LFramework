using System.Collections.Generic;

namespace GameLogic
{
    public static partial class Constant
    {
        public static class Setting
        {
            // ChangeScene
            public const string ChangeSceneNameKey = "ChangeSceneName";
            public const string ChangeSceneFormKey = "ChangeSceneForm";

            public const string Language = "Setting.Language";

            // Audio
            public const string AudioGroupBgm = "Bgm";
            public const string AudioGroupSound = "Sound";
            public const string AudioGroupUISound = "UISound";
            public const string AudioGroupMuted = "Setting.{0}Muted";
            public const string AudioGroupVolume = "Setting.{0}Volume";
            public const string BgmMuted = "Setting.BgmMuted";
            public const string BgmVolume = "Setting.BgmVolume";
            public const string SoundMuted = "Setting.SoundMuted";
            public const string SoundVolume = "Setting.SoundVolume";
            public const string UISoundMuted = "Setting.UISoundMuted";

            public const string UISoundVolume = "Setting.UISoundVolume";

            // 组名和代理数量
            public static readonly Dictionary<string, int> AudioGroupDict = new Dictionary<string, int>()
            {
                { AudioGroupBgm, 2 },
                { AudioGroupSound, 5 },
                { AudioGroupUISound, 3 }
            };


            // UI
            // DepthFactor = 1000
            public const string UIGroupBackground = "Background";
            public const string UIGroupNormal = "Normal";
            public const string UIGroupPopTip = "PopTip";
            public const string UIGroupGuide = "Guide";
            public const string UIGroupTop = "Top";
            public const string UIGroupEffect = "Effect";
            public const string UIGroupDebug = "Debug";

            public static readonly string[] UIGroupNames = new[]
            {
                UIGroupBackground, UIGroupNormal, UIGroupPopTip, UIGroupGuide, UIGroupTop,
                UIGroupEffect, UIGroupDebug
            };
        }
    }
}