# ninjaTax UX/UI Redesign — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement a 7-pillar UX/UI redesign for the ninjaTax Vietnamese accounting webapp using progressive enhancement on top of ASP.NET Core MVC Razor.

**Architecture:** CSS design system (custom properties) + 3 small JS modules (keys, locale, grid wrapper) + AG Grid Community for data tables + ApexCharts for dashboard. Server-side Razor views enhanced progressively — pages work without JS, JS adds power features.

**Tech Stack:** Bootstrap 5, AG Grid Community 33.x, ApexCharts 4.x, Inter font, browser Intl API, ClosedXML for Excel export.

**Spec:** `docs/superpowers/specs/2026-09-27-ux-redesign-design.md`

## Global Constraints

- .NET 10 (`net10.0`), C# nullable reference types enabled
- Warnings treated as errors (`dotnet build` must produce 0 warnings)
- No Account 911 anywhere (TT99 invariant)
- Double-entry balance: `TongNo == TongCo` in all journal postings
- All number display: `1.234.567,89` (Vietnamese locale)
- All date display: `DD/MM/YYYY`
- AG Grid Community edition only (MIT license)
- No SPA rewrite — Razor views + progressive JS enhancement

## Review Focus

1. **Number formatting round-trip** — user types `1234567.89`, display shows `1.234.567,89`, POST sends `1234567.89` back. Lossy conversion breaks accounting.
2. **Keyboard shortcut conflicts** — F2/F9 may conflict with browser dev tools or accessibility tools. Must not preventDefault on inputs when typing text.
3. **AG Grid column state corruption** — localStorage state from an old column definition applied to a new one causes crash. Must handle gracefully.
4. **Status transition security** — client-side field locking is cosmetic. Server must reject edits to posted documents regardless of UI state.
5. **Drill-down URL injection** — query string params for drill-down (e.g., `?taikhoan=131`) must be validated server-side to prevent injection.

---

### Task 1: CSS Design System Foundation

**Files:**
- Create: `wwwroot/css/ninjatax-theme.css`
- Modify: `Views/Shared/_Layout.cshtml`
- Test: visual inspection (no unit test for CSS)

**Interfaces:**
- Consumes: nothing
- Produces: CSS custom properties `--nt-primary`, `--nt-bg`, `--nt-border`, `--nt-text`, `--nt-draft`, `--nt-posted`, `--nt-cancelled`, `--nt-spacing-*`, `--nt-font-size-*`. Classes: `.nt-badge`, `.nt-badge--draft`, `.nt-badge--posted`, `.nt-badge--cancelled`, `.nt-card`, `.nt-page-header`, `.nt-table`, `.nt-field--error`, `.nt-action-bar`.

- [ ] **Step 1: Create `wwwroot/css/ninjatax-theme.css`**

Define all CSS custom properties from the spec's Slice 1 (color palette, spacing tokens, typography). Add classes:
- `.nt-badge` + `--draft` / `--posted` / `--cancelled` — pill badges with status colors
- `.nt-card` — white card with `--nt-border`, 4px radius, `--nt-spacing-lg` padding
- `.nt-page-header` — flex row: h4 title + breadcrumb left, action buttons right
- `.nt-table` — compact 36px row height, 13px font, alternating rows, hover tint
- `.nt-field--error` — red border + light red bg on child inputs, `.nt-field__help` red text
- `.nt-action-bar` — flex row: secondary left, primary right, gap between save/post buttons

- [ ] **Step 2: Update `_Layout.cshtml`**

Add Inter font link from Google Fonts CDN. Add `<link>` for `ninjatax-theme.css` after `site.css`. Set `<body class="nt-app">` with `background: var(--nt-bg); font-family: var(--nt-font-family); font-size: var(--nt-font-size-base);`.

- [ ] **Step 3: Verify**

Run `dotnet build`. Open http://localhost:5188 — background should be `#F8FAFC`, font should be Inter 14px. No broken layouts.

- [ ] **Step 4: Commit**

