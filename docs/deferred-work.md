# Deferred work – Tile Master 3D

Ghi mọi chỗ khác bản gốc, chỗ UNVERIFIED và việc để sau.

## Môi trường
- Mở project bằng Unity 6000.3.10f1, game gốc build bằng 6000.3.20f1 (người dùng chọn để khỏi tải bản mới).
- PlayerSettings không đọc được từ APK (AssetRipper lỗi type 129): đặt tay package id, orientation, icon.

## Fake SDK – làm sau core
- Quảng cáo (AdvertisingProxy, AdManager, AdvertisingMgr, RewardVideo…): màn quảng cáo giả vài giây rồi callback.
- IAP (IAPManager, FQDev.StoreKit, Unity IAP): mua luôn thành công, cộng đúng gói theo cfbt_Stores.
- Analytics (AppsflyerSDK, FirebaseSDK, ThinkingAnalyticsSDK, FacebookSDK, LogEvent): ghi log local.
- Đăng nhập Facebook / Sign in with Apple: no-op, chỉ guest.

## Server fake – làm sau core
- service.fuqiangame.com ~45 endpoint (danh sách ở WORK/reports/urls.txt): token, data, time, activity, mail, notice, rank, team, utils.
- File tĩnh OSS: remote config (dùng bản preinstall trong Resources), video quảng cáo tự làm.

## Để sau mốc 1
- GM/debug panel, Reporter (Unity Logs Viewer).
