# CHIẾN LƯỢC KIỂM THỬ PHẦN MỀM (TESTING STRATEGY)
## HỆ THỐNG KẾ TOÁN & THUẾ DOANH NGHIỆP TINH GỌN - NINJATAX (TT99/2025/TT-BTC)

- **Dự án**: `ninjaTax`
- **Phiên bản tài liệu**: 1.0-PROD
- **Ngày ban hành**: 28/09/2026
- **Trạng thái**: Có hiệu lực
- **Phê duyệt bởi**: Lead Business Analyst & Principal Software Engineer (20+ năm kinh nghiệm ERP/Fintech & Thanh tra Thuế)
- **Vị trí lưu trữ**: `docs/test-strategy.md`

---

## 1. INTRODUCTION & PURPOSE (GIỚI THIỆU & MỤC ĐÍCH)

### 1.1. Bối cảnh & Giới thiệu
`ninjaTax` là ứng dụng web quản lý tài chính, kế toán doanh nghiệp và lá chắn rủi ro thanh tra thuế được phát triển trên nền tảng **ASP.NET Core MVC (.NET 10)** kết hợp **Entity Framework Core 10** và kiến trúc CSDL đa nền tảng (**Multi-DB: MariaDB 12+, SQLite, PostgreSQL, SQL Server**). Hệ thống được thiết kế đặc thù nhằm đáp ứng toàn diện và tức thời các quy định mới nhất của **Thông tư số 99/2025/TT-BTC** (Bộ Tài chính ban hành, có hiệu lực từ 01/01/2026), **Luật Kế toán số 88/2015/QH13**, **Chuẩn mực Kế toán Việt Nam (VAS)** và định hướng tiệm cận chuẩn mực quốc tế **IFRS**.

### 1.2. Mục đích Tài liệu
Tài liệu Chiến lược Kiểm thử này xác lập khung nguyên tắc, phương pháp luận, phạm vi, cấp độ kiểm thử và tiêu chuẩn chấp thuận (Acceptance Criteria) cho toàn bộ chu trình phát triển phần mềm (SDLC) của `ninjaTax`. Mục tiêu cốt lõi là bảo đảm 100% tính chính xác của dữ liệu tài chính (Zero Floating-point Drift), duy trì các bất biến kế toán nghiêm ngặt (Strict Accounting Invariants), loại bỏ hoàn toàn rủi ro sai lệch sổ sách khi thanh tra thuế và bảo đảm hệ thống sẵn sàng vận hành bền bỉ trên môi trường Production.

---

## 2. TEST OBJECTIVES (MỤC TIÊU KIỂM THỬ)

### 2.1. Functional Objectives (Mục tiêu Chức năng & Nghiệp vụ Kế toán)
1. **Bất biến Cân đối Kép (Double-Entry Equality)**:
   - 100% chứng từ phát sinh ghi sổ (General Ledger Vouchers) phải thỏa mãn:
     $$\sum \text{Phát Sinh Nợ (TongNo)} = \sum \text{Phát Sinh Có (TongCo)}$$
   - Tuyệt đối không cho phép ghi sổ chứng từ có sai số lệch Nợ/Có dù chỉ 1 đồng hoặc 0.0001 đơn vị tiền tệ.
2. **Chuẩn hóa Chu trình Kết chuyển Cuối kỳ (Account 911 Period Closing)**:
   - Toàn bộ doanh thu/thu nhập (5xx, 7xx) và chi phí/giá vốn (6xx, 8xx, 8211) phải được kết chuyển trung gian qua **Tài khoản 911** theo đúng Thông tư 99/2025/TT-BTC.
   - Bất biến đóng sổ: Sau khi chạy kết chuyển, số dư cuối kỳ của TK 911 phải tuyệt đối bằng 0 (`DuNo == 0 && DuCo == 0`).
   - Phân định ranh giới vận hành: Các chứng từ nghiệp vụ thông thường (thu, chi, bán hàng, mua hàng, khấu hao, bảng lương) tuyệt đối không được phép sử dụng TK 911.