```bash
git add wwwroot/css/ninjatax-theme.css Views/Shared/_Layout.cshtml
git commit -m "feat(ui): add CSS design system foundation — palette, spacing, typography, status badges"
```

---

### Task 2: Keyboard Navigation System

**Files:**
- Create: `wwwroot/js/ninjatax-keys.js`
- Modify: `Views/Shared/_Layout.cshtml` (add script ref)
- Test: manual keyboard test on existing form pages

**Interfaces:**
- Consumes: Task 1 CSS classes for `.nt-action-bar` button styling
- Produces: Global JS module `NtKeys` with functions `registerShortcut(key, handler)`, `initFieldNavigation()`, `initAutoFocus()`. HTML data attributes: `data-action="create|save|post|cancel"`, `data-field-order="N"`, `data-autofocus`.

- [ ] **Step 1: Create `wwwroot/js/ninjatax-keys.js`**

Implement:
- `NtKeys.init()` — called on DOMContentLoaded. Registers global keydown listener.
- Default shortcuts: F2 → `[data-action="create"]`, Ctrl+S → `[data-action="save"]` (preventDefault to block browser save), F9 → `[data-action="post"]`, Escape → `[data-action="cancel"]`.
- Enter key on `[data-field-order]` inputs: find next `[data-field-order]` by numeric order, focus it. Skip if inside `<textarea>`.
- Auto-focus: on load, focus `[data-autofocus]` or first `[data-field-order="1"]`.
- `?` key (outside input): toggle shortcut help overlay div.

- [ ] **Step 2: Create shortcut help overlay markup**

Add a hidden `<div id="nt-shortcut-help">` in `_Layout.cshtml` with a table of shortcuts. Styled as semi-transparent overlay, centered, dismissable with Escape.

- [ ] **Step 3: Add script reference to `_Layout.cshtml`**

Add `<script src="~/js/ninjatax-keys.js"></script>` before closing `</body>`, after bootstrap bundle.

- [ ] **Step 4: Add `data-action` attributes to one form view**

Update `Views/ButToan/Create.cshtml`: add `data-action="save"` to save button, `data-field-order="1"` to first input, sequential order on remaining inputs, `data-autofocus` on first input.

- [ ] **Step 5: Verify**

Run `dotnet run`. Open ButToan/Create. Press `?` → help overlay appears. Tab/Enter through fields. Ctrl+S triggers save. Escape closes. F2 on index pages triggers create.

- [ ] **Step 6: Commit**

```bash
git add wwwroot/js/ninjatax-keys.js Views/Shared/_Layout.cshtml Views/ButToan/Create.cshtml
git commit -m "feat(ui): add keyboard navigation — shortcuts F2/Ctrl+S/F9/Esc, Enter/Tab field nav, auto-focus"
```

---

### Task 3: Vietnamese Localization Layer

**Files:**
- Create: `wwwroot/js/ninjatax-locale.js`
- Create: `wwwroot/js/ninjatax-search.js`
- Create: `Helpers/StringExtensions.cs` (RemoveDiacritics utility)
- Test: `ninjaTax.Tests/Helpers/StringExtensionsTests.cs`

**Interfaces:**
- Consumes: nothing
- Produces: JS functions `NtLocale.formatVND(number)` → `string`, `NtLocale.parseVND(string)` → `number`, `NtLocale.formatDate(Date)` → `string`. C# extension `string.RemoveDiacritics()` → `string`. JS function `NtSearch.normalize(string)` → `string` (strips diacritics client-side).

- [ ] **Step 1: Write failing test for `RemoveDiacritics`**

```csharp
// ninjaTax.Tests/Helpers/StringExtensionsTests.cs
[Fact]
public void RemoveDiacritics_VietnameseText_ReturnsUnaccented()
{
    Assert.Equal("Nguyen Van A", "Nguyễn Văn A".RemoveDiacritics());
    Assert.Equal("cong ty ABC", "công ty ABC".RemoveDiacritics());
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test --filter "RemoveDiacritics"`. Expected: FAIL — method not found.

