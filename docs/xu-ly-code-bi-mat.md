# Xử lý khi pseudo-C thiếu hoặc code không còn thân

Cập nhật: 2026-10-27

Tài liệu này bổ sung cho `quy-trinh-dung-lai-game.md` và `workflow-apk-to-unity.md`. Nó trả lời một câu hỏi: **khi không tìm được logic của một method thì làm gì**. Không phải mọi chỗ trong này đã chạy thật.

## 0. Cách đọc nhãn (bắt buộc đọc trước)

Mỗi mục có đúng một nhãn:

| Nhãn | Nghĩa | Được làm gì |
| --- | --- | --- |
| **[đã kiểm chứng]** | Đã chạy thật trên game mẫu Royal Smash 1.16.477 (IL2CPP, ARMv7), có kết quả ghi lại | Làm theo, nhưng vẫn kiểm lại trên game mới vì địa chỉ, phiên bản, cấu trúc code khác nhau |
| **[một phần]** | Đã làm một phần, hoặc đã có dữ liệu nhưng chưa dùng hết | Làm theo phần đã ghi rõ; phần còn lại coi như đề xuất |
| **[đề xuất, chưa kiểm chứng]** | Suy từ hiểu biết chung về Unity/IL2CPP, **chưa chạy trên game nào trong dự án này** | Không tin 100%. Thử trên một trường hợp nhỏ trước, ghi kết quả, rồi đổi nhãn |

Quy tắc cho AI đọc tài liệu này:

- Không được báo cáo một việc là "đã làm theo quy trình đã kiểm chứng" nếu mục đó mang nhãn đề xuất.
- Khi thử một mục đề xuất, ghi kết quả (chạy được / không chạy được / chạy khác mô tả) vào phần "Lịch sử thay đổi" cuối file và sửa nhãn.
- Khi một mục đã kiểm chứng tỏ ra sai trên game mới, sửa nội dung và ghi lý do; đừng lặng lẽ làm khác.

## 1. Hai câu hỏi khác nhau

Trước khi xử lý, xác định mình đang gặp loại nào:

1. **Pseudo-C thiếu, nhưng code vẫn còn trong binary.** Decompiler làm rơi chi tiết. Cách xử lý: đọc assembly ARM và dữ liệu trong binary. Đây là phần lớn công việc của game mẫu. → Mục 2.
2. **Method không có thân trong binary** (bị strip, bị inline, nằm ở file khác, nằm ngoài APK). → Mục 3.

Ngoài ra còn loại thứ ba không do build gây ra: **mình tạm thay code gốc vì phụ thuộc chưa dịch xong.** → Mục 4.

## 2. Pseudo-C thiếu nhưng code vẫn còn **[đã kiểm chứng]**

Toàn bộ mục này đã gặp và xử lý thật trên game mẫu. Danh sách đầy đủ (hơn 70 bẫy) ở mục 7.4 của `workflow-apk-to-unity.md`. Tóm tắt các dạng chính:

