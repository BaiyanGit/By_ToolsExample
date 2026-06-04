# Recorder SDK 绋冲畾鎬ф€讳綋楠屾敹璇存槑

## 1. 褰撳墠 SDK 鏋舵瀯鎬昏

### Core

`RecorderSdk/Core` 鏄?Recorder SDK 鐨勬牳蹇冪洰褰曪紝褰撳墠鍖呭惈褰曞埗鍏ュ彛銆佸懡浠ゆ瀯寤恒€佺姸鎬佹満銆侀敊璇爜銆佷簨浠躲€丼essionHistory 鍜岄厤缃牎楠岋紱SmokeTest 浣嶄簬 `RecorderSdk/Demo/Scripts`銆?
涓昏鍏ュ彛锛?
- `CrossPlatformScreenRecorder`锛歎nity 灞傚綍鍒跺叆鍙ｏ紝璐熻矗寮€濮嬪綍鍒躲€佸仠姝㈠綍鍒躲€佺姸鎬佹祦杞€佷簨浠跺彂甯冨拰 SessionHistory銆?- `RecordConfigProvider`锛氬綍鍒跺墠璇诲彇褰撳墠骞冲彴姝ｅ湪浣跨敤鐨勯厤缃€?- `RecorderConfigValidator`锛氬綍鍒跺墠鏍￠獙閰嶇疆銆?- `RecorderPathService`锛氳緭鍑鸿矾寰勪笌榛樿鐩綍澶勭悊銆?
### ConfigRegistry

`RecorderConfigRegistry` 璐熻矗閰嶇疆绠＄悊锛屼笉鍚姩褰曞埗銆佷笉鎷?FFmpeg銆佷笉淇敼 `RecorderState`銆?
鑱岃矗锛?
- 鎵弿閰嶇疆鐩綍銆?- 鎸?`configId` 寤虹珛绱㈠紩銆?- 璇诲彇鍜岃缃綋鍓嶉厤缃€?- 鍒涘缓銆佸厠闅嗐€佸垹闄ょ敤鎴烽厤缃€?- 閲嶅懡鍚?`displayName`銆?- 淇濆瓨閰嶇疆骞跺崌绾у埌 `schemaVersion = 2`銆?- 鏃ч厤缃縼绉诲拰 AutoFix warnings銆?- 褰撳墠閰嶇疆涓㈠け鏃跺洖閫€骞冲彴 Medium 妯℃澘銆?
### CommandBuilder

`FFmpegCommandBuilder` 鍙礋璐ｆ瀯寤?FFmpeg 鍛戒护锛屼笉璐熻矗杩涚▼鎵ц鍜岀姸鎬佷慨鏀广€?
鑱岃矗锛?
- Windows 鏈湴褰曞埗鍛戒护銆?- Windows 鎺ㄦ祦鍛戒护銆?- Linux 鏈湴褰曞埗鍛戒护銆?- Linux 鎺ㄦ祦鍛戒护銆?- 闊宠棰戝悎骞跺懡浠ゃ€?
杩斿洖 `FFmpegCommand`锛屽寘鍚細

- `executablePath`
- `arguments`
- `rawCommandLine`

### StateMachine

`RecorderState` 鎻忚堪涓绘祦绋嬬姸鎬侊細

- `Uninitialized`
- `Ready`
- `Starting`
- `Recording`
- `Stopping`
- `Merging`
- `Completed`
- `Error`

`RecorderStateTransition` 璐熻矗鍚堟硶娴佽浆鍒ゆ柇锛岄潪娉曟祦杞繑鍥?`RecorderErrorCode.StateTransitionInvalid` 骞惰Е鍙?warning銆?
### Event System

鏂颁簨浠剁粺涓€浣跨敤 `RecorderEventArgs`锛屽寘鍚細

- `sessionId`
- `state`
- `outputPath`
- `message`
- `eventType`
- `errorCode`
- `errorCodeText`
- `exception`

鏃т簨浠?`OnRecordStarted`銆乣OnRecordStopped`銆乣OnRecordingStateChanged` 淇濈暀鍏煎锛屼絾鐢辨柊娴佺▼缁熶竴椹卞姩銆?
### SessionHistory