- [ ] **Step 3: Implement `RemoveDiacritics` in `Helpers/StringExtensions.cs`**

```csharp
public static string RemoveDiacritics(this string text)
```
Use `string.Normalize(NormalizationForm.FormD)` + filter `UnicodeCategory.NonSpacingMark` + `Normalize(FormC)`. Also handle `đ` → `d`, `Đ` → `D` explicitly (not decomposed by Unicode normalization).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test --filter "RemoveDiacritics"`. Expected: PASS.

- [ ] **Step 5: Create `wwwroot/js/ninjatax-locale.js`**

Implement `NtLocale` object:
- `formatVND(value)`: `new Intl.NumberFormat('vi-VN').format(value)` — outputs `1.234.567,89`
- `parseVND(str)`: strip dots, replace comma with period, `parseFloat`
- `formatDate(date)`: `new Intl.DateTimeFormat('vi-VN').format(date)` — outputs `27/09/2026`
- Auto-attach: on DOMContentLoaded, find all `[data-format="currency"]` inputs. On blur → format, on focus → show raw value for editing.

- [ ] **Step 6: Create `wwwroot/js/ninjatax-search.js`**

Implement `NtSearch.normalize(str)`: port the same diacritics-stripping logic to JS. Apply to search inputs with `[data-search="unaccent"]` — normalize input before sending to server.

- [ ] **Step 7: Add script references to `_Layout.cshtml`**

Add `<script src="~/js/ninjatax-locale.js"></script>` and `<script src="~/js/ninjatax-search.js"></script>`.

- [ ] **Step 8: Verify**

`dotnet build` (0 warnings). `dotnet test` (all pass). Open a form with currency field — type 1234567.89, blur shows `1.234.567,89`.

- [ ] **Step 9: Commit**

```bash
git add Helpers/StringExtensions.cs ninjaTax.Tests/Helpers/StringExtensionsTests.cs wwwroot/js/ninjatax-locale.js wwwroot/js/ninjatax-search.js Views/Shared/_Layout.cshtml
git commit -m "feat(ui): add Vietnamese localization — number/date formatting, diacritics-insensitive search"
```

---

### Task 4: Workflow Status UI

**Files:**
- Create: `Views/Shared/_ActionBar.cshtml`
- Create: `wwwroot/js/ninjatax-workflow.js`
- Modify: `Views/ButToan/Create.cshtml` — use `_ActionBar`
- Modify: `Views/ButToan/Details.cshtml` — status-based field locking
- Test: visual inspection + manual test

**Interfaces:**
- Consumes: Task 1 CSS classes `.nt-badge--*`, `.nt-action-bar`. Task 2 `data-action` attributes.
- Produces: Razor partial `_ActionBar` accepting `ViewData["TrangThai"]`. JS function `NtWorkflow.lockFields(status)` that sets readonly on all form inputs when status != Draft.

- [ ] **Step 1: Create `Views/Shared/_ActionBar.cshtml`**

Razor partial that reads `ViewData["TrangThai"]` (int enum). Renders:
- Draft (0): "Lưu nháp (Ctrl+S)" outline button + "Ghi sổ (F9)" green solid button + "Hủy bỏ (Esc)" link
- Posted (1): "Bỏ ghi sổ" warning button only
- Cancelled (2): no action buttons, just status badge

- [ ] **Step 2: Create `wwwroot/js/ninjatax-workflow.js`**

`NtWorkflow.init()`: read `data-trang-thai` attribute from form. If posted/cancelled, set all `input, select, textarea` to `readonly`/`disabled`. Add grey overlay class for cancelled.

- [ ] **Step 3: Apply to `ButToan/Create.cshtml` and `ButToan/Details.cshtml`**

Add `@await Html.PartialAsync("_ActionBar")` at form bottom. Set `ViewData["TrangThai"]` in controller action. Add `data-trang-thai` to form element.

- [ ] **Step 4: Add script reference and verify**

Add `ninjatax-workflow.js` to layout. Open a posted ButToan — fields should be readonly, "Bỏ ghi sổ" shown. Draft — full editing.

- [ ] **Step 5: Commit**

```bash
git add Views/Shared/_ActionBar.cshtml wwwroot/js/ninjatax-workflow.js Views/ButToan/Create.cshtml Views/ButToan/Details.cshtml Views/Shared/_Layout.cshtml
git commit -m "feat(ui): add workflow status UI — draft/posted/cancelled badges, action bar, field locking"
```

---

### Task 5: Smart Error Handling & Validation

**Files:**
- Create: `wwwroot/js/ninjatax-toast.js`
- Create: `wwwroot/js/ninjatax-validation.js`
- Create: `Models/Services/BusinessException.cs`
- Modify: `Views/Shared/Error.cshtml` — redesign
- Test: `ninjaTax.Tests/Services/BusinessExceptionTests.cs`

**Interfaces:**
- Consumes: Task 1 CSS `.nt-field--error`, `.nt-field__help`. Task 3 `NtLocale.formatVND` for error messages with amounts.
- Produces: C# `BusinessException(string userMessage, string? fieldName)`. JS `NtToast.show(message, type)` where type = `success|warning|error`. JS `NtValidation.init()` that wires inline validation.

- [ ] **Step 1: Write failing test for `BusinessException`**

```csharp
[Fact]
public void BusinessException_CarriesUserMessageAndField()
{
    var ex = new BusinessException("TK 1111 không đủ số dư (Thiếu 5.000.000đ)", "SoTien");
    Assert.Equal("TK 1111 không đủ số dư (Thiếu 5.000.000đ)", ex.UserMessage);
    Assert.Equal("SoTien", ex.FieldName);
}
```

- [ ] **Step 2: Run test — expected FAIL**

- [ ] **Step 3: Implement `BusinessException` in `Models/Services/BusinessException.cs`**

Simple class extending `Exception` with `UserMessage` and `FieldName` properties.

- [ ] **Step 4: Run test — expected PASS**

- [ ] **Step 5: Create `wwwroot/js/ninjatax-toast.js`**

`NtToast.show(message, type, durationMs=5000)`: creates a Bootstrap-styled toast div, appends to `#nt-toast-container`, auto-removes after duration. Types map to colors: success=green, warning=amber, error=red.

