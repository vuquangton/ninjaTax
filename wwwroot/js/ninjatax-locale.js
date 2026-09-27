/**
 * ninjaTax Vietnamese Locale Module
 * ─────────────────────────────────
 * Provides:
 *   NtLocale.formatVND(value)       — number → "1.234.567,89"
 *   NtLocale.parseVND(str)          — "1.234.567,89" → 1234567.89
 *   NtLocale.formatDate(date|str)   — Date → "27/09/2026"
 *   NtLocale.parseDate(str)         — "27/09/2026" → Date
 *   NtLocale.initCurrencyInputs()   — auto-attach to [data-format="currency"]
 *   NtLocale.initDateInputs()       — display DD/MM/YYYY hint for date inputs
 *
 * HTML data attributes:
 *   data-format="currency"    — attach VND formatting on this input
 *   data-format="date"        — attach date formatting hint
 */

(function (global) {
    'use strict';

    const _vndFormatter = new Intl.NumberFormat('vi-VN', {
        minimumFractionDigits: 0,
        maximumFractionDigits: 2,
    });

    const _dateFormatter = new Intl.DateTimeFormat('vi-VN', {
        day:   '2-digit',
        month: '2-digit',
        year:  'numeric',
    });

    /**
     * Format a number using Vietnamese locale: 1234567.89 → "1.234.567,89"
     * @param {number|string} value
     * @returns {string}
     */
    function formatVND(value) {
        const n = typeof value === 'string' ? parseVND(value) : Number(value);
        if (isNaN(n)) return '';
        return _vndFormatter.format(n);
    }

    /**
     * Parse a Vietnamese-formatted number string back to a number.
     * "1.234.567,89" → 1234567.89
     * @param {string} str
     * @returns {number}
     */
    function parseVND(str) {
        if (!str && str !== 0) return NaN;
        // Remove thousand separators (.) and replace decimal comma (,) with dot
        const cleaned = String(str)
            .replace(/\./g, '')
            .replace(',', '.');
        return parseFloat(cleaned);
    }

    /**
     * Format a Date to DD/MM/YYYY string.
     * @param {Date|string|number} date
     * @returns {string}
     */
    function formatDate(date) {
        if (!date) return '';
        const d = date instanceof Date ? date : new Date(date);
        if (isNaN(d.getTime())) return String(date);
        return _dateFormatter.format(d);
    }

    /**
     * Parse a DD/MM/YYYY string to a Date object.
     * @param {string} str  — e.g. "27/09/2026"
     * @returns {Date|null}
     */
    function parseDate(str) {
        if (!str) return null;
        const parts = str.split('/');
        if (parts.length !== 3) return null;
        const [day, month, year] = parts.map(Number);
        const d = new Date(year, month - 1, day);
        return isNaN(d.getTime()) ? null : d;
    }

    /**
     * Attach VND formatting to all [data-format="currency"] inputs.
     * - On focus: show raw numeric value for editing
     * - On blur:  show formatted value
     * - On form submit: hidden input carries raw value to ensure server gets number
     */
    function initCurrencyInputs() {
        const inputs = document.querySelectorAll('[data-format="currency"]');
        inputs.forEach(input => {
            // Store raw value as data attribute
            const raw = parseVND(input.value);
            if (!isNaN(raw)) {
                input.dataset.rawValue = raw;
                input.value = formatVND(raw);
            }

            input.addEventListener('focus', function () {
                const r = parseVND(this.dataset.rawValue || this.value);
                this.value = isNaN(r) ? '' : String(r);
                this.select();
            });

            input.addEventListener('blur', function () {
                const r = parseVND(this.value);
                this.dataset.rawValue = isNaN(r) ? '' : String(r);
                this.value = isNaN(r) ? '' : formatVND(r);
            });

            // Before form submit, restore raw value so server gets a number
            const form = input.closest('form');
            if (form) {
                form.addEventListener('submit', function () {
                    const r = parseVND(input.dataset.rawValue || input.value);
                    if (!isNaN(r)) input.value = String(r);
                }, { once: false });
            }
        });
    }

    /**
     * Show formatted currency amounts in display-only [data-display="currency"] elements.
     */
    function initCurrencyDisplays() {
        document.querySelectorAll('[data-display="currency"]').forEach(el => {
            const raw = parseVND(el.textContent || el.innerText || '');
            if (!isNaN(raw)) el.textContent = formatVND(raw);
        });
    }

    function init() {
        initCurrencyInputs();
        initCurrencyDisplays();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    global.NtLocale = {
        formatVND,
        parseVND,
        formatDate,
        parseDate,
        initCurrencyInputs,
        initCurrencyDisplays,
    };

})(window);
