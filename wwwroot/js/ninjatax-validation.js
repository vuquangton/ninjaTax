/**
 * ninjaTax Validation Module
 * ───────────────────────────
 * Provides:
 *   NtValidation.init() — Attach validation listeners and live balance checking
 */

(function (global) {
    'use strict';

    function initRequiredValidation() {
        document.querySelectorAll('input[required], select[required], textarea[required], [data-required]').forEach(input => {
            const container = input.closest('.nt-field') || input.parentElement;

            function validate() {
                const val = input.value ? input.value.trim() : '';
                if (!val) {
                    if (container) container.classList.add('nt-field--error');
                    let help = container ? container.querySelector('.nt-field__help') : null;
                    if (!help && container) {
                        help = document.createElement('span');
                        help.className = 'nt-field__help';
                        help.textContent = 'Trường này là bắt buộc nhập.';
                        container.appendChild(help);
                    }
                } else {
                    if (container) container.classList.remove('nt-field--error');
                }
            }

            input.addEventListener('blur', validate);
            input.addEventListener('input', () => {
                if (input.value && input.value.trim() && container) {
                    container.classList.remove('nt-field--error');
                }
            });
        });
    }

    function initBalanceCheck() {
        document.querySelectorAll('[data-balance-check]').forEach(container => {
            function updateBalance() {
                let totalDebit = 0;
                let totalCredit = 0;

                container.querySelectorAll('[data-balance-debit]').forEach(el => {
                    const val = parseFloat(el.value || el.textContent) || 0;
                    totalDebit += val;
                });

                container.querySelectorAll('[data-balance-credit]').forEach(el => {
                    const val = parseFloat(el.value || el.textContent) || 0;
                    totalCredit += val;
                });

                const indicator = container.querySelector('.nt-balance-indicator');
                if (indicator) {
                    const isBalanced = Math.abs(totalDebit - totalCredit) < 0.0001;
                    if (isBalanced) {
                        indicator.className = 'nt-balance-indicator nt-balance-indicator--balanced';
                        indicator.innerHTML = '<i class="bi bi-check-circle-fill me-1"></i> Cân đối Nợ = Có (' + totalDebit.toLocaleString('vi-VN') + ' đ)';
                    } else {
                        indicator.className = 'nt-balance-indicator nt-balance-indicator--unbalanced';
                        indicator.innerHTML = '<i class="bi bi-exclamation-circle-fill me-1"></i> Không cân đối (Lệch: ' + Math.abs(totalDebit - totalCredit).toLocaleString('vi-VN') + ' đ)';
                    }
                }
            }

            container.addEventListener('input', updateBalance);
            updateBalance();
        });
    }

    function init() {
        initRequiredValidation();
        initBalanceCheck();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    global.NtValidation = {
        init,
        initRequiredValidation,
        initBalanceCheck
    };

})(window);