- [ ] **Step 6: Create `wwwroot/js/ninjatax-validation.js`**

`NtValidation.init()`: find all `[data-required]` inputs. On blur, if empty, add `.nt-field--error` to parent, show help text. On input, remove error state. Find `[data-balance-check]` containers — compute TongNo/TongCo live, show green/red indicator.

- [ ] **Step 7: Add toast container to `_Layout.cshtml`**

Add `<div id="nt-toast-container" class="toast-container position-fixed top-0 end-0 p-3"></div>` and script refs.

- [ ] **Step 8: Redesign `Views/Shared/Error.cshtml`**

Replace default error page with styled card: Vietnamese message, ninjaTax branding, "Quay lại trang chủ" button. No technical stack trace for production.

- [ ] **Step 9: Verify**

`dotnet build` + `dotnet test`. Leave required field blank → red border + help text. Balance mismatch → red indicator.

- [ ] **Step 10: Commit**

```bash
git add Models/Services/BusinessException.cs ninjaTax.Tests/Services/BusinessExceptionTests.cs wwwroot/js/ninjatax-toast.js wwwroot/js/ninjatax-validation.js Views/Shared/Error.cshtml Views/Shared/_Layout.cshtml
git commit -m "feat(ui): add smart error handling — BusinessException, toast notifications, inline validation"
```

---

### Task 6: AG Grid Integration

**Files:**
- Create: `wwwroot/js/ninjatax-grid.js`
- Create: `wwwroot/css/ninjatax-grid.css`
- Create: `Views/Shared/_AgGrid.cshtml`
- Modify: `Views/ButToan/Index.cshtml` — migrate to AG Grid
- Modify: `Controllers/ButToanController.cs` — add JSON API endpoint
- Test: `ninjaTax.Tests/Controllers/ButToanControllerTests.cs` (API endpoint test)

