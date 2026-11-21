# Tile Master 3D 1.12.9 – dựng lại từ APK

**Cấu trúc (từ 2026-11-20):** `client/` = project Unity (Assets, Packages, ProjectSettings…), `server/` = server game (chưa làm). `HANDOFF.md`, `CLAUDE.md`, `docs/`, `Tools/`, `videos/` ở gốc. Đường dẫn `Assets/...` trong HANDOFF/docs ghi trước ngày này = `client/Assets/...`.

Đọc `HANDOFF.md` trước mỗi phiên: đó là nguồn trạng thái duy nhất (mục tiêu, phương án đã duyệt, pha, nhật ký, việc kế tiếp).
Quy trình: `docs/quy-trinh-dung-lai-game.md` (cổng vào), tra cứu `docs/workflow-apk-to-unity.md` theo mục. Mọi chỗ khác bản gốc ghi `docs/deferred-work.md`.

## Tham số
PACKAGE_ID=com.higgs.tilemaster3d  UNITY_VERSION=6000.3.20f1 (gốc; mở bằng 6000.3.10f1)  BACKEND=IL2CPP metadata v39  ABI=arm64-v8a  RP=Built-in
REPO=<thư mục này> (project Unity = REPO/client)  WORK=Documents/TileMaster3D/TileMaster3D-work  GHIDRA=~/.local/share/ghidra/ghidra_12.1.4_PUBLIC  EDITOR=~/Unity/Hub/Editor/6000.3.10f1/Editor/Unity

## Kiểm và tiêu chí xong
- Sau mỗi đợt dịch: `asm_check` theo assembly phải 0 lỗi (Assembly-CSharp, OSA...). Báo cáo từng chunk ở `WORK/reports/b3-<chunk>.md`, dòng đầu là số lớp xong.
- Mốc 1 xong khi: vào được game từ loading, chơi 1 level (rơi/chọn/ghép, booster, thắng/thua) không exception, so với video tham chiếu.
- Agent bị ngắt giữa chừng thì giao lại cùng chunk, agent đọc báo cáo rồi làm tiếp.

## Bẫy hay gặp
- Metadata v39: Il2CppDumper không đọc được, dùng REA + AssetRipper 2.0.0. Không có APS2: RELA thường → `apply_rela.py`. Ghidra base 0x100000.
- AssetRipper cần `assets/` và `lib/` nằm cạnh nhau (`WORK/input`).
- `grep` là alias ugrep (dùng `command grep`). `sed -i` trên ổ data báo lỗi quyền: sửa bằng Python hoặc Edit.
- pseudo-C và dump rất dài: lọc bằng `Tools/reverse` / `WORK/tools/disasm.py` hoặc giao `explorer`, không đọc nguyên vào phiên chính.

## Quy tắc
- Không đoán logic: mọi dòng C# có bằng chứng từ pseudo-C/ARM/literal; thiếu bằng chứng thì gắn UNVERIFIED và ghi deferred.
- Không đổi tên class, namespace, field serialize; không xóa/tạo lại `.meta` của script. Chỉ điền thân hàm.
- Không cắt ads/IAP/live-ops/analytics: UI ngay, SDK qua interface no-op, fake làm sau core. Server: fake offline trước.
- Không chạy Unity batch khi Editor đang mở project này. Tối đa 2–3 agent song song, không trùng file.
- Project quản lý bằng SVN (chưa phải working copy): không tạo `.git`; không tự commit.
