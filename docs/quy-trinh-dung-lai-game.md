# Quy trình dựng lại game từ APK (bản tối ưu, ver4)

Cập nhật: 2026-11-06. Gộp từ 3 dự án đã làm thật:

| Dự án | Engine | Mục tiêu | Kết quả và thời gian | Bài học chi tiết |
| --- | --- | --- | --- | --- |
| Royal Smash 1.16.477 (Windows) + 1.18.737 (Linux) | Unity IL2CPP, ARMv7 / ARM64 | Bản 1.16: dựng lại gần y hệt. Bản 1.18: học cơ chế | 1.16: nhiều tuần, khoảng 18.600 hàm, vẫn đang dịch. 1.18: 8 tài liệu cơ chế trong 1 buổi chiều, prototype SmashProto từ hôm sau | `docs/bai-hoc/royalsmash.md` |
| PixelFlow 0.37.1 | Unity IL2CPP, ARMv7 | Có full source để reskin rồi phát hành | Khoảng 5 ngày tới ~55% mốc "bản test team" | `docs/bai-hoc/pixelflow.md` |
| Dicero! 1.7.2 | Cocos Creator 3.8.8 (JS) | Chạy toàn bộ game và có server riêng | Chơi được cả 6 chế độ sau ~1 đêm + 1 buổi | `docs/bai-hoc/dicero.md` |

Tài liệu này là **cổng vào**: các pha, quyết định và quy tắc. Lệnh chi tiết, bảng lỗi và hơn 100 bẫy nằm ở `docs/workflow-apk-to-unity.md` (chỉ dùng để tra, không cần đọc hết). Khi method không còn thân trong binary, đọc `docs/xu-ly-code-bi-mat.md`.

Mỗi mục ghi nguồn kiểm chứng: **[RS]** Royal Smash, **[PF]** PixelFlow, **[DI]** Dicero, **[TM]** Tile Master 3D (ARM64, Unity 6000.3, mới chạy thử Pha 0), **[đề xuất]** chưa chạy thật.

---

## 1. Pha 0: nhận diện và chốt mục tiêu (bắt buộc, trước mọi việc khác)

### 1.1 Mục tiêu và 3 quy tắc phạm vi cố định (ghi vào MỘT file handoff duy nhất)

Bài học [DI]: Dicero có 3 file handoff, mỗi file ghi một phạm vi khác nhau, nên agent làm sai hướng.

Chỉ hỏi người dùng **mục tiêu cuối**:
- (a) Học cơ chế để viết game mới.
- (b) Chạy được bản gốc để tham khảo hoặc test nội bộ.
- (c) Có full source để reskin và phát hành lên store.

Ba quy tắc dưới đây là **mặc định cố định** do người dùng chốt ngày 2026-11-06, không hỏi lại. Một project chỉ được đổi khi người dùng nói rõ, và phải ghi lại trong handoff của project đó.

1. **Plugin / thư viện bên thứ 3:**
   - Kiểm kê mọi plugin game dùng, đối chiếu `dump.cs` với `Documents/plugin` và `Documents/plugin_decompile_game/plugin_v2`.
   - Bản dùng được (official, đúng phiên bản, khớp type) thì **COPY** vào project. **Giữ nguyên bản gốc** trong thư mục plugin, không move, không sửa. Giữ GUID như mục C3.
   - Plugin không có bản dùng được thì **tự dịch từ native** để logic giống game gốc, nhưng **chỉ dịch phần game thực dùng** (đo bằng `used_scripts_scan.py`/`stub_scan.py`), không dịch phần thừa.
   - Trong `plugin_v2`, chỉ bản `official-local-copy` là package thật; bản `reconstructed-partial` vẫn còn stub [PF].
2. **Không cắt live-ops, quảng cáo, IAP, analytics.** Game có phần nào thì làm đủ phần đó, theo 2 bước:
   1. **Làm UI ngay trong lộ chính:** màn hình, popup, nút, luồng điều hướng giống game gốc. Lời gọi SDK đi qua interface, phía sau tạm là no-op.
   2. **Làm chức năng fake SAU KHI core gameplay xong** (ghi sẵn vào `docs/deferred-work.md` mục "Fake SDK – làm sau core"):
      - Quảng cáo: màn quảng cáo giả vài giây, rồi trả callback thành công hoặc bỏ qua giống SDK thật.
      - IAP: mua luôn thành công, cộng đúng gói hàng theo dữ liệu game.
      - Live-ops: lịch event và remote config lấy từ dữ liệu có trong APK/cache. Thứ gì chỉ có trên server hãng thì dựng dữ liệu mẫu và ghi deferred.
      - Analytics: ghi log local.
   - Tất cả đặt sau interface, để sau này thay SDK hoặc dịch vụ thật mà không sửa logic game.