`CrossPlatformScreenRecorder` 缁存姢 SessionHistory锛岀敤浜庤褰曞紑濮嬨€佸仠姝€佸悎骞躲€佹帹娴併€侀敊璇拰璀﹀憡绛変細璇濆巻鍙层€?
鍏紑鎺ュ彛锛?
- `GetSessionHistory()`
- `ClearSessionHistory()`
- `SessionHistory`
- `CurrentSession`
- `LastSession`

### SmokeTest

`RecorderSdkSmokeTest` 鏄?Unity 鍐呭彲鎸傝浇杩愯鐨勬渶灏忔祴璇曡剼鏈紝瑕嗙洊鐘舵€併€佷簨浠躲€丼essionHistory 鍜岄厤缃鐞嗚涓恒€?
## 2. 涓変釜闃舵瀹屾垚鍐呭

### Phase1锛氱姸鎬佷笌閿欒绋冲畾

- 鏂板 `RecorderState`銆?- 鏂板 `RecorderErrorCode`銆?- 鏂板 `RecorderResult`銆?- 鏂板 `RecorderStateTransition`銆?- `StartRecordingAsync()` / `StopRecordingAsync()` 鍙?await銆?- `StartRecording()` / `StopRecording()` 淇濈暀涓?Unity Button 鍏煎鍏ュ彛銆?- 鏄庣‘闈炴硶 Start / Stop / 鐘舵€佹祦杞殑閿欒鐮併€?- 鍒犻櫎 `GetPlatformDefaultFFmpegPath()` 涓敊璇殑 `Application.Quit()`銆?
### Phase2锛氫簨浠舵祦涓?SessionHistory 绋冲畾