3. **Bóc tách Số dư Hai chiều Không bù trừ (Two-Way Non-Offsetting Trial Balance)**:
   - Bảng Cân đối tài khoản 8 cột (Trial Balance) đối với các tài khoản lưỡng tính (`TinhChat = LuongTinh`: 131, 331, 138, 338, 333) phải tổng hợp số dư theo từng Đối tượng công nợ (`DoiTuongId`). Gross Nợ và Gross Có phải cùng hiển thị đồng thời, không bù trừ contra netting.
4. **Bảo toàn Hàng Tồn kho (VAS 02 - Anti-Negative Stock)**:
   - Xuất kho vật tư hàng hóa (FIFO, Bình quân tức thời, Bình quân cuối kỳ) và hủy phiếu nhập không được làm số lượng hoặc giá trị tồn kho tại bất kỳ kho nào rơi vào trạng thái âm (`TonKho >= 0`).
5. **Đối soát Khép kín Sổ phụ vs Sổ Cái (Subledger Reconciliation)**:
   - Số dư công nợ phải thu (TK 131), phải trả (TK 331), tồn kho (TK 152, 1561) và nguyên giá/khấu hao TSCĐ (TK 211, 214) trên phân hệ chi tiết phải khớp 100% với Sổ Cái tương ứng (`ChenhLech == 0`).

### 2.2. Performance Objectives (Mục tiêu Hiệu năng & Quy mô Dữ liệu)
1. **Độ trễ phản hồi trang (Page Response Time)**:
   - 95% các yêu cầu HTTP GET tra cứu Sổ Nhật Ký Chung, Sổ Cái và Bảng Cân Đối Tài Khoản phản hồi dưới **500ms** trên tập dữ liệu 100.000 dòng bút toán.
2. **Tốc độ Xử lý Ghi sổ theo Lô (Batch Posting Throughput)**:
   - Ghi sổ bảng lương 500 nhân sự (sinh 3 bút toán cân đối tự động) và tính khấu hao định kỳ hoàn thành dưới **2.000ms**.
3. **Độ chính xác Tiền tệ (Decimal Precision)**:
   - Sử dụng kiểu dữ liệu `decimal(19, 4)` trên MariaDB/MySQL/PostgreSQL/SQL Server. Zero floating-point drift, không xảy ra hiện tượng làm tròn sai lệch trên báo cáo tài chính B01-DN, B02-DN, B03-DN.

### 2.3. Security Objectives (Mục tiêu An ninh & Kiểm soát Rủi ro)
1. **Chống giả mạo & Xuyên thủng Dữ liệu (Tamper-proofing & Audit Trail)**:
   - Cơ chế khóa sổ kế toán (`NgayKhoaSo`): Cấm mọi hành vi thêm, sửa, xóa chứng từ có ngày hạch toán nhỏ hơn hoặc bằng ngày khóa sổ.
   - Tính toàn vẹn của Hệ thống tài khoản (COA): Nghiêm cấm xóa hoặc thay đổi mã tài khoản khi đã có giao dịch phát sinh trong `ChiTietButToans`.
2. **Phòng chống Lỗ hổng Web Phổ biến (OWASP Top 10)**:
   - 100% các request HTTP POST/PUT/DELETE bắt buộc xác thực token chống CSRF (`@Html.AntiForgeryToken()` / `[ValidateAntiForgeryToken]`).
   - Phòng chống SQL Injection thông qua việc sử dụng triệt để Entity Framework Core Parameterized Queries; không sử dụng chuỗi SQL ghép nối thô (raw concatenation).
   - Mã hóa XSS trên Razor Views thông qua cơ chế tự động HTML Encoding của ASP.NET Core.

### 2.4. Accessibility & Usability Objectives (Mục tiêu Tiện dụng & Chuẩn In ấn)
1. **Tốc độ Thao tác Bàn phím Kế toán (Keyboard-first Navigation)**:
   - Hỗ trợ đầy đủ phím tắt nghiệp vụ chuẩn: `F2` (Thêm mới), `Ctrl + S` (Lưu nháp), `F9` (Ghi sổ/Duyệt), `Esc` (Đóng/Hủy), `Enter`/`Tab` (Di chuyển ô lưới).