3. **Server:** game cần server thì **dựng server fake chạy offline trước** để game chạy đủ luồng. **Chỉ khi game đã hoàn chỉnh** mới làm server thật dùng lâu dài. Thiết kế server thật theo mục 4 bước 8 [DI].

### 1.2 Nhận diện kỹ thuật

| Câu hỏi | Cách xem | Ảnh hưởng |
| --- | --- | --- |
| Engine | `libunity.so` + `globalgamemanagers` → Unity. `libcocos.so` + `*.jsc` → Cocos | Chọn lộ trình |
| Unity backend | `libil2cpp.so` + `global-metadata.dat` → IL2CPP. `Assembly-CSharp.dll` → Mono | Mono không phải dịch ARM |
| ABI | `lib/armeabi-v7a` hay `lib/arm64-v8a` | Chọn bộ tool pha C6 |
| Packed relocation | `readelf -d libil2cpp.so` có `ANDROID_RELA` (APS2) | Phải chạy `arm64/unpack_aps2.py` trước REA/AssetRipper [RS]. Không có APS2 nhưng có `RELA`/`RELACOUNT` thường (ô dữ liệu = 0, addend nằm trong bảng reloc): chạy `arm64/apply_rela.py` để có bản reloc cho Ghidra/`gen_sigs.py`; REA và AssetRipper đọc thẳng file gốc được [TM] |
| Bản metadata | 4 byte sau magic `AF 1B B1 FA` của `global-metadata.dat` | v39 (Unity 6000.3): Il2CppDumper 6.7.46 không đọc. REA `inspect_il2cpp_metadata` và Cpp2IL trong AssetRipper 2.0.0 đọc được, map đủ 95.400 method [TM] |
| Bản Unity / Cocos | `globalgamemanagers`, `cc.config.json` | Phải dùng đúng bản |
| Addressables / bundle | `catalog.bin`, `aa/` | Pha C5 |
| Mã hóa script | Cocos: `.jsc` = XXTEA; brute-force key bằng `strings libcocos.so` [DI] | Lộ B |

### 1.3 Chọn lộ trình nhanh nhất đạt mục tiêu

Chi phí giữa các lộ trình chênh nhau khoảng 10 lần, nên không trộn lẫn chúng [RS].

| Lộ trình | Khi nào | Thời gian thực tế |
| --- | --- | --- |
| **A. Nghiên cứu cơ chế** | Mục tiêu (a) | 1–2 ngày cho 8 nhóm cơ chế [RS] |
| **B. Chạy lại build gốc** | Code nằm trong APK ở dạng chạy lại được: JS (Cocos), Mono DLL, Lua | 1 đêm + 1 buổi tới khi chơi được hết [DI] |
| **C. Dựng lại source Unity IL2CPP** | Mục tiêu (c), hoặc (b) khi không chạy lại được bản gốc | Nhiều ngày tới nhiều tuần [PF][RS] |

Quy tắc: **luôn thử lộ B trước** nếu engine cho phép [DI]. Dicero từng giao Codex port JS→C# bằng tay, hết quota giữa chừng nên phải bỏ hướng đó. Chạy lại bản gốc thì khớp bản gốc mà không tốn công.

### 1.4 Phân tích lộ trình thật kỹ, rồi chờ duyệt

Ngay khi nhận APK và yêu cầu, **phân tích kỹ trước khi làm bất cứ pha nào khác**, vì chọn sai lộ trình tốn gấp nhiều lần. Viết báo cáo vào `HANDOFF.md`, mục "Phân tích lộ trình", gồm:

1. **Số liệu đo được, không đoán:**
   - Engine, backend, ABI, APS2, bản engine.
   - Số type và method của code game (tách khỏi thư viện), số stub sau AssetRipper (nếu đã xuất).
   - Danh sách plugin: có bản official / phải dịch / không dùng.
   - SDK có trong game: ads, IAP, live-ops, analytics.
   - Mức phụ thuộc server: game chạy offline được không, có những API nào.
2. **So sánh lộ A/B/C và các phương án lai.** Ví dụ: chạy lại build gốc để lấy hành vi tham chiếu trong khi dựng source Unity; JS chạy trong Unity bằng Jint [DI].
   - Mỗi phương án ghi: đạt mục tiêu tới đâu, thời gian ước lượng (dựa trên số liệu của 3 dự án ở đầu tài liệu), rủi ro, phần nào phải dịch tay.
3. **Đề xuất một phương án tối ưu kèm lý do**, danh sách pha cụ thể cho game này, và mốc kiểm tra đầu tiên.
4. **Dừng và chờ người dùng duyệt.** Chưa được duyệt thì không tạo project, không dịch.

**Xong khi:** handoff có mục tiêu, 3 quy tắc phạm vi, bản phân tích lộ trình đã được duyệt, bảng tham số (`PACKAGE_ID`, `ENGINE_VERSION`, `BACKEND`, `ABI`, `REPO`, `WORK`, `GHIDRA`, `EDITOR`) và lộ trình đã chọn.

