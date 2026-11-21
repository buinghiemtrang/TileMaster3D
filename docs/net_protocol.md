# Tile Master 3D - network protocol (client side view)

Source: `Assets/Scripts/Assembly-CSharp/**` (decompiled rebuild). Written for the in-process fake server
(`FakeServer/*.cs`, hooked via `BestHTTP.HTTPManager.Interceptor`). Hosts:
- `https://service.fuqiangame.com` (API)  - `https://match3d.oss-accelerate.aliyuncs.com` (static files)

Conventions: "[ok]" = what the client needs to take the success path. Newtonsoft matches JSON keys
**case-insensitively**, so `Data` (OwnHttp classes) and `data` (global/_FrameWork classes) both read wire key `data`.
Unknown extra keys are ignored.

## 0. Cross-cutting

### 0.1 HTTP layers (all on BestHTTP `HTTPRequest`)
| Layer | Classes | Success test | Notes |
|---|---|---|---|
| A. `OwnHttp/*` | `RequestsMgr.I.Create`, `CheckRepeatRequestsMgr.I.Creat` -> `OwnSingleHttp`; parse via `OwnHttp.ParsingServerData.SendInfo<T>/SendListInfo<T>/SendInfo` | `req.State==Finished && resp.IsSuccess` -> `RequestState.Success` | Adds header `client-version: <Application.version>`. `Fail` => parse returns wrapper `code=-1`; `Aborted` => parse returns **null**. Per-request `CustomTimeout` (default 100s, ticked 1/s) => `Fail`. |
| B. `_FrameWork/Http/*` | `RequestBase<RepeatRequestsManager/OnlyRequestsManager>` -> `SingleHttp`; `_FrameWork.Http.ParsingServerData.ParsingData<T>` | same | Same `client-version` header. Non-Success or null body => wrapper `code=-1`. Repeat manager re-sends every `interval` s until Success (`UserLabelGetCountry`). |
| C. global `OwnHttpMgr.I.SendGet/SendPost/SendDelete` + `HttpMgrBase.SendInfo/SendListInfo` / `RequestMgr` | callback gets `HTTPResponse` or **null** | callback gets `resp` only when `Finished && IsSuccess`; otherwise null (Aborted: callback not fired). `HttpMgrBase.SendInfo` null => wrapper `code=-1`. | Adds `client-version`. Default Timeout 60s/Connect 20s (team mgrs set 5s/2s). Dedupe by `key`. |
| D. direct `new HTTPRequest` | `Authentication/HttpMgr` (login), `DataCenter/HttpManager` + `DataManager` (/data), `HTTPMgr.Request` (OSS), `RankingControlBase`, `RankingUtils`, `FacebookLoginCheck` | `Finished && IsSuccess` (HttpManager: non-success => callback(req,null)) | Header names vary (`client_version` with underscore for login, /data, FB check, GetMonthIssue). |

`HTTPResponse.IsSuccess` (shim `Scripts/BestHTTP/BestHTTP/HTTPResponse.cs`): status 200..299 or 304.
=> **Any non-2xx is a failure everywhere; business errors must be HTTP 200 with a `code` in the body.**
Retries: only UserLabelGetCountry (repeat 10s), DataManager dispatch queue (doubling backoff), ServerDateTimeMgr (10s until ok), RedDot (10s poll).

Common wrapper (all three ServerDataRoot copies have the same shape):
```
{ "code": int, "msg": string, "data": T | [T] }   // ServerDataRootBase{code,msg}; ServerDataRoot<T>{data}; ServerDataListRoot<T>{data:List<T>}
```
Success is `code == 1` everywhere **except** `appGetNotice` (`code == 0`, sec 1.7) and `connect/token` (code unused).
`GetMonthIssue` additionally needs `msg == ""` (empty string, not null).