2. **Quy chuẩn In ấn Mẫu biểu Bắt buộc (Statutory Print Stylesheet)**:
   - Các mẫu biểu pháp lý: Sổ Nhật Ký Chung (S03a-DN), Sổ Cái (S03b-DN), Báo cáo Nhập-Xuất-Tồn (S10-DN) phải render chuẩn khổ giấy A4 dọc/ngang khi in (`@media print`), tự động ẩn thanh điều hướng, nút bấm thao tác và hiển thị đầy đủ tiêu đề, chữ ký của Giám đốc, Kế toán trưởng, Người lập biểu.

---

## 3. SCOPE (PHẠM VI KIỂM THỬ)

### 3.1. In-Scope Features (Phạm vi Kiểm thử Bắt buộc)
Hệ thống kiểm thử tập trung vào 10 phân hệ cốt lõi đã được xây dựng trong codebase:
1. **Phân hệ Tổng Hợp & Sổ Cái (General Ledger - Phase 9 & 10)**:
   - Sổ Nhật ký chung (S03a-DN), Sổ Cái tài khoản (S03b-DN), Bảng Cân đối tài khoản 8 cột (Trial Balance).
   - Động cơ kết chuyển cuối kỳ qua TK 911 và phân phối LNST vào TK 4212.
   - Bóc tách số dư 2 bên cho các tài khoản lưỡng tính 131, 331, 333.
   - Đối soát tự động Sổ phụ (Subledger) và Sổ cái (GL).
2. **Phân hệ Danh mục Hệ thống Tài khoản (COA - Phase 11)**:
   - Cây phân cấp tài khoản cha - con, kiểm tra tiền tố mã con, tự động kích hoạt `LaTaiKhoanSoCai = true` cho tài khoản mẹ.
   - Ràng buộc khóa đổi mã, cấm xóa tài khoản đã phát sinh bút toán và ngăn chặn vô hiệu hóa tài khoản mẹ khi còn con hoạt động.
3. **Phân hệ Kho & Hàng Tồn Kho (VAS 02 / TT99 - Phase 8)**:
   - Phiếu nhập kho, xuất kho, tính giá xuất kho (FIFO, Bình quân tức thời, Bình quân cuối kỳ).
   - Báo cáo S10-DN và thuật toán kiểm tra chống xuất âm kho (`ValidateStockAvailability`).
4. **Phân hệ Phòng Ban & Trung Tâm Chi Phí (Phase 7)**:
   - Cây cơ cấu tổ chức phòng ban, gán mã tài khoản chi phí mặc định (6421, 6422, 154) và hạch toán chi phí phân đoạn.
5. **Phân hệ Doanh Nghiệp, Đa Chi Nhánh & Khóa Sổ Kế Toán (Phase 6)**:
   - Quản trị pháp nhân, chi nhánh trực thuộc, thiết lập chính sách thuế/kế toán và khóa sổ kế toán.
6. **Phân hệ Mua Hàng & Bán Hàng (Phase 1 & 2)**:
   - Hóa đơn mua hàng (ghi nhận công nợ 331, thuế VAT 1331, kho 1561/152).
   - Hóa đơn bán hàng (ghi nhận doanh thu 511, thuế VAT 33311, công nợ 131, tự động trích giá vốn 632/156).
7. **Phân hệ Tiền Mặt & Ngân Hàng (Thu/Chi)**:
   - Phiếu thu (1111/1121 đối ứng 131), Phiếu chi (331/642 đối ứng 1111/1121).
   - Cơ chế cảnh báo an toàn chi âm quỹ và cảnh báo thanh toán tiền mặt $\ge 20$ triệu đồng.
8. **Phân hệ Tiền Lương & Bảo Hiểm Xã Hội (Payroll & Statutory Insurance)**:
   - Chấm công, bảng tính lương, tự động sinh 3 bút toán cân đối ghi sổ: Lương phải trả (642/334), Bảo hiểm DN gánh (642/338) và Trích trừ lương NLĐ (334/338, 334/3335).
9. **Phân hệ Tài Sản Cố Định & Khấu Hao (Fixed Assets & Depreciation)**:
   - Danh mục TSCĐ, trích khấu hao tự động theo Thông tư 45/2013/TT-BTC, sinh bút toán Sổ Cái Nợ 642/Có 214.