| Dạng mất | Ví dụ thật | Cách lấy lại | Công cụ |
| --- | --- | --- | --- |
| Tail call (`b <hàm>` cuối hàm) bị bỏ | `Awake` nối 2 nút, pseudo-C chỉ thấy 1; nút Close không chạy | Quét tail call, đọc assembly các dòng nghi bị bỏ | `TailCalls.java`, `tailcall_audit.py` |
| Pseudo-C chép thân hàm đích vào hàm gọi | `OnEnable` chứa luôn thân `OnShow` | So thân với hàm RVA liền kề; không dịch trùng | `tailcall_audit.py` (`INLINED`) |
| Mất try/catch | Exception làm chết cả coroutine | Đọc bảng LSDA và landing pad | `lsda.py` |
| Mất store / side-effect | `IsCompleted = true` không có trong pseudo-C | Đọc assembly hàm đó | `arm_dis.py dis` |
| Rơi đối số vector qua stack, đảo thứ tự cộng float | Vật bay lệch, giá trị lệch nhỏ | Đọc assembly từng lệnh VFP/NEON | `arm_dis.py dis` |
| Hằng số float/chuỗi sai hoặc thiếu | Ngưỡng, epsilon, khóa JSON | Đọc literal pool, giải ô metadata | `arm_dis.py words`, `read_floats.sh`, `litann.py` |
| Hai `if` liên tiếp thành `if/else`; serializer dừng sau setter đầu | Một nhánh không bao giờ chạy | Đọc toàn bộ khoảng RVA của hàm | `arm_dis.py dis` |
| `.ctor`/`.cctor` bị bỏ qua khi dịch | Cờ static sai, list null, vật lý khác gốc | Dịch cả constructor, kể cả class lồng | `fields.py` |
| Comment `Method$...` của decompiler ghi sai bản generic | Gọi nhầm specialization | Xem lệnh là `bl` tĩnh hay `blx` qua thanh ghi | `arm_dis.py dis` |
| Getter stub `=> null`/`=> default` bị coi là đã dịch | Crash chìm lúc khởi động | Quét mọi getter trả hằng, đọc literal thật | grep + `stub_index.py` |

Nguyên tắc **[đã kiểm chứng]**: pseudo-C trông cụt, ngắn bất thường hoặc "rõ ràng thiếu gì đó" thì nghi decompiler, không nghi game. Không viết bản "tương đương" để lấp. Trên game mẫu, một hiệu ứng viết "tương đương" đã che mất lỗi logic thật, chỉ phát hiện khi đọc lại ARM.

## 3. Method không có thân trong binary

### 3.1 Nguyên tắc

- **[đề xuất, chưa kiểm chứng]** Nếu game gốc có chạy một đoạn code, đoạn đó phải nằm đâu đó trong binary: dưới dạng hàm riêng, bản inline, bản generic dùng chung, hoặc trong một file `.so` khác. Thứ bị mất khi build là tên, ranh giới hàm và cấu trúc, còn hành vi đã chạy thì không mất. Trên game mẫu chưa gặp trường hợp nào trái điều này, nhưng cũng chưa thống kê có hệ thống.
- **[đề xuất, chưa kiểm chứng]** Code bị UnityLinker strip (managed code stripping) thường mất khỏi cả IL lẫn metadata, nên **không sinh stub nào**. Hệ quả: phần lớn code bị strip mình không nhìn thấy và cũng không cần làm gì. Method có stub nhưng không có RVA thường là abstract, interface, extern hoặc generic mở, ít khi là "bị strip".
- **[đã kiểm chứng]** Số liệu game mẫu khớp với nhận định trên: trong assembly game chính `Core` (7.461 method), `stub_index.py` chỉ báo **17 `norva`** (không có RVA). Toàn bộ stub có 2.773 `norva`, phần lớn thuộc thư viện bên thứ ba có chữ ký stub viết khác `dump.cs`. Trạng thái `nobody` (1.488) là extern/interface, **không phải** method bị strip.

### 3.2 Bảng phân loại

Mỗi method stub còn rỗng (sau khi đã loại các trường hợp ở mục 2) xếp vào đúng một loại, kèm bằng chứng.

