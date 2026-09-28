# KẾ HOẠCH PHÂN CHIA SPRINT TRIỂN KHAI (SPRINT EXECUTION ROADMAP)
## CHUẨN HÓA THÔNG TƯ 99/2025/TT-BTC & TÀI KHOẢN 911 (PHASE 10)

- **Dự án**: `ninjaTax`
- **Mục tiêu**: Chuẩn hóa 100% theo Thông tư 99/2025/TT-BTC của Bộ Tài chính: Đưa Tài khoản 911 vào quy trình kết chuyển cuối kỳ (đóng sạch số dư về 0) và hoàn thiện cơ chế bóc tách hai chiều tài khoản lưỡng tính trên Bảng Cân đối tài khoản.
- **Phương pháp quản lý**: Agile/Scrum kết hợp Test-Driven Development (TDD).

```
+======================================================================================================+
|                                    LỘ TRÌNH 5 SPRINT TRIỂN KHAI CHUẨN HÓA TT99                       |
+======================================================================================================+
|                                                                                                      |
|   +--------------------------+     +--------------------------+     +--------------------------+     |
|   |         SPRINT 1         |     |         SPRINT 2         |     |         SPRINT 3         |     |
|   | NGUYÊN TẮC & CƠ SỞ DỮ LIỆU| --> | ENGINE KẾT CHUYỂN TK 911 | --> | TRIAL BALANCE 8 CỘT      |     |
|   | - Sửa AGENTS/GEMINI.md   |     | - Quy trình 3 chặng      |     | - Bóc tách lưỡng tính    |     |
|   | - Enum LoaiTaiKhoan = 9  |     | - Lãi: 911 -> 4212       |     |   TK 131, 331 2 bên      |     |
|   | - Seed TK 911 vào DB     |     | - Lỗ: 4212 -> 911        |     | - Dòng TK 911 dư cuối = 0|     |
|   +--------------------------+     +--------------------------+     +--------------------------+     |
|                                                                                  |                   |
|                                    +--------------------------+                  |                   |
|                                    |         SPRINT 4         |                  |                   |
|                                    | GIAO DIỆN & SỔ CÁI S03B  | <----------------+                   |
|                                    | - Sổ Cái TK 911          |                                      |
|                                    | - Wireframe UI KetChuyen |                                      |
|                                    | - AG Grid filter         |                                      |
|                                    +--------------------------+                                      |
|                                                  |                                                   |
|                                                  v                                                   |
|                                    +--------------------------+                                      |
|                                    |         SPRINT 5         |                                      |
|                                    | TEST SUITE & REVIEW TOÀN |                                      |
|                                    | - Cập nhật 143+ tests cũ |                                      |
|                                    | - Full build 0 warnings  |                                      |
|                                    | - Standards & Spec Review|                                      |
|                                    +--------------------------+                                      |
|                                                                                                      |
+======================================================================================================+
```

---

## 🏃 SPRINT 1: NGUYÊN TẮC HỆ THỐNG & MÔ HÌNH DỮ LIỆU TÀI KHOẢN 911

- **Mục tiêu**: Loại bỏ triệt để xung đột giữa quy định cũ của repo với Thông tư 99/2025/TT-BTC, kích hoạt TK 911 trong mô hình dữ liệu.
- **Thời lượng ước tính**: 0.5 ngày.
- **Hạng mục công việc (Backlog Items)**:
  1. `SP1-01`: Cập nhật `AGENTS.md` (Dòng 23): Chuyển từ "Cấm 911" sang "Chuẩn hóa TK 911 theo TT99/2025/TT-BTC, bắt buộc kết chuyển sạch số dư cuối kỳ = 0".
  2. `SP1-02`: Cập nhật `GEMINI.md`: Đồng bộ nguyên tắc TK 911.
  3. `SP1-03`: Mở rộng enum `LoaiTaiKhoan` trong `Models/Entities/TaiKhoan.cs` thêm `XacDinhKetQuaKinhDoanh = 9`.
  4. `SP1-04`: Thêm seed data tài khoản `911 - Xác định kết quả kinh doanh` vào `Data/DbInitializer.cs`.
  5. `SP1-05`: Kiểm tra build và kiểm thử khởi tạo CSDL SQLite in-memory.
- **Định nghĩa Hoàn thành (DoD)**:
  - `dotnet build` đạt 0 warning, 0 error.
  - Test `DbInitializer` nạp thành công TK 911 mà không vi phạm ràng buộc DB.

---

## 🏃 SPRINT 2: ENGINE KẾT CHUYỂN TỰ ĐỘNG QUA TK 911 (TDD)

- **Mục tiêu**: Xây dựng lại logic kết chuyển cuối kỳ trong `PeriodClosingService` thành quy trình 3 chặng chuẩn mực của Bộ Tài chính.
- **Thời lượng ước tính**: 1 ngày.
- **Hạng mục công việc (Backlog Items)**:
  1. `SP2-01`: Viết Unit Test TDD `PeriodClosingTt99Tests.cs` (RED):
     - Kiểm tra kết chuyển doanh thu (511, 515, 711) sang Có 911.
     - Kiểm tra kết chuyển chi phí (632, 635, 641, 642, 811, 8211) sang Nợ 911.
     - Kiểm tra kết chuyển Lãi sang Nợ 911 / Có 4212.
     - Kiểm tra kết chuyển Lỗ sang Nợ 4212 / Có 911.
     - Assert: TK 911 có phát sinh và số dư cuối kỳ = 0.
  2. `SP2-02`: Tái cấu trúc phương thức `TaoButToanKetChuyenAsync` trong `PeriodClosingService.cs` (GREEN).
  3. `SP2-03`: Bổ sung tính toán và hạch toán tự động thuế TNDN hiện hành (`Nợ 911 / Có 8211`).
  4. `SP2-04`: Đảm bảo bất biến `TongNo == TongCo` trên toàn bộ chứng từ kết chuyển `PKT-KC-YYYYMM`.
