# ninjaTax UX/UI Redesign — Design Spec
**Date:** 2026-09-27  
**Approach:** Progressive Enhancement with Lightweight Libraries (Approach A)  
**Architecture:** ASP.NET Core MVC Razor + JS enhancement layer

---

## Design Principles

1. **Keyboard-first, mouse-optional** — every action reachable without mouse
2. **Compact density** — enterprise data density, not consumer whitespace
3. **Vietnamese-native** — numbers, dates, search tuned for VN accountants
4. **Progressive enhancement** — pages work without JS, JS adds power features
5. **Reusable design tokens** — CSS custom properties, shared partials, one JS bundle

---

## Technology Stack

| Layer | Library | Version | Size (gzip) | License |
|-------|---------|---------|-------------|---------|
| Data Grid | AG Grid Community | 33.x | ~300KB | MIT |
| Charts | ApexCharts | 4.x | ~120KB | MIT |
| Number/Date formatting | Browser `Intl` API | native | 0KB | — |
| Keyboard shortcuts | Custom `ninjatax-keys.js` | — | ~2KB | — |
| Diacritics search | Server-side + `ninjatax-search.js` | — | ~1KB | — |
| CSS Design System | Custom `ninjatax-theme.css` + Bootstrap 5 | — | ~5KB | — |
| Column persistence | `localStorage` | native | 0KB | — |

---

## Slice 1: Design System Foundation (CSS + Layout)

### Goal
Establish the visual foundation all other slices build on. No JS, pure CSS.

### Deliverables

#### 1.1 Color Palette (`ninjatax-theme.css`)
```css
:root {
  /* Primary — calming blue */
  --nt-primary: #2563EB;
  --nt-primary-light: #DBEAFE;
  --nt-primary-dark: #1E40AF;

  /* Neutral — for backgrounds, borders, text */
  --nt-bg: #F8FAFC;
  --nt-bg-card: #FFFFFF;
  --nt-border: #E2E8F0;
  --nt-text: #1E293B;
  --nt-text-muted: #64748B;

  /* Workflow status */
  --nt-draft: #F59E0B;       /* amber/yellow */
  --nt-draft-bg: #FFFBEB;
  --nt-posted: #10B981;      /* green */
  --nt-posted-bg: #ECFDF5;
  --nt-cancelled: #EF4444;   /* red */
  --nt-cancelled-bg: #FEF2F2;

  /* Spacing — compact enterprise density */
  --nt-spacing-xs: 0.25rem;   /* 4px */
  --nt-spacing-sm: 0.5rem;    /* 8px */
  --nt-spacing-md: 0.75rem;   /* 12px */
  --nt-spacing-lg: 1rem;      /* 16px */

  /* Typography */
  --nt-font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  --nt-font-size-sm: 0.8125rem;   /* 13px — compact tables */
  --nt-font-size-base: 0.875rem;  /* 14px — body text */
  --nt-font-size-lg: 1rem;        /* 16px — headings */
}
```

#### 1.2 Compact Table Styles
- Row height: 36px (vs Bootstrap default 48px)
- Font size: 13px for table cells
- Alternating row colors: `--nt-bg` / `--nt-bg-card`
- Hover: subtle blue tint `--nt-primary-light`

#### 1.3 Form Styles
- Input height: 34px (compact)
- Label: 13px, `--nt-text-muted`, uppercase tracking
- Focus ring: `--nt-primary` 2px outline
- Error state: `--nt-cancelled` border + red help text below
- Required field: subtle red asterisk after label