10. **Phân hệ Báo Cáo Tài Chính & Lá Chắn Rủi Ro Thuế (Tax Audit Shield)**:
    - B01-DN (Bảng cân đối kế toán), B02-DN (Báo cáo KQKD), B03-DN (Lưu chuyển tiền tệ trực tiếp), B09-DN (Thuyết minh BCTC).
    - Quyết toán thuế TNDN, kiểm soát tạm nộp 80% 4 quý và thuật toán phát hiện rủi ro thanh tra thuế.

### 3.2. Out-of-Scope Exclusions (Các Hạng mục Ngoại trừ & Lý do)
| Hạng mục Ngoại trừ | Lý do & Biện pháp Dự phòng |
|---|---|
| **Cổng kết nối Hóa đơn điện tử Trực tiếp (VNPT, Viettel, MISA meInvoice API)** | Cần chứng thư số USB Token / HSM thực tế và tài khoản kết nối Sandbox của nhà mạng. Hiện tại kiểm thử bằng Mock dữ liệu ký số XML/JSON. |
| **Cổng thanh toán Trực tuyến Ngân hàng (Open Banking / Napas)** | Phụ thuộc vào hợp đồng pháp lý mở cổng kết nối ngân hàng thương mại. Kiểm thử dừng ở mức luồng Giấy báo Có / Giấy báo Nợ nội bộ. |
| **OCR Quét Hóa đơn bằng Trí tuệ Nhân tạo (Machine Learning Invoice OCR)** | Mô hình AI OCR đang ở giai đoạn nghiên cứu (Phase 12+), chưa tích hợp vào core hạch toán production. |

---

## 4. TESTING APPROACH (PHƯƠNG PHÁP TIẾP CẬN KIỂM THỬ)

Chiến lược áp dụng mô hình **Kim tự tháp Kiểm thử (Test Pyramid)** kết hợp phương pháp **TDD (Test-Driven Development)**:

```
                      / \
                     /   \
                    / E2E \       <-- Playwright / UI Sanity (Khởi động trang, Forms)
                   /-------\
                  /  SYSTEM \     <-- Quy trình Kế toán Xuyên suốt (Order-to-Cash, Procure-to-Pay)
                 /-----------\
                / INTEGRATION \   <-- Multi-DB Seam (MariaDB, SQLite), EF Core Constraints
               /---------------\
              /      UNIT       \ <-- 148+ Tests: Domain Invariants, Math Formulas, Balance Tests
             /-------------------\
```

### 4.1. Test Levels (Các Cấp độ Kiểm thử)

#### 1. Unit Testing (Kiểm thử Đơn vị)
- **Mục tiêu**: Kiểm tra tính đúng đắn của từng hàm, công thức tính toán tài chính và các quy tắc nghiệp vụ độc lập.
- **Công cụ**: `xUnit 2.9.3`, `Microsoft.NET.Test.Sdk 17.14.1`, In-memory SQLite Database.
- **Tập trung**:
  - Tính hợp lệ bút toán: `ButToanService.KiemTraHopLe()` (TongNo == TongCo, bắt buộc chứng từ gốc).
  - Thuật toán khấu hao tài sản cố định: `FixedAssetDepreciationTests`.
  - Công thức tính thuế TNCN lũy tiến từng phần và tạm nộp thuế TNDN 80%.
  - Phân cấp cây tài khoản và kiểm soát tiền tố: `TaiKhoanServiceTests`.

#### 2. Integration Testing (Kiểm thử Tích hợp)
- **Mục tiêu**: Kiểm thử sự phối hợp giữa Service Layer, Entity Framework Core và Hệ quản trị CSDL vật lý.
- **Công cụ**: `Microsoft.EntityFrameworkCore.Sqlite`, `MySql.EntityFrameworkCore 10.0.9`.
- **Tập trung**:
  - `MultiDatabaseTests`: Khả năng hoán chuyển provider (`DatabaseProvider = MariaDb | Sqlite | PostgreSql | SqlServer`).
  - `MariaDbMigrationTests`: Kiểm tra kết nối thực tế tới MariaDB 12+ trên cổng 3306, chạy `EnsureCreatedAsync()`, nạp seed data và kiểm tra khóa chính bigint, kiểu số decimal(19, 4).
  - Khóa sổ kế toán: Kiểm tra tích hợp giữa `AccountingBookLockingTests` và tầng Repository khi cố tình ghi nhận chứng từ vào kỳ khóa sổ.

