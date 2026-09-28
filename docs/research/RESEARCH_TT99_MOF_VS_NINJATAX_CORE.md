# BÁO CÁO NGHIÊN CỨU & ĐỐI CHIẾU TOÀN DIỆN THÔNG TƯ 99/2025/TT-BTC TỪ BỘ TÀI CHÍNH (MOF.GOV.VN) VỚI CORE LOGIC NINJATAX

- **Tác giả thẩm định**: Lead Business Analyst (20+ năm ERP/Fintech) & Kế toán trưởng Doanh nghiệp (20+ năm thực chiến VAS/IFRS/Thanh tra Thuế).
- **Cơ sở pháp lý**: Thông tư số **99/2025/TT-BTC** ban hành ngày 27/10/2025 bởi Bộ Tài chính, có hiệu lực từ 01/01/2026, thay thế Thông tư 200/2014/TT-BTC, Thông tư 75/2015/TT-BTC, Thông tư 53/2016/TT-BTC và Thông tư 195/2012/TT-BTC.
- **Nguồn tài liệu nghiên cứu**: Cổng thông tin điện tử Bộ Tài chính (`mof.gov.vn`), Hệ thống Hỏi đáp Chính sách Tài chính (CSTC - MoF), Phụ lục I (33 biểu mẫu chứng từ kế toán), Phụ lục II (Danh mục Hệ thống Tài khoản kế toán doanh nghiệp), và Phụ lục III (Hệ thống Báo cáo Tài chính).

---

## 🏛️ PHẦN 1: TỔNG QUAN NỘI DUNG CỐT LÕI CỦA THÔNG TƯ 99/2025/TT-BTC

Thông tư 99/2025/TT-BTC là cuộc đại cải cách chế độ kế toán doanh nghiệp Việt Nam theo hướng:
1. **Tiệm cận IFRS & Chuẩn mực quốc tế**:
   - Tăng cường nguyên tắc giá trị hợp lý (Fair Value), bản chất hơn hình thức (Substance over Form).
   - Tự chủ trong việc lựa chọn đồng tiền hạch toán và quy đổi ngoại tệ sang VNĐ.
2. **Cấu trúc 3 Phụ lục chính thức**:
   - **Phụ lục I**: 33 biểu mẫu chứng từ kế toán thuộc 5 nhóm (Lao động tiền lương, Hàng tồn kho, Bán hàng, Tiền tệ, Tài sản cố định). Doanh nghiệp được chủ động thiết kế biểu mẫu nhưng phải đảm bảo đủ các yếu tố pháp lý bắt buộc của Luật Kế toán.
   - **Phụ lục II**: Danh mục Hệ thống tài khoản kế toán thống nhất áp dụng cho mọi loại hình doanh nghiệp.
   - **Phụ lục III**: Hệ thống Báo cáo Tài chính năm và giữa niên độ (B01-DN, B02-DN, B03-DN, B09-DN).

---

## 🔍 PHẦN 2: BẢNG SO SÁNH ĐỐI CHIẾU (GAP ANALYSIS) GIỮA TT 99/2025/TT-BTC VÀ CORE LOGIC HIỆN TẠI