| # | Loại | Nhận biết | Xử lý | Nhãn |
| --- | --- | --- | --- | --- |
| 1 | Bị strip, không ai gọi | Không có RVA; không có lời gọi trong pseudo-C; tên không xuất hiện trong YAML (`m_MethodName` của UnityEvent, Animation Event) hoặc chuỗi metadata (`SendMessage`, `Invoke("Tên")`) | Để stub, ghi sổ "strip, không caller" | **[đề xuất, chưa kiểm chứng]** |
| 2 | Bị inline vào chỗ gọi | Thân lặp lại ở nhiều hàm gọi; hoặc hàm runtime chưa tên `rt_<rva>` mang logic managed | Đặt tên bản inline bằng so khớp thân hàm; dựng method từ bản inline, so nhiều chỗ gọi nếu có | **[một phần]**: `name_inline_copies.py` đặt được 32 tên trên game mẫu; bản inline khác cách truyền tham số (con trỏ thay vì giá trị) phải đọc tay. Việc "dựng method từ nhiều chỗ gọi" chưa làm có hệ thống |
| 3 | Generic không có bản riêng | `RVA: -1` trong `dump.cs` | Lấy RVA trong khối `GenericInstMethod` sau đó; ưu tiên `__Il2CppFullySharedGenericType`, không có thì bản cụ thể hóa đầu tiên | **[đã kiểm chứng]** (`stub_index.py`, `norva` từ 5.668 xuống 2.773) |
| 4a | Burst job | `[BurstCompile]`, `IJob*`, `*$BurstDirectCall` | Dịch từ bản managed có RVA trong `libil2cpp.so`; cần khớp bit thì decompile `lib_burst_generated.so` | **[một phần]**: game mẫu đã dịch JobTween từ bản managed trong `libil2cpp.so` và chạy job Burst thật trong editor. Chưa bao giờ decompile `lib_burst_generated.so`, chưa so bản Burst với bản managed |
| 4b | Native plugin | `extern` + `[DllImport]` | Decompile `.so` đó, hoặc dùng SDK gốc nếu công khai | **[đề xuất, chưa kiểm chứng]** |
| 5 | Logic ngoài APK | Gọi mạng, remote config, tải asset từ CDN | Lấy từ dữ liệu: DTO phía client, cache trên máy đã chơi, traffic. Không có thì mock và ghi giá trị giả định | **[một phần]**: đã mock Backend/Ads/IAP/Analytics; đã tải đủ 34 bundle CDN và lấy cache từ điện thoại (bản khác), **chưa nhập vào project**. Chưa bắt traffic |
| 6 | Có trong binary nhưng không đọc nổi | Decompile lỗi/timeout; code rác của hàm khác lẫn vào; logic quá rối | Đọc ARM trực tiếp; vẫn không đủ thì theo thứ tự nguồn bằng chứng ở 3.4 | **[đã kiểm chứng]** cho bước đọc ARM (ví dụ `RunStageTransition` khoảng 1.500 dòng ARM, `Grant` của Economy lẫn code hàm khác). Bước "theo nguồn bằng chứng thấp hơn" chưa cần dùng |

### 3.3 Quy trình cho một method

**[đề xuất, chưa kiểm chứng]** (từng bước riêng lẻ đã làm trên game mẫu, nhưng chưa chạy thành một quy trình phân loại có hệ thống):

1. Tra RVA trong `dump.cs`. Có RVA mà chưa thấy thân thì là lỗi tra cứu, không phải strip: mở pseudo-C/assembly tại RVA đó.
2. Không có RVA: kiểm generic mở (loại 3), abstract/interface/extern.
3. Tìm người gọi: grep tên method trong toàn bộ pseudo-C và mọi file YAML (`.prefab`, `.unity`, `.anim`, `.controller`, `.asset`); grep chuỗi trong metadata.
4. Không ai gọi → loại 1.
5. Có người gọi nhưng không có hàm riêng → tìm thân đã inline (loại 2).
6. Burst/extern → loại 4. Gọi mạng/dữ liệu từ xa → loại 5.
7. Còn lại → loại 6.

Làm hàng loạt: viết script chạy bước 1–4 cho các assembly game (bỏ thư viện bên thứ ba và SDK sẽ mock), xuất bảng `method · loại · bằng chứng`. **Script này chưa tồn tại** trong `Tools/reverse/`.

### 3.4 Thứ tự nguồn bằng chứng khi phải viết bù

Dùng nguồn cao nhất có được. Nguồn thấp hơn chỉ để lấp phần nguồn cao hơn không trả lời được.