#### 3. System Testing (Kiểm thử Hệ thống Nghiệp vụ)
- **Mục tiêu**: Kiểm tra các chu trình kế toán khép kín từ đầu vào đến báo cáo tài chính.
- **Kịch bản kiểm thử (Accounting Business Cycles)**:
  - **Chu trình Bán hàng - Thu tiền (O2C)**: Tạo Hóa đơn bán hàng $\rightarrow$ Ghi sổ bút toán doanh thu/giá vốn $\rightarrow$ Xuất kho vật tư $\rightarrow$ Lập phiếu thu tiền mặt $\rightarrow$ Đối trừ công nợ $\rightarrow$ Kiểm tra Bảng Cân Đối Tài Khoản.
  - **Chu trình Mua hàng - Thanh toán (P2P)**: Lập Hóa đơn mua hàng $\rightarrow$ Nhập kho vật tư $\rightarrow$ Lập phiếu chi thanh toán $\rightarrow$ Kiểm tra sổ quỹ 1111 và công nợ người bán 331.
  - **Chu trình Tiền lương - Quyết toán**: Chấm công $\rightarrow$ Tính bảng lương $\rightarrow$ Ghi sổ lương/BHXH $\rightarrow$ Lập bảng kê quyết toán thuế TNCN mẫu 05-1/BK-QTT-TNCN.
  - **Chu trình Kết chuyển - Lập Báo cáo tài chính**: Quét toàn bộ doanh thu/chi phí $\rightarrow$ Kết chuyển qua TK 911 $\rightarrow$ Đóng sạch số dư TK 911 về 0 $\rightarrow$ Kết chuyển LNST sang TK 4212 $\rightarrow$ Kết xuất BCTC B01, B02, B03.

#### 4. End-to-End (E2E) & UI Verification
- **Mục tiêu**: Kiểm tra giao diện người dùng trên trình duyệt web, đảm bảo render đúng DOM, CSS, AG Grid và khả năng in ấn biểu mẫu.
- **Phương pháp**: HTTP Integration Testing thông qua ASP.NET Core `TestServer` / `WebApplicationFactory` kết hợp xác thực mã phản hồi HTTP 200 OK trên trình duyệt.

### 4.2. Testing Techniques (Kỹ thuật Thiết kế Ca Kiểm thử)
1. **Phân tích Giá trị Biên (Boundary Value Analysis - BVA)**:
   - Số dư tồn kho bằng đúng 0 (xuất vừa hết hàng) vs âm 0.0001 (bị chặn).
   - Thanh toán tiền mặt: 19.999.999 đ (cho phép) vs 20.000.000 đ (cảnh báo vi phạm điều kiện khấu trừ thuế GTGT).
   - Tỷ lệ tạm nộp thuế TNDN 4 quý: 79.99% (cảnh báo phạt nộp chậm) vs 80.00% (an toàn).
2. **Phân vùng Tương đương (Equivalence Partitioning - EP)**:
   - Các nhóm tài khoản: Tài sản (1xx, 2xx), Nợ phải trả (3xx), Vốn CSH (4xx), Doanh thu (5xx), Chi phí (6xx, 8xx), Kết quả kinh doanh (911).
   - Tính chất số dư: Dư Nợ, Dư Có, Lưỡng tính, Không có số dư.
3. **Bảng Quyết định & Kiểm thử Trạng thái (Decision Table & State Transition Testing)**:
   - Trạng thái chứng từ: `ChuaGhiSo` $\rightarrow$ `DaGhiSo` $\rightarrow$ `DaHuy`.
   - Trạng thái khóa sổ: `DangMoSo` vs `DaKhoaSo`.

---

## 5. TEST AUTOMATION (TỰ ĐỘNG HÓA KIỂM THỬ)

