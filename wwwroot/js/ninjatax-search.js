/**
 * ninjaTax Search Module — Diacritics-insensitive search
 * ────────────────────────────────────────────────────────
 * Provides:
 *   NtSearch.normalize(str)        — strips Vietnamese diacritics from a string
 *   NtSearch.initSearchInputs()    — attach to [data-search="unaccent"] inputs
 *
 * On search input, the query is normalized before sending to the server,
 * so "nguyen" finds "Nguyễn" and vice versa.
 *
 * The server-side mirrors this via Helpers/StringExtensions.RemoveDiacritics().
 */

(function (global) {
    'use strict';

    // Vietnamese-specific replacements that Unicode FormD doesn't handle
    const _dMap = [
        [/đ/g, 'd'], [/Đ/g, 'D'],
    ];

    // Combining diacritical marks unicode range
    const _combiningRegex = /[\u0300-\u036f]/g;

    /**
     * Strip Vietnamese diacritical marks from a string.
     * "Nguyễn Văn A" → "Nguyen Van A"
     * "đường" → "duong"
     *
     * @param {string} str
     * @returns {string}
     */
    function normalize(str) {
        if (!str) return '';
        let result = String(str);

        // Handle đ/Đ first
        for (const [pattern, replacement] of _dMap) {
            result = result.replace(pattern, replacement);
        }

        // Use NFD to decompose accented chars, then strip combining marks
        if (result.normalize) {
            result = result.normalize('NFD').replace(_combiningRegex, '');
        }

        return result;
    }

    /**
     * Attach normalization to all [data-search="unaccent"] inputs.
     * Adds a hidden sibling input with the normalized value that gets POSTed/GETted.
     * Also intercepts form GET submissions to normalize the search query.
     */
    function initSearchInputs() {
        document.querySelectorAll('[data-search="unaccent"]').forEach(input => {
            // Create a hidden input that carries the normalized value
            const hiddenName = input.name ? (input.name + '_normalized') : null;
            let hidden = null;
            if (hiddenName) {
                hidden = document.createElement('input');
                hidden.type = 'hidden';
                hidden.name = hiddenName;
                input.insertAdjacentElement('afterend', hidden);
            }

            function updateNormalized() {
                if (hidden) hidden.value = normalize(input.value);
            }

            input.addEventListener('input', updateNormalized);
            input.addEventListener('change', updateNormalized);
            updateNormalized(); // init
        });
    }

    function init() {
        initSearchInputs();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    global.NtSearch = {
        normalize,
        initSearchInputs,
    };

})(window);