| Ưu tiên | Nguồn | Cho biết | Nhãn |
| --- | --- | --- | --- |
| 1 | Assembly ARM | Logic chính xác từng lệnh | **[đã kiểm chứng]** |
| 2 | Dữ liệu asset: field prefab, JSON level, AnimationCurve, ScriptableObject | Tham số, ngưỡng, đường cong | **[đã kiểm chứng]** (ví dụ custom ease của coin bay lấy từ curve trong scene) |
| 3 | Phân tích động trên game gốc (mục 3.5) | Giá trị thật lúc chạy, thứ tự gọi | **[đề xuất, chưa kiểm chứng]** |
| 4 | Video/ảnh game gốc **cùng phiên bản** | Nhịp, thời gian, hình dạng | **[một phần]**: đã so một video, phát hiện video là bản khác (1.16.494 so với 1.16.477) nên kết luận "giữ theo binary". Chưa so có hệ thống |
| 5 | Viết bù theo hành vi quan sát | Lấp chỗ còn trống | **[đề xuất, chưa kiểm chứng]**: game mẫu chưa phải viết bù kiểu này |

Không dùng làm bằng chứng **[đã kiểm chứng]**: tên hàm/field (chỉ gợi ý), comment `Method$...` chưa kiểm `bl`, video khác phiên bản, trí nhớ của AI về game tương tự.

### 3.5 Phân tích động bằng Frida **[đề xuất, chưa kiểm chứng]**

Chưa chạy trên game mẫu. Mô tả dưới đây là hướng làm; thử trên một hàm đơn giản trước.

- Chạy game gốc trên máy Android/emulator đã root, chỉ với bản cài của mình. Tắt mạng nếu game có chống gian lận phía server.
- Cài `frida-server` đúng ABI lên máy, `frida-tools` trên máy tính.
- Địa chỉ hàm = base của `libil2cpp.so` + RVA (không cộng image base của Ghidra).
- Hook đầu/cuối hàm, ghi tham số, giá trị trả về, field của `this` theo offset trong `dump.cs`.

```javascript
// Ví dụ ARMv7 (chưa chạy thử): ghi một field float của this
const base = Module.findBaseAddress("libil2cpp.so");
const fn = base.add(0x1EF7F04);              // RVA tra trong dump.cs
Interceptor.attach(fn, {
  onEnter(args) {
    this.self = args[0];                     // tham số 0 = this
    console.log(`enter f=${this.self.add(0x2C).readFloat()}`);
  },
  onLeave(ret) { console.log(`leave ret=${ret}`); }
});
```

Điểm cần tự kiểm khi thử:

- ARMv7 truyền float qua thanh ghi số nguyên hay VFP tùy quy ước gọi; đọc assembly đầu hàm trước khi giải `args`.
- Method IL2CPP có thêm tham số `MethodInfo*` cuối; struct trả về có thể qua con trỏ ẩn.
- Có thư viện đọc metadata IL2CPP lúc chạy (ví dụ frida-il2cpp-bridge) để hook theo tên. Chưa thử; kiểm nó hỗ trợ phiên bản metadata của game.
- Hook chỉ thấy nhánh đã chạy.

## 4. Tạm thay vì phụ thuộc còn stub **[đã kiểm chứng]**

Loại này không do build gây ra. Code gốc vẫn còn và đã đọc được, nhưng nó gọi tới một hệ thống mình chưa dịch, nên tạm viết đường khác để chạy được. Game mẫu có nhiều trường hợp:

| Ví dụ trên game mẫu | Tạm thay bằng | Trạng thái |
| --- | --- | --- |
| `async Promise` của thư viện Promise nội bộ | Coroutine | Đã gỡ: dịch thư viện Promise thật rồi chuyển lại |
| Tween JobTween (scale/slide cannon) | Driver `Update` riêng | Đã gỡ: dịch nền JobTween rồi gọi lại `JTLocalMove` như gốc |
| `SfxPlayer.Instance` còn stub trả `null`, code gốc không null-check | Thêm null-guard quanh đúng lời gọi | Còn tạm (âm thanh hoãn) |
| Cờ "đã xem" của popup giới thiệu lưu qua `Config` | `HashSet` trong bộ nhớ | Đã chuyển sang `Config` khi dịch xong; code còn giữ `HashSet` dự phòng song song |
| Chèn PlayerLoop qua `PlayerLoopUtility` còn stub | Adapter chèn PlayerLoop trực tiếp | Đã gỡ: dịch `PlayerLoopUtility` rồi gọi lại đường gốc |