---

## 2. Chuẩn bị chung

- **Thư mục** (theo skill `decompile-game-workflow`):
  - `Documents/<Game>/`: APK và `WORK` (dump, pseudo-C, log, report, backup).
  - `unity_game/game_decompile/game_unity|game_cocos/<Game>`: project đang làm (`REPO`).
  - `game_decompile/trangbn/<Game>`: bản sạch để commit SVN.
- **Biến môi trường:** `REPO`, `WORK`, `GHIDRA`, `EDITOR`, `ABI`. Mọi tool trong gói đọc biến môi trường, không ghi cứng đường dẫn.
- **Phần mềm** (Linux, cài không cần sudo [RS]):
  - Đúng bản Unity, AssetRipper 2.0.0 (chạy được headless: `--headless --port 8765`, `POST /LoadFolder`, `POST /Export/UnityProject`) [RS].
  - Il2CppDumper, Ghidra 12.x + JDK 21 (`-Xmx8G`).
  - REA MCP (`inspect_il2cpp_metadata`), Python + `UnityPy lz4 pyelftools capstone texture2ddecoder Pillow`, Node (nvm).
- **Tài nguyên:**
  - Chỉ 1 Ghidra trên 1 project Ghidra.
  - Chỉ 1 Unity Editor trên 1 project, và không chạy Unity khi agent đang sửa `.cs` [PF].
  - Tối đa 2–3 agent song song.
- **Ghi thời gian:** mỗi pha ghi giờ bắt đầu, giờ kết thúc, số hàm, số lô vào handoff. Cả 3 dự án đều thiếu số liệu này nên khó ước lượng lần sau [RS][DI].

---

## 3. Lộ C: dựng lại Unity IL2CPP (lộ chính)

Thứ tự các pha dưới đây đã sửa theo bài học PixelFlow: **kiểm kê và thay thư viện TRƯỚC khi dịch**, và **dịch lõi trước, ngọn sau**. Không đi theo vòng "probe → gặp 1 NRE → sửa 1 class". Ở PixelFlow vòng đó tốn 1,5 ngày và người dùng phải dừng lại để đổi cách làm [PF].

### C1. Xuất resource (AssetRipper)

- Cài đặt: `ScriptExportMode: Decompiled`, `ShaderExportMode: Dummy`, `BundledAssetsExportMode: GroupByBundleName`, `SpriteExportMode: Yaml`, `TextExportMode: Parse` [RS].
- Gộp mọi APK split vào một thư mục. Nếu có APS2, thay `libil2cpp.so` bằng bản đã áp reloc trước khi xuất [RS].
- Sau khi xuất, kiểm MonoBehaviour/ScriptableObject bị rỗng (layout lỗi). Dùng `bulk/recover_mb.py` đọc lại từ bundle gốc; PixelFlow có 38 asset rỗng làm thắng không có thưởng [PF].
- **Xong khi:** có scene, prefab; danh sách asset rỗng đã được khôi phục hoặc ghi vào deferred.

### C2. Metadata

- **ARMv7:** chạy Il2CppDumper → `dump.cs`, `script.json`, `stringliteral.json`, `DummyDll/` [RS][PF].
- **ARM64 có APS2:** chạy `arm64/unpack_aps2.py` → REA `inspect_il2cpp_metadata` → `arm64/gen_sigs.py` (sigs/structs) + `arm64/field_offsets.py` [RS, đã chạy cho nghiên cứu, chưa chạy cho lộ C].
- **ARM64 RELA thường, metadata v39** [TM, đã kiểm chứng ở mức chạy thử]: `arm64/apply_rela.py` (301.452 relative + 2.278 abs) → REA `inspect_il2cpp_metadata` có `binary_path` → `export_evidence_bundle` → `arm64/gen_sigs.py` (86.203 chữ ký, 9.765 struct). Ở v39 `Il2CppMethodDefinition` dài 30 byte (v31: 36), `flags` nằm ở `size - 8`; `gen_sigs.py` tự suy ra từ bảng section. Bundle xuất có 2 record (summary + đầy đủ): tool phải chọn record `operation == inspect_il2cpp_metadata`.
- **Ô metadata (TypeInfo, MethodRef, StringLiteral…) trong `.data`** [TM, một phần]: ở v27+ mỗi ô chứa sẵn giá trị mã hóa `(kiểu << 29) | (index << 1) | 1` trước khi runtime khởi tạo; kiểu 1 = TypeInfo (index vào bảng `Il2CppType` của binary), 3 = MethodDef, 4 = FieldRef, 5 = StringLiteral, 6 = MethodRef (methodSpecs). Đã giải đúng mẫu (20.291/20.304 string literal), chưa có tool đặt tên ô vào Ghidra.
- **Xong khi:** tra được RVA và offset field của class game.

### C3. Compile sạch + thay thư viện bằng bản thật (gộp B2 của PixelFlow)

