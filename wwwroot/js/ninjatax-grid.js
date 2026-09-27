/**
 * ninjaTax AG Grid Wrapper
 * ─────────────────────────
 * Provides:
 *   NtGrid.create(containerId, columnDefs, dataUrl, gridId)
 */

(function (global) {
    'use strict';

    function create(containerId, columnDefs, dataUrl, gridId) {
        const eGridDiv = document.getElementById(containerId);
        if (!eGridDiv) return null;

        // Ensure default formatters are wired up
        columnDefs.forEach(col => {
            if (col.type === 'ntCurrency') {
                col.cellClass = (col.cellClass || '') + ' nt-grid-cell-number';
                col.valueFormatter = params => {
                    if (params.value === null || params.value === undefined) return '0';
                    return (global.NtLocale ? global.NtLocale.formatVND(params.value) : Number(params.value).toLocaleString('vi-VN')) + ' đ';
                };
            }
            if (col.type === 'ntDate') {
                col.valueFormatter = params => {
                    if (!params.value) return '';
                    return global.NtLocale ? global.NtLocale.formatDate(params.value) : params.value;
                };
            }
            if (col.type === 'ntStatus') {
                col.cellRenderer = params => {
                    const status = params.value;
                    if (status === 1 || status === '1' || status === 'Đã ghi sổ') {
                        return '<span class="nt-badge nt-badge--posted"><i class="bi bi-check-circle me-1"></i>Đã ghi sổ</span>';
                    }
                    if (status === 2 || status === '2' || status === 'Đã hủy') {
                        return '<span class="nt-badge nt-badge--cancelled"><i class="bi bi-x-circle me-1"></i>Đã hủy</span>';
                    }
                    return '<span class="nt-badge nt-badge--draft"><i class="bi bi-pencil-square me-1"></i>Nháp</span>';
                };
            }
        });

        const gridOptions = {
            columnDefs: columnDefs,
            rowModelType: 'infinite',
            cacheBlockSize: 50,
            maxBlocksInCache: 10,
            animateRows: true,
            pagination: true,
            paginationPageSize: 50,
            datasource: {
                getRows: function (params) {
                    const page = Math.floor(params.startRow / 50) + 1;
                    const pageSize = params.endRow - params.startRow;
                    let sort = null;
                    let dir = null;

                    if (params.sortModel && params.sortModel.length > 0) {
                        sort = params.sortModel[0].colId;
                        dir = params.sortModel[0].sort;
                    }

                    const url = new URL(dataUrl, window.location.origin);
                    url.searchParams.set('page', page);
                    url.searchParams.set('pageSize', pageSize);
                    if (sort) {
                        url.searchParams.set('sort', sort);
                        url.searchParams.set('dir', dir);
                    }

                    fetch(url.toString())
                        .then(res => res.json())
                        .then(data => {
                            params.successCallback(data.rows, data.totalCount);
                        })
                        .catch(err => {
                            console.error('Lỗi khi tải dữ liệu grid:', err);
                            params.failCallback();
                        });
                }
            },
            onColumnMoved: () => saveState(gridId, gridOptions),
            onColumnResized: () => saveState(gridId, gridOptions),
            onColumnVisible: () => saveState(gridId, gridOptions),
            onGridReady: function (params) {
                restoreState(gridId, params.api);
            }
        };

        if (global.agGrid) {
            return global.agGrid.createGrid(eGridDiv, gridOptions);
        } else {
            console.warn('agGrid library not loaded');
            return null;
        }
    }

    function saveState(gridId, gridOptions) {
        if (!gridId || !gridOptions.api) return;
        try {
            const state = gridOptions.api.getColumnState();
            localStorage.setItem('nt_grid_state_' + gridId, JSON.stringify(state));
        } catch (e) {
            console.error('Error saving grid state', e);
        }
    }

    function restoreState(gridId, api) {
        if (!gridId || !api) return;
        try {
            const saved = localStorage.getItem('nt_grid_state_' + gridId);
            if (saved) {
                api.applyColumnState({ state: JSON.parse(saved), applyOrder: true });
            }
        } catch (e) {
            console.error('Error restoring grid state', e);
        }
    }

    global.NtGrid = {
        create
    };

})(window);