```
+======================================================================================================================+
|                                    MA TRẬN ĐỐI CHIẾU TT 99/2025/TT-BTC VỚI CODEBASE NINJATAX                         |
+======================================================================================================================+
| TT | CHỦ ĐỀ NGHIỆP VỤ           | THÔNG TƯ 99/2025/TT-BTC (MOF.GOV.VN)      | CODE LOGIC HIỆN TẠI (NINJATAX) | ĐÁNH GIÁ/GAP |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 1  | Tài khoản 911              | BẮT BUỘC TỒN TẠI. Dùng tập hợp doanh thu  | BỊ CẤM TUYỆT ĐỐI (Strict Ban)  | ❌ SAI LỆCH   |
|    | (Xác định KQKD)            | và chi phí trong kỳ, không có số dư cuối  | trong AGENTS.md, DbInitializer,| NGHIÊM TRỌNG |
|    |                            | kỳ. Lãi/lỗ chuyển từ 911 -> 4212.         | kết chuyển thẳng 5/6/7/8->4212.| (Cần sửa gấp)|
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 2  | Báo cáo Tài chính          | Mẫu B01-DN (Báo cáo Tình hình Tài chính), | Đã implement B01-DN, B02-DN,   | ✅ TUÂN THỦ   |
|    | Biểu mẫu chuẩn             | B02-DN (KQKD), B03-DN (Lưu chuyển tiền    | B03-DN, B09-DN trong           | RẤT TỐT      |
|    |                            | tệ trực tiếp), B09-DN (Thuyết minh BCTC). | FinancialReportService.cs.     |              |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 3  | Nguyên tắc Không Bù Trừ    | Số dư TK lưỡng tính (131, 331, 333, 421)  | B01-DN đã bóc tách lưỡng tính  | ⚠️ CẦN HOÀN   |
|    | (Non-Offsetting Rule)      | phải bóc tách theo từng đối tượng pháp    | qua TinhSoDuLuongTinhAsync;    | THIỆN TRÊN   |
|    |                            | nhân, không bù trừ chéo khi lên BCTC.     | nhưng Trial Balance 8 cột      | TRIAL BALANCE|
|    |                            |                                           | còn bù trừ Net.                |              |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 4  | Tài khoản Chi phí          | Phân loại TK 641 (Bán hàng) và TK 642     | Đã hỗ trợ cả TK 641, 642, 6421,| ✅ TUÂN THỦ   |
|    | Bán hàng & QLDN            | (Quản lý DN), hoặc TK 6421 & 6422.        | 6422 trong danh mục TK.        | TỐT          |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 5  | Đánh giá Ngoại tệ & Tỷ giá | Tự chọn ngoại tệ hạch toán; đánh giá lại  | Đã có bảng ExchangeRateHistory,| ⚠️ CHƯA CÓ   |
|    | cuối kỳ                    | số dư tiền tệ ngoại tệ cuối năm qua TK 413| nhưng chưa có Service tự động  | AUTO ENGINE  |
|    |                            | theo tỷ giá mua/bán của NHTM nơi mở TK.   | revalue số dư TK 1112/1122.    | CHO TỶ GIÁ   |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 6  | Trích trước Sửa chữa lớn   | DỪNG trích trước chi phí sửa chữa lớn     | Chưa có module trích trước TK  | ℹ️ KHÔNG VI  |
|    | Tài sản cố định            | TSCĐ nếu chưa thực hiện (không dùng 335). | 335 cho SCL; chi phí ghi nhận  | PHẠM         |
|    |                            | Thực tế phát sinh mới phân bổ qua 242.    | thẳng hoặc phân bổ qua 242.    |              |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 7  | Doanh thu Phân bổ theo     | Phân bổ doanh thu theo giá trị hợp lý     | Bán hàng nhận doanh thu thời   | ℹ️ ĐỦ CHO SME|
|    | Nghĩa vụ Thực hiện (IFRS15)| từng nghĩa vụ đối với hợp đồng nhiều phần.| điểm xuất hóa đơn/giao hàng.   |              |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 8  | Tài sản sinh học (TK 215)  | Bổ sung TK 215 cho nông/lâm/thủy sản      | Chưa có TK 215 (hệ thống phục  | ℹ️ BACKLOG   |
|    |                            | (cây lâu năm/súc vật làm việc vẫn 211).   | vụ thương mại/dịch vụ/kho).    | MỞ RỘNG      |
+----+----------------------------+-------------------------------------------+--------------------------------+--------------+
| 9  | Khóa sổ & Đóng kỳ Kế toán  | Bắt buộc có mốc khóa sổ (Fiscal Lock),    | Đã có CauHinhKeToan.NgayKhoaSo | ✅ TUÂN THỦ   |
|    | (Fiscal Freeze)            | chứng từ kỳ đã chốt không được sửa/xóa.   | và kiểm tra chặn ghi lùi ngày. | TỐT          |
+======================================================================================================================+
```