**Interfaces:**
- Consumes: Task 1 CSS variables for grid theming. Task 3 `NtLocale` for currency/date formatters in cells.
- Produces: JS function `NtGrid.create(containerId, columnDefs, dataUrl, gridId)` → AG Grid instance. Razor partial `_AgGrid` that renders the container div + script block. API contract: `GET /ButToan/ListJson?page=1&pageSize=50&sort=NgayHachToan&dir=desc` → `{ rows: [...], totalCount: N }`.

- [ ] **Step 1: Add AG Grid to project**

Download AG Grid Community CSS + JS to `wwwroot/lib/ag-grid/` (or add CDN links to `_Layout.cshtml`). Add `ninjatax-grid.css` for theme overrides mapping AG Grid variables to `--nt-*` tokens.

- [ ] **Step 2: Create `wwwroot/js/ninjatax-grid.js`**

`NtGrid.create(containerId, columnDefs, dataUrl, gridId)`:
- Sets up AG Grid with infinite row model
- Datasource fetches from `dataUrl` with pagination/sort/filter params
- Row height 36px, header height 38px
- Default column types: `ntCurrency` (uses `NtLocale.formatVND`), `ntDate` (uses `NtLocale.formatDate`), `ntStatus` (renders `.nt-badge`)
- Column state save/restore from localStorage keyed by `gridId`
- Vietnamese locale strings for filter UI

- [ ] **Step 3: Create `Views/Shared/_AgGrid.cshtml`**

Partial accepting `gridId`, `dataUrl`, `columnDefsJson` via ViewData. Renders a `<div id="@gridId" class="ag-theme-alpine nt-grid" style="height:600px"></div>` + `<script>NtGrid.create(...)</script>`.

- [ ] **Step 4: Write failing test for ButToan JSON API**

```csharp
[Fact]
public async Task ListJson_ReturnsPagedResults()
{
    // Arrange: seed 3 ButToan records
    // Act: GET /ButToan/ListJson?page=1&pageSize=2
    // Assert: response has rows.Length == 2, totalCount == 3
}
```

- [ ] **Step 5: Run test — expected FAIL**

- [ ] **Step 6: Add `ListJson` action to `ButToanController`**

```csharp
[HttpGet]
public async Task<IActionResult> ListJson(int page = 1, int pageSize = 50, string? sort = null, string? dir = null)
```
Returns JSON with `rows` and `totalCount`. Apply sorting/filtering/pagination via EF Core.

- [ ] **Step 7: Run test — expected PASS**

- [ ] **Step 8: Migrate `Views/ButToan/Index.cshtml` to AG Grid**

Replace `<table>` with `@await Html.PartialAsync("_AgGrid")`. Define columns: SoChungTu (pinned left), NgayHachToan (date), DienGiai (text), TongNo (currency), TongCo (currency), TrangThai (badge).

- [ ] **Step 9: Verify**

`dotnet build` + `dotnet test`. Open ButToan/Index — grid loads, columns freeze, filters work, reorder persists. Scroll 10k rows if data exists.

- [ ] **Step 10: Commit**

```bash
git add wwwroot/lib/ag-grid/ wwwroot/js/ninjatax-grid.js wwwroot/css/ninjatax-grid.css Views/Shared/_AgGrid.cshtml Views/ButToan/Index.cshtml Controllers/ButToanController.cs ninjaTax.Tests/Controllers/ButToanControllerTests.cs Views/Shared/_Layout.cshtml
git commit -m "feat(ui): integrate AG Grid — frozen columns, filters, virtual scroll, column persistence"
```

---

### Task 7: Dashboard & Drill-Down Reports

**Files:**
- Create: `wwwroot/js/ninjatax-charts.js`
- Create: `Controllers/DashboardApiController.cs`
- Create: `Models/Services/IDashboardService.cs`
- Create: `Models/Services/DashboardService.cs`
- Create: `Models/ViewModels/DashboardViewModel.cs`
- Modify: `Views/Home/Index.cshtml` — full dashboard redesign
- Test: `ninjaTax.Tests/Services/DashboardServiceTests.cs`

