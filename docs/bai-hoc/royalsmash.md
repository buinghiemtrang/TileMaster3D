# Bài học Royal Smash (Linux 1.18.737 vs gói ver3 Windows 1.16.477)

Viết tắt nguồn: [H]=RoyalSmash_1.18.737/00_SESSION_HANDOFF.md; [T]=RoyalSmash_1.18.737/tools/*; [D0]=ver3/DOC-TRUOC.md; [Q]=ver3/docs/quy-trinh-dung-lai-game.md; [W]=ver3/docs/workflow-apk-to-unity.md; [P]=SmashProto/HANDOFF.md hoặc AGENTS.md. Gốc: `/mnt/data/BuiNghiemTrang_Works/Project/Unity/` (Documents/RoyalSmash_1.18.737, Documents/GetSrcGame/quy-trinh-dung-lai-game-unity-ver3, unity_game/game_decompile/game_unity/SmashProto).

## 1) Tóm tắt + timeline

Hai việc khác mục tiêu:
- **Linux 1.18.737 (ARM64, Unity IL2CPP): nghiên cứu cơ chế**, KHÔNG dựng lại game. Mục tiêu "học cơ chế để làm game khác tương tự, không dùng code/asset của họ" [H dòng đầu]. Chỉ dịch ~2.500 hàm theo nhóm cơ chế rồi viết tài liệu + code C# mới.
- **Windows 1.16.477 (ARMv7), gói ver3: dựng lại gần y hệt** (~18.600 hàm code game, "sau nhiều tuần vẫn đang dịch dần") [Q §1].

Timeline Linux (mốc từ [H], [P], mtime file; mtime của decomp/tools là lúc copy nên chỉ tin mtime các file .md):
- 2026-10-27 ~16:35: có 3 APK (base, arm64 config, UnityDataAssetPack) kéo từ máy Samsung [H §3].
- 10-27 17:27 → 20:26: viết xong 8 tài liệu 01..08 (mtime .md; 01 lúc 17:27, 08 lúc 20:26). Các nhóm decomp 01..07 lấy theo thứ tự cơ chế (shoot_ball, obstacle, level_data, boosters, special, meta, foundation). Tức khoảng 1 buổi chiều/tối sau khi hạ tầng xong. Phần dựng hạ tầng (APS2, Ghidra project, signatures) trước 17:27, thời gian cụ thể KHÔNG có ghi lại.
- 10-28: bắt đầu prototype SmashProto; cùng ngày bot qua 20 màn, import art RS tạm, dựng Splash/MainMenu/Cannon theo video [P §1, 1b, 1c]. Dừng live-ops theo chọn của user [H §2].

Timeline ver3 (changelog cuối [W] "Lịch sử thay đổi quy trình"): 2026-10-08 tạo tài liệu (việc dựng lại đã chạy trước đó), 10-08..10-09 làm pha 6 hàng loạt + decompile toàn bộ .so qua đêm, 10-11/12 các nhóm Blocker/Booster/Tween, 10-31..11-01 các checkpoint M09–M14 và lô 012–022 (Sirenix/Odin), 11-02 thêm xu-ly-code-bi-mat, 11-06 gộp ver1+ver2 thành ver3. Changelog KHÔNG ghi giờ/ngày công theo pha; chỉ suy được theo ngày và số lô (xem mục 3).

## 2) Cái hiệu quả (Linux)

1. **Giữ `-noanalysis`, chỉ decompile theo mục tiêu.** Auto-analysis toàn phần chạy >65 phút chưa xong [H §5]; thay bằng đặt tên 120k hàm + áp chữ ký/struct + 31k hàm đích gọi, mỗi nhóm decompile ~1–3 phút [H §3, §5; T/ghidra/CreateCallTargets.java].
2. **Chọn hàm theo seeds.tsv bằng awk theo tên lớp** → targets.tsv → 1 lệnh analyzeHeadless [H §5; T/seeds.tsv]. Nhóm hàm theo cơ chế, không theo toàn bộ namespace.
3. **MarkNoReturnTargeted.java chạy trước decompile**: nếu không, decompiler đọc lấn sang hàm khác, ra pseudocode hàng nghìn dòng [H §5; T/ghidra/MarkNoReturnTargeted.java header]. Đây là đối trọng của bước "định nghĩa helper runtime" bên ver3 nhưng không cần biết địa chỉ helper từng bản build.
4. **Áp chữ ký + struct từ metadata** (`gen_sigs.py` → sigs.tsv, structs.tsv, `ApplySigsAndDecompile.java`): pseudocode có tên field/param thay vì offset trần [T/gen_sigs.py header; T/field_offsets.py].
5. **`unpack_aps2.py` (Python thuần) tạo `libil2cpp_reloc.so`** dùng chung cho REA, Cpp2IL, AssetRipper [H §5; T/unpack_aps2.py header: áp RELATIVE = base0+addend, S+A cho symbol định nghĩa, vô hiệu DT_ANDROID_RELA].
6. **REA (`inspect_il2cpp_metadata`) cho metadata** → `il2cpp.json`, `il2cpp_bin_ev2.json`; `ns.py`, `cls.py`, `mem.py` đọc JSON để liệt kê namespace/lớp/member (quy mô 3.900 type / 23.000 method code studio [01_Gameplay_Map.md; T/ns.py]). Cpp2IL bị chặn tải nên bỏ [H §4].
7. **AssetRipper headless để lấy số liệu serialize** (prefab, scene, SO, level JSON 2.380 file): `--headless --port 8765`, `POST /LoadFolder`, `POST /Export/UnityProject`, field `Path` [H §5]. Tài liệu cơ chế lấy hằng số từ asset, công thức từ pseudocode, đánh dấu *(suy luận)* [02_Mechanic_Shoot_Ball.md mục Nguồn/Mức độ tin cậy].
8. **`tools/state/` giữ lại toàn bộ trạng thái** (project Ghidra, .so reloc, JSON) để phiên sau tiếp tục không dựng lại [H §3].
9. **Tách đích**: tài liệu 01..08 có C# viết mới + HANDOFF "mở session đọc file này trước, không cần đọc lại pseudocode" [H dòng 8; P dòng 5]. Prototype dùng handoff thay vì đọc lại nguồn.
10. **Vòng prototype**: bot test trong Unity qua 20 màn ngay lần đầu, Codex review tìm 2 bug thật trong SmashLevel, khóa Editor bằng edlock.sh khi nhiều agent [P §1, 1c; AGENTS.md "Phối hợp hai agent"].

## 3) Cái chậm / lãng phí + nguyên nhân

Linux:
- Auto-analysis Ghidra >65 phút không xong [H §5] -> đã bỏ. Bài học: không bao giờ chạy lại.
- Không có `show.sh`/lọc nhiễu thì pseudocode ngập khai báo biến; `tools/show.sh` lọc dòng `undefined/float/int...;` và WARNING [T/show.sh].
- Cpp2IL bị hệ thống chặn tải -> phải đi vòng bằng REA + script tự viết (gen_sigs, field_offsets), tốn công viết tool [H §4].
- AssetRipper cần gộp 3 APK vào một thư mục + thay libil2cpp.so bằng bản reloc, nếu không script/field hỏng [H §5].
- Prototype: 1280×720 `game_view` bị méo, phải dùng chụp 1080×1920 [P §1c]; scratchpad chứa script test (bot.sh, shot.sh, edlock.sh) "có thể mất" -> mất công dựng lại [P §2].
- Không ghi giờ thực từng bước; mất cơ hội đo -> đề xuất ghi mốc thời gian vào handoff.

ver3 (suy từ changelog, [W] cuối):
- **Dịch lại thư viện bên thứ ba lớn**: lô 012–022 ngày 2026-11-01 là Sirenix/Odin (serializer, ImmutableList, TypeExtensions, DeepReflection, JSON...). 10 mục changelog trong MỘT ngày cho 11 lô, mỗi lô 150–200 hàng, "direct 22–42 checks", "ghi hai giới hạn rgctx, dừng sau Sirenix". Đây là chỗ tốn nhất theo số lượng mục lục nhưng ít giá trị gameplay. Trước đó M09–M14 (~71–423 hàng/checkpoint, SDK/DTO mock) cũng là helper/SDK boundary [W changelog 2026-10-31..11-01].
- **Vòng phản ứng (reactive loop)**: hầu hết dòng changelog 10-09/10-11/10-12 bắt đầu bằng "Thêm bẫy ..." sau khi người dùng Play thấy sai (level5 tự đổ, popup không hiện, khung 9-slice lệch, menu bật sớm sau Continue). Cả chuỗi 10-09 xoay quanh một level5 tự đổ qua 5–6 vòng (collider bake, randomYaw enum sai, Space.World/Self) [W changelog 2026-10-09].
- **Mỗi lô cần compile + validator riêng + "main"**, người dùng phải đổi nhịp: main chỉ ở lô chia hết cho 10 [W changelog 2026-11-01 cuối].
- **Decompile toàn bộ .so (343.323 hàm)** chạy 4–5 tiến trình Ghidra song song cả đêm, mỗi bản copy project phình 1,3 GB; chỉ cần ~18.600 hàm game [Q §4, W pha 6 bước 8]. Phần lớn không cần.
- Windows riêng: MAX_PATH 260, ký tự cấm trong tên file (`<>|:?*"`), `.bat` treo chờ phím, JAVA_HOME `\`, NotOwnerException khi chép project Ghidra, UnicodeEncodeError cp1252 [W phụ lục A dòng ~381–396]. Toàn bộ là chi phí do nền tảng.

## 4) Pha ver3 + "Xong khi" (ngắn) [Q §4–6; W các mục "## Pha N"]

| Pha | Việc | Xong khi |
|---|---|---|
| 0 Nhận diện | Giải nén XAPK, xác định Unity/IL2CPP/Mono, version, Addressables | Bảng tham số điền đủ, đã quyết IL2CPP/Mono |
| 1 AssetRipper | Xuất resource (ScriptExportMode Decompiled, Shader Dummy, Sprite Yaml...) | Có scene, prefab; git lfs ls-files không rỗng |
| 2 Il2CppDumper | dump.cs, script.json, stringliteral | Tra được RVA class game bằng grep |
| 3 Compile sạch | Thay stub package bằng package thật, remap GUID, fix_decompile | `grep -c 'error CS'` = 0, không Missing Script hàng loạt |
| 4 Shader | extract_shaders, remap về URP/TMP, viết lại từ GLSL | Không còn DummyShaderTextExporter, menu hiển thị đúng |
| 5 Resource mất | fix_sprite_atlas, fix_packed_normals, entry Addressables từ catalog.bin | Hết ô trắng sprite, Play không InvalidKeyException |
| 6 Pseudo-C Ghidra | Import -noanalysis, đặt tên hàm, define helper, DecompileBulk, postprocess | Class đích không còn `unreachable`, đọc từ đầu tới return |
| 7 Dịch class | Foundation -> startup -> gameplay -> UI -> LiveOps; đối chiếu ARM | (không có "Xong khi" cụ thể; theo checklist mục 9 [Q]) |
| 8 Kiểm tra | Validator Play-mode + ảnh + so APK gốc | Báo rõ mỗi cơ chế qua tầng nào |

## 5) Điểm yếu của ver3

1. **Quá dài**: workflow-apk-to-unity.md = 160 KB, 677 dòng, 111 mục "- **" ở 7.4 và nhiều dòng >1500 ký tự (5 dòng); phụ lục A/B, changelog 100+ dòng. AI không đọc "hết một lượt" như [W §0 bước 1] yêu cầu được. Gói tóm tắt [Q] 19,6 KB cũng chưa ngắn.
2. **Gắn với Windows**: `D:\Tools\dex2jar`, `~/Desktop/royal-smash`, `.bat`, PowerShell, MAX_PATH; chính changelog thừa nhận ver2 ghi cứng `D:\Dev\royal-smash-work` lệch với DOC-TRUOC [W changelog 2026-11-06]. "Tên file full-width + restore_names.py" chỉ để sống với Windows [W phụ lục B].
3. **ARMv7 cứng**: image base `0x10000`, `ldr pc`, PIC, literal pool, `vcvt` ARM, `arm_dis.py` dùng cộng/trừ 0x10000 [Q §4, W 7.4]. DOC-TRUOC tự nhận ARM64 "chưa chạy thật" [D0 "Những phần chưa từng chạy"] -> Linux ARM64 là phần mới, mọi bẫy PIC/literal-pool/RVA cần kiểm lại.
4. **Vòng phản ứng**: bẫy thêm sau khi lỗi xuất hiện (changelog 10-09 dài), không có bước chủ động "audit trước" (stub_scan mới sinh sau). Kiểm chứng cuối vẫn dựa người dùng Play/chụp ảnh [W phụ lục D].
5. **Mục tiêu "giống y hệt" không có điều kiện dừng**: pha 7 không có "Xong khi"; phạm vi lan sang thư viện bên thứ ba (Sirenix/Odin, JobTween, collections, JSON) và SDK boundary, mỗi lô 150–200 hàng. Cần quy tắc "mock hoặc dùng package thật nếu có thể" (xem mục 8).
6. **Trộn "đã kiểm chứng" và "đề xuất"** — đã được xử lý ở xu-ly-code-bi-mat.md bằng nhãn, nhưng workflow chính vẫn chứa nhãn rải rác [chưa kiểm chứng] (Mono, Frida, AssetRipper headless — mà Linux đã chạy được AssetRipper headless).
7. **Tên "Royal Smash" gắn khắp**: menu "Royal Smash/...", validator chỉ dùng được cho game đó (mau-tham-khao), nhiều ví dụ phải điền lại [D0].
8. **Nhiều bước từng cần nay có đường ngắn hơn**: bước 2 pha 6 (tự xác định helper IL2CPP, veneer, helper_names.tsv) thay được bằng cách đánh dấu noreturn mục tiêu + áp chữ ký (cách Linux) mà không cần định nghĩa tay từng helper — chưa kiểm cho ARM64 nhưng đã chạy cho 2.500 hàm Linux [H §5, W pha 6].
9. **Quy mô đa agent**: [W 7.4 dòng ~314] "chạy nhiều agent song song trên cùng working tree: chia theo file, cấm compile, người điều phối compile một lần" — phát hiện sau khi gộp bị lỗi, chưa có cơ chế khóa Editor (SmashProto có edlock.sh).

## 6) Công cụ (path + mục đích)

Linux `RoyalSmash_1.18.737/tools/`:
- `unpack_aps2.py`: áp APS2 packed relocation (ELF64 aarch64) -> libil2cpp_reloc.so.
- `gen_sigs.py`: metadata + .so reloc + evidence REA -> sigs.tsv, structs.tsv (Ghidra).
- `field_offsets.py` (+ `field_offsets.json`): offset field từ MetadataRegistration.
- `ghidra/MarkNoReturnTargeted.java`: đánh dấu noreturn từ hàm đích, trim body; `OnlyNoReturnAnalysis.java`: pre-script tắt mọi analyzer trừ noreturn; `CreateCallTargets.java`: tạo hàm ở mọi call target; `FixBodies.java`: tách code bị hút vào qua tail branch; `ApplySigsAndDecompile.java`: struct + chữ ký + decompile; `SeedAndDecompile.java`: seed + decompile.
- `show.sh`: in pseudocode đã lọc nhiễu; `seeds.tsv`, `sigs.tsv`, `structs.tsv`: bảng hàm/chữ ký/struct.
- `ns.py`, `cls.py`, `mem.py`: duyệt metadata JSON; `foundation_overview.py`, `level_stats.py`, `obstacle_table.py`: tổng hợp số liệu.
- Cài (không cần sudo) [H §4]: Node 24 + `rea` 6.8.0 (MCP), JDK 21 Temurin, JADX 0.7.1, Apktool 3.0.3, Ghidra 12.1.4 (~/.local/share/ghidra), AssetRipper 2.0.0 (~/.local/share/assetripper/AssetRipper.GUI.Free).

ver3 `Tools/reverse/` (generic trừ ghi chú): `scan_scripts.py` (guid -> class), `remap_guids.py`, `fix_decompile.py`, `fix_asmdef_refs.py`, `compile_batch.sh`, `mirror_compile.sh` (bản sao project để compile song song), `run_editor_method.sh`, `extract_shaders.py`/`remap_shaders.py` (cần UNITY_VERSION), `fix_sprite_atlas.py`, `fix_packed_normals.py`, `addressables_entries.py` + `Assets/Editor/*Addressables*`, `stub_index.py`/`stub_scan.py` (liệt kê method còn rỗng), `ghidra/{DecompileBulk.java,bulk_postprocess.py,make_bulk_targets.py,ApplyIl2cppNames.java,DefineHelpers.java,TailCalls.java,tailcall_audit.py,lsda.py,arm_dis.py,read_floats.sh,litann.py}` (nhiều cái ARMv7-specific). Validator mẫu `mau-tham-khao/validators-royal-smash/*` không chép sang game khác.

## 7) Bẫy kỹ thuật tổng quát (không phụ thuộc game)

- IL2CPP: decompiler mất tail call (`b hàm` cuối), mất try/catch (cần LSDA), mất float/vector, đảo thứ tự cộng, gộp hai `if` thành `if/else`, hàm kế bên chép đuôi vào hàm ngắn [Q §7].
- Stub `=> null`/`return default` trông như đã dịch; `.cctor`/`.ctor` bị bỏ làm sai static/list null [Q §7].
- Hàm không return/PIC khiến Ghidra đọc lấn: luôn đánh noreturn trước [H §5 Linux; Q §7 ver3 qua helper].
- Pseudo-C trên Linux dùng tên namespace khác tên assembly: `Services.Service`, `Foundation.DependencyInjection.*`, `Foundation.Events.*`, `Foundation.SaveSystem.*` ... [H §5].
- Cờ static method: đọc từ global-metadata.dat, mỗi method 36 byte, flags ở +28 [H §5; T/gen_sigs.py].
- libil2cpp.so có thể dùng APS2 packed relocation: REA/Cpp2IL/AssetRipper cần bản đã áp reloc [H §5].
- AssetRipper: script chỉ là stub, shader dummy, SpriteAtlas mất, normal map AG xuất sai, `.wav` đổi thành `.ogg` làm lệch Addressables [Q §4; W 7.4 dòng ~317].
- Hằng số float: đọc trực tiếp từ binary; ARM `vcvt` NaN -> 0 khác CLR [W 7.4 2026-10-11].
- Phân biệt "compile sạch / validator PASS" và "giống game gốc"; ghi tầng kiểm [Q §6].
- Thư viện "bên thứ ba" cũng có thể là stub; so sánh trần trước khi sửa code mình [Q §7].
- Đồng hồ batch Unity khác thật; kiểm bằng số frame [Q §7]. Không chạy Unity batch khi Editor đang mở [D0].
- Google/CDN: asset chỉ có trên CDN không có trong APK -> mock, ghi deferred [Q §7b].

## 8) Đề xuất (cho workflow mới, ngắn gọn, ưu tiên Linux/ARM64)

1. **Tách theo mục tiêu ngay pha 0**: (A) nghiên cứu cơ chế (Linux: APS2 + noreturn + seeds, không cần compile project) hay (B) dựng lại giống y hệt (ver3). Hai lộ trình có chi phí lệch 10 lần; đừng trộn.
2. **Một file ngắn (<=10–15 KB) làm cổng vào**, phần chi tiết/bảng lỗi để theo chủ đề, nạp theo nhu cầu (skill nhỏ + references). Bỏ nhật ký theo lô khỏi tài liệu quy trình; để ở CLAUDE.md/DEV_LOG.
3. **Chuẩn hóa pha 6 theo cách Linux**: `unpack_aps2` (nếu .so có APS2) -> REA metadata -> `gen_sigs`/`field_offsets` -> Ghidra project `-noanalysis` + CreateCallTargets -> `MarkNoReturnTargeted` -> `ApplySigsAndDecompile` theo targets từ seeds.tsv. Giữ DefineHelpers/veneer chỉ như phương án dự phòng khi pseudo-C vẫn cụt.
4. **Giới hạn phạm vi dịch**: tiêu chí "dịch / dùng package thật / mock" cho từng nhóm; tuyệt đối không dịch lại Sirenix/Odin/JSON/collection nếu có package; ghi tiêu chí dừng pha 7 (ví dụ: luồng boot -> menu -> level 1..N -> thắng/thua PASS bằng validator + ảnh so video).
5. **Chủ động thay phản ứng**: sau mỗi nhóm chạy `stub_scan`/audit tail call/getter trả hằng/field init `.ctor` trước khi nhờ người dùng Play; có sẵn bảng bẫy dạng checklist (mục 7) thay vì 70 bẫy dài.
6. **Ghi thời gian/số hàm theo pha** vào handoff (giờ bắt đầu/kết thúc, số hàm, số lô) để lần sau ước lượng được; hiện cả hai nơi đều thiếu.
7. **Tách Windows ra phụ lục**; ARM64 nâng thành mặc định; kiểm lại `arm_dis.py`, literal-pool, base address (ELF linked at 0 trên Linux/Ghidra seed base được truyền vào `gen_sigs.py`) cho ARM64 rồi đổi nhãn "chưa kiểm chứng".
8. **Đa agent**: giữ mô hình SmashProto — Claude trưởng nhóm, giao Codex theo phạm vi file, một agent điều khiển Editor, khóa edlock, tối đa 2–3 agent song song [P AGENTS.md; memory limit-parallel-agents]; chia Ghidra decompile giữa các agent bằng targets_*.tsv riêng nhưng KHÔNG chạy 2 analyzeHeadless trên cùng project (Ghidra khóa project; ver3 giải bằng copy project 1,3 GB/bản [W pha 6 bước 8]).
