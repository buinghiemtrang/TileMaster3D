# Bài học Dicero (Cocos web track + Unity DiceroProto)

Viết tắt nguồn: DL = DiceroCocos/docs/DEV_LOG.md; PG = DiceroCocos/docs/PROGRESS.md; RM = DiceroCocos/README.md; HO = Documents/dicero-1-7-2/00_SESSION_HANDOFF.md; REP = DiceroCocos/docs/reports/*; MEM = ~/.claude/projects/-home-trangbn/memory/*. (Gốc: /mnt/data/BuiNghiemTrang_Works/Project/Unity)

## 1) Tóm tắt + timeline

Dicero! 1.7.2 = Cocos Creator 3.8.8, JS/TS chạy trên SystemJS, client-server protobuf. Hai track song song: (a) DiceroCocos chạy lại JS gốc trên engine Cocos web tự build + server giả; (b) DiceroProto Unity (hiện chỉ là project art-reference + importer, chưa có gameplay).

| Ngày | Mốc | Nguồn |
|---|---|---|
| 2026-10-28 sáng | Giải mã .jsc (XXTEA key `64gNefx9NKIZF4NH`), export toàn bộ art/spine/audio/prefab, giải 2.418 bảng dữ liệu, dựng DiceroProto (Unity 6000.3.10f1 + spine 4.2) | HO mục 1; MEM dicero-reverse-engineering |
| 2026-10-28 chiều tối | Hướng Unity: Codex port tay JS->C#, hết limit 19:50, bỏ; chuyển sang Jint chạy JS gốc trong Unity (parity trace với node cho ch1/2/10/50/120) | HO "Buổi 2 - đổi hướng logic" |
| 2026-10-28 tối | Mở track Cocos; `docs/design/net_protocol.md` viết 20:59 (đọc code, chưa chạy gì) | REP mtime; net_protocol.md đầu file |
| 2026-10-28 | M1: engine 3.8.8 web render được màn Login; M2: login->Main->Battle bằng server giả | DL mục 1 (M1, M2) |
| 2026-10-29 ~02:00-06:40 | M3: autoplay phá đảo chapter 1; M4: meta + cả 6 chế độ chơi (Coin, Co-op, Starhunt, HeroBattle, GuildBoss) chạy trọn vòng trên UI thật | DL header "Cập nhật 2026-10-29 06:40"; mtime codex_modes_report 04:36, codex_meta3 04:48 |
| 2026-10-29 trưa-chiều | Crawler, harness 191 handler/2.275 ca, đóng gói dist/DiceroWeb, playtest + quay video (14:09); user đổi hướng: bỏ bản online-vs-Habby, làm PRIVATE SERVER | DL mục 2; REP/playtest_report.md |
| 2026-11-02/03 | Dọn docs, chuyển DEV_LOG, SVN r2957 (thiếu file), `server/` Mốc A: proto độc lập 633 message, codec, verify 633/633 | MEM dicero-cocos; PG Mốc A |

Kết luận: từ lúc bắt đầu track Cocos (tối 10-28) đến "chơi được cả 6 chế độ" (sáng 10-29) chỉ ~1 đêm + 1 buổi; phần còn lại (10-29 chiều -> 11-03) là kiểm chứng, đóng gói, hướng server thật. Track Unity cùng thời điểm vẫn ở mức importer + Jint parity cho 1 chapter (HO; DiceroProto/README.md: "No gameplay code").

## 2) Cái hiệu quả

1. **Chạy lại build gốc thay vì viết lại**: APK đã chứa bản build Cocos 3.8.8 đầy đủ; chỉ cần engine web cùng version + JS giải mã + stub native. Không phải hiểu logic để chạy. (DL "Hướng đã chọn")
2. **Tái dùng nguyên module của game cho phần mạng**: fake server dùng chính `NetUtils.encrypt/decrypt`, `Request/Response`, `index.proto.*` qua `System.import('chunks:///_virtual/<X>.ts')`, nên không phải tự dịch protocol. (DL M2 "Server giả trong trang"; RM "Cách hoạt động")
3. **Đọc code ra tài liệu giao thức trước** (net_protocol.md: URL `api/<Name>`, envelope "FFFF" + CRC + shuffle, `head` luôn phải có) rồi mới viết server. (net_protocol.md 1.1-1.4)
4. **Kiểm chứng bằng UI thật + autoplay qua UI** thay vì chỉ gọi handler: `drive.js` (Chrome headless giữ mở, điều khiển HTTP), `autoplay_*` ra bằng chứng ảnh cho từng chế độ. (DL mục 1 M3/M4)
5. **Chốt hướng rẻ nhất, bỏ hướng đắt sớm**: user bỏ APK-native/PairIP và bản online; bỏ transpile JS->C# sang Jint. (DL mục 2 "QUYẾT ĐỊNH MỚI"; HO "đổi hướng logic")
6. **Cờ dev trên URL** (`?fresh ?unlockall ?god ?god5 ?godx=N ?reset ?lang=`) cho test nhanh các màn sâu. (RM "Chạy và các cờ dev")
7. **Dữ liệu thưởng lấy từ bảng game** (`fakeserver_data.js`) nên khớp client; `?fresh` phải `localStorage.clear()` mới vào đúng tutorial. (DL M3, pitfalls)
8. **Chia Codex theo module đóng kín**: meta, modes, meta2/3/4, harden; mỗi việc có file báo cáo trong docs/reports. (REP/codex_*_report.md)

## 3) Cái chậm / lãng phí + nguyên nhân

- **Codex port tay JS->C# (T1/T2) rồi bỏ**: tốn quota, hết limit lúc 19:50 giữa chừng; phải đổi sang Jint. Nguyên nhân: chọn hướng viết lại logic trong khi có thể chạy JS gốc. (HO "Codex hết limit... Bỏ hướng port tay")
- **Codex burn limit**: model mặc định gpt-6-astra hết limit trong ~1h; sau đó phải ép `gpt-6.1-sol`/`gpt-6-luna`, prompt hẹp. (MEM codex-usage)
- **Codex bị sandbox chặn socket**: không mở được Chrome (`setsockopt: Operation not permitted`), tốn ~45K token cho e2e. (MEM codex-usage; REP/codex_e2e2_report.md, codex_meta_report.md "Live UI remains untested")
- **`tools/e2e.js` do Codex viết: chạy thật 4 lần đều FAIL do lỗi script** (menuDrain chạy sau điều hướng, đóng luôn màn đích...), nhiều vòng sửa mà chưa ổn; phải dùng `autoplay.sh` + `sweep.sh` thay. (DL mục 1 "tools/e2e.js"; RM "chưa ổn định")
- **Harness codec của Codex pass nhưng UI từ chối**: `GainTaskBatch` trả `finish` là object thay vì int32 -> client "Invalid Parameter[1001]"; harness không bắt được, phải test UI. Sau đó Codex mới thêm bắt "invalid int32" và log "handler error" làm fail. (DL M4 "Màn phụ"; REP/codex_harden_report.md; MEM dicero-cocos)
- **Playtest dài vô ích**: video chapter 30 đánh 552s không tới win (quái HP ~7000 + hồi máu) -> "không dùng". Co-op boss 298K HP autoplay không hạ được. (REP/playtest_report.md; DL M4 Co-op)
- **Sót file khi SVN**: r2957 thiếu web/index.html, style.css, src/, cocos-js/, 98 thư mục texture; phải `svn add` bù. (MEM dicero-cocos 2026-11-03)
- **Ổ dữ liệu chậm**: nạp module game ~1 phút mỗi lần chạy tool server. (PG Mốc A ghi chú)
- **Tài liệu lệch**: 3 file handoff ở 3 nơi (HO ghi "viết code mới từ đầu", DL ghi "chạy code gốc"; memory dicero-reverse-engineering còn nói "no Habby code reuse"). Gây nhầm phạm vi. (HO mục 1b vs 4; MEM dicero-reverse-engineering)

## 4) Yêu cầu / định hướng của người dùng

- Mục tiêu: chạy TOÀN BỘ game bằng code+resource đã dịch ngược; user là thành viên team dev nên dùng nội bộ. (DL header; HO header)
- Cocos track kỳ vọng nhanh hơn Unity. (MEM dicero-cocos "Why")
- Chạy nội bộ team trong LAN, bật/tắt server = cho phép/ngắt; độc lập hoàn toàn khỏi Habby. (DL mục 2 QUYẾT ĐỊNH MỚI)
- Server dài hạn: reskin + phát hành Google Play/App Store, nên không được là đồ bỏ: handler dạng hàm thuần `(state, req)->(state, resp)`, lưu trữ sau interface (SQLite -> PostgreSQL), tài khoản/token thật, kiểm hóa đơn IAP sau, wire protocol độc lập client. (PG "Định hướng chung"; MEM dicero-cocos 2026-11-03)
- Client cuối là **Unity**; server không nạp code Cocos, có `.proto` độc lập + test vector + PROTOCOL.md; chỉ thêm message, không sửa cũ; không lên kế hoạch chống gian lận bằng replay logic JS của client. (PG; MEM dicero-cocos)
- Subagent model thấp, Codex chọn model rẻ, tối đa 2-3 agent cùng lúc. (MEM subagent-small-models, codex-usage, limit-parallel-agents)
- Việc lớn xong: playtest tổng thể trước khi bàn giao, có video theo từng tính năng + cách chạy. (MEM playtest-before-handover; REP/playtest_report.md)
- Chỉ SVN (RabbitVCS/SmartSVN), không git, không tự `svn add/commit`. (MEM svn-only-workflow; skill decompile-game-workflow bước 4-5)
- Tiếng Việt khi trả lời; mỗi dự án tiếp tục tới mục tiêu handoff, không dừng giữa chừng. (MEM work-to-handoff-goal, subagent-small-models)

## 5) Công cụ (path + mục đích)

Phần giải mã/export (Documents/dicero-1-7-2/tools/):
- `jsc_dec.py` giải XXTEA .jsc; `cocos_lib.py` đọc import JSON/pack/CCON Cocos 3.8 compiled; `build_index.py` tạo work/index.json; `tex.py` giải ASTC; `export_textures.py`, `export_spine_media.py`, `export_prefabs.py` xuất art/spine/prefab; `decode_tables.mjs` giải 2.418 bảng (Codex); `xxbrute.c` brute-force key trong libcocos.so. (HO mục 3; MEM dicero-reverse-engineering)

Cocos track (DiceroCocos/tools/):
- `build_engine.js` build cocos-engine v3.8.8 -> web/cocos-js/cc.js; `assemble_web.py` dựng web/assets + settings + chunks từ APK + JS giải mã (ASTC->PNG, config `encrypted:false`); `package.sh` đóng gói dist; `build_apk.sh` hướng APK cũ (đã bỏ). (RM bảng tools; DL M1)
- `server.js` static :7788 (bind mọi IP cho LAN); `start.sh` in URL/cờ. (RM)
- `drive.js` Chrome headless giữ mở + API HTTP :7789 (/click /shot /eval /log /rec); `shot.js` chạy kịch bản steps/*.json; `autoplay.sh` + `autoplay_step.js` + `autoplay_brain.js` tự chơi qua UI (xử lý tutorial/popup, reroll tạo combo); `sweep.sh` + `close_step.js` bấm lần lượt các màn; `crawl.js` crawler UI; `guide_probe.js`; `montage.py` ghép ảnh. (DL M3/M4, RM)
- `test_fakeserver.js`, `test_meta2.js`, `test_meta4.js`, `test_modes.js`, `fakeserver_coverage.js`, `fakeserver_habby_test.js`: gọi handler với encode/decode protobuf + envelope thật. `e2e.js`: hồi quy 7 flow, chưa ổn. (REP/codex_harden_report.md; DL)
- `svn_ignore.sh` đặt svn:ignore (deps dist shots videos *.log *.keystore). 
- Fake server trong trang: `web/src/{hooks,fakeserver,fakeserver_handlers,fakeserver_data,fakeserver_meta*,fakeserver_modes,fakeserver_habby}.js`. (RM)
- Server thật (Mốc A): `server/tools/extract_proto.js` (633 message -> .proto), `server/tools/verify_codec.js` (633/633, 12.793 kiểm), `server/src/net/codec.js`, `server/docs/PROTOCOL.md`, `server/test/vectors/envelopes.json`. (PG Mốc A)

Unity track (DiceroProto): `Assets/Editor/DiceroImport/DiceroResourceImporter.cs` (menu Tools/Dicero/Import Resources, 9-slice từ capInsets, spine .skel.bytes, PMA off), batch test `Dicero.Editor.DiceroProjectSetup.BatchTest`; Jint host `Assets/StreamingAssets/DiceroJs/host.js` + `Assets/Dicero/Logic/DiceroJs.cs`; parity `CoreDev/JintTest`. (DiceroProto/README.md, TEST_REPORT.md; HO 1b)

## 6) Bẫy kỹ thuật

### 6a) Riêng Cocos (3.8.x)
- .jsc = XXTEA; key tìm bằng brute-force `strings libcocos.so` so với một file .jsc ngắn; output là JS thường. (MEM dicero-reverse-engineering)
- Dữ liệu bảng: `combindata/alldata` + `alldatajson` (index tên/độ dài), định dạng Luban-style; **phải chạy module `schema.ts` bằng node** mới decode được. (MEM; HO)
- Compressed UUID + versions map để tìm import/native; `cc.config.*.json` `encrypted:true` chỉ nói về script -> đổi `false` khi thay bằng JS giải mã. (MEM; DL M1)
- Texture Android = ASTC, trình duyệt desktop không đọc -> chuyển PNG và vá `ImageAsset._deserialize` (ext astc->png). (DL M1 hooks.js)
- Build engine: platform `HTML5` (không có WEB_MOBILE); phải `npm run build:debug-infos && build:const`; babel `loose:true` sinh `_i13 is not defined` -> dùng `targets: chrome 100`; import-map tương đối theo `src/`; cần emscripten externals (spine/bullet/box2d/meshopt) lấy từ cocos-engine-external tag v3.8.8-2. Quét mọi class `cc.*` trong import để bật feature còn thiếu (marionette, procedural-animation, ui-skew). (DL pitfalls, M1)
- Thiếu `assets/env` -> game gọi thật ra server dev: tạo env (`{"e":11,"z":9}`) + CSP `connect-src 'self'` + chặn XHR/fetch/WS ngoài. Đừng gỡ guard. (DL pitfalls)
- `HabbySdk` dùng `cc.native.reflection` chứ không `window.jsb` -> alias; SDK tracker stub bằng Proxy. (DL pitfalls)
- Fake server trả lời tức thì làm scene HotUpdate bị huỷ trước khi UI mở (`setParent ... reading 'push'`) -> thêm độ trễ giả 30ms (GetReview 600ms). (DL pitfalls)
- Response body rỗng -> client `data={}` nên `.forEach` crash; fakeserver luôn chèn field lạ #1000. Luôn gửi `head` (kể cả rỗng `0A 00`), HTTP 200 cho lỗi app. (DL pitfalls; net_protocol.md 1.4)
- Protobuf: response chọn class theo tên request (`<Name>Response`), 63 mutation không có named response -> dùng `ServerEmptyBody`; kiểu field sai (object vs int32) -> "Invalid Parameter[1001]". 633 message chỉ trích được sang `.proto` độc lập và verify 2 chiều với codec game. (REP/codex_harden_report.md; PG Mốc A)
- Envelope mã hoá luôn bật: "FFFF" + series + CRC32 + blockSize + shuffle block đảo ngược; CRC sai -> kick login. (net_protocol.md 1.3)
- Handler của fakeserver_meta* giữ tham chiếu `S`/`S.meta` lúc nạp -> không refactor sang per-player state bằng cách đổi object; hướng: mỗi người chơi một vm context rồi chuyển dần sang hàm thuần. (PG Mốc A ghi chú)
- PairIP chỉ bảo vệ lớp Java; gỡ = đổi `android:name` của Application, giữ libpairipcore.so; chạy native offline còn phải port fake server sang JSB + mã hoá lại .jsc. (DL mục 2 Hướng B)
- `Heart` thiếu `guildId` -> client xoá guild mỗi 30s. (DL M4)
- Cocos capInsets là [left, top, right, bottom], Unity border là (left, bottom, right, top); Spine atlas không premultiplied. (HO mục 3; DiceroProto/README.md)

### 6b) Áp dụng mọi engine
- **Tìm đường chạy lại build gốc trước**; chỉ port khi không thể. Chạy bản gốc cho kết quả khớp gốc miễn phí; port tay mất công và có parity risk. (so sánh DL vs HO 1b)
- Viết tài liệu giao thức từ code trước khi dựng server giả; tái dùng codec của game để tránh tự suy luận. (net_protocol.md)
- Chặn mạng ra ngoài (CSP/guard) ngay từ đầu để không gọi nhầm server thật của hãng. (DL pitfalls)
- **Test codec != test UI**: harness chỉ chứng minh encode/decode; UI mới lộ field sai kiểu, callback trừ tiền 2 lần, màn lock. Luôn có ít nhất một vòng UI thật (drive/autoplay) có ảnh làm bằng chứng. (DL M4; REP/codex_modes_report.md "Real UI testing remains with the main session")
- Headless browser: `drive.js` giữ 1 Chrome mở, điều khiển qua HTTP, tọa độ thiết kế = (worldX, 1560-worldY); chỉ 1 Chrome khi quay video, `--dpr 1`. Autoplay cần xử lý popup/tutorial/tips theo tên node. (DL M3; mục 2)
- Dùng cờ `?god/?godx` để test sâu vì quái cuối quá khỏe cho autoplay; nhưng phải có lượt chạy không cheat (`?fresh` autoplay phá chapter 1-2 không cheat) để chứng minh luồng đúng. (DL M3, "Autoplay thông minh")
- Server giả nên thiết kế để chuyển thành server thật: state tách khỏi handler, per-player, test vector độc lập, không phụ thuộc code client. (PG)
- Tool dev cần dữ liệu ngoài thì dùng biến môi trường (`DICERO_SRC`, `DICERO_MODULES`), không hard-code đường dẫn máy; nhưng `work/modules` nằm ổ chậm. (PG; RM)
- `codex exec` chạy nền phải `< /dev/null`; không `pkill -f <chuỗi có trong chính lệnh>`; sửa file trên ổ dữ liệu bằng python vì `sed -i` báo permission. (DL pitfalls)

## 7) Đề xuất quy trình chung decompile (Unity + Cocos)

1. **Giai đoạn 0 - xác định engine + bảo vệ**: Cocos (jsc/XXTEA, dữ liệu Luban) hay Unity (IL2CPP + global-metadata). Hỏi user mục tiêu (học cơ chế / reskin / chạy lại / server) và ghi vào handoff MỘT nơi duy nhất, đừng để 3 file nói khác nhau. (HO vs DL)
2. **Giải mã + export dữ liệu** vào `Documents/<game>/{tools,work,data,export}` (không sửa APK). Lưu key, định dạng, bẫy vào memory ngay khi tìm ra.
3. **Chọn đường chạy sớm - thử "chạy lại build gốc" trước**: Cocos -> engine web cùng version; Unity -> thử chạy IL2CPP/Mono qua trình tải riêng hoặc Jint-kiểu embed khi game là JS; chỉ port tay khi bất khả. Mốc kiểm tra: 1) render màn đầu, 2) vào Main, 3) 1 trận, 4) từng chế độ.
4. **Server song song từ đầu**: đọc protocol -> tài liệu -> fake server dùng codec gốc -> chuyển ra server thật với `.proto` độc lập + test vector, handler hàm thuần, storage sau interface (PG Mốc A->D). Với client Unity cuối cùng, chuẩn bị PROTOCOL.md + test vector để C# cài lại.
5. **Bộ test 3 tầng**: (i) codec/handler harness (Codex làm được, chạy trong node), (ii) UI headless `drive.js`/autoplay/sweep/crawl (Claude hoặc tester sonnet, vì Codex sandbox chặn socket), (iii) playtest tổng + video theo tính năng. Mọi lỗi harness bỏ sót phải thành ca test mới ngay (như GainTaskBatch).
6. **Chia việc**: Codex chỉ nhận việc code thuần có file đầu ra + báo cáo + tool harness, model rẻ (luna/sol low), prompt nêu tên file, cấm mở browser/port; Claude chạy UI thật. Tối đa 2-3 agent, checkpoint handoff sau mỗi agent, yêu cầu dòng tiến độ trong báo cáo. (MEM codex-usage, limit-parallel-agents)
7. **Bàn giao/SVN**: cuối mỗi việc lớn chạy playtest tổng, ghi handoff; khi user xác nhận xong mới rsync sang `trangbn/<Game>` (loại deps/dist/shots/videos/node_modules/keystore/log), rà file >50MB + khóa/đường dẫn máy, KHÔNG svn add/commit trừ khi user bảo. Sau khi chép, so danh sách file với `web/` thật chạy được (bài học r2957 thiếu index.html, src/, cocos-js/, 98 thư mục texture): làm checkout sạch rồi chạy thử trước khi báo xong. Đặt `svn:ignore` (không dùng .gitignore). Client/server chia `client/`, `server/`. (skill decompile-game-workflow; MEM dicero-cocos)
8. **Đo thời gian theo mốc** (M1 render, M2 Main, M3 1 trận, M4 meta/chế độ) để so sánh Unity vs Cocos có số liệu; ghi ngày giờ vào handoff mỗi mốc (DL làm tốt, HO thiếu giờ).
