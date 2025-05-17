# SCPSLAudioAPi

这是一个API库，该API进行了依赖嵌套，不需要添加其他依赖。

该API库向SCP:SL服务器添加了<b>玩家式播放音乐组件</b>与<b>音乐假人生成</b>，同时添加了更多事件以便开发者使用。

## 示例:

![示例图片](https://github.com/MengXinSheQu/AudioApi/blob/main/Image/Example.png)


## 安装:

从[Releases](https://github.com/MengXinSheQu/AudioApi/releases/latest)下载最新版本`AudioAPi.dll`，这个依赖不限制版本，你可以通过`EXILED`框架或`LabApi`框架使用(NWAPI除外)

## 使用:

注意: 这个并不是一个音乐播放插件，他更偏向于处理音频/假人添加。

这个API能让你的插件更快捷的播放音频，但是该播放受SCPSL的VoiceMessage限制(仅能播放48000采样率 单轨道ogg格式音频)。

## 处理:

你可以复制你想要的[音乐](https://www.bilibili.com/video/BV1ZLrhYtE69k)，到[哔哩哔哩视频下载](https://snapany.com/zh/bilibili)下载视频，通过AdobeAU或其他软件转换为48000采样率的单轨道ogg格式音频。

## 开发小贴士:

你可以继承VoicePlayerBase，来制作其他的组件。

同时添加了以下四种事件，你可以通过事件更好的制作你的插件。

```
public static event Action<TrackSelectingEventArgs> OnTrackSelecting; //选择音频
public static event Action<TrackSelectedEventArgs> OnTrackSelected; //音频选择完成
public static event Action<TrackLoadedEventArgs> OnTrackLoaded; //音频加载
public static event Action<TrackFinishedEventArgs> OnFinishedTrack; //音频播放完成
```