### 0.2 Headers
- `Authorization: Bearer <token>` - team/*, ranking/get, common/*, data, getFBDeletion, DELETE data (token = `DataManager.I.Token` = `DB.Account.Token`).
- **Signed family** (mail/*, rank/api/*, utils validate*): `RequestsMgr.GetCommonHeaders(isEnterToken)` =
  `Content-Type: application/json, clientId: TileMaster3D, timestamp: <unix sec from ServerDateTimeMgr.NetTimeUtc>, requestId: <guid>, solt: <userId or "TileMaster3D">`;
  mail adds `Authorization: <raw token, NO "Bearer ">`; then `sign` header = hex SHA256 of
  `"ae0b9ae818784f22aaf5a16f69ddd501|" + solt + "|" + join("|", values of all non-ignored headers + body params, ordered by key)`
  (ignored: Content-Type, solt, client_secret, Authorization). Body = JSON of the params. **A fake may ignore `sign`/`timestamp`.**
- `CommonServiceToken: 9b5ba0c0419840ccab872275f41e5cc7` on `common/time`, `common/Get*Activity`, `common/GetMonthIssue`, `team/GetGeneralActivity`.
- notice: `Authorization: <static HS256 JWT, no Bearer>` (name "Tile Master 3D", id 42).
- Every layer A/B/C request also carries `client-version`.

### 0.3 Login / token flow
1. `LoadingScene` (`SceneLoading.IELoading`): `DataCenterLoginMgr.Init()` -> `DataManager.LoadLocalData()`; if `DB.Account.Token` non-empty (persisted) -> `DataManager.Logined(token,userid)` (bLogined=true, **no network**).
   If `DB.User.Maxlv != 0` (has progress) the `/data` GET is skipped entirely.
2. If not logged in: `OwnServerLogin.Login(PlatformType.Guest, ...)` -> `POST /connect/token` (sec 1.4). Loading waits max ~5s then aborts guest sign-in and continues offline (scene still loads; guest login can be re-triggered later by `GuestLoginManager.Login` from team / region-rank screens).
3. `LoginFinish(code=1, ServerInfo)` -> `pAuth = info.data` -> `SignIn("guest")`: `DataManager.Logined(pAuth.token, pAuth.user.UserId)` then `GET /data`.
   `/data` code!=1 or HTTP failure => `Logout()+SignOut(false)` + `Guest_Login Fail` (login failed, token cleared).
4. `LoginFinishListen`: data `{}` (count 0) => `_UploadDoc` (`DB.Account.RefreshAccount()` copies Token/Userid/Name/Email.. from pAuth into DB.Account, then uploads every Remote field via `POST /data`); non-empty => merge/replace local from remote (guest: `func_merge_data`, max of levels etc.; non-guest with progress shows comparison dialog).
5. Token later: `Authorization: Bearer <token>` (team, ranking, data, common) or raw (mail). `DB.Account.Userid` (= `ServerInfo.data.user.UserId`) is the signing "solt" and the `member`/`userId` in ranking; `userId.Substring(0,8)` is applied in ranking rows => **userId must be >= 8 chars** (use 24-hex ObjectId-like).
6. The client never decodes the JWT; any non-empty string works.

### 0.4 `/data` = cloud save (one Mongo-like document per user)
`DataManager` (`SceneLoading` sets URL `https://service.fuqiangame.com/data`).
- Document = dictionary keyed by DB class name (`DB.cs` Create<T>: Account, Achieve, Bag, BakingMasterActivity, Booster, CardSystem, Chest, FacebookAdValue, FirebaseAdValue, GiftNoAds, GiftStalls, House3DTask, Login, Mail, MonthRankingActivity, Other, PropIceBreakingGift, Recall, RechargeActivity, RegionRankActivity, RewardAdView, RewardVideo, RoyalPassActivity, ShortBattlePass, SignBoard, SpecialBlockData, StarRankingActivity, StoreMultipleActivity, Synthetic, Team, Tmp, Track, TreasureHuntActivity, Turntable, UnityAdValue, User, UserLevelReward, ValueOnceTrack, ValueRepeatTrack, VictoriesActivity) -> object whose keys are the C# **field names** (e.g. `User.maxlv`, `User.sumStar`, `Account.name`; ranking queries use these paths). Only `DBPutType.Remote` fields sync.
- GET -> `{code:1,msg:"",data:{ "<DbName>": <object or JSON string>, ... }}`. Client does `data[name].ToString()` then JSON-parses, so a value may be an object or a JSON string. Missing names fine (defaults). `data` must be non-null.
- POST body (`Content-Type: application/json`, raw): **flat dotted-path updates** `{"User.maxlv": 12, "Bag.prop": {...}, "Account.name": "x"}` (key = `<DbName>.<fieldName>`, value = native JSON of the field). Server must `$set` those paths (create nested objects). Response `{code:1}` => success; else client re-queues with doubling backoff (5s,10s,..) per DB, persisted in PlayerPrefs.
- DELETE (`ClearAccount`): body `{"GuestDeviceid":"<deviceUniqueIdentifier or ''>"}`; `code==1` => account wiped, LoadingScene reloads. Other code => error dialog.
- Fake: keep one JSON doc per userId (`fake_server.json`); it also feeds `ranking/get`.

### 0.5 Region / country
`UserLabelGetCountry.RequestCountry()` (in `SceneLoading.Start`) -> `POST /utils/api/v1/getUserNation` (sec 1.2). Client stores `Nation = data.nation.ToLower()` (static + PlayerPrefs `CountryCode`); on change sets `Global.NeedReloadConfig=true`. Case irrelevant ("VN"/"vn" -> "vn").
`NativePlatform.Helper.GetCountryCode()` returns `Nation` when non-empty (else Java `getCountryZipCode`, else `RegionInfo.CurrentRegion` lower-case). Consumers: `RegionRankActivityManager` (matches `cfgRegionRankRegions.RegionCode.ToLower()`, else region "unknown"), `UserLabelGetters`, "br" privacy check, LogEvent. **Use `"nation":"VN"`.** `data.nation` must be non-null (NRE on `.ToLower()`).

### 0.6 Server time
`ServerDateTimeMgr.RequestRealTime(cb)` (sec 1.3). Parsed with `ParseFull(str, isLocal:true)` = exact `yyyyMMddHHmmss`, **assumed device-local wall clock** (stored as `_serverTimeUtc`; `_serverTime = that + local UTC offset`). To keep `NetTimeUtc.TotalSeconds()` equal to real epoch (mail `expireAt`, header `timestamp`, region-rank timestamps) return the **device-local** wall-clock time, not UTC. [uncertain: the real server probably returned UTC; a tz skew is harmless unless fake data uses real-epoch `expireAt`]. Afterwards `NetTime(Utc) = base + (Time.realtimeSinceStartup - _startTime)`; before the first sync it uses `DateTime.UtcNow/Now`. `IsServerTimeSynced=true` unlocks `JudgeIsNewActivity()` and `NoticeHttpMgr.RequestNotice`. Failure => retry every 10s forever.

---
# 1. Boot / loading path (LoadingScene), in order

## 1.1 GET `https://match3d.oss-accelerate.aliyuncs.com/tilemaster3d/config/android/<Application.version>/version_manifest.json`
- `Downloaded.UpdateVersion.CheckVersion()` from `SceneLoading.Start` (skipped if no network). Layer D (`HTTPMgr.Request`, 5s timeouts, no headers).
- Response `VersionInfo` = `{"cfgs": {"<file name>": "<hash/version string>"}}`. Compared with local `version_manifest.json`; every differing entry is downloaded as `.../<version>/<file name>` **text** (`WriteFile(name, DataAsText)`) = remote config override (json cfgs), then event `UpdateVersionData`.
- Failure/non-2xx/404 => `func_finish` immediately (no override). **Safe fake: 404, or `{"cfgs":{}}`.**

## 1.2 POST `/utils/api/v1/getUserNation`
- Layer B repeat manager, key `User_Label_Get_Country`, retried every 10s until success; headers `Content-Type: application/json`, `timestamp: <unix sec>`, no auth, empty body.
- Response `ServerDataRoot<NationInfo>`: `{"code":1,"msg":"","data":{"ip":"1.2.3.4","nation":"VN"}}` ([ok] code==1 && data!=null). Drives region (0.5).

## 1.3 GET `/common/time?format=yyyyMMddHHmmss`
- `ServerDateTimeMgr.RequestRealTime(NoticeHttpMgr.I.RequestNotice)` from `IELoading`. Layer A `CheckRepeatRequestsMgr` key `GetRealTime`; header `CommonServiceToken`; connect timeout 30s.
- Response `ServerDataRoot<string>`: `{"code":1,"msg":"","data":"20261110083015"}` [ok] code==1 && data!=null; **exact 14-digit format** (wrong format: `ParseFull` returns old value but still marks synced). Timezone: 0.6.

## 1.4 POST `/connect/token` (guest login)
- `OwnServerLogin.LoginGuest` via `Authentication.HttpMgr.Post` (layer D). Headers `Content-Type: application/x-www-form-urlencoded`, `client_version`.
- Form fields: `grant_type=external`, `scope=all.full_access`, `client_id=TileMaster3D`, `client_secret=ae0b9ae818784f22aaf5a16f69ddd501`, `external_token=<SystemInfo.deviceUniqueIdentifier>` (google/facebook/apple: SDK token), `provider=guest|google|facebook|apple`.
- Response `ServerInfo`:
```
{"access_token":"...","expires_in":3600,"token_type":"Bearer","scope":"all.full_access","code":1,"msg":"",
 "data":{"token":"<string>","user":{"UserId":"<24 hex>","DisplayName":"","Email":"","PhotoUrl":"","ProviderId":"guest","ExternalId":"<deviceId>"}}}
```
- Used: `data.token` (-> DB.Account.Token; empty => `_LoginFail`), `data.user.UserId` (-> Account.Userid), `DisplayName` (copied to Account.Name only if non-empty; `""` keeps client default "Player<ts>"), `Email/PhotoUrl/ProviderId/ExternalId` copied. `access_token/expires_in/code/msg` unused (any 2xx + parseable JSON counts as login success). **`data` and `data.user` must be non-null** (NRE). Non-2xx => login failure dialog (`Language124`)/`Guest_Login Fail`.

## 1.5 GET `/data`  (0.4)
Right after login (also `DataManager.LoadData` when logged in and Maxlv==0). Layer D; headers `client_version`, `Content-Type: application/json`, `Authorization: Bearer <token>`.
Response `DataInfo {code:int,msg:string,data:Dictionary<string,object>}`. [ok]: `{"code":1,"msg":"","data":{}}` for a new user (triggers upload of local data), else the saved doc. `code!=1` => login failure.

## 1.6 POST `/data` (background sync whenever DB fields change; also right after first login)
Body per 0.4, `Authorization: Bearer`. Response `DataInfo`; success iff `code==1`. Safe: `{"code":1,"msg":""}`.

## 1.7 POST `/notice/api/v1/app/appGetNotice`
- `NoticeHttpMgr.RequestNotice` = callback of 1.3 (only when time synced). Layer C, key `Http_GetNotices`; headers `Authorization: <static JWT>`, `Content-Type: application/json`. Body `{"fromTime":<unix sec NetTime>,"system":"android","ver":"<Application.version>"}`.
- Response `ServerDataListRoot<NoticeInfo>` (global): **success is `code == 0`** (client returns on `code != 0 || data==null || Count==0`).
  `NoticeInfo {int id; string projId; string content; string system; string category; string versions; string title; long fromTime; long toTime; long createAt; bool isUsed}`.
  `category=="Login"` -> `pNoticeInfo` (highest id, login popup); `category=="GameLED"` -> LED list (`isUsed==true` dropped).
- Safe fake: `{"code":0,"msg":"","data":[]}`.

## 1.8 POST `/utils/api/v1/getFBDeletion` (only when logged in with Facebook, platform id 0)
Layer D, 5s; headers `client_version`, `Content-Type`, `Authorization: Bearer`. Response `FacebookDeletionRoot`: `{"code":1,"msg":"","data":{"isDelete":false,"deletion":null}}`; `data` must be non-null (NRE). `isDelete:true` wipes the account. Failure => treated as not deleted.

## 1.9 GET `/common/GetMonthIssue`  (after loading, on entering MainScene)
`RankingUtils.ReqMonthIssue` (layer D, 5s); headers `Content-Type`, `CommonServiceToken`, `client_version`. Response (private `ServerInfoData`): `{"code":1,"msg":"","data":"<issue string>"}`; [ok] `code==1`, **`msg==""`**, `data!=null` -> `DB.User.MonthIssue`. Issue is used as a Mongo path segment (`User.MonthIssueSumStar.<issue>`) so no dots; suggested `"202611"` [format uncertain]. Failure only logged (month ranking tab empty while `MonthIssue==""`).

---
# 2. Lobby / always-on

## 2.1 POST `/mail/api/v1/app/appFindEmail`  (system mail list)
`EmailManager.GetEmails` on events `Update_Account`/`AccountDataSynced` and every 300s; only if `DB.Account.Token` non-empty. Layer A, signed family + `Authorization: <raw token>`.
- Body `{"pullReceived":1,"pullClaimed":0,"uid":"<userId>","lang":"en-US"}` (lang: zh-CN, fr-FR, de-DE, ja-JP, ko-KR, pt-PT, ru-RU, es-ES, zh-TW, else en-US).
- Response `EmailServerDataRoot`: `{"code":1,"msg":"","data":{"list":[EmailServerInfo],"total":0}}`.
  `EmailServerInfo {string addition, content, emailId, title, templateCode, vars; long createAt, expireAt (unix sec); int status}`. Client keeps entries with `status>=0 && status!=2`, unique `emailId`, `NetTimeUtc.TotalSeconds() <= expireAt`; status 0=unread, 1=read; rewards come via `MailData(info)` (`EmailServerRewardData` list `[{id,count}]` -> "id|count,..").
- Safe fake: `{"code":1,"msg":"","data":{"list":[],"total":0}}` (`data!=null` required; `list` may be null).

## 2.2 POST `/mail/api/v1/app/appGetEmail` (mark read)
Body `{"lang":..,"emailId":"..","uid":".."}` signed. Client only checks Success state, then refetches 2.1. Safe `{"code":1,"msg":""}`.

## 2.3 POST `/mail/api/v1/app/appClaimEmail`
Same body. Response `ServerDataRootBase`; `code==1` => reward granted client-side + list refetch; else callback(false). Safe `{"code":1,"msg":""}`.

## 2.4 GET `/team/CheckNewTeamBoard`  (red dot; every 10s from `RedDotHttpMgr`)
Layer C, key `CHECK_NEW_TEAMBOARD`, Bearer. Response `{"code":1,"msg":"","data":true|false|null}`. Safe `{"code":1,"msg":"","data":false}`.

## 2.5 GET `/team/GetLives`  (energy gifts)
Callers: `GameMail/EnergyMail.GetEnergy` (layer A, only if `bLogined`, key `Mail_GetEnergy`), `ShareHttpMgr.GetEnergy` (layer C, key `TeamHttp_GetEnergy`). Bearer.
- Response `ServerDataListRoot<EnergysInfo>`: `{"code":1,"data":[{"Lives":{"<guid>":{"GUID":"..","SendUserId":"..","SendUserName":".."}}}]}`.
- Safe: `{"code":1,"msg":"","data":[{"Lives":{}}]}` (EnergyMail needs `Count!=0`; `GetEnergyLayer` reads `data[0]` so an **empty list throws**; `Lives` may be null).

## 2.6 POST `/team/ReceiveLive`
Callers: `EnergyMail.ReceiveEnergy` (layer A, body `{"GUIDS":["id",..]}`), `EnergyHttpMgr.ReceiveEnergy` (body `{"GUID":"id"}`). Response Base, `code==1` => energy credited. Safe `{"code":1,"msg":""}`.

## 2.7 GET `https://match3d.oss-accelerate.aliyuncs.com/tilemaster3d/advertvideo/<file>` (custom ad video)
`AdvertisingMgr.LoadData` (cfg `Json/cfgAdvertising`, `_data.file`), saved to `_data.url`; layer D, 5s. Entry `id==1` is local. Fake: 404 (video simply not ready).

---
# 3. Rankings (Rank tab, `RankingRootControl`)

## 3.1 GET `/ranking/get?query=<mongo json>&projection=<json>&sort=<json>&limit=<n>`  (generic Mongo query over the /data docs)
`RankingControlBase.RequestRankingData` (layer D, 5s); headers `Content-Type`, `Authorization: Bearer`, `client-version`. Response is parsed **without checking `code`**; any 2xx = success. Cached 5s in StorageMgr.
Row = `{"userId":"<id>","projection":{"_id":{...},"Account":{"name","photoUrl"},"User":{...}}}` in `{"code":1,"msg":"","data":[rows]}`. Query strings are raw (un-encoded) JSON with `{}` `$` `"`.
| Caller | query / sort | projection fields parsed |
|---|---|---|
| `StarRankingControl` (personal total) | `{"$expr":{"$lt":["$User.sumStar",{"$multiply":[{$toInt:'$User.maxlv'},2000]}]}}`, sort `User.sumStar:-1`, limit 1000 | `Account.name,photoUrl`, `User.sumStar`, `User.medal` |
| `MonthRankingControl` (personal month) | `{"User.MonthIssueSumStar.<MonthIssue>":{$type:"number"}}`, sort same path, limit 1000 | + `User.MonthIssueSumStar` = `{"<MonthIssue>":int}` (**KeyNotFound if dict present without that key**) |
| `MonthRankingHttpMgr.GetPersonalRankingData` | same month query, limit `cf_const.monthRankingActivityLimit` | `PersonalMonthStarRankingData{userId,projection}`; needs code==1 && data!=null |
| `TeamHttpMgr.GetTeamMembers` | `{"Team.TeamId":"<id>"}`, sort `User.maxlv`, limit 50 | `MemberInfo{userId, projection{User{maxlv},Account{name},Team{Helps,IsLeader}}}`; handler needs code==1 |
| `TreasureHuntHttpMgr.GetTeamMembers` | `{$and:[{"Team.TeamId":..},{"TreasureHuntActivity.allScores.<team>.<issue>":{$type:"number"}}]}`, limit 50 | `TreasureHuntMemberInfo{userId,projection{Account{name},TreasureHuntActivity{allScores{team:{issue:int}}}}}` |
- Safe fake: **do not return an empty list for Star/Month ranking**: `DeserializeProcessRespondData` does `_data[_data.Count-1]` when the own row is missing => ArgumentOutOfRange. Return >=1 row (own row built from stored /data, plus optional bots with 24-hex ids; `Account` may be null -> "Player<8 chars>"), or answer HTTP 500 for the client's failed/retry state. Empty `data:[]` is fine for the team-member variants.

## 3.2 GET `/team/GetTeamRanking?match=<agg>&group=<agg>&sort=<agg>&lookup=<agg>&project=<agg>&limit=100`
`TeamMonthRankingControl` / `TeamSumRankingControl` (layer D, same headers as 3.1). Strings are Mongo aggregation stages (fake can ignore). Response (private DataRoot, code not checked):
`{"code":1,"msg":"","data":{"ranking":[{"sum":123,"team":{"id","name","badge","description","type","requiredLevel","members","limit","created":"ISO"}}],"my":{"sum":..,"team":{..}} | null}}`.
Safe: `{"code":1,"msg":"","data":{"ranking":[],"my":null}}` (`data` and `ranking` non-null; empty OK).

## 3.3 GET `/common/GetGeneralActivity?type=WholeMonth`  (month-ranking activity period)
`MonthRankingHttpMgr.GetPersonalRankingActivity` (layer C), headers Bearer + `CommonServiceToken`. Response `ServerDataRoot<MonthRankingActivityInfo>`: `{"code":1,"data":{"issue":"2026Y11M","startTime":"2026-11-01T00:00:00","endTime":"2026-11-30T23:59:59","currentTime":"...","endTimeSpan":<long seconds-left>,"available":true}}`. [ok] code==1 && data!=null; times parsed by `DateTime.TryParse`; issue key derived client-side as `yyyy"Y"MM"M"` from startTime; `endTimeSpan>=1` = still running. Safe "no activity": 404 or `{"code":-1,"msg":"none"}`. `/common/GetTestActivity` is the test twin (same shape/headers, not used in the release path).

---
# 4. Team feature (unlocks at `cf_const.teamUnlockLv`)
All layer C (`OwnHttpMgr`), `Authorization: Bearer <token>`, `Content-Type: application/json` on POST; replies are dispatched to UI by key; UI acts on `code==1`; anything else or HTTP failure shows retry / network tip (`Language55`). Timeouts 5s/2s.

| Method + path | Request | Response T | Notes |
|---|---|---|---|
| GET `/team/GetMyTeam` | - | `ServerDataRoot<TeamInfo>` | polled every 10s in team UI. **No team => `{"code":1,"msg":"","data":null}`** (UI shows team list). |
| GET `/team/GetTeams[?name=<q>]` | name = search | `ServerDataListRoot<TeamInfo>` | safe `data:[]` (list must be non-null; search reads `.Count`). |
| POST `/team/CreateTeam` | `{name,badge,description,type,requiredLevel}` | `ServerDataRoot<TeamInfo>` | return the created team. |
| POST `/team/UpdateTeam` | `{badge,description,type,requiredLevel}` | `ServerDataRoot<TeamInfo>` | |
| POST `/team/JoinTeam` | `{TeamId,RequiredLevelField:"User.maxlv",UserName}` | Base | |
| POST `/team/LeaveTeam` | `{AlternateLeaderSort:"{'User.maxlv':-1}",UserName}` | Base | |
| POST `/team/KickTeam` | `{UserId,UserName,AlternateLeaderSort}` | Base | |
| GET `/team/GetJoinTeamApplies` | - | `ServerDataListRoot<ApplyJoinTeamInfo>` rows `{JoinTeamApply:{GUID,TeamId,UserId,UserName,Created}}` | safe `data:[]` |
| POST `/team/AgreeJoinTeamApply`, `/team/RejectJoinTeamApply` | `{GUID}` | Base | |
| GET `/team/GetBoards` | - | `ServerDataListRoot<BoardsInfo>` = `data:[{"Boards":{"<guid>":BoardInfo}}]` | safe `{"code":1,"data":[]}`. `BoardInfo{_t,GUID,Type(JoinTeam/LeaveTeam/RequestingLife/RoyalPassActivity/ShortBattlePass/else chat),UserId,UserName,Created,Medal?,Content,LifeReceived,LifeMax,LifeSenders[],EndTime,Extension(json string dict),Receivers[]}` |
| POST `/team/AskLive` | `{UserName,Medal}` | Base | request energy |
| POST `/team/Help` | `{GUID,UserName}` | Base | give energy |
| GET `/team/GetLastRequestingLift` | - | `ServerDataRoot<ApplyEnergyInfo>` `{guid,type,userId,userName,lifeReceived,lifeMax,lifeSenders[],created,endTime,endTimeSpan(long ticks),canHelp}` | safe `{"code":1,"data":null}` (client then chains GetLives) |
| POST `/team/Chat` | `{UserName,Content,Medal}` | Base | |
| POST `/team/AddTeamBoardReceiver` | `{GUID}` | Base | |
| POST `/team/CreateTeamBoard` | `{UserName,Medal,Type:"RoyalPassActivity"\|"ShortBattlePass",Content,Extension}` | Base | RoyalPassHttpMgr / ShortBattlePassMgr after purchase; non-1 => retried later. Safe `{"code":1,"msg":""}`. |

`TeamInfo {string id,name,badge,description,type; DateTime created (ISO 8601); int requiredLevel, members}`. Also see 2.4-2.6 (CheckNewTeamBoard, GetLives, ReceiveLive) and ranking 3.1/3.2 (team members/ranking).
Cleanly disabling teams: answer 200 with the "no team" shapes above.

---
# 5. Activities
All need `code==1 && data!=null` to start; **`{"code":-1}` or 404 keeps an activity inactive**. Shared info shape (`RoyalPassActivityInfo`, `TreasureHuntActivityInfo`(+myTeam), `RequestIssueData`):
`{"issue":"<str>","startTime":"<ISO>","endTime":"<ISO>","currentTime":"<ISO>","endTimeSpan":<long>,"available":true,"myTeam":{id,name,created,badge,description,type,requiredLevel,members,limit}|null}`.
`issue` must be non-null when `data` is present (`info.issue.Equals` NRE). `available:false` leaves the Started state off.

## 5.1 GET `/team/GetPassActivity` - Royal (Master) Pass; `RoyalPassHttpMgr.GetActivity`; Bearer; `ServerDataRoot<RoyalPassActivityInfo>`.
## 5.2 GET `/team/GetActivity` - Treasure Hunt; `TreasureHuntHttpMgr.GetActivity`; Bearer; `ServerDataRoot<TreasureHuntActivityInfo>` (`myTeam` feeds team score board; also uses the ranking/get variant in 3.1).
## 5.3 Baking Master - `Game/Activity/BakingMaster/ActivityHttpMgr` (layer A, entrance / enter-level / root dialogs)
- GET `/team/GetGeneralActivity?type=<Year|Month|Week|Day>&days=<n>&offsetForDays=<n>&minutes=<n>` (from `cf_const.activityBakingMasterDate`), headers Bearer + `CommonServiceToken`; `ServerDataRoot<RequestIssueData>`; issue key = `"BakingMaster_"+issue`; state resets when issue changes.
- GET `/team/GetMatchs?issue=BakingMaster_<issue>` -> `ServerDataListRoot<RequestMatchData>` `[{id,issue,teamId,userId,levelId,grade,extension:"{\"userName\":\"..\"}"}]` (safe `[]`, `data` non-null).
- POST `/team/SetMatch` body `{Issue,LevelId,Grade,Extension}` -> `ServerDataRoot<RequestMatchData>` (echo row; code==1 && data!=null updates own grade).
- DELETE `/team/DeleteZeroGradeMatch?issue=BakingMaster_<issue>` -> `ServerDataListRoot<RequestMatchData>`; code==1 clears loser data.

## 5.4 Region Rank (`RegionRankServerManager`, layer A, signed family without Authorization)
Require `DataManager.bLogined` (else upload is skipped; may trigger `GuestLoginManager.Login`).
- POST `/rank/api/v1/app/upCustomRank` body `{"list":[{"rankType","member","score","expireAt","isAddUp","extraData"}]}`; rankType `region_rank_personal_<regionCode>_<issue>` (member=userId, extraData=`{"name":..,"medal":..}`) and `region_rank_national_<issue>` (member=regionCode, isAddUp:true). Response Base `code==1` => ok. Safe `{"code":1,"msg":""}`.
- POST `/rank/api/v1/app/findCustomRankList` body `{"rankType","expireAt","member","page":1,"size":100}`. Response `ServerDataRoot<RegionRankServerData>`:
  `{"code":1,"msg":"","data":{"list":[{"key":"<member>","score":123,"data":"<json string {name,medal} or empty>"}],"nextRefreshAt":<unix sec>,"selfRank":0,"total":0}}`. `list` must be non-null. Empty list => client sets `NextRefreshAt=now` (re-polls). The activity schedule itself comes from local config (`cf_regionRank_const`), not from the server. regionCode = "vn" if in `cfgRegionRankRegions` else "unknown".

---
# 6. IAP validation (Android; `FQDev.StoreKit`)
With scripting define `FAKE_SDK`, `IAPAndroidOrderValidateManager.OrderValidate` already short-circuits (valid+sandbox, no deliver, no network), so 6.1/6.2 are not hit. Otherwise:
## 6.1 POST `/utils/api/v1/validateGoogleOrderV2` (signed; body `{purchaseToken,subsOrProductId,dillId,subscription}`) -> `ServerDataRoot<OrderValidateInfo{dillId:string,status:int,isSandbox:bool}>`; `code==1 && data!=null`; `status==0` valid new order (then deliver); `status==1` valid only for NonConsumable; else invalid. Safe: `{"code":1,"msg":"","data":{"dillId":"<same>","status":0,"isSandbox":true}}`.
## 6.2 POST `/utils/api/v1/deliverIosOrder` (name reused for Android; body `{dillId}`, signed) - fire-and-forget, any reply (`{"code":1}`).
## 6.3 Legacy/unused on Android: POST `/utils/api/v1/validateGoogleOrder` (`IAPOrderValidateManager`, body `{purchaseToken,subsOrProductId,subscription}`, ok when `code==1 && data.purchaseState==0`), POST `/utils/api/v1/validateIosOrderV2` and `validateIosOrder` (iOS; `data{Status,IsSandbox,environment}`; layer B `OnlyRequestsManager`).

---
# 7. Fake-server checklist (priority)
1. `POST /connect/token` (form) -> sec 1.4 JSON; stable `UserId`/`token` per `external_token`.
2. `GET/POST/DELETE /data` -> persist/merge per user (0.4); GET returns `{"code":1,"msg":"","data":{...}}`.
3. `GET /common/time` -> `{"code":1,"msg":"","data":"yyyyMMddHHmmss"}` (device-local wall clock).
4. `POST /utils/api/v1/getUserNation` -> `{"code":1,"msg":"","data":{"ip":"127.0.0.1","nation":"VN"}}`.
5. `POST /notice/api/v1/app/appGetNotice` -> `{"code":0,"msg":"","data":[]}`.
6. `GET /common/GetMonthIssue` -> `{"code":1,"msg":"","data":"202611"}`.
7. `POST /mail/api/v1/app/appFindEmail` -> `{"code":1,"msg":"","data":{"list":[],"total":0}}`; appGetEmail/appClaimEmail -> `{"code":1,"msg":""}`.
8. Team: `CheckNewTeamBoard` -> `data:false`; `GetMyTeam` -> `data:null`; `GetLives` -> `data:[{"Lives":{}}]`; `GetTeams/GetBoards/GetJoinTeamApplies` -> `data:[]`.
9. `GET /ranking/get` -> list with >=1 row (own); `GET /team/GetTeamRanking` -> `{"code":1,"data":{"ranking":[],"my":null}}`.
10. Activities (3.3, 5.x) and OSS files: 404 or `{"code":-1}`; `rank/api upCustomRank` `{"code":1}`, `findCustomRankList` empty list.
Rules: respond 200 + `code` for business results; non-2xx = failure signal; reply 404 for anything unneeded (an unanswered request just hangs the UI until its timeout: ranking/get has 5s, most others 5-100s).