- 鏂板缁熶竴 `RecorderEventArgs`銆?- 鏂板 `RecorderSessionEventType`銆?- 鏂板 `OnRecorderProgress`銆?- 缁熶竴 Warning / Error 浜嬩欢鏉ユ簮銆?- 閿欒浜嬩欢鍜?Warning 鍧囨惡甯?`RecorderErrorCode`銆?- 鍚庡彴 Merge 瀹屾垚鏃讹紝濡傛灉宸叉湁鏂板綍鍒讹紝涓嶈鐩栧叏灞€鐘舵€併€?- SessionHistory 璁板綍 Start / Stop / Merge / Stream / Error / Warning銆?- 鎻愪緵 `GetSessionHistory()` 鍜?`ClearSessionHistory()`銆?
### Phase3锛氶厤缃鐞嗙ǔ瀹?
- 鏂板 `RecorderConfigRegistry`銆?- 鏂板 `RecorderConfigResult`銆?- 閰嶇疆涓昏韩浠界粺涓€涓?`configId`銆?- UI 鏄剧ず鍚嶇粺涓€涓?`displayName`銆?- 鏄剧ず鍣ㄥ悕绉扮粺涓€涓?`captureDisplayName`銆?- 鏂伴厤缃繚瀛樻椂涓嶅啀鍐?`configName` / `fileName`銆?- `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 鍙繚瀛?`schemaVersion`銆乣platform`銆乣currentConfigId`銆?- 鏃?`Creat_`銆佹棫 `fileName` 鎸囧悜銆佹棫 `configName` 瀛楁淇濇寔鍏煎璇诲彇銆?- AutoFix 缁熶竴杩斿洖 warnings銆?
## 3. 褰撳墠鍏紑 API 鍒楄〃

### CrossPlatformScreenRecorder

- `Task<RecorderResult> StartRecordingAsync()`
  寮€濮嬪綍鍒讹紝璋冪敤鏂瑰彲 await锛岃繑鍥炴垚鍔熺姸鎬併€侀敊璇爜銆佽緭鍑鸿矾寰勫拰 sessionId銆?
- `Task<RecorderResult> StopRecordingAsync()`
  鍋滄褰曞埗锛岃皟鐢ㄦ柟鍙?await锛岃繑鍥炴垚鍔熺姸鎬併€侀敊璇爜銆佽緭鍑鸿矾寰勫拰 sessionId銆?
- `void StartRecording()`
  Unity Button 鍏煎鍏ュ彛锛屽唴閮ㄨ皟鐢?`StartRecordingAsync()`銆?
- `void StopRecording()`
  Unity Button 鍏煎鍏ュ彛锛屽唴閮ㄨ皟鐢?`StopRecordingAsync()`銆?
- `IReadOnlyList<RecorderSessionInfo> GetSessionHistory()`
  鑾峰彇鍙 SessionHistory銆?
- `void ClearSessionHistory()`
  娓呯┖ SessionHistory銆?
- `RecorderState State`
  褰撳墠褰曞埗鐘舵€併€?
- `RecorderSessionInfo CurrentSession`
  褰撳墠褰曞埗鎴栧悗鍙板鐞嗕腑浼氳瘽淇℃伅銆?
- `RecorderSessionInfo LastSession`
  鏈€杩戜竴娆′細璇濅俊鎭€?
### RecorderConfigRegistry

- `RecorderConfigResult Scan()`
  鎵弿閰嶇疆鐩綍骞跺缓绔嬬储寮曘€?
- `RecorderConfigResult GetCurrentConfig()`
  鑾峰彇褰撳墠骞冲彴姝ｅ湪浣跨敤鐨勯厤缃€?
- `RecorderConfigResult SetCurrentConfig(string configId)`
  鍒囨崲褰撳墠閰嶇疆銆?
- `RecorderConfigResult CreateUserConfig(string displayName)`
  鍒涘缓鐢ㄦ埛閰嶇疆銆?
- `RecorderConfigResult CloneConfig(string sourceConfigId, string displayName)`
  鍏嬮殕妯℃澘鎴栫敤鎴烽厤缃负鏂扮殑鐢ㄦ埛閰嶇疆銆?
- `RecorderConfigResult SaveConfig(RecorderParamsConfig config)`
  淇濆瓨閰嶇疆锛岃嚜鍔ㄥ崌绾у埌鏂扮増缁撴瀯銆?
- `RecorderConfigResult DeleteUserConfig(string configId)`
  鍒犻櫎鐢ㄦ埛閰嶇疆锛岀姝㈠垹闄ゆā鏉裤€?
- `RecorderConfigResult DeleteConfig(string configId)`
  鍒犻櫎閰嶇疆鐨勫澶栧吋瀹瑰叆鍙ｏ紝鍐呴儴浠嶇姝㈠垹闄ゆā鏉裤€?
- `RecorderConfigResult RenameDisplayName(string configId, string displayName)`
  淇敼閰嶇疆鏄剧ず鍚嶇О銆?
- `RecorderConfigResult FallbackToDefaultTemplate()`
  鍥為€€褰撳墠骞冲彴 Medium 妯℃澘銆?
## 4. 褰撳墠浜嬩欢鍒楄〃

- `OnRecorderStateChanged`
  鐘舵€佸彉鍖栦簨浠讹紝鐘舵€佸垏鎹㈠悗瑙﹀彂銆?
- `OnRecorderWarning`
  璀﹀憡浜嬩欢锛岀敤浜庨潪娉曠姸鎬佽皟鐢ㄣ€侀潪娉曠姸鎬佹祦杞€侀潪鑷村懡閰嶇疆鎴栨祦绋嬮棶棰樸€?
- `OnRecorderError`
  閿欒浜嬩欢锛岀敤浜?Start / Stop / Merge / FFmpeg / 閰嶇疆鏍￠獙绛夊け璐ュ満鏅€?
- `OnRecorderProgress`
  涓氬姟杩涘害浜嬩欢锛岀敤浜?StartSucceeded銆丼topRequested銆丼topSucceeded銆丮ergeStarted銆丮ergeSucceeded銆丼treamEnded 绛夋祦绋嬭妭鐐广€?
- `OnRecordStarted`
  鏃у吋瀹逛簨浠讹紝寮€濮嬪綍鍒舵垚鍔熷悗瑙﹀彂锛屽彧浼犺緭鍑鸿矾寰勩€?
- `OnRecordStopped`
  鏃у吋瀹逛簨浠讹紝鍋滄褰曞埗鎴愬姛鍚庤Е鍙戯紝鍙紶杈撳嚭璺緞銆?
- `OnMergeStarted`
  鍚庡彴鍚堝苟寮€濮嬩簨浠躲€?
- `OnMergeCompleted`
  鍚庡彴鍚堝苟瀹屾垚浜嬩欢锛屾垚鍔熷拰澶辫触鍧囬€氳繃 `RecorderEventArgs.errorCode` 鍒ゆ柇銆?
- `OnRecordingStateChanged`
  鏃у吋瀹逛簨浠讹紝浼?`bool IsRecording`銆?
- `OnConfigChanged`
  閰嶇疆鍙樺寲浜嬩欢锛屽綋鍓嶉鐣欑粰閰嶇疆鍒囨崲鎴栧悗缁厤缃埛鏂版祦绋嬨€?
## 5. 褰撳墠 JSON 閰嶇疆瑙勮寖

鏂扮増閰嶇疆鏂囦欢缁熶竴浣跨敤 `schemaVersion = 2`銆?
鏍稿績韬唤瀛楁锛?
- `schemaVersion`
  閰嶇疆缁撴瀯鐗堟湰锛屽綋鍓嶄负 2銆?
- `configId`
  绋冲畾鍞竴 ID锛岀敤浜庡唴閮ㄥ紩鐢ㄥ拰鏂囦欢鍚嶆帹瀵硷紝鍙厑璁歌嫳鏂囧皬鍐欍€佹暟瀛楀拰涓嬪垝绾裤€?
- `displayName`
  UI 鏄剧ず鍚嶇О锛屽彲浠ヤ腑鏂囷紝鍙互淇敼锛屼笉鍙備笌鏂囦欢璺緞銆?
- `captureDisplayName`
  褰曞埗鏄剧ず鍣ㄥ悕绉帮紝渚嬪 `\\.\DISPLAY1`銆?
- `currentConfigId`
  UseWin / UseLinux 褰撳墠浣跨敤閰嶇疆鎸囬拡銆?
涓嶅啀浣跨敤锛?
- `configName`
  鍙綔涓烘棫閰嶇疆鍏煎璇诲彇锛屼笉鍐欏叆鏂扮増 JSON銆?
- `fileName`
  鍙綔涓鸿繍琛屾椂璺緞缂撳瓨鎴栨棫閰嶇疆鍏煎锛屼笉鍐欏叆鏂扮増 JSON銆?
Use 鎸囬拡鏍煎紡锛?
```json
{
  "schemaVersion": 2,
  "platform": "Win",
  "currentConfigId": "template_win_medium"
}
```

閰嶇疆瀛楁椤哄簭閬靛惊锛?
韬唤淇℃伅 -> 骞冲彴妯″紡 -> 杈撳嚭 -> 鐢婚潰 -> 瑙嗛缂栫爜 -> WebM 瑙嗛 -> 闊抽 -> 鎺ㄦ祦 -> FFmpeg -> 楂樼骇鍙傛暟銆?
## 6. 褰撳墠閿欒鐮佸垪琛?
- `None`
  鏃犻敊璇€?
- `ConfigNull`
  閰嶇疆涓虹┖銆?
- `ConfigInvalid`
  閰嶇疆鏍￠獙澶辫触銆?
- `ConfigNotFound`
  鎸囧畾 `configId` 涓嶅瓨鍦ㄣ€?
- `ConfigDuplicateId`
  鎵弿鎴栦繚瀛樻椂鍙戠幇閲嶅 `configId`銆?
- `ConfigMigrationFailed`
  鏃ч厤缃縼绉诲け璐ャ€?
- `ConfigSaveFailed`
  閰嶇疆淇濆瓨澶辫触銆?
- `ConfigDeleteFailed`
  閰嶇疆鍒犻櫎澶辫触锛屾垨灏濊瘯鍒犻櫎妯℃澘閰嶇疆銆?
- `ConfigInvalidId`
  `configId` 闈炴硶銆?
- `ConfigAutoFixed`
  閰嶇疆琚?AutoFix 淇锛岄€氬父浣滀负 warning銆?
- `ConfigDirectoryMissing`
  閰嶇疆鐩綍涓嶅瓨鍦ㄤ笖鍒涘缓澶辫触锛屾垨鎵弿澶辫触銆?
- `InitializeFailed`
  鍒濆鍖栧け璐ャ€?
- `AlreadyRunning`
  褰撳墠宸插湪鍚姩銆佸綍鍒躲€佸仠姝㈡垨 FFmpeg 杩涚▼杩愯涓€?
- `NotRecording`
  鏈浜庡綍鍒剁姸鎬佸嵈璋冪敤 Stop銆?
- `StateBusy`
  姝ｅ湪鍚姩鎴栧仠姝紝鏆備笉鑳介噸澶嶈皟鐢ㄣ€?
- `StateTransitionInvalid`
  鐘舵€佹満闈炴硶娴佽浆銆?
- `FFmpegPathMissing`
  FFmpeg 璺緞涓虹┖鎴栨枃浠朵笉瀛樺湪銆?
- `FFmpegStartFailed`
  FFmpeg 杩涚▼鍚姩澶辫触銆?
- `AudioStartFailed`
  闊抽閲囬泦鍚姩澶辫触銆?
- `AudioStopFailed`
  闊抽閲囬泦鍋滄澶辫触銆?
- `DisplayNotFound`
  閰嶇疆涓殑鏄剧ず鍣ㄧ储寮曟垨鍚嶇О涓嶅彲鐢ㄣ€?
- `StreamUrlEmpty`
  鎺ㄦ祦妯″紡涓嬫帹娴佸湴鍧€涓虹┖銆?
- `StreamUrlInvalid`
  鎺ㄦ祦鍦板潃鏍煎紡闈炴硶锛屽綋鍓嶅彧鏀寔 RTMP / RTMPS銆?
- `StopFailed`
  鍋滄閲囬泦鎴栧仠姝?FFmpeg 澶辫触銆?
- `MergeFailed`
  闊宠棰戝悎骞跺け璐ャ€?
- `TempFileNotReady`
  涓存椂鏂囦欢鏈湪瓒呮椂鏃堕棿鍐呭噯澶囧ソ銆?
- `Unknown`
  鏈垎绫婚敊璇€?
## 7. 褰撳墠 SessionHistory 瀛楁璇存槑

- `sessionId`
  鏈褰曞埗浼氳瘽鍞竴 ID銆?
- `startTime`
  鍏煎瀛楁锛岃〃绀轰細璇濆紑濮嬫椂闂淬€?
- `startedAt`
  浼氳瘽寮€濮嬫椂闂淬€?
- `endedAt`
  褰撳墠鍘嗗彶浜嬩欢缁撴潫鏃堕棿鎴栬褰曟椂闂淬€?
- `durationMs`
  浠庡紑濮嬪埌褰撳墠鍘嗗彶鑺傜偣鐨勬寔缁绉掓暟銆?
- `outputPath`
  杈撳嚭鏂囦欢璺緞鎴栨帹娴佸湴鍧€鐩稿叧杈撳嚭璺緞銆?
- `videoTempPath`
  瑙嗛涓存椂鏂囦欢璺緞銆?
- `audioTempPath`
  闊抽涓存椂鏂囦欢璺緞銆?
- `isStreaming`
  鏄惁涓烘帹娴佹ā寮忋€?
- `isMerged`
  鏄惁宸茬粡瀹屾垚闊宠棰戝悎骞躲€?
- `mergeOutputPath`
  鍚堝苟瀹屾垚鍚庣殑杈撳嚭璺緞銆?
- `state`
  璁板綍璇ュ巻鍙叉椂鐨?RecorderState銆?
- `eventType`
  璁板綍璇ュ巻鍙叉椂鐨勪笟鍔′簨浠剁被鍨嬨€?
- `errorCode`
  閿欒鐮佹垨 warning 鐮併€?
- `errorCodeText`
  閿欒鐮佸瓧绗︿覆銆?
- `message`
  鍘嗗彶璇存槑鏂囨湰銆?
- `configId`
  鏈褰曞埗浣跨敤鐨勯厤缃?ID銆?
- `configName`
  鍏煎瀛楁锛屽綋鍓嶄娇鐢ㄩ厤缃樉绀哄悕绉般€?
- `displayName`
  鏈褰曞埗浣跨敤鐨勯厤缃樉绀哄悕绉般€?
## 8. 褰撳墠楠岃瘉鐘舵€?
### rg 妫€鏌ラ€氳繃椤?
- Core 涓嬪瓨鍦?`StartRecordingAsync`銆乣StopRecordingAsync`銆乣RecorderResult`銆乣RecorderState`銆乣RecorderErrorCode`銆?- Core 涓嬪瓨鍦ㄧ粺涓€浜嬩欢 `OnRecorderStateChanged`銆乣OnRecorderWarning`銆乣OnRecorderError`銆乣OnRecorderProgress`銆乣OnMergeStarted`銆乣OnMergeCompleted`銆?- Core 涓嬪瓨鍦?`GetSessionHistory()` 鍜?`ClearSessionHistory()`銆?- Core 涓嬪瓨鍦?`RecorderConfigRegistry`銆乣RecorderConfigResult`銆乣RecorderConfigMigrator`銆?- `Assets/StreamingAssets/RecorderSDK/Configs` 涓?JSON 宸叉棤 `configName` / `fileName` / 鏈満缁濆璺緞娈嬬暀銆?- `UseWinRecordConfig.json` / `UseLinuxRecordConfig.json` 浣跨敤 `currentConfigId`銆?
### git diff --check