1. Thay stub package Unity bằng package thật theo thứ tự: `scan_scripts.py` → xóa stub → mở Unity tải package → `remap_guids.py --apply` → lặp `fix_decompile.py`. Package registry bị treo (ví dụ Burst 575 MB) thì dùng bản local [RS][PF].
2. **Thư viện bên thứ 3**, chọn theo thứ tự [PF][RS]:
   1. Có trong thư mục plugin của người dùng (bản official) → **copy** vào project, giữ nguyên bản gốc.
   2. Mã nguồn mở → tải **đúng phiên bản** khớp với các type trong `dump.cs`. Ví dụ UIExtensions 2.3.2 khớp 186 type, còn bản 3.0.0 thiếu type.
   3. Không có cả hai → tự dịch từ native, nhưng chỉ **phần game thực dùng** (đo bằng `bulk/used_scripts_scan.py`), không dịch phần thừa.
   - SDK ads/IAP/live-ops/analytics: không dịch SDK của hãng. Tạo interface + bản no-op để UI chạy được (quy tắc 1.1 mục 2).
   - **Không bao giờ** dịch lại toàn bộ thư viện như Sirenix/Odin, JSON, collection. Royal Smash ver3 mất trọn 1 ngày cho 11 lô Odin [RS].
3. Thay thư viện thì **giữ GUID**: stage ở `WORK/libs/stage/<Asm>/` + `guidmap.tsv`, backup stub, cài một lượt, compile một lần. Script nằm trong DLL thì fileID = MD4, tính bằng `bulk/unity_fileid.py` [PF].
4. DLL đặt `isExplicitlyReferenced: 1` mà không asmdef nào tham chiếu thì bị loại khỏi build player [RS].
- **Xong khi:** `error CS` = 0; không có Missing Script hàng loạt; bảng "thư viện → nguồn (plugin copy / official tải về / dịch phần dùng / SDK qua interface)" đã ghi.

### C4. Kiểm kê stub (bắt buộc, trước khi dịch hàm nào)

- Chạy `stub_index.py` + `stub_scan.py` để có danh sách mọi thân rỗng, kể cả `=> null/default`, getter, ctor, `readonly List` không khởi tạo.
- Chạy `bulk/used_scripts_scan.py <scene/prefab>` để biết script nào thực sự được dùng.
- Phân loại từng assembly/namespace vào 4 nhóm: **thư viện thật / SDK qua interface (UI làm ngay, fake làm sau core) / không dùng (không chạy tới, kể cả thư viện thừa) / dịch**. Không có nhóm "cắt tính năng": live-ops, ads, IAP vẫn thuộc nhóm "dịch" ở phần UI và logic phía game. Số liệu PixelFlow: 50.381/56.597 hàm rỗng; chỉ ~11.600 hàm phải dịch, ~12.300 cắt, ~25k là thư viện, ~5k là SDK [PF].
- Chia nhóm "dịch" thành đợt theo namespace, mỗi đợt 50–200 class, xếp theo thứ tự ở C7.
- **Xong khi:** có `WORK/reports/inventory.md` (số hàm theo nhóm) và `batches.tsv`.

### C5. Shader và resource bị mất (song song với C6)

- Dùng `extract_shaders.py` (dump GLSL) và `remap_shaders.py`. Shader còn lại viết lại từ GLSL, **giữ tên, property và GUID** [RS][PF].
- Chạy `fix_sprite_atlas.py`, `fix_packed_normals.py`, rồi `addressables_entries.py` (đã khớp được khi AssetRipper đổi `.wav` thành `.ogg`) [RS].
- Video H.264 không import được trên Unity Linux → đổi sang VP8 `.webm`, giữ GUID [PF].
- Prefab mất tham chiếu cross-bundle (fileID 0) thì gán lại bằng tay [PF].
- **Xong khi:** không còn shader dummy trên các material đang dùng, không còn ô trắng, không còn `InvalidKeyException`.

### C6. Pseudo-C hàng loạt (chỉ các namespace thuộc nhóm "dịch")

Không decompile toàn bộ `.so`. Royal Smash ver3 decompile 343 nghìn hàm chạy cả đêm, trong khi chỉ cần ~18 nghìn. Không chạy auto-analysis: chạy hơn 65 phút vẫn chưa xong [RS].