**Interfaces:**
- Consumes: Task 1 CSS for card styling. Task 3 `NtLocale.formatVND` for KPI display. Task 6 AG Grid for drill-down list pages.
- Produces: API endpoints `GET /api/Dashboard/kpis`, `GET /api/Dashboard/cashflow?months=12`, `GET /api/Dashboard/topdebtors?count=10`. Service `IDashboardService` with methods `GetKpisAsync()`, `GetCashFlowAsync(int months)`, `GetTopDebtorsAsync(int count)`.

- [ ] **Step 1: Write failing test for `DashboardService.GetKpisAsync`**

```csharp
[Fact]
public async Task GetKpisAsync_ReturnsRevenueExpenseReceivablePayable()
{
    // Arrange: seed ButToan with known debit/credit to revenue (511), expense (642), AR (131), AP (331)
    // Act
    var kpis = await service.GetKpisAsync();
    // Assert
    Assert.True(kpis.DoanhThu > 0);
    Assert.True(kpis.ChiPhi > 0);
    Assert.True(kpis.PhaiThu >= 0);
    Assert.True(kpis.PhaiTra >= 0);
}
```

- [ ] **Step 2: Run test — expected FAIL**

- [ ] **Step 3: Implement `IDashboardService` and `DashboardService`**

`GetKpisAsync()`: query ButToan/ChiTietButToan aggregating by TaiKhoan prefix (511=revenue, 642=expense, 131=AR, 331=AP).
`GetCashFlowAsync(months)`: group by month, sum inflows (TK 111/112 debit) vs outflows (credit).
`GetTopDebtorsAsync(count)`: aggregate TK 131 balance by DoiTuong, order descending, take N.

- [ ] **Step 4: Run test — expected PASS**

- [ ] **Step 5: Create `DashboardApiController`**

Three GET endpoints returning JSON. Register `IDashboardService` in DI.

- [ ] **Step 6: Create `wwwroot/js/ninjatax-charts.js`**

`NtCharts` wrapper:
- `createKpiCards(containerId, data)` — renders 4 KPI cards with formatted VND values
- `createLineChart(containerId, data, drilldownUrl)` — ApexCharts line chart, click fires navigation
- `createBarChart(containerId, data, drilldownUrl)` — horizontal bar, click drills to customer detail
- `createDonutChart(containerId, data)` — expense breakdown
- Vietnamese number formatting on Y-axis via `NtLocale.formatVND`

- [ ] **Step 7: Redesign `Views/Home/Index.cshtml`**

Replace welcome page with dashboard layout:
- Row 1: 4 KPI cards (DoanhThu, ChiPhi, PhaiThu, PhaiTra) — each clickable, navigates to filtered list
- Row 2: Cash flow line chart (left), Top 10 debtors bar chart (right)
- Row 3: Expense donut (left), Monthly revenue bar (right)
- All charts fetch data from `/api/Dashboard/*` endpoints on page load

- [ ] **Step 8: Add drill-down links**

KPI cards: click DoanhThu → `/BanHang`, PhaiThu → `/CongNo?taikhoan=131`
Bar chart bars: click customer → `/CongNo/Index?doituong={id}`
Breadcrumb updates based on drill-down context from query params.

- [ ] **Step 9: Verify**

`dotnet build` + `dotnet test`. Dashboard loads with charts. Click KPI → navigates to list. Click bar → drills to detail.

- [ ] **Step 10: Commit**

```bash
git add Controllers/DashboardApiController.cs Models/Services/IDashboardService.cs Models/Services/DashboardService.cs Models/ViewModels/DashboardViewModel.cs wwwroot/js/ninjatax-charts.js Views/Home/Index.cshtml ninjaTax.Tests/Services/DashboardServiceTests.cs Views/Shared/_Layout.cshtml
git commit -m "feat(ui): add dashboard with KPI cards, ApexCharts, drill-down navigation"
```
