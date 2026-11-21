# Bài học PixelFlow (Unity IL2CPP, dựng lại từ APK) — thực tế, không phải lý thuyết

Viết tắt: PROJ = .../game_decompile/game_unity/PixelFlow; WORK = .../Documents/PixelFlow/PixelFlow-work; HO = PROJ/HANDOFF.md.

## 1) Tóm tắt dự án + timeline
Game: com.loomgames.pixelflow 0.37.1, Unity 6000.0.58f2, IL2CPP ARMv7, metadata v31 (PROJ/CLAUDE.md). AssetRipper xuất được 4 scene, 499 prefab, 15.718 C# nhưng **toàn thân hàm rỗng (stub)**. Đo 2026-11-04: 50.381/56.597 hàm rỗng (WORK/reports/inventory-b1.md). Khác Dicero (chạy lại code gốc), ở đây phải **dịch tay từ pseudocode Ghidra** (HO dòng 2026-11-04 17h).

Timeline (HO + CLAUDE.md):
- 11-02: dựng project, WORK, dump Il2Cpp, Ghidra import, remap 79 GUID/10.196 ref, 28 shader map (còn 225 dummy), compile batch 011/012 sạch. Burst tải treo, dùng bản copy local.
- 11-03: dịch tay theo lớp (Shooter, Map, Conveyor, tap-flow); Dreamteck Splines dịch từ native (user chọn); DOTween: user chọn bản free gốc 1.2.825 tải từ web + Pro dịch. Cuối ngày level tự tạo 380/380 pixel.
- 11-04: probe 059 = **thắng level đầu tiên** (batchmode). Feature đợt 1-3 (Codex+sonnet). ~17h user TẠM DỪNG để hệ thống lại luồng (xem mục 4). Chạy B1 kiểm kê, B2 thay thư viện, bắt đầu B3 trích Ghidra hàng loạt. Codex hết quota 18:09.
- 11-05: B3 g2 (6 agent sonnet theo chunk), shader thật (GLSL dump), regress 11/11 Win, đổi ưu tiên sang đồ họa/Boot/Home/popup; tới Home -> Gameplay -> Win -> Continue lên lv231 (đêm).
- 11-06: vòng thua, powerups, ContentLock, EventSystem, VFX/UI anim; "Play test Unity xong 100%" cho việc 5 (~11h45). Ước tính user báo ~55% so với mốc "bản test team" (HO #8). Còn: feature lv140–2700 (lớn nhất ~13%), lobby UI, build Android + test GUI. Tổng ~5 ngày làm liên tục, chủ yếu agent.

## 2) Cái hiệu quả
1. **Probe Play mode headless + chấm điểm bằng log** (WORK Assets/Editor/PixelFlowPlayProbe.cs, tools/regress.py): tự tap, trace pixels/ammo/state, phát hiện NRE theo vị trí. Cho vòng phản hồi khách quan "level Win/FAIL" thay vì đoán (HO 11-03/11-04, probe 017→059).
2. **Hồi quy sau mỗi đợt**: danh sách level đại diện cho từng feature (74/8/14/40/55/100/120/140/171/200/230) + FEATURE_CHECKLIST.md thống kê feature theo 2700 level (PROJ/docs/FEATURE_CHECKLIST.md). Test 30–40 level phủ feature thay vì 2700.
3. **Native trước, đoán sau**: mọi sửa ghi UNVERIFIED nếu không có pseudocode; brief agent cấm "viết bản có vẻ đúng" (WORK/tools/b3/prompt_template.txt). Vài chỗ agent đoán vẫn lọt (AmmoOptions {10,20,40}, HO 11-05).
4. **Điền máy hàng loạt, có xác minh ARM**: fill_event_stubs (508 hàm), fill_event_accessors (865), fill_null_getters2 (344 getter `=> null` đã kiểm `ldr r0,[r0,#off]; bx lr`), 869 field `readonly List<>` thêm `= new()` (HO 11-03 chốt). Rẻ hơn dịch tay hàng chục nghìn hàm tầm thường.
5. **Thay thư viện bên thứ 3 bằng bản thật giữ GUID** (UniTask, DOTween, TMP, UIExtensions, AnimationSequencer, SceneSystem, Cinemachine 3.1.6, DOTween Pro DLL): gấp nhiều lần dịch tay (HO 11-04 B2, WORK/libs/stage2/README.md).
6. **Giữ shader GUID khi thay dummy bằng shader thật**; dump GLSL từ APK (WORK/shaders/) cho agent viết lại HLSL; so với video gốc (YouTube playlist user gửi) để biết sai màu (HO 11-05).
7. **Báo cáo mỗi agent có dòng đầu `Tiến độ: X/Y` + backup riêng** (backup/b3/<nhóm>/): agent chết giữa chừng thì giao lại đúng phần còn (HO #8, 11-05 "4 agent bị ngắt").
8. Play test bằng "chuột thật" (PIXELFLOW_BOOT_REALCLICK) lộ lỗi mà click giả bỏ qua: EventSystemLoader stub (HO 11-06 ~11h).

## 3) Cái chậm / lãng phí + nguyên nhân
1. **Làm theo lớp chức năng "lần theo NRE" trước khi kiểm kê** (11-03 → 11-04 17h): mỗi probe lộ 1 stub mới (GetOrAddComponent, DOTween stub, DataList, PersistentVar, Condition.Get trả false...), dịch từng cụm. User phải dừng để đổi cách làm. Nguyên nhân: không đo stub toàn project từ đầu. HO 11-04 17h.
2. **Lõi reactive bị stub làm hỏng nhiều thứ ở xa**: shooter luôn xanh + "999" đạn thật ra do `PlainVar/PlainEvent` Register rỗng; mất cả buổi chiều 11-05 theo dõi shader/renderer sai (HO 11-05 ~16h30). Bài học ghi sẵn: "kiểm stub ở thư viện lõi (Crescive.*) trước khi dịch module ngọn".
3. **Chẩn đoán sai từ công cụ đo**: "bàn chơi trống" là lỗi probe chụp ảnh (camera tạm clearFlags=Nothing nhưng URP vẫn clear) — nhiều checkpoint (Codex b6, Claude cm2) kết luận sai về Cinemachine/level (HO "CHECKPOINT Claude 2026-11-05 khuya").
4. **Quota/limit làm đứt việc**: Codex hết 11-04 18:09 giữa runner, save chưa restore; Claude 5h tới 95% nhiều lần; agent chết do limit/auth (HO 11-05, #9). Codex luna dịch 121 lớp Data ctor thành 0/121 (chỉ đổi Clone), phải giao lại gpt-6.1-sol (HO 11-04).
5. **Probe lần đầu hồi quy sai**: runner chạy mỗi level 1 process Unity (~30s mở + compile 5–10 phút nếu .cs đổi). Sửa .cs khi probe chạy -> compile chồng, kết quả rác (HO 11-04 "Không sửa .cs khi probe đang chạy").
6. **Agent đồng thời sửa file chung/compile dở**: regress mở Unity lúc agent navtimers sửa dở -> lỗi compile CountdownTimerController (HO #8 r1106b). Có agent báo "xong" nhưng không sửa file nào (svc/dreamteck-b 11-04, phải giao lại).
7. **Stub tiện ích nền ẩn**: `AutoTriggerMethodExtensions.Has*` trả false làm mọi component trigger Awake/OnEnable/Start sai (chữ đè nhau ở màn thua); `CallWithDelay`, `PersistentVar.Value` setter không RaiseEvents gây "kẹt popup" (HO #9). 802 hàm extension rỗng phải liệt kê riêng (WORK/reports/stub-extensions-1106.txt).
8. **Kiểm thử trên GUI chậm**: Editor Linux xuống 0.5–1 FPS khi mất focus -> regress dài phải chạy CLI; GUI chỉ để xem hình (HO 11-05).
9. Ghidra: đôi lúc kẹt hàng chục phút ở 1 hàm; hai Ghidra cùng DB không được (HO "Cách tiếp tục").
10. Asset lõi rỗng: 38 MonoBehaviour AssetRipper xuất RỖNG (layout lỗi) -> thắng không có thưởng, Continue không về Home; phải viết tools/recover_mb.py đọc raw bundle (HO #6).

## 4) Thay đổi quy trình người dùng yêu cầu
- **2026-11-04 ~17h**: dừng kiểu "sửa theo lỗi" (probe -> NRE -> sửa), yêu cầu phân tích lại; sau đó chốt: mục tiêu = full source Unity để reskin lên store; bản test team giống ~100% game gốc trừ live-ops/ads/IAP; cắt live-ops; chỉ dùng asset trả phí có trong Documents/plugin (HO dòng 283–287).
- Luồng mới **B1 kiểm kê -> B2 thay thư viện hàng loạt -> B3 trích native hàng loạt + dịch song song theo namespace, mỗi đợt compile Unity 1 lần -> B4 bộ level phủ feature + runner nhiều level/1 phiên -> B5 Boot/Home/UI/VFX/audio -> test toàn game**.
- Kết quả B1: 11.600 hàm cần dịch, 12.300 cắt, ~25k thư viện bên thứ 3 (cài bản thật), ~5k SDK no-op (inventory-b1.md).
- 11-05 ~12h: "làm ĐỒ HỌA/màu/popup/luồng trước, gác feature 20–36" (PROJ/docs/NEXT_SESSION_PROMPT.md). 11-06: user chốt thứ tự 1(vòng thua)→3→4→5→2→6(gỡ debug)→7(build Android) và "làm liên tục, không dừng" (HO #8, #9).
- Quy tắc ràng buộc: tối đa 2–3 agent song song (memory limit-parallel-agents); không chạy Unity khi agent/Codex sửa C#; không chạy 2 Unity trên 1 project; không git (SVN); sau việc lớn phải **tự play test bằng chuột thật rồi mới bàn giao** (HO 11-06 ~11h: "user muốn mình tự mở Unity test như người chơi thật").
- Chỉ cài Android Build Support + Switch Platform khi làm bản test (đổi platform reimport toàn bộ) (HO 11-05).
- Lưu ý: lúc đổi chiến lược user cũng chọn từng nhánh cho phụ thuộc: Dreamteck (dịch native), DOTween (free gốc + Pro dịch), sau đổi sang DOTween Pro DLL thật khi user thêm plugin_v2 (libs/stage2/README.md).

## 5) Công cụ mới (WORK/tools)
- asm_check.sh `<Assembly> [out]`: build offline 1 assembly bằng .rsp của Unity (Bee), không chạm Library; kiểm cú pháp nhanh thay vì mở Unity. Hạn chế: file .cs mới chưa nằm trong rsp.
- regress.py `<tag> [levels]`: mỗi level 1 process Unity, backup/restore save + SHA-256, report reports/regress-<tag>.md. regress_queue.py: nhiều level/1 phiên (cần ép domain reload). boot_run.py `<tag> [giây] [clicks]`: chạy Bootloader->Home->Gameplay có đồ họa, chụp ảnh, click theo tên nút (+realClick), restore save.
- stub_scan.py `[--depth N] <prefab|scene>`: đệ quy theo GUID liệt kê script thật sự dùng + số thân stub theo assembly. Dùng để chọn phạm vi dịch.
- tools/b3/run_b3.sh + prompt_template.txt + b3_coverage.py: pipeline Ghidra headless trích 31.122 hàm/5.428 lớp (6 nhóm) -> .named.c -> Codex/sonnet dịch theo chunk, có kiểm coverage và fail-fast.
- name_c.py (+ ResolveVeneers.java): thay `func_0x…` bằng tên hàm trong pseudocode; recover_mb.py (UnityPy raw bundle -> viết lại .asset rỗng theo thứ tự field); unity_fileid.py (fileID MonoBehaviour từ MD4 namespace+class, để remap script sang DLL thật); fill_event_stubs/fill_event_accessors/fill_null_getters2.py (điền máy, ARM-verified); stub_extensions list; verify_b2_codex.py (runner 11 level của Codex).
- Probe C#: Assets/Editor/PixelFlowPlayProbe.cs, PixelFlowBootProbe.cs, PixelFlowLocalizationSetup.cs (đăng ký 405 bảng Addressables). Env: PIXELFLOW_PROBE_LEVEL, _SHOT, _BADPLAY, PIXELFLOW_BOOT_AUTOPLAY, _REALCLICK, PIXELFLOW_GRAPHICS.
- Unity MCP (com.coplaydev.unity-mcp 10.2.0 chép vào WORK/tools/packages, HTTP 8080) để điều khiển Editor.

## 6) Bẫy kỹ thuật tổng quát (áp dụng game khác)
- **Stub "return default" không nổ mà âm thầm sai**: getter `=> null`, event add/remove rỗng, ctor rỗng (collection null), `readonly List` không khởi tạo, Clone trả null, `Condition.Get` trả false -> game "chạy" nhưng không thắng/thua. Quét stub bằng regex `\)\s*\{\s*\}` và `return default|null|false`.
- Lõi reactive/DI/Var/Event/MonoSingleton/Finder/Extensions phải dịch TRƯỚC; đa số lỗi "xa" bắt nguồn từ đây.
- Finder/ServiceLocator khởi tạo ở Boot: probe mở thẳng Gameplay sẽ NRE giả. Dễ bị lẫn lỗi game thật.
- Project tắt domain reload: static state/ctor không chạy lại giữa 2 lần Play -> probe/queue treo (HO 11-05 cli1105b).
- AssetRipper: material dùng 923/1052 shader dummy -> màn trắng; 38 MonoBehaviour layout lỗi; prefab mất ref cross-bundle (fileID 0) cần gán tay (Powerups.prefab); file `Circle/circle` casing Windows vs Linux; video H.264 không import trên Unity Linux -> đổi VP8 .webm giữ GUID.
- Unity Linux: Editor mất focus còn 0.5–1 FPS; batch mà không thoát để lại Temp/UnityLockfile; `pgrep -f 'Editor/Unity '` khớp chính lệnh; dừng giữa chừng có thể không restore save (so SHA-256 backup/boot-save-<tag>).
- Chụp ảnh probe bằng camera overlay có thể xoá thế giới 3D (URP); dùng RT trong suốt rồi ghép CPU.
- Save: sửa file save KHÔNG đổi level khi boot (nguồn level khác); dùng env PROBE_LEVEL hoặc LevelNumbersProvider.
- Package registry sai/treo (Burst 1.8.20 575MB) -> dùng local; Cinemachine bản registry khác private layout so với dump nhưng 3.1.6 chạy được.
- Dummy asmdef stub: phải thêm reference khi dịch (Crescive.* thiếu ref là lỗi compile hay gặp; ghi trong từng port report).
- ARM Ghidra pseudocode: vtable +0x150 = Detach; offset field map theo thứ tự khai báo kèm base class; coroutine `<X>d__N` -> async/IEnumerator; Ghidra cắt cụt ctor/finally -> nghi ngờ phải đối chiếu asm.
- `grep` là ugrep alias; `grep -P` trên bảng tên lớn treo -> dùng command grep / Python.

## Plugin / thư viện bên thứ 3
- Nguồn trả phí user cấp: Documents/plugin và plugin_decompile_game/plugin_v2 (HO 11-04, libs/stage2/README.md). Phân loại: official-local-copy (Coffee UIEffect 5.10.9, UIParticle 3.3.14, DOTween Pro 1.0.430) vs reconstructed-partial (UIExtensions, NiceVibrations: còn stub, không dùng).
- Quy trình: stage ở WORK/libs/stage/<Asm>/ + guidmap.tsv (giữ GUID asmdef và GUID script cũ), backup stub vào WORK/backup/b2-stubs/, cài một lượt, compile 1 lần. Thư viện mã nguồn mở -> tải ĐÚNG version khớp type trong dump (AnimationSequencer 0.5.4 khớp 44 type, UIExtensions 2.3.2 khớp 186 type; 3.0.0 thiếu type game dùng).
- Không có bản thật (Dreamteck, Obi, TrueShadow, Shapes, RayFire, SuperScrollView, Odin): dịch phần dùng từ native (Dreamteck 748 hàm; TrueShadow 28 lớp + 9 shader; UIParticle 24 lớp dù bản 3.3.14 bị loại vì thiếu field serialize). Shapes 0 component -> bỏ.
- Khi đã nhập DLL thật thì fileID script = MD4 -> unity_fileid.py.

## Subagent / Codex
- Codex gpt-6.1-sol (low) dịch tốt các đợt 20–120 lớp (feat1/2/3, cond 73/73, datactor 121/121); gpt-6-luna fail 0/121. Hết quota 5h/tuần nhanh (tuần 91% ngày 11-05); khi đó chuyển hết sang sonnet (HO 11-05).
- Sonnet agent: chunk theo namespace với danh sách file (tools/b3/*-files.txt), 2–3 agent/lần, mỗi agent có backup+report riêng, KHÔNG chạy Unity. Main session chạy compile + probe. Các agent hay chết vì limit/auth: bắt buộc dòng Tiến độ để resume.
- Gemini CLI: login cá nhân bị chặn, chỉ API key (HO #9).

## 7) Đề xuất cho quy trình chung
1. Bước 0 bắt buộc: **đo stub + kiểm kê** (stub_scan, inventory) và phân loại 4 nhóm: thư viện thật / SDK no-op / cắt / dịch, TRƯỚC khi dịch gì. Hỏi user 3 câu ngay đầu: mục tiêu cuối, license plugin, cắt live-ops đến đâu.
2. Thứ tự dịch: lõi (Var/Event/Singleton/Extensions/Finder/Conditions/DataService) -> thư viện bên thứ 3 thật -> gameplay -> Boot/Home/UI -> feature -> VFX/audio. Đừng đi theo NRE ngọn.
3. Dựng sớm 3 công cụ: asm_check (offline compile), probe headless + regress có backup/restore save, boot_run (đường Boot->Home->Gameplay có ảnh). Chạy hồi quy mỗi đợt, compile Unity đúng 1 lần/đợt.
4. Điền máy các mẫu stub lặp (event, getter, collection init) bằng script có kiểm ARM; ghi danh sách + backup.
5. Mọi giá trị đoán phải gắn UNVERIFIED trong report; brief agent cấm đoán; gom báo cáo vào 1 chỗ (reports/<chunk>-port.md với dòng Tiến độ).
6. Đo bằng công cụ phải được kiểm chứng trước khi tin (ảnh probe, cờ env); kết luận "lỗi game" chỉ sau khi loại lỗi công cụ.
7. Chia tài nguyên: Codex cho đợt lớn tuần tự (có prompt template), sonnet song song ≤3 chunk không trùng file, main giữ compile/test; đặt checkpoint HANDOFF ngay khi quota ~90%.
8. Play test bằng input thật (chuột, EventSystem) và so với video/ảnh gốc trước khi bàn giao; checklist feature theo dữ liệu level (FEATURE_CHECKLIST.md) thay vì "đã compile".
