/**
 * ninjaTax Toast Notification Module
 * ────────────────────────────────────
 * Provides:
 *   NtToast.show(message, type, durationMs)  — display a toast
 *   NtToast.success(message)                 — green toast
 *   NtToast.warning(message)                 — amber toast
 *   NtToast.error(message)                   — red toast
 *
 * Requires: #nt-toast-container in the DOM (added by _Layout.cshtml)
 *
 * Type values: 'success' | 'warning' | 'error'
 */

(function (global) {
    'use strict';

    const ICONS = {
        success: 'bi-check-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        error:   'bi-x-circle-fill',
    };

    /**
     * Show a toast notification.
     * @param {string}  message    — the message text (may contain HTML)
     * @param {string}  type       — 'success' | 'warning' | 'error'
     * @param {number}  durationMs — auto-dismiss after this many ms (default 5000)
     */
    function show(message, type, durationMs) {
        type = type || 'success';
        durationMs = typeof durationMs === 'number' ? durationMs : 5000;

        const container = document.getElementById('nt-toast-container');
        if (!container) return;

        const icon = ICONS[type] || ICONS.success;

        const toast = document.createElement('div');
        toast.className = `nt-toast nt-toast--${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.innerHTML = `
            <i class="bi ${icon} flex-shrink-0" style="font-size:1rem;margin-top:0.125rem;"></i>
            <span>${message}</span>
            <button type="button" class="nt-toast__close" aria-label="Đóng" onclick="this.closest('.nt-toast').remove()">&times;</button>
        `;

        container.appendChild(toast);

        // Auto-dismiss
        if (durationMs > 0) {
            setTimeout(() => {
                toast.style.opacity = '0';
                toast.style.transition = 'opacity 0.3s ease';
                setTimeout(() => toast.remove(), 300);
            }, durationMs);
        }
    }

    function success(message, durationMs) { show(message, 'success', durationMs); }
    function warning(message, durationMs) { show(message, 'warning', durationMs); }
    function error(message, durationMs)   { show(message, 'error', durationMs); }

    /**
     * Show TempData messages as toasts.
     * Reads data attributes from hidden span elements:
     *   <span data-toast-success="@TempData["ThongBaoThanhCong"]"></span>
     */
    function initTempDataToasts() {
        document.querySelectorAll('[data-toast-success]').forEach(el => {
            const msg = el.getAttribute('data-toast-success');
            if (msg) success(msg);
        });
        document.querySelectorAll('[data-toast-warning]').forEach(el => {
            const msg = el.getAttribute('data-toast-warning');
            if (msg) warning(msg);
        });
        document.querySelectorAll('[data-toast-error]').forEach(el => {
            const msg = el.getAttribute('data-toast-error');
            if (msg) error(msg);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTempDataToasts);
    } else {
        initTempDataToasts();
    }

    global.NtToast = { show, success, warning, error };

})(window);