Quy tắc **[đã kiểm chứng]**:

- Ghi vào `docs/deferred-work.md` ngay lúc viết, kèm tên hệ thống đang chờ.
- Không đổi nhánh logic xung quanh để "cho chạy được"; chỉ thay đúng chỗ gọi vào phụ thuộc.
- Khi dịch xong phụ thuộc, quay lại gỡ adapter. Bản tạm có thể che lỗi: trên game mẫu, bản coroutine thiếu một `await` và một `finally` so với bản async gốc, chỉ lộ ra khi chuyển lại.
- Trước khi kết luận code mới dịch sai, cô lập xem thư viện bên dưới có phải stub không (game mẫu: `MongoDB.Bson` serialize ra 0 byte vì cả thư viện là stub).

## 5. Ghi chép và đánh dấu

- **[đã kiểm chứng]** Mọi chỗ khác bản gốc ghi vào `docs/deferred-work.md` ngay lúc làm; báo cáo nói rõ đã qua tầng kiểm nào (validator / xem bằng mắt / so game gốc).
- **[đề xuất, chưa kiểm chứng]** Đặt comment chuẩn ngay trên method để grep được:

```csharp
// [RECON:stripped]  Không có RVA, không caller trong pseudo-C/YAML. Để stub.
// [RECON:inlined]   Dựng từ bản inline trong Foo.Update (RVA 0x...) và Bar.Tick (RVA 0x...).
// [RECON:adapter]   Tạm thay vì <hệ thống> còn stub. Gỡ khi dịch xong <hệ thống>.
// [RECON:observed]  Không có bản gốc. Viết theo trace/video <nguồn, ngày>.
// [RECON:mock]      Logic server/SDK. Trả giá trị cố định ..., nguồn ...
```

- **[đề xuất, chưa kiểm chứng]** Với code `[RECON:observed]`, validator phải so với trace hoặc video của game gốc; so với chính code mình viết không chứng minh gì. Không gọi phần này là "đã khôi phục".

## 6. Prompt mẫu giao cho AI

```text
Đọc docs/xu-ly-code-bi-mat.md. Chú ý nhãn: chỉ mục [đã kiểm chứng] được coi là quy trình đã chạy thật;
mục [đề xuất, chưa kiểm chứng] phải thử nhỏ trước và ghi kết quả.

Việc: với assembly <tên>, liệt kê method stub còn rỗng hoặc trả hằng (=> null, => default, return default).
Trước hết loại các trường hợp pseudo-C thiếu nhưng code còn (mục 2): đọc ARM, tail call, LSDA, literal.
Phần còn lại xếp vào loại 1–6 (mục 3.2), mỗi method kèm bằng chứng: RVA, dòng pseudo-C của chỗ gọi,
kết quả grep YAML/metadata.
- Loại 1: để stub. Loại 2, 3, 6: dịch từ pseudo-C + ARM, không đoán theo tên.
- Loại 4: dùng bản managed trong libil2cpp.so trước. Loại 5: mock, ghi giá trị giả định và nguồn.
- Chỗ tạm thay vì phụ thuộc còn stub: theo mục 4, ghi deferred-work.
- Chỉ viết bù theo hành vi khi ARM không đủ; khi đó đánh dấu và nêu nguồn quan sát.
Báo cáo: bảng method · loại · bằng chứng · việc đã làm · tầng kiểm đã qua. Nói rõ cái nào chưa kiểm.
Không commit khi chưa được yêu cầu.
```

## Lịch sử thay đổi

| Ngày | Thay đổi | Lý do |
| --- | --- | --- |
| 2026-10-27 | Tạo tài liệu. Gắn nhãn kiểm chứng cho từng mục; thêm loại "tạm thay vì phụ thuộc còn stub"; đính chính `nobody` là extern/interface, không phải method bị strip | Tách rõ phần rút từ Royal Smash và phần đề xuất, để AI/người đọc khác không tin 100% phần chưa chạy |
