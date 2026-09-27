/**
 * ninjaTax Workflow Module
 * ─────────────────────────
 * Provides:
 *   NtWorkflow.init()           — auto-init on DOMContentLoaded
 *   NtWorkflow.lockFields(status) — lock form fields based on document status
 *
 * HTML data attributes:
 *   data-trang-thai="0|1|2"    — on the <form> element
 *     0 = Nhap (Draft)    → fields editable
 *     1 = DaGhiSo (Posted) → fields readonly
 *     2 = DaHuy (Cancelled) → fields readonly + grey overlay
 */

(function (global) {
    'use strict';

    const STATUS = { DRAFT: 0, POSTED: 1, CANCELLED: 2 };

    /**
     * Lock all form fields when status is Posted or Cancelled.
     * @param {HTMLFormElement} form
     * @param {number} status
     */
    function lockFields(form, status) {
        if (!form || status === STATUS.DRAFT) return;

        // Lock all inputs, selects, textareas
        form.querySelectorAll('input, select, textarea').forEach(el => {
            el.setAttribute('readonly', 'readonly');
            el.setAttribute('disabled', 'disabled');
        });

        // Lock buttons (except unpost)
        form.querySelectorAll('button[type="submit"]').forEach(btn => {
            if (!btn.hasAttribute('data-action') || btn.dataset.action !== 'unpost') {
                btn.disabled = true;
            }
        });

        // Add cancelled visual overlay
        if (status === STATUS.CANCELLED) {
            form.classList.add('nt-form--cancelled');
        }
    }

    /**
     * Add workflow status badge to page header if not already present.
     * @param {number} status
     */
    function addStatusBadgeToHeader(status) {
        const header = document.querySelector('.nt-page-header__title, h2, h1');
        if (!header) return;

        const existing = document.querySelector('.nt-badge[data-workflow-badge]');
        if (existing) return; // already present

        let badgeClass = '';
        let label = '';
        if (status === STATUS.DRAFT) {
            badgeClass = 'nt-badge--draft';
            label = 'Nháp';
        } else if (status === STATUS.POSTED) {
            badgeClass = 'nt-badge--posted';
            label = 'Đã ghi sổ';
        } else if (status === STATUS.CANCELLED) {
            badgeClass = 'nt-badge--cancelled';
            label = 'Đã hủy';
        }

        if (label) {
            const badge = document.createElement('span');
            badge.className = `nt-badge ${badgeClass} ms-2`;
            badge.setAttribute('data-workflow-badge', '');
            badge.textContent = label;
            header.appendChild(badge);
        }
    }

    function init() {
        const forms = document.querySelectorAll('form[data-trang-thai]');
        forms.forEach(form => {
            const status = parseInt(form.getAttribute('data-trang-thai'), 10);
            if (isNaN(status)) return;

            lockFields(form, status);
            addStatusBadgeToHeader(status);
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    global.NtWorkflow = {
        init,
        lockFields,
        STATUS,
    };

})(window);