#### 1.4 Card & Page Layout
- Page background: `--nt-bg` (#F8FAFC)
- Content cards: white with 1px `--nt-border`, 4px radius, 16px padding
- Page header: title + breadcrumb + action buttons row

#### 1.5 Workflow Status Badges
```html
<span class="nt-badge nt-badge--draft">Nháp</span>
<span class="nt-badge nt-badge--posted">Đã ghi sổ</span>
<span class="nt-badge nt-badge--cancelled">Đã hủy</span>
```

#### Files touched
- `wwwroot/css/ninjatax-theme.css` — new
- `wwwroot/css/site.css` — update to import theme
- `Views/Shared/_Layout.cshtml` — add theme CSS link + Inter font

#### Verification
- Visual inspection: all pages use new palette
- No broken layouts
- `dotnet build` passes

---

## Slice 2: Keyboard Navigation System

### Goal
Accountants never leave the keyboard during data entry.

### Deliverables

#### 2.1 Global Shortcut Registry (`ninjatax-keys.js`)
```javascript
// Shortcut map — extensible per page
const SHORTCUTS = {
  'F2':      () => document.querySelector('[data-action="create"]')?.click(),
  'Ctrl+S':  () => document.querySelector('[data-action="save"]')?.click(),
  'F9':      () => document.querySelector('[data-action="post"]')?.click(),
  'Escape':  () => document.querySelector('[data-action="cancel"]')?.click(),
};
```

#### 2.2 Tab/Enter Field Navigation
- `Enter` on any input advances to next `[data-field-order]` input
- `Tab` follows native browser order (same sequence)
- `Shift+Tab` goes back
- Custom `data-field-order="1"` attributes on form inputs define the business flow

#### 2.3 Auto-Focus
- On page load, focus the element with `data-autofocus` or `data-field-order="1"`
- On modal open, focus first input inside modal

#### 2.4 Shortcut Help Overlay
- `?` key opens a floating help panel showing all available shortcuts
- Semi-transparent overlay, dismissable with `Escape`

#### 2.5 Visual Shortcut Hints
- Action buttons show shortcut hint: `Thêm mới (F2)`, `Lưu (Ctrl+S)`, `Ghi sổ (F9)`
- Hints in muted small text below or beside the button

#### Files touched
- `wwwroot/js/ninjatax-keys.js` — new
- `Views/Shared/_Layout.cshtml` — add script reference
- All form views — add `data-field-order`, `data-autofocus`, `data-action` attributes

#### Verification
- Manual test: open PhieuThu/Create, type F2 → form opens, Tab through fields, Ctrl+S saves
- No conflicts with browser defaults (F5 refresh still works)

---

## Slice 3: Vietnamese Localization Layer

### Goal
All numbers, dates, and search behave Vietnamese-native.

### Deliverables

#### 3.1 Number Formatting (`ninjatax-locale.js`)
```javascript
// Format: 1.234.567,89
function formatVND(value) {
  return new Intl.NumberFormat('vi-VN').format(value);
}

// Parse back: "1.234.567,89" → 1234567.89
function parseVND(str) {
  return parseFloat(str.replace(/\./g, '').replace(',', '.'));
}
```
- Applied to all `<input type="text" data-format="currency">` fields
- Format on blur, parse on focus (show raw number for editing)
- Real-time formatting in display-only cells

#### 3.2 Date Format
- All date inputs use `DD/MM/YYYY` format
- `<input type="text" data-format="date">` with input mask
- Validation: reject invalid dates, future dates where applicable
- Integration with existing `[DataType(DataType.Date)]` model annotations

#### 3.3 Diacritics-Insensitive Search (Server-Side)
- Add `TenKhongDau` (unaccented name) computed/shadow column to searchable entities
- Use C# `RemoveDiacritics()` utility on save
- MariaDB query: `WHERE TenKhongDau LIKE @search OR Ten LIKE @search`
- Client-side: `ninjatax-search.js` strips diacritics from search input before sending

#### 3.4 Locale Configuration
```javascript
const NT_LOCALE = {
  thousandSeparator: '.',
  decimalSeparator: ',',
  dateFormat: 'DD/MM/YYYY',
  currency: 'VNĐ',
  currencyPosition: 'suffix',
};
```

#### Files touched
- `wwwroot/js/ninjatax-locale.js` — new
- `wwwroot/js/ninjatax-search.js` — new
- `Models/Entities/` — add `TenKhongDau` to `NhanVien`, `VatTuHangHoa`, `DoiTuong`
- `Data/AppDbContext.cs` — computed column config
- `Models/Services/` — search methods updated

#### Verification
- Unit test: `formatVND(1234567.89)` → `"1.234.567,89"`
- Unit test: `parseVND("1.234.567,89")` → `1234567.89`
- Unit test: search "nguyen" returns entities with "Nguyễn"
- `dotnet test` passes

---

## Slice 4: AG Grid Integration (Power Data Grids)

### Goal
Replace all `<table>` list views with AG Grid for enterprise-grade data handling.

### Deliverables

#### 4.1 AG Grid Setup
- Install AG Grid Community via CDN or npm
- Create `ninjatax-grid.js` wrapper with default config:
  - Vietnamese locale strings
  - Default column types (currency, date, text)
  - Compact row height (36px)
  - Theme: `ag-theme-alpine` customized with `--nt-*` variables

#### 4.2 Grid Features (out-of-box from AG Grid)
- **Frozen columns**: `pinned: 'left'` for SoChungTu, MaDoiTuong
- **Column filters**: `filter: 'agTextColumnFilter'` / `'agNumberColumnFilter'`
- **Column reorder**: `enableColResize: true, enableColMove: true`
- **Column visibility**: right-click context menu to show/hide columns
- **Virtual scrolling**: `rowModelType: 'infinite'` for 10k+ rows
- **Sorting**: multi-column sort with Shift+click

#### 4.3 Column Persistence
```javascript
// Save to localStorage on column change
gridOptions.onColumnMoved = (e) => saveColumnState(gridId);
gridOptions.onColumnResized = (e) => saveColumnState(gridId);
gridOptions.onColumnVisible = (e) => saveColumnState(gridId);

// Restore on page load
function restoreColumnState(gridId) {
  const state = localStorage.getItem(`nt-grid-${gridId}`);
  if (state) gridOptions.api.applyColumnState(JSON.parse(state));
}
```

#### 4.4 Server-Side Data Source
- Each list controller exposes a JSON API endpoint: `GET /api/{Entity}/list?page=1&pageSize=50&sort=Date&filter=...`
- AG Grid's `datasource.getRows()` calls this endpoint
- Response: `{ rows: [...], totalCount: 12345 }`

#### 4.5 Migration Pattern
- Create `_AgGrid.cshtml` partial with standard grid container
- Each Index view replaces `<table>` with grid div + column definition
- Prioritize high-traffic views first: ButToan/Index, BanHang/Index, MuaHang/Index

#### Files touched
- `wwwroot/lib/ag-grid/` — AG Grid CSS + JS (CDN or local)
- `wwwroot/js/ninjatax-grid.js` — new wrapper
- `wwwroot/css/ninjatax-grid.css` — AG Grid theme overrides
- `Views/Shared/_AgGrid.cshtml` — new partial
- `Views/ButToan/Index.cshtml` — migrate first
- Controllers — add JSON API endpoints

#### Verification
- ButToan/Index loads 10k rows without browser hang
- Frozen SoChungTu column stays visible on horizontal scroll
- Column reorder persists across page refresh
- Filter by amount > 1,000,000 works
- `dotnet build` + `dotnet test` pass

---

## Slice 5: Workflow Status UI

### Goal
Visual clarity on document lifecycle — Draft → Posted → Cancelled.

### Deliverables

#### 5.1 Status Badge Component
Already defined in Slice 1 CSS. Now wire to data:
```csharp
public enum TrangThaiChungTu
{
    Nhap = 0,      // Draft
    DaGhiSo = 1,   // Posted
    DaHuy = 2       // Cancelled
}
```

#### 5.2 Action Button Layout
```html
<div class="nt-action-bar">
  <div class="nt-action-bar__secondary">
    <a data-action="cancel" class="btn btn-outline-secondary">
      Hủy bỏ <small class="text-muted">(Esc)</small>
    </a>
  </div>
  <div class="nt-action-bar__primary">
    <button data-action="save" class="btn btn-outline-primary me-2">
      <i class="bi bi-save"></i> Lưu nháp <small class="text-muted">(Ctrl+S)</small>
    </button>
    <button data-action="post" class="btn btn-success">
      <i class="bi bi-check-circle"></i> Ghi sổ <small class="text-muted">(F9)</small>
    </button>
  </div>
</div>
```
- "Lưu nháp" = outline style (secondary action)
- "Ghi sổ" = solid green (primary action, visually dominant)
- Physical separation prevents accidental clicks
- Keyboard shortcuts shown inline

#### 5.3 Status-Based Field Locking
- When `TrangThai == DaGhiSo`: all form fields become `readonly`, action bar shows "Bỏ ghi sổ" button
- When `TrangThai == DaHuy`: fields readonly, grey overlay, no action buttons
- Draft: full editing enabled

#### 5.4 Status Filter on List Pages
- AG Grid filter preset buttons: "Tất cả | Nháp | Đã ghi sổ | Đã hủy"
- Color-coded row backgrounds in grid matching status

#### Files touched
- `wwwroot/css/ninjatax-theme.css` — action bar styles
- `Views/Shared/_ActionBar.cshtml` — new partial
- All Create/Edit views — use `_ActionBar` partial
- `Models/Entities/` — add `TrangThaiChungTu` enum if not exists
- Grid column definitions — add status column with cell renderer

#### Verification
- Draft document: all fields editable, both buttons visible
- Posted document: fields locked, "Bỏ ghi sổ" button shown
- Cancelled document: grey overlay, no actions
- Status badges render correct colors in grid

---

## Slice 6: Smart Error Handling & Validation

### Goal
Errors speak Vietnamese accountant language, not HTTP codes.

### Deliverables

#### 6.1 Business Error Messages
Replace generic errors with domain-specific messages:
```csharp
// Instead of: "Invalid balance"
// Show: "Tài khoản 1111 không đủ số dư để chi (Thiếu 5.000.000đ)"
public class BusinessException : Exception
{
    public string UserMessage { get; }
    public string? FieldName { get; }
}
```

#### 6.2 Inline Field Validation
```css
.nt-field--error input {
  border-color: var(--nt-cancelled);
  background-color: var(--nt-cancelled-bg);
}
.nt-field--error .nt-field__help {
  color: var(--nt-cancelled);
  font-size: var(--nt-font-size-sm);
  margin-top: var(--nt-spacing-xs);
}
```
- Red border + light red background on invalid fields
- Help text appears below: "Mã số thuế phải có 10 hoặc 13 chữ số"
- Required fields show red asterisk in label

#### 6.3 Toast Notifications
- Success: green toast "Phiếu thu PT-001 đã lưu thành công"
- Warning: amber toast "Chênh lệch Nợ - Có: 500.000đ"
- Error: red toast with business message
- Auto-dismiss after 5 seconds, manual dismiss with × button

#### 6.4 Real-Time Balance Check
- On journal entry forms: live `TổngNợ - TổngCó` display
- Updates as user types amounts
- Shows green checkmark when balanced, red warning when not
- Prevents form submission if unbalanced

#### 6.5 Global Error Handler
- Replace ASP.NET default error page with styled Vietnamese error page
- Log technical details server-side, show user-friendly message client-side
- "Đã xảy ra lỗi. Vui lòng thử lại hoặc liên hệ quản trị viên."

#### Files touched
- `wwwroot/css/ninjatax-theme.css` — error/toast styles
- `wwwroot/js/ninjatax-toast.js` — new
- `wwwroot/js/ninjatax-validation.js` — new
- `Models/Services/` — `BusinessException` class
- `Views/Shared/Error.cshtml` — redesign
- `Views/Shared/_ValidationSummary.cshtml` — new partial with styled errors

#### Verification
- Unit test: posting with insufficient balance returns `BusinessException` with Vietnamese message
- UI test: invalid MST field shows red border + help text
- Toast appears on successful save
- Balance indicator works real-time on ButToan/Create

---

## Slice 7: Dashboard & Drill-Down Reports

### Goal
Sếp sees business health at a glance. Accountant drills from summary to source document.

### Deliverables

#### 7.1 Dashboard Layout (`Home/Index.cshtml`)
```
┌─────────────────────────────────────────────────────┐
│  KPI Cards (4 across)                               │
│  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐              │
│  │Doanh │ │Chi   │ │Phải  │ │Phải  │              │
│  │thu   │ │phí   │ │thu   │ │trả   │              │
│  │tháng │ │tháng │ │(131) │ │(331) │              │
│  └──────┘ └──────┘ └──────┘ └──────┘              │
│                                                     │
│  ┌─────────────────────┐ ┌─────────────────────┐   │
│  │ Dòng tiền 12 tháng  │ │ Top 10 Công nợ      │   │
│  │ (Line chart)        │ │ (Horizontal bar)     │   │
│  └─────────────────────┘ └─────────────────────┘   │
│                                                     │
│  ┌─────────────────────┐ ┌─────────────────────┐   │
│  │ Cơ cấu Chi phí      │ │ Doanh thu theo      │   │
│  │ (Donut chart)       │ │ tháng (Bar chart)    │   │
│  └─────────────────────┘ └─────────────────────┘   │
└─────────────────────────────────────────────────────┘
```

#### 7.2 Drill-Down Chain
```
B01 (Bảng CĐKT)
  → Click TK 131 total
    → CongNo/Index?taikhoan=131 (list of customers with balances)
      → Click customer "Công ty ABC"
        → CongNo/Details?doituong=ABC (list of invoices for that customer)
          → Click invoice "HD-001"
            → BanHang/Details/123 (source document)
```
- Each drill-down level passes filter params via query string
- Breadcrumb updates: `Dashboard > Công nợ TK 131 > Công ty ABC > HD-001`
- Back button preserves drill-down context

#### 7.3 ApexCharts Integration
- `ninjatax-charts.js` wrapper with default Vietnamese locale
- Chart click events trigger drill-down navigation
- Responsive: charts resize on window change
- Export: each chart has download PNG button

#### 7.4 Export to Excel/PDF
- Every report/list page has Export buttons in toolbar
- Excel: use ClosedXML (server-side), preserve VN number format
- PDF: use QuestPDF or browser print CSS
- File naming: `BaoCao_CongNo_2026-09-27.xlsx`

#### 7.5 Dashboard Data API
- `GET /api/Dashboard/kpis` — returns 4 KPI values
- `GET /api/Dashboard/cashflow?months=12` — monthly cash flow data
- `GET /api/Dashboard/topdebtors?count=10` — top AR balances
- All endpoints return JSON, consumed by ApexCharts

#### Files touched
- `wwwroot/lib/apexcharts/` — ApexCharts JS (CDN or local)
- `wwwroot/js/ninjatax-charts.js` — new wrapper
- `Views/Home/Index.cshtml` — full redesign with dashboard
- `Controllers/DashboardApiController.cs` — new API controller
- `Models/Services/IDashboardService.cs` — new service
- `Models/ViewModels/DashboardViewModel.cs` — new
- Report views — add Export buttons

#### Verification
- Dashboard loads in <1s with sample data
- Click on TK 131 KPI card navigates to CongNo filtered list
- Excel export opens correctly in Excel with `1.234.567` format
- Charts render on mobile viewport

---

## Slice Dependency Graph

```
Slice 1 (CSS Foundation)
  ├── Slice 2 (Keyboard) — uses theme CSS
  ├── Slice 3 (Localization) — uses theme CSS
  ├── Slice 5 (Workflow Status) — uses status colors
  └── Slice 6 (Error Handling) — uses error colors
       
Slice 3 (Localization)
  └── Slice 4 (AG Grid) — needs locale formatters
       
Slice 4 (AG Grid) + Slice 5 (Workflow)
  └── Slice 7 (Dashboard) — needs grid + status for drill-down views
```

**Recommended execution order:** 1 → 2 → 3 → 5 → 6 → 4 → 7

Slice 1 is the foundation. Slices 2, 3, 5, 6 are independent after Slice 1. Slice 4 depends on 3. Slice 7 depends on 4 + 5.

---

## Out of Scope (future slices)
- Dark mode toggle (can add later with CSS custom properties swap)
- User preference persistence in database (currently localStorage)
- Print-optimized stylesheets for statutory reports
- Mobile-specific touch gestures
- Real-time collaboration / live updates via SignalR
