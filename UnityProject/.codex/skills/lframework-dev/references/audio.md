# Audio

Preferred GameLogic usage:

- Prefer `GameEntry.Audio` extension methods in GameLogic/business code instead of directly building audio asset paths and calling `IAudioManager.PlayAudio`.
- `GameEntry.Audio.PlayBgm(musicId, userData = null)`: stop current BGM, read `TbSound`, build BGM asset path, play looped 2D music, and remember the BGM serial id.
- `GameEntry.Audio.StopBgm()`: stop remembered BGM with fade-out and clear the stored serial id.
- `GameEntry.Audio.PlaySound(audioId, bindingTrans = null, userData = null)`: read `TbSound`, build sound asset path, and optionally bind playback to a transform.
- `GameEntry.Audio.PlayUISound(uiSoundId, userData = null)`: read `TbSound`, build UI sound asset path, and play a non-looping 2D UI sound.
- `GameEntry.Audio.IsMuted(audioGroupName)`, `Mute(audioGroupName, mute)`: read or update an audio group's mute flag; `Mute` persists to `GameEntry.Setting`.
- `GameEntry.Audio.GetVolume(audioGroupName)`, `SetVolume(audioGroupName, volume)`: read or update an audio group's volume; `SetVolume` persists to `GameEntry.Setting`.

Core framework API quick reference:

- Audio groups: `HasAudioGroup`, `GetAudioGroup`, `GetAllAudioGroups`, `AddAudioGroup`.
- Playback: `PlayAudio(audioAssetName, audioGroupName, ...)` returns a serial id. Use the overload that matches priority, `PlayAudioParams`, binding transform, world position, or `userData`.
- Stop/load control: `StopAudio(serialId[, fadeOutSeconds])`, `StopAllLoadedAudios([fadeOutSeconds])`, `StopAllLoadingAudios()`.
- Pause/resume: `PauseAudio(serialId[, fadeOutSeconds])`, `ResumeAudio(serialId[, fadeInSeconds])`.
- Group settings: `IAudioGroup.Mute`, `IAudioGroup.Volume`.
- Per-play settings: create `PlayAudioParams` through `PlayAudioParams.Create()`, then set fields such as `Priority`, `Loop`, `VolumeInAudioGroup`, `FadeInSeconds`, and `SpatialBlend`.

Source paths:
- `Assets/LFramework/Runtime/Component/Audio/AudioComponent.cs`
- `Assets/LFramework/Runtime/Component/Audio/AudioGroup.cs`
- `Assets/LFramework/Runtime/Component/Audio/AudioAgent.cs`
- `Assets/LFramework/Runtime/Component/Audio/PlayAudioParams.cs`
- `Assets/LFramework/Runtime/Component/Audio/IAudio*.cs`
- `Assets/GameScripts/GameLogic/Component/Audio/AudioExtension.cs`

`AudioComponent` registers `IAudioManager` and owns audio groups, agents, playback ids, and resource release for loaded audio clips. `AudioExtension` provides GameLogic helpers for DataTable-driven BGM, sound, UI sound, mute, and volume.

Responsibility:

- Create/manage audio groups.
- Load audio clips through `IResourceManager`.
- Play audio through available `AudioAgent`s or replace by priority rules.
- Stop audio and release audio assets through `IAudioRelease`.
- Store BGM id and user volume/mute settings in project extension methods.

Lifecycle:

- `Awake()` registers `IAudioManager`.
- `Start()` obtains `IResourceManager`.
- `OnUpdate()` updates audio groups/agents.
- `Shutdown()` stops playback and releases audio resources.

`AudioExtension` reads `GameEntry.DataTable.TbSound`, builds asset paths through `AssetUtility`, creates `PlayAudioParams`, and calls `IAudioManager.PlayAudio`.

Cleanup rules:

- `PlayAudioParams` implements `IReference`; release ownership is handled by AudioComponent after completion/failure.
- Stop BGM through the extension so stored BGM serial state remains correct.
- Verify audio group names against DataTable and configured groups.