- **Định nghĩa Hoàn thành (DoD)**:
  - Toàn bộ test trong `PeriodClosingTt99Tests` pass 100%.
  - Chứng từ kết chuyển sinh ra hạch toán chuẩn đối ứng 911.

---

## 🏃 SPRINT 3: BÓC TÁCH LƯỠNG TÍNH TRÊN TRIAL BALANCE 8 CỘT (TDD)

- **Mục tiêu**: Hoàn thiện thuật toán lập Bảng Cân đối tài khoản tuân thủ nguyên tắc không bù trừ (Non-Offsetting Rule) theo Khoản 1 Điều 7 Thông tư 99/2025/TT-BTC.
- **Thời lượng ước tính**: 1 ngày.
- **Hạng mục công việc (Backlog Items)**:
  1. `SP3-01`: Viết Unit Test TDD `GeneralLedgerReportTests.cs` (RED):
     - Test tình huống KH A dư Nợ 10M, KH B dư Có 5M trên TK 131.
     - Test NCC X dư Có 20M, NCC Y dư Nợ 8M trên TK 331.
     - Assert Trial Balance thể hiện đồng thời cả Dư Nợ và Dư Có, không cấn trừ ròng.
     - Assert dòng TK 911 xuất hiện với Dư đầu = 0, Dư cuối = 0, Tổng PS Nợ = Tổng PS Có.
  2. `SP3-02`: Nâng cấp phương thức `LayBangCanDoiTaiKhoanAsync` trong `GeneralLedgerService.cs` (GREEN):
     - Bóc tách số dư Nợ và Có chi tiết theo từng đối tượng pháp nhân cho nhóm TK lưỡng tính.
  3. `SP3-03`: Đảm bảo 3 cặp cột (Đầu kỳ, Phát sinh trong kỳ, Cuối kỳ) cân đối tuyệt đối (`CanDoiHoanToan == true`).
- **Định nghĩa Hoàn thành (DoD)**:
  - Test suite `GeneralLedgerReportTests` pass 100%.
  - Dữ liệu Trial Balance khớp với các báo cáo tài chính B01-DN và B02-DN.

---

## 🏃 SPRINT 4: GIAO DIỆN SỔ CÁI S03b-DN & TƯƠNG TÁC NGƯỜI DÙNG

- **Mục tiêu**: Cập nhật giao diện Web MVC để kế toán viên và kế toán trưởng có thể tra cứu Sổ Cái TK 911 và in ấn biểu mẫu chuẩn nộp Cơ quan Thuế.
- **Thời lượng ước tính**: 0.5 ngày.
- **Hạng mục công việc (Backlog Items)**:
  1. `SP4-01`: Cập nhật màn hình `Views/GeneralLedger/SoCai.cshtml`:
     - Tối ưu hóa hiển thị khi chọn tài khoản 911: phân nhóm rõ ràng dòng kết chuyển DT, dòng kết chuyển CP và dòng kết chuyển LNST.
  2. `SP4-02`: Cập nhật `Views/GeneralLedger/KetChuyenCuoiKy.cshtml`:
     - Bổ sung thông tin nhật ký kết chuyển chi tiết qua TK 911.
  3. `SP4-03`: Tinh chỉnh CSS print stylesheet cho mẫu biểu S03b-DN khi in khổ giấy A4 dọc.
- **Định nghĩa Hoàn thành (DoD)**:
  - Kế toán có thể tra cứu và xem chi tiết phát sinh TK 911 từ thanh điều hướng.
  - Giao diện in ấn hiển thị chuẩn quy cách Thông tư 99.

---

## 🏃 SPRINT 5: HIỆU CHỈNH REGRESSION TESTS & NGHIỆM THU REVIEW

- **Mục tiêu**: Rà soát toàn bộ các bài test cũ từng assert cấm 911, chạy kiểm thử hồi quy toàn diện và nghiệm thu code review.
- **Thời lượng ước tính**: 0.5 ngày.
- **Hạng mục công việc (Backlog Items)**:
  1. `SP5-01`: Cập nhật các test suite có assertion cấm 911:
     - `MultiDatabaseTests.cs`
     - `DepartmentPayrollPostingTests.cs`
     - `MariaDbMigrationTests.cs`
     - `CashReceiptTests.cs`
  2. `SP5-02`: Chạy `dotnet test` toàn bộ giải pháp (Đạt 145+ tests Green).
  3. `SP5-03`: Chạy `dotnet build` xác nhận 0 Warning, 0 Error.
  4. `SP5-04`: Thực hiện Dual-Axis Code Review (Standards Review & Spec Review).
  5. `SP5-05`: Đồng bộ CodeGraph và Git commit nghiệm thu hoàn tất Phase 10.
- **Định nghĩa Hoàn thành (DoD)**:
  - 100% tests passed.
  - Review 2 trục đạt chuẩn.
  - Hệ thống hoàn toàn sẵn sàng vận hành trên môi trường Production.