---

## 📌 PHẦN 3: PHÂN TÍCH CHI TIẾT CÁC LỖ HỔNG (VULNERABILITY DETAILS)

### 1. Vấn đề Chí Mạng: Tài khoản 911 (Xác định Kết quả Kinh doanh)
- **Luật định TT 99/2025**:
  - TK 911 phản ánh toàn bộ doanh thu, thu nhập thuần và toàn bộ chi phí hợp lý phát sinh trong kỳ kế toán.
  - Sổ Cái TK 911 là chứng từ bắt buộc phải in ra giấy (Mẫu S03b-DN) để trình Đoàn kiểm toán độc lập và Đoàn thanh tra thuế khi quyết toán thuế TNDN.
  - Nếu không có TK 911, Thanh tra thuế sẽ coi là **sổ sách kế toán không ghi chép đầy đủ các bước hạch toán theo quy định**, áp dụng xử phạt từ 20.000.000 đến 30.000.000 VNĐ theo Điều 8 Nghị định 41/2018/NĐ-CP và có nguy cơ bị **ấn định thuế**.
- **Codebase hiện tại**:
  - `AGENTS.md` áp đặt quy tắc "Strict Ban 911" do hiểu lầm rằng TT99 bỏ TK 911. Thực tế TT99 chỉ bãi bỏ TT 200, nhưng **vẫn giữ nguyên TK 911** trong Phụ lục II.

### 2. Vấn đề Bóc Tách Hai Chiều TK Lưỡng Tính (131, 331) trên Trial Balance
- **Quy tắc TT 99**:
  - Bảng Cân đối tài khoản không được bù trừ giữa dư Nợ của Khách hàng A với dư Có của Khách hàng B.
  - Tổng số dư Nợ TK 131 trên Bảng Cân đối = Tổng số dư Nợ chi tiết của tất cả các khách hàng có dư Nợ.
  - Tổng số dư Có TK 131 trên Bảng Cân đối = Tổng số dư Có chi tiết của tất cả các khách hàng có dư Có.
- **Codebase hiện tại**:
  - Trong `FinancialReportService.cs` (B01-DN) đã làm đúng nguyên tắc này qua hàm `TinhSoDuNoTaiKhoanLuongTinhAsync`.
  - Nhưng trong `GeneralLedgerService.cs` (Trial Balance 8 cột) lại đang tính Net `(noDauKy - coDauKy)` theo mã tài khoản tổng quát, dẫn đến số dư trên Trial Balance bị cấn trừ phẳng.

---

## 🚀 PHẦN 4: KHUYẾN NGHỊ TÁI CẤU TRÚC CODEBASE TỪ LEAD BA

Để đưa `ninjaTax` đạt **100% Production Ready & Pháp lý Chuẩn mực Bộ Tài chính**, đề xuất lộ trình chuẩn hóa gồm 3 trọng tâm:

1. **Hiệu chỉnh Quy tắc Repo (AGENTS.md & GEMINI.md)**:
   - Gỡ bỏ dòng cấm TK 911.
   - Thay bằng quy tắc chuẩn: *"TK 911 bắt buộc sử dụng trong quy trình kết chuyển cuối kỳ; kết chuyển sạch không để dư cuối kỳ (`DuNo == 0 && DuCo == 0`)."*
2. **Kích hoạt TK 911 trong Hệ thống Tài khoản & Engine Kết Chuyển**:
   - Seed TK 911 vào `DbInitializer.cs`.
   - Cập nhật `PeriodClosingService.cs` ghi nhận qua 3 bước: DT sang Có 911; CP sang Nợ 911; Kết chuyển Lãi/Lỗ ròng từ 911 sang 4212.
3. **Hoàn thiện Bóc Tách Lưỡng Tính trên Trial Balance 8 Cột**:
   - Nhóm theo `DoiTuongId` đối với các TK 131, 331 trước khi tổng hợp số dư Nợ và Có lên Bảng Cân đối tài khoản.