| ABI | Cách làm | Nguồn |
| --- | --- | --- |
| ARMv7 | Import `-noanalysis` → `ApplyIl2cppNames.java` → `DefineHelpers.java` (helper runtime riêng từng build) → `DecompileTargets.java`/`DecompileBulk.java` → `ResolveVeneers.java` + `bulk/name_c.py` / `bulk_postprocess.py` → `.named.c` | [RS][PF] |
| ARM64 | Import `-noanalysis` → `arm64/ghidra/CreateCallTargets.java` → `MarkNoReturnTargeted.java` (bắt buộc, nếu không pseudo-C đọc lấn thành hàng nghìn dòng) → `ApplySigsAndDecompile.java` theo `seeds.tsv`; xem bằng `arm64/show.sh` | [RS, ~2.500 hàm] |
| ARM64, project Ghidra mới (lộ C) | Một lệnh `analyzeHeadless ... -noanalysis` với các postScript: `SeedAndDecompile.java seeds.tsv <targets rỗng> <out>` (tạo + đặt tên 86k hàm, ~80 giây) → `CreateCallTargets.java` (4.759 hàm) → `MarkNoReturnTargeted.java targets.tsv` → `ApplySigsAndDecompile.java structs.tsv sigs.tsv targets.tsv <out>`. Image base Ghidra = `0x100000` (seed = RVA + 0x100000). 77/77 hàm ra pseudo-C có tên field và kiểu, tổng 2 phút. Còn thiếu: helper runtime chưa đặt tên (`initialize_runtime_metadata`, write barrier, `class_init`), ô metadata hiện là `DAT_...`, và hàm có nhánh đuôi nuốt thân hàm kế (`Init` chứa luôn `Clear`) phải chạy `FixBodies.java` | [TM, một phần: chưa dịch sang C#] |

- Pipeline tự động theo nhóm: `bulk/b3/run_b3.sh` (trích → kiểm coverage → đặt tên → tùy chọn giao Codex dịch). PixelFlow đã trích 31.122 hàm / 5.428 lớp [PF].
- Ghidra có lúc kẹt hàng chục phút ở một hàm; bỏ qua hàm đó và ghi lại [PF].
- **Xong khi:** mỗi class thuộc nhóm "dịch" có `.named.c` đọc được từ đầu tới `return`.

### C7. Điền máy các mẫu tầm thường, rồi dịch theo đợt

**Điền máy trước** (rẻ hơn dịch tay hàng chục nghìn hàm) [PF]. Dùng script có kiểm ARM, luôn có backup và danh sách:
- `bulk/fill_event_accessors.py`: 865 event add/remove.
- `bulk/fill_event_stubs.py`: 508 hàm Trigger/Subscribe.
- `bulk/fill_null_getters2.py`: 344 getter `ldr r0,[r0,#off]; bx lr`, chỉ ARMv7.
- Thêm `= new()` cho `readonly List/Dictionary` (869 field).

**Thứ tự dịch** [PF][RS]:
1. Lõi: DI/Finder/ServiceLocator, Var/Event reactive, Singleton, Extensions, Conditions, Promise/async, Pool, Save/DataService.
2. Boot và load scene.
3. Gameplay chính.
4. Home/UI/popup.
5. Các feature theo level.
6. VFX/audio.

Lý do lõi đi trước: ở PixelFlow, shooter luôn xanh và hiện "999 đạn" thật ra do `PlainVar/PlainEvent` là stub, mất cả buổi chiều đi tìm lỗi shader. `AutoTriggerMethodExtensions.Has*` trả false làm sai mọi component trigger [PF].

**Quy tắc dịch** (giữ nguyên từ ver3):
- Mọi dòng C# phải có bằng chứng từ pseudo-C, ARM hoặc literal.
- Không đổi tên class, field serialize hay `.meta`.
- Class `<X>d__N` thành `async`/`IEnumerator`; `<>c` thành lambda.
- Thiếu bằng chứng thì để nguyên, gắn **UNVERIFIED** và ghi `docs/deferred-work.md`. Không viết bản "có vẻ đúng".

**Cách chia việc trong một đợt** [PF]:
- Mỗi agent hoặc Codex nhận 1 chunk namespace kèm danh sách file, không trùng file.
- Prompt dùng `bulk/b3/prompt_template.txt`.
- Backup `WORK/backup/<đợt>/` trước lần sửa đầu.
- Report `WORK/reports/<chunk>.md`, dòng đầu `Tiến độ: X/Y`, để resume khi agent chết vì limit.
- Agent **không chạy Unity**. Phiên chính kiểm bằng `bulk/asm_check.sh <Assembly>` (compile offline bằng `.rsp` của Unity), rồi **compile Unity một lần mỗi đợt**.
- Sau mỗi đợt chạy lại `stub_scan.py`: agent có thể báo "xong" mà không sửa file nào [PF].

**Xong khi** (pha 7 của ver3 không có điều kiện này): mọi class trong `batches.tsv` có trạng thái done/partial kèm bằng chứng, số stub nhóm "dịch" bằng 0 hoặc đã ghi deferred, và bộ hồi quy ở C8 PASS.

### C8. Kiểm tra (song song từ khi Boot chạy được)

| Tầng | Công cụ | Chứng minh được |
| --- | --- | --- |
| 1. Probe headless | Probe Play-mode (mẫu `mau-tham-khao/probe-pixelflow/`): tự tap, ghi trace, chấm Win/Fail; runner backup/restore save + SHA-256 | Không crash, logic chạy tới kết quả |
| 2. Luồng thật có ảnh | Boot → Home → Gameplay bằng **chuột thật** (`boot_run.py` + realClick). Click giả bỏ sót được lỗi EventSystem [PF] | UI, bố cục, popup |
| 3. So với bản gốc | Video/ảnh **cùng phiên bản** (playlist người dùng gửi) | Giống về hình, nhịp, màu |

- **Bộ level đại diện:** thống kê feature từ dữ liệu level (`FEATURE_CHECKLIST.md`), chọn 30–40 level phủ mọi feature thay vì chơi hết 2.700 level [PF].
- **Kiểm công cụ đo trước khi tin nó:** lỗi "bàn chơi trống" của PixelFlow hóa ra do probe chụp ảnh (camera overlay trong URP), nhiều checkpoint đã kết luận nhầm [PF].
- **Bẫy riêng của Unity Linux** [PF]:
  - Editor mất focus chỉ còn 0,5–1 FPS, nên hồi quy chạy bằng CLI.
  - Project tắt domain reload thì static không reset giữa hai lần Play.
  - Probe mở thẳng Gameplay có thể gặp NRE giả do Boot chưa khởi tạo Finder.
- Validator PASS không có nghĩa là giống game gốc; báo rõ mỗi phần đã qua tầng nào.

### C9. Hoàn thiện và build

- Làm Boot/Home/UI/VFX/audio. Thứ tự lớn: **core gameplay chính hoàn tất → fake SDK** (ads, IAP, live-ops, analytics theo quy tắc 1.1 mục 2) → server fake đủ luồng (nếu có) → gỡ debug → build.
- **Server thật** chỉ bắt đầu khi game đã hoàn chỉnh trên server fake.
- **Cài Android Build Support và Switch Platform ở cuối cùng**, vì đổi platform sẽ reimport toàn bộ [PF].
- Play test tổng thể như người chơi thật trước khi bàn giao (mục 7).

---

## 4. Lộ B: chạy lại build gốc (đã làm với Cocos [DI])

1. **Giải mã:**
   - `.jsc` bằng `cocos/jsc_dec.py`; key tìm bằng `cocos/xxbrute.c` với `strings libcocos.so`.
   - Đọc bundle/import/CCON bằng `cocos/cocos_lib.py` + `build_index.py`.
   - Dữ liệu bảng (Luban) phải chạy module `schema.ts` của game bằng node để decode (`decode_tables.mjs`).
2. **Dựng engine đúng phiên bản** (`cocos/build_engine.js`) và ghép `web/` từ APK (`cocos/assemble_web.py`: ASTC→PNG, `encrypted:false`).
3. **Chặn mạng ra ngoài ngay từ đầu:** CSP `connect-src 'self'` + hook XHR/fetch/WS, để không gọi nhầm server thật của hãng.
4. **Stub native/SDK** (`jsb`, `cc.native.reflection`, tracker bằng Proxy).
5. **Server giả dùng lại codec của chính game** (`NetUtils`, `Request/Response`, proto). Viết `net_protocol.md` từ code trước khi viết handler.
   - Luôn gửi `head`.
   - Lỗi app vẫn trả HTTP 200.
   - Thêm độ trễ giả khoảng 30 ms, vì trả lời tức thì làm hỏng scene HotUpdate.
6. **Các mốc** (ghi giờ): M1 render màn đầu → M2 vào Main → M3 chơi xong 1 trận → M4 meta + mọi chế độ.
7. **Test:**
   - Harness codec chỉ chứng minh encode/decode. Phải có vòng UI thật: `cocos/drive.js` (Chrome headless điều khiển qua HTTP) + autoplay + ảnh làm bằng chứng.
   - Ví dụ `GainTaskBatch`: harness PASS nhưng UI báo "Invalid Parameter[1001]".
   - Có cờ dev (`?god`, `?unlockall`), nhưng phải có một lượt chạy không cheat.
8. **Server thật dài hạn**: chỉ làm **sau khi game đã hoàn chỉnh trên server fake offline**. Các nguyên tắc dưới đây vẫn áp dụng ngay từ khi viết server fake, để sau này chuyển sang server thật không phải viết lại:
   - Handler là hàm thuần `(state, req) -> (state, resp)`, storage sau interface (SQLite → PostgreSQL).
   - `.proto` độc lập + test vector + `PROTOCOL.md` để client Unity sau này dùng lại.
   - Không phụ thuộc code của client.

Với Unity Mono, lộ B tương ứng là decompile DLL ra C# rồi chỉ sửa chỗ hỏng **[đề xuất]**.

---

## 5. Lộ A: nghiên cứu cơ chế [RS]

1. Làm C2 và C6 theo ABI, nhưng **chọn hàm theo cơ chế** (`seeds.tsv` lọc theo tên class), không theo namespace.
2. Lấy hằng số từ asset (AssetRipper headless: prefab, SO, level JSON) và công thức từ pseudo-C. Chỗ nào suy luận thì đánh dấu *(suy luận)*.
3. Mỗi cơ chế viết một tài liệu (`01_Gameplay_Map`, `02_Mechanic_*`…) có mục Nguồn và Mức độ tin cậy, kèm handoff nói rõ "phiên sau đọc file này, không đọc lại pseudo-C".
4. Prototype viết code mới; test bằng bot chạy qua các màn.
5. Giữ toàn bộ trạng thái (project Ghidra, `.so` đã reloc, JSON) ở `tools/state/` để không phải dựng lại.

---

## 6. Chia việc cho agent và Codex

| Ai | Giao gì | Không giao |
| --- | --- | --- |
| Phiên chính (Claude) | Quyết định, compile Unity, probe, play test, gộp kết quả, cập nhật handoff | — |
| Sonnet subagent | Dịch theo chunk, đọc nhiều file rồi tóm tắt, chạy test dài | Chạy Unity khi agent khác đang sửa file |
| Codex `gpt-6.1-sol` (low) | Các đợt dịch lớn tuần tự, harness Node, việc code thuần có file đầu ra | Mở trình duyệt hoặc socket (sandbox chặn), test UI |
| Codex `gpt-6-luna` | Việc nhỏ, đơn giản | Dịch hàng loạt (từng ra 0/121 lớp [PF]) |

- Tối đa 2–3 agent song song, không trùng file. Mỗi agent có backup, report riêng và dòng `Tiến độ: X/Y`.
- Kiểm lại kết quả agent: so diff, chạy `stub_scan`, compile. Agent từng báo xong mà không sửa gì [PF], và `e2e.js` do Codex viết fail 4/4 lần [DI].
- `codex exec` chạy nền phải có `< /dev/null`. Không `pkill -f` với chuỗi có trong chính lệnh [DI].
- Khi quota tới khoảng 90%: checkpoint vào HANDOFF ngay [PF].

---

## 7. Báo cáo, ghi chép, bàn giao

- **Một nguồn trạng thái:** `CLAUDE.md`/`HANDOFF.md` ở gốc `REPO`, gồm tham số, mục tiêu, lộ trình, trạng thái pha, nhật ký có giờ, việc kế tiếp và danh sách mock/cắt.
- **`docs/deferred-work.md`:** ghi mọi chỗ khác bản gốc ngay lúc làm.
- **Bẫy mới, tool mới:** thêm vào `docs/workflow-apk-to-unity.md` (mục 7.4, phụ lục B) kèm một dòng changelog. Thông tin riêng của game không đưa vào tài liệu quy trình.
- **Trước khi bàn giao một việc lớn:** play test tổng thể như người chơi thật (input thật), so với video gốc, ghi cách chạy.
- **SVN:** chỉ chép sang `trangbn/<Game>` sau khi người dùng xác nhận game xong.
  - Loại bỏ deps, Library, log, ảnh, video, keystore.
  - Đặt `svn:ignore` (`cocos/svn_ignore.sh`).
  - **Checkout sạch rồi chạy thử** trước khi báo xong: commit r2957 của Dicero thiếu `index.html`, `src/` và 98 thư mục texture [DI].
  - Không tự `svn add`/commit. Game có server thì chia `client/` và `server/`.

---

## 8. Bẫy hay gặp nhất (danh sách kiểm sau mỗi đợt)

Đầy đủ ở mục 7.4 của `docs/workflow-apk-to-unity.md`.

| Bẫy | Cách kiểm hoặc xử lý | Nguồn |
| --- | --- | --- |
| Stub `return default/null/false`, getter, event, ctor rỗng: không nổ lỗi mà chạy sai âm thầm | `stub_scan.py`, các script `fill_*` | [PF][RS] |
| Lõi reactive/extension là stub gây lỗi ở nơi xa | Dịch lõi trước; nghi lõi trước khi nghi module ngọn | [PF] |
| Pseudo-C mất tail call, try/catch, phép phủ định, đuôi hàm, đối số struct trên stack; gộp hai `if` thành `if/else` | Đối chiếu ARM: `TailCalls.java`, `lsda.py`, `arm_dis.py` | [RS] |
| Hàm noreturn làm Ghidra đọc lấn sang hàm khác | ARM64: `MarkNoReturnTargeted`; ARMv7: `DefineHelpers` | [RS] |
| Địa chỉ: `rt_` dùng RVA, `_UNK_` dùng địa chỉ Ghidra (ARMv7 cộng `0x10000`) | Kiểm base trước khi disassemble | [RS] |
| Thư viện "bên thứ 3" cũng là stub | Thử riêng thư viện trước khi sửa code mình | [RS][PF] |
| AssetRipper: MonoBehaviour rỗng, shader dummy, mất atlas, normal map AG, `.wav` đổi thành `.ogg` | `recover_mb.py`, `fix_*`, `addressables_entries.py` | [PF][RS] |
| Công cụ đo sai (ảnh probe, click giả) | Kiểm chứng công cụ trước khi kết luận lỗi game | [PF] |
| Sửa file save không đổi được level khi boot | Dùng env hoặc hook của probe | [PF] |
| Harness PASS nhưng UI từ chối | Luôn có một vòng UI thật | [DI] |
| Video tham khảo khác phiên bản | Hỏi phiên bản trước khi sửa | [RS] |
| `grep` là alias ugrep; `grep -P` trên bảng tên lớn bị treo; `sed -i` trên ổ data báo lỗi quyền | Dùng `command grep` hoặc Python | [PF][DI] |

---

## 9. Bộ công cụ trong gói

| Thư mục | Nội dung | Kiểm chứng |
| --- | --- | --- |
| `Tools/reverse/` (gốc) | Bộ của ver3: project Unity, shader, atlas, Addressables, Ghidra ARMv7, `stub_index.py`, `stub_scan.py`, `arm_dis.py`, `lsda.py`… | [RS] ARMv7 |
| `Tools/reverse/arm64/` | `unpack_aps2.py`, `apply_rela.py` (RELA thường, không APS2), `gen_sigs.py` (tự nhận v31/v39), `field_offsets.py`, `show.sh`, `ns/cls/mem.py`, `ghidra/{CreateCallTargets,MarkNoReturnTargeted,OnlyNoReturnAnalysis,FixBodies,ApplySigsAndDecompile,SeedAndDecompile}.java` | [RS] ARM64, lộ A; [TM] chạy thử lộ C trên 77 hàm |
| `Tools/reverse/bulk/` | `asm_check.sh`, `used_scripts_scan.py`, `fill_event_accessors.py`, `fill_event_stubs.py`, `fill_null_getters2.py` (ARMv7), `recover_mb.py`, `unity_fileid.py`, `name_c.py` (helper là của PixelFlow, phải sửa), `b3/{run_b3.sh,b3_coverage.py,prompt_template.txt}` | [PF] ARMv7 |
| `Tools/cocos/` | `jsc_dec.py`, `xxbrute.c`, `cocos_lib.py`, `build_index.py`, `tex.py`, `export_*.py`, `decode_tables.mjs`, `build_engine.js`, `assemble_web.py`, `drive.js`, `montage.py`, `svn_ignore.sh` | [DI] Cocos 3.8.8 |
| `Assets/Editor/` | `GameViewResolution.cs`, 2 script Addressables (chép sau C3) | [RS] |
| `mau-tham-khao/` | Validator của Royal Smash; probe/regress của PixelFlow. **Chỉ đọc để học cách viết, không chép vào project** | — |

Các tool trong `arm64/`, `bulk/` và `cocos/` đã được bỏ đường dẫn ghi cứng và chuyển sang đọc `REPO`, `WORK`, `EDITOR`, `GHIDRA`, `ABI`, `GPROJ`, `GRPS`, `GAME`, `COCOS_JS_MAIN`. Sau khi sửa đường dẫn mới chỉ kiểm cú pháp, **chưa chạy lại trên game mới**. Lần đầu dùng, hãy chạy thử nhỏ.

---

## 10. Checklist cho người điều phối

- [ ] Pha 0: mục tiêu, 3 quy tắc phạm vi, bảng tham số đã ghi trong MỘT handoff
- [ ] Bản phân tích lộ trình (số liệu đo, so sánh A/B/C/lai, đề xuất) đã được người dùng duyệt
- [ ] Plugin dùng được đã COPY (bản gốc còn nguyên); plugin thiếu chỉ dịch phần game dùng
- [ ] UI của ads/IAP/live-ops/analytics có ngay; fake chức năng đã ghi deferred, làm sau core gameplay
- [ ] Game có server: chạy đủ trên server fake offline trước; server thật chỉ làm sau khi game hoàn chỉnh
- [ ] (C) Thư viện đã thay bằng bản thật, giữ GUID; bảng nguồn thư viện đã có
- [ ] (C) `inventory.md` + `batches.tsv` có trước đợt dịch đầu tiên
- [ ] (C) Lõi đã dịch và kiểm trước gameplay/UI
- [ ] Mỗi đợt: `asm_check` → 1 lần compile Unity → `stub_scan` → hồi quy bộ level đại diện
- [ ] (B) Mạng ra ngoài đã chặn; có vòng UI thật, không chỉ harness
- [ ] Mỗi pha có giờ bắt đầu/kết thúc trong handoff
- [ ] `deferred-work.md` ghi đủ chỗ khác bản gốc và chỗ UNVERIFIED
- [ ] Play test tổng thể bằng input thật + so video cùng phiên bản trước khi bàn giao
- [ ] SVN: checkout sạch chạy được, không tự commit
