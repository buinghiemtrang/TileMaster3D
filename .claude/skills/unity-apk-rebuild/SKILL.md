---
name: unity-apk-rebuild
description: Dựng lại một game Android từ APK/XAPK/APKM (Unity IL2CPP/Mono là chính; Cocos/JS chạy lại build gốc; hoặc chỉ nghiên cứu cơ chế) thành bản chạy được, giống bản gốc, chỉ để học và nghiên cứu. Dùng khi người dùng muốn rebuild/khôi phục/decompile game Unity từ APK, dịch stub C# từ pseudo-C/ARM của libil2cpp.so, xử lý method không còn thân, hoặc tiếp tục một dự án dựng lại game đang làm dở.
---

# Dựng lại game Unity từ APK (để nghiên cứu)

Skill này chỉ là lối vào. Toàn bộ quy trình, công cụ và bẫy nằm trong tài liệu của project; đọc chúng, đừng làm theo trí nhớ.

## Phạm vi

- Chỉ để học và nghiên cứu, với file APK từ bản cài người dùng có quyền dùng. Không phát hành lại, không đưa code hay asset dựng lại lên nơi công khai.
- Phạm vi cố định (mục 1.1 của `docs/quy-trinh-dung-lai-game.md`):
  - Plugin dùng được thì COPY từ `Documents/plugin` / `plugin_v2`, giữ bản gốc; plugin thiếu thì tự dịch, chỉ phần game dùng.
  - Live-ops, quảng cáo, IAP, analytics: không cắt. Làm UI ngay, fake chức năng sau core gameplay, qua interface.
  - Server: fake offline trước, server thật sau khi game hoàn chỉnh.

## Đầu mỗi phiên

1. Có `CLAUDE.md` ở gốc `REPO` → đọc trước để biết đang ở pha nào, việc kế tiếp là gì. Chưa có → đây là phiên đầu, làm mục "Phiên đầu tiên" bên dưới.
2. Đọc theo thứ tự: `DOC-TRUOC.md` → `docs/quy-trinh-dung-lai-game.md` (cổng vào, đọc hết) → khi làm từng pha thì tra `docs/workflow-apk-to-unity.md` theo mục (lệnh, phụ lục A bảng lỗi, mục 7.4 bẫy, phụ lục B công cụ), không đọc hết → `docs/xu-ly-code-bi-mat.md` (khi pseudo-C thiếu hoặc method không có thân) → `docs/bai-hoc/*.md` khi cần lý do.
3. Đọc `docs/deferred-work.md` nếu có.

## Phiên đầu tiên

- Kiểm biến môi trường `REPO`, `WORK`, `GHIDRA`, `EDITOR` (và `ABI`, `RS_WORK` nếu cần) theo `DOC-TRUOC.md`. Thiếu thì hỏi người dùng, không tự đoán đường dẫn.
- Tạo `CLAUDE.md` theo mẫu phụ lục E của `docs/workflow-apk-to-unity.md`: bảng tham số Pha 0, Trạng thái, Nhật ký, Việc kế tiếp, danh sách mock. Tạo `docs/deferred-work.md`.
- Pha 0: hỏi người dùng mục tiêu cuối (3 quy tắc phạm vi đã cố định, không hỏi lại), nhận diện engine/backend/ABI/APS2, phân tích thật kỹ và so sánh lộ trình A (nghiên cứu) / B (chạy lại build gốc) / C (dựng lại source IL2CPP) / phương án lai, kèm số liệu đo, thời gian và rủi ro. Ghi vào MỘT handoff, **chờ người dùng duyệt** rồi mới làm tiếp.
- Lộ C: C1 AssetRipper → C2 metadata → C3 compile sạch + thay thư viện thật giữ GUID → C4 kiểm kê stub (bắt buộc trước khi dịch) → C5 shader/resource → C6 pseudo-C chỉ cho nhóm "dịch" → C7 điền máy + dịch theo đợt, lõi trước → C8 kiểm 3 tầng → C9 hoàn thiện, build. Đạt "Xong khi" của pha mới sang pha sau.

## Tin tưởng theo nhãn (bắt buộc)

Tài liệu gắn nhãn cho từng mục:

| Nhãn | Cách dùng |
| --- | --- |
| `[đã kiểm chứng]` | Đã chạy thật trên game mẫu Royal Smash. Làm theo, nhưng kiểm lại trên game này |
| `[một phần]` | Chỉ phần ghi rõ là đã làm; phần còn lại coi như đề xuất |
| `[đề xuất, chưa kiểm chứng]` | Thử nhỏ trước, báo kết quả, rồi mới dùng rộng. Không báo là "làm theo quy trình đã kiểm chứng" |

Mọi ví dụ trong gói (đường dẫn, tên class, địa chỉ, file `examples/*.royal-smash-*`) là của game mẫu. Với game mới phải tự xác định lại, không chép nguyên.

## Quy tắc làm việc

- **Không đoán logic.** Mọi dòng C# phải có bằng chứng từ pseudo-C, assembly ARM hoặc dữ liệu trong binary. Pseudo-C trông cụt thì nghi decompiler, đọc assembly.
- Không đổi tên class, namespace, asmdef, tên và kiểu field serialize; không xóa/tạo lại `.meta` của script. Chỉ điền thân hàm.
- Mọi chỗ khác bản gốc ghi vào `docs/deferred-work.md` ngay lúc làm.
- Compile sạch hoặc validator PASS không có nghĩa là "giống game gốc". Báo rõ mỗi cơ chế đã qua tầng kiểm nào: validator / người dùng xem bằng mắt / so với game gốc cùng phiên bản.
- Không chạy Unity batch khi người dùng đang mở Editor cùng project.
- Báo ngắn sau mỗi bước, nói thẳng điều chưa kiểm chứng. Chỉ commit/push khi người dùng yêu cầu.
- Cuối mỗi đợt: cập nhật `CLAUDE.md` (trạng thái, nhật ký, việc kế tiếp); gặp lỗi, bẫy hoặc viết công cụ mới thì cập nhật `docs/workflow-apk-to-unity.md` và thêm một dòng "Lịch sử thay đổi quy trình".

## Việc của người điều phối (nói cho người dùng biết khi cần)

AI không tự làm hết được. Người dùng phải: cài phần mềm (đúng bản Unity, AssetRipper, Il2CppDumper, Ghidra + JDK 21, Python), chạy các bước có giao diện (AssetRipper, Unity Hub, mở project lần đầu), Play và xem bằng mắt, quyết định mock/ưu tiên/commit, và giao việc tiếp qua nhiều phiên. Đây là việc nhiều tuần. Khi một bước cần người dùng, dừng lại và nói rõ cần làm gì.