`git diff --check` 宸查€氳繃锛屼粎鏈?Git 鎻愮ず鏈潵鍙兘灏?LF 鏇挎崲涓?CRLF锛屾病鏈夌┖鐧介敊璇€?
### dotnet build 璇存槑

`dotnet build Assembly-CSharp.csproj --no-restore` 涓嶄綔涓?Unity 椤圭洰鐨勭湡瀹炵紪璇戝叆鍙ｃ€?
褰撳墠宸ョ▼鐨?`Assembly-CSharp.csproj` 灞炰簬 Unity / Rider 璁捐鏈熷伐绋嬶紝鏇惧嚭鐜帮細

- 鐢熸垚澶辫触
- 0 涓鍛?- 0 涓敊璇?
鍥犳涓嶅啀鎶婅鍛戒护浣滀负楠屾敹闃诲椤广€?
### Unity BatchMode 璇存槑

Unity BatchMode 缂栬瘧鍛戒护宸茶褰曞埌 README銆傚綋鍓嶆満鍣ㄦ墽琛屾椂鍥?License Client IPC 瓒呮椂锛屾湭杩涘叆鑴氭湰缂栬瘧闃舵锛屽睘浜庣幆澧冮檺鍒躲€?
## 9. 褰撳墠宸茬煡椋庨櫓

- 杩樻病鍦ㄧ湡瀹?Unity Editor 鍐呭畬鎴愪竴娆″畬鏁磋剼鏈紪璇戦獙璇併€?- 杩樻病鍋?Windows 瀹炴満 FFmpeg 褰曞埗楠岃瘉銆?- 杩樻病鍋?Linux 瀹炴満楠岃瘉銆?- 杩樻病鍋氶暱鏃堕棿褰曞埗鍘嬪姏娴嬭瘯銆?- 杩樻病鍋氬娆?Start / Stop 鍘嬪姏娴嬭瘯銆?- 杩樻病鍋氭帹娴?RTMP / RTMPS 瀹炴満绋冲畾鎬ч獙璇併€?- BackendFactory 鏆傛湭鍋氥€?- Profile 瀵煎叆瀵煎嚭鏆傛湭鍋氥€?- Backend 鎻掍欢鍖栨帴鍙ｅ凡鏈夊熀纭€鏂囦欢锛屼絾褰撳墠闃舵娌℃湁鏇挎崲鐜版湁鍚庣瀹炵幇銆?