### 5.1. Ma trận Tự động hóa (What to Automate vs What Stays Manual)
| Hạng mục Kiểm thử | Mức độ Tự động hóa | Công cụ & Cơ chế Thực thi | Lý do Phân loại |
|---|---|---|---|
| **Logic Kế toán Core & Bất biến TT99** | 100% Automated | `dotnet test` (xUnit + EF Core In-Memory/SQLite) | Bắt buộc chạy tự động trước mọi commit để đảm bảo không hồi quy logic tài chính. |
| **Chuyển đổi Provider CSDL (Multi-DB)** | 100% Automated | `MultiDatabaseTests.cs` | Xác thực tính độc lập CSDL (MariaDB, PostgreSQL, SQLite, SQL Server). |
| **Kiểm tra Schema & Di chuyển CSDL** | 100% Automated | `MariaDbMigrationTests.cs` | Đảm bảo seed data và cấu trúc bảng trên MariaDB 12+ luôn sẵn sàng. |
| **Kiểm tra Mã Biên dịch & Cảnh báo** | 100% Automated | `dotnet build` (`TreatWarningsAsErrors=true`) | Đạt chuẩn 0 Warning, 0 Error trước khi bàn giao. |
| **Trải nghiệm Người dùng & Phím tắt UI** | Bán tự động (Semi-Automated) | Kiểm tra trực quan trên Web (F2, F9, Tab, Enter) | Thẩm định độ mượt của kế toán viên khi thao tác nhập liệu tốc độ cao. |
| **Kiểm tra Quy cách Bản in Giấy (A4 Print)** | Manual Inspection | Trình xem trước bản in trình duyệt (`Ctrl + P`) | Xác định ngắt trang, canh lề biểu mẫu chuẩn thanh tra thuế. |

### 5.2. Công cụ Tự động hóa (Test Tooling Stack)
- **Framework kiểm thử**: `xUnit 2.9.3`.
- **Test Runner & SDK**: `Microsoft.NET.Test.Sdk 17.14.1`, `xunit.runner.visualstudio 3.1.4`.
- **Code Coverage Collector**: `coverlet.collector 6.0.4`.
- **Database Mocking**: `Microsoft.EntityFrameworkCore.Sqlite 10.0.12`.
- **Physical RDBMS Testing**: `MySql.EntityFrameworkCore 10.0.9` kết nối máy chủ MariaDB nội bộ.

---

## 6. TEST ENVIRONMENTS (MÔI TRƯỜNG KIỂM THỬ)

| Tiêu chí | Môi trường Kiểm thử Cục bộ (Local Dev / Test) | Môi trường Kiểm thử Tích hợp (Staging / CI) | Môi trường Sản xuất (Production) |
|---|---|---|---|
| **Hệ điều hành** | Windows 11 / Linux Ubuntu | Linux Docker Container | Windows Server 2025 / Linux Ubuntu Server |
| **.NET Runtime** | .NET 10 (`net10.0.401`) | .NET 10 SDK / Runtime | .NET 10 ASP.NET Core Runtime |
| **Hệ Quản trị CSDL** | SQLite In-Memory + MariaDB 12+ Local | MariaDB 12+ Dedicated Test Instance | MariaDB 12+ Enterprise Cluster (Port 3306) |
| **Chuỗi Kết nối DB** | `Data Source=:memory:` & `Server=localhost;Port=3306;Database=ninjataxdb` | Dynamic CI Secret Connection String | Bảo mật qua Vault / Environment Variables |
| **Dữ liệu Kiểm thử** | `DbInitializer.SeedDataAsync()` | Bộ dữ liệu giả lập 1 năm tài chính đầy đủ | Dữ liệu thực của Doanh nghiệp (Mã hóa SSL) |
| **Cổng Dịch vụ (Port)** | `http://localhost:5188` | `http://staging.ninjatax.vn:5188` | `https://app.ninjatax.vn` (SSL 443) |

---

## 7. ENTRY & EXIT CRITERIA (TIÊU CHÍ BẮT ĐẦU & KẾT THÚC)

### 7.1. Entry Criteria (Điều kiện Bắt đầu Kiểm thử)
1. Mã nguồn biên dịch thành công 100% bằng lệnh `dotnet build` với **0 cảnh báo (0 Warnings)** và **0 lỗi (0 Errors)** (`TreatWarningsAsErrors = true`).
2. Môi trường CSDL MariaDB/SQLite đã được khởi tạo schema đầy đủ qua `EnsureCreatedAsync()` hoặc Migrations.
3. Toàn bộ các yêu cầu nghiệp vụ và tài liệu BRD/Spec tương ứng đã được phê duyệt.

