/**
 * ninjaTax Keyboard Navigation Module
 * ─────────────────────────────────────
 * Provides:
 *   NtKeys.init()                        — called automatically on DOMContentLoaded
 *   NtKeys.registerShortcut(key, fn)     — register a custom shortcut
 *   NtKeys.initFieldNavigation()         — wire Enter→next field on [data-field-order]
 *   NtKeys.initAutoFocus()               — focus first field on page load
 *
 * HTML data attributes used:
 *   data-action="create|save|post|cancel"  — shortcut targets
 *   data-field-order="N"                   — tab-order integer (1-based)
 *   data-autofocus                         — explicit auto-focus target
 */

(function (global) {
    'use strict';

    /** Map of "KeyString" → handler function */
    const _shortcuts = {};

    /** Normalise a KeyboardEvent into a canonical key string like "F2", "Ctrl+S", "F9" */
    function _normalise(e) {
        const parts = [];
        if (e.ctrlKey)  parts.push('Ctrl');
        if (e.altKey)   parts.push('Alt');
        if (e.shiftKey) parts.push('Shift');
        parts.push(e.key);
        return parts.join('+');
    }

    /** Find the element with the next data-field-order after `currentOrder` */
    function _nextField(currentOrder) {
        const all = Array.from(
            document.querySelectorAll('[data-field-order]')
        ).filter(el => !el.disabled && !el.readOnly && el.offsetParent !== null);

        const orders = all
            .map(el => ({ el, n: parseInt(el.getAttribute('data-field-order'), 10) }))
            .filter(o => !isNaN(o.n))
            .sort((a, b) => a.n - b.n);

        const idx = orders.findIndex(o => o.n === currentOrder);
        if (idx === -1) return null;
        // wrap around to first if at last
        return orders[(idx + 1) % orders.length]?.el ?? null;
    }

    /**
     * Wire Enter key on every [data-field-order] input to jump to the next field.
     * Skips <textarea> (Enter = newline there).
     */
    function initFieldNavigation() {
        document.addEventListener('keydown', function (e) {
            if (e.key !== 'Enter') return;
            const target = e.target;
            if (!target || !target.hasAttribute('data-field-order')) return;
            if (target.tagName === 'TEXTAREA') return;
            if (target.tagName === 'BUTTON' || target.tagName === 'A') return;

            const currentOrder = parseInt(target.getAttribute('data-field-order'), 10);
            const next = _nextField(currentOrder);
            if (next) {
                e.preventDefault();
                next.focus();
                if (next.select) next.select();
            }
        });
    }

    /**
     * Auto-focus [data-autofocus] or first [data-field-order="1"] on page load.
     */
    function initAutoFocus() {
        const explicit = document.querySelector('[data-autofocus]');
        if (explicit) {
            setTimeout(() => { explicit.focus(); if (explicit.select) explicit.select(); }, 50);
            return;
        }
        const first = document.querySelector('[data-field-order="1"]');
        if (first) {
            setTimeout(() => { first.focus(); if (first.select) first.select(); }, 50);
        }
    }

    /**
     * Register a custom shortcut.
     * @param {string} key  — e.g. "F2", "Ctrl+S", "F9"
     * @param {Function} fn — handler (receives the KeyboardEvent)
     */
    function registerShortcut(key, fn) {
        _shortcuts[key] = fn;
    }

    /** Toggle the shortcut-help overlay */
    function _toggleHelp() {
        const panel = document.getElementById('nt-shortcut-help');
        if (!panel) return;
        panel.classList.toggle('is-visible');
    }

    /** Hide the shortcut-help overlay */
    function _hideHelp() {
        const panel = document.getElementById('nt-shortcut-help');
        if (panel) panel.classList.remove('is-visible');
    }

    /** Main global keydown handler */
    function _onKeyDown(e) {
        const tag = (e.target && e.target.tagName) ? e.target.tagName.toUpperCase() : '';
        const inInput = ['INPUT', 'SELECT', 'TEXTAREA'].includes(tag);

        const key = _normalise(e);

        // ── Global shortcuts (fire even inside inputs) ───────────────────
        if (key === 'Escape') {
            _hideHelp();
            const cancel = document.querySelector('[data-action="cancel"]');
            if (cancel) { e.preventDefault(); cancel.click(); }
            return;
        }

        if (key === 'Ctrl+S') {
            e.preventDefault();
            const save = document.querySelector('[data-action="save"]');
            if (save) save.click();
            return;
        }

        // ── Shortcuts that should NOT fire while typing in an input ──────
        if (inInput) return;

        if (key === 'F2') {
            e.preventDefault();
            const create = document.querySelector('[data-action="create"]');
            if (create) create.click();
            return;
        }

        if (key === 'F9') {
            e.preventDefault();
            const post = document.querySelector('[data-action="post"]');
            if (post) post.click();
            return;
        }

        if (key === '?') {
            e.preventDefault();
            _toggleHelp();
            return;
        }

        // ── Custom shortcuts ─────────────────────────────────────────────
        if (_shortcuts[key]) {
            e.preventDefault();
            _shortcuts[key](e);
        }
    }

    /** Initialise everything */
    function init() {
        document.addEventListener('keydown', _onKeyDown);
        initFieldNavigation();
        initAutoFocus();

        // Close help panel on clicking backdrop
        const panel = document.getElementById('nt-shortcut-help');
        if (panel) {
            panel.addEventListener('click', function (e) {
                if (e.target === panel) _hideHelp();
            });
        }
    }

    // Auto-init on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Expose public API
    global.NtKeys = {
        init,
        registerShortcut,
        initFieldNavigation,
        initAutoFocus,
    };

})(window);