### 7.2. Exit Criteria (Điều kiện Đạt Chuẩn Hoàn thành - Definition of Done)
1. **100% Unit Tests & Integration Tests PASS** (Tối thiểu 148/148 tests Green, 0 failed, 0 skipped).
2. **Không còn bất kỳ khiếm khuyết mức Nghiêm trọng (Blocker/Critical)** liên quan đến bất biến kế toán:
   - Không có chứng từ lệch Nợ/Có (`TongNo != TongCo`).
   - Không có bút toán thường nào sử dụng TK 911.
   - Tài khoản 911 sau kết chuyển phải có số dư bằng 0.
   - Không có hiện tượng âm kho trái quy định VAS 02.
3. **Độ bao phủ mã nguồn (Code Coverage)** đối với Service Layer đạt tối thiểu **85%**.
4. **Báo cáo Code Review Đạt chuẩn**: Đã thực hiện kiểm tra chéo 2 trục (Standards Review & Spec Review) và giải quyết triệt để các phản biện chuyên môn.
5. **CodeGraph và Git Sync**: Đồng bộ chỉ mục CodeGraph thành công và hoàn thành commit sạch trên nhánh `master`.

---

## 8. RISK ASSESSMENT & MITIGATION (ĐÁNH GIÁ & GIẢM THIỂU RỦI RO)

| # | Rủi ro Tiềm ẩn (Risk Event) | Mức độ | Tác động Nghiệp vụ / Kỹ thuật | Biện pháp Phòng ngừa & Giảm thiểu (Mitigation Strategy) |
|---|---|:---:|---|---|
| **R1** | **Sai số trôi nổi tiền tệ (Floating Drift)** | Cao | Số dư sổ cái bị lệch vài xu/hào, làm mất cân đối Bảng Cân Đối Tài Khoản khi thanh tra thuế. | Sử dụng tuyệt đối kiểu `decimal(19, 4)` trong C# và cấu hình EF Core `HasPrecision(19, 4)`. Tuyệt đối cấm dùng `float` hay `double` cho các trường tiền tệ. |
| **R2** | **Xuyên thủng kỳ đã khóa sổ kế toán** | Rất Cao | Doanh nghiệp bị phạt vi phạm hành chính từ 20-30 triệu đồng theo NĐ 41/2018/NĐ-CP do sửa đổi số liệu báo cáo đã nộp cơ quan thuế. | Tầng Service (`ButToanService`, `ThuChiService`) cài đặt bất biến kiểm tra: Nếu `NgayHachToan <= NgayKhoaSo` $\rightarrow$ Chặn đứng và ném ngoại lệ ngay lập tức. |
| **R3** | **Lệch công nợ do contra-netting sai quy định** | Cao | Sai lệch Báo cáo tài chính B01-DN (Khoản 1 Điều 7 TT99 nghiêm cấm bù trừ số dư khách hàng khác nhau). | Thiết lập bài kiểm thử `LayBangCanDoiTaiKhoanAsync_TwoWayAccounts_CalculatesNonOffsettingBalances`, bóc tách độc lập Dư Nợ và Dư Có theo `DoiTuongId`. |
| **R4** | **Xuất âm kho vật tư hàng hóa** | Trung bình | Giá vốn hàng bán tính sai lệch, cơ quan thuế ấn định chi phí không hợp lý. | Kiểm tra tiền điều kiện tồn kho khả dụng (`ValidateStockAvailabilityAsync`) trước khi cho phép tạo phiếu xuất kho. |
| **R5** | **Gãy toàn vẹn dữ liệu khi xóa tài khoản kế toán** | Cao | Các dòng chi tiết bút toán mồ côi tài khoản, gãy báo cáo Sổ Cái. | Cài đặt bất biến kiểm tra trong `TaiKhoanService.XoaAsync`: Nếu tồn tại bản ghi trong `ChiTietButToans` $\rightarrow$ Nghiêm cấm xóa tuyệt đối. |

---

## 9. ROLES & RESPONSIBILITIES (VAI TRÒ & TRÁCH NHIỆM)

```
+--------------------------------------------------------------------------------------------------+
|                              MA TRẬN TRÁCH NHIỆM KIỂM THỬ (RACI MATRIX)                          |
+----------------------+--------------------+--------------------+--------------------+------------+
| Hạng mục Công việc   | Lead BA / KT Trưởng| Software Architect | Developer / TDD Eng| QA / Tester|
+----------------------+--------------------+--------------------+--------------------+------------+
| Định nghĩa Tiêu chí  |        Accountable |        Consulted   |        Informed    | Responsible|
| Nghiệp vụ & Bất biến |                    |                    |                    |            |
+----------------------+--------------------+--------------------+--------------------+------------+
| Viết Unit Tests (TDD)|        Consulted   |        Accountable |        Responsible | Responsible|
+----------------------+--------------------+--------------------+--------------------+------------+
| Kiểm thử Tích hợp DB |        Informed    |        Accountable |        Responsible | Responsible|
+----------------------+--------------------+--------------------+--------------------+------------+
| Kiểm tra Mẫu biểu In |        Accountable |        Informed    |        Responsible | Responsible|
+----------------------+--------------------+--------------------+--------------------+------------+
| Ký duyệt Release PROD|        Accountable |        Accountable |        Informed    | Informed   |
+----------------------+--------------------+--------------------+--------------------+------------+
```

1. **Lead Business Analyst & Kế toán trưởng**:
   - Thẩm định tính tuân thủ pháp lý theo Thông tư 99/2025/TT-BTC và chuẩn mực VAS/IFRS.
   - Định nghĩa các bất biến kế toán (Account 911 rules, Double-entry balance, Non-offsetting balances).
   - Nghiệm thu cuối cùng các biểu mẫu tài chính nộp cơ quan thuế.
2. **Software Architect & Lead Engineer**:
   - Giám sát kiến trúc Multi-DB seam và độ chính xác của các kiểu dữ liệu EF Core.
   - Thực hiện quy trình Dual-Axis Code Review (Standards & Spec).
   - Duy trì nguyên tắc Zero Warnings và tối ưu hiệu năng truy vấn Sổ Cái.
3. **Developer / TDD Engineer**:
   - Thực thi chu trình Red-Green-Refactor cho mọi chức năng mới.
   - Viết các ca kiểm thử hồi quy bảo vệ bất biến hệ thống trước khi gửi code review.
4. **QA / Test Engineer**:
   - Thực hiện kiểm thử toàn trình (E2E), thẩm định phím tắt và trải nghiệm giao diện người dùng.

---

## 10. REPORTING & METRICS (BÁO CÁO & CHỈ SỐ ĐO LƯỜNG)

### 10.1. Chỉ số Đo lường Chất lượng Cốt lõi (Quality Metrics)
1. **Tỷ lệ Kiểm thử Thành công (Test Pass Rate)**:
   $$\text{Pass Rate} = \frac{\text{Số lượng Tests Thành Công}}{\text{Tổng số Tests}} \times 100\% = 100\%$$
   *(Tiêu chuẩn: Luôn duy trì 100% Pass Rate trên nhánh `master`).*
2. **Số lượng Cảnh báo Biên dịch (Compiler Warnings Count)**:
   - Tiêu chuẩn: **Bắt buộc = 0 Warning** (Cấu hình `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`).
3. **Mức độ Che phủ Mã nguồn (Code Coverage)**:
   - Core Services (`GeneralLedgerService`, `PeriodClosingService`, `TaiKhoanService`, `InventoryService`): $\ge 85\%$.
   - Controllers: $\ge 70\%$.
4. **Mật độ Khiếm khuyết Tài chính (Financial Defect Density)**:
   - Mục tiêu: **0 lỗi** trên môi trường Staging/Production liên quan đến cân đối kế toán.

### 10.2. Quy trình Báo cáo Kiểm thử (Reporting Protocol)
- **Sau mỗi lần thực thi kiểm thử cục bộ**:
  - Chạy `dotnet test --logger "console;verbosity=normal"`.
  - Kết quả báo cáo tóm tắt: Tổng số tests, Passed, Failed, Skipped, Thời gian thực thi.
- **Trước khi tạo bản phát hành (Release Gate)**:
  - Xuất báo cáo kiểm thử hoàn chỉnh lưu kèm hồ sơ kỹ thuật của phiên bản.
  - Biên bản nghiệm thu kỹ thuật xác nhận đầy đủ 10 phân hệ nghiệp vụ đã vượt qua kiểm thử hồi quy.
