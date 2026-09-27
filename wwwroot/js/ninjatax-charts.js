/**
 * ninjaTax Charts Wrapper (ApexCharts)
 * ──────────────────────────────────────
 * Provides:
 *   NtCharts.initDashboard() — Loads and mounts all dashboard charts
 */

(function (global) {
    'use strict';

    function formatVND(val) {
        return (global.NtLocale ? global.NtLocale.formatVND(val) : Number(val).toLocaleString('vi-VN')) + ' đ';
    }

    function initKpis() {
        fetch('/api/DashboardApi/kpis')
            .then(res => res.json())
            .then(data => {
                const elRev = document.getElementById('kpiDoanhThu');
                const elExp = document.getElementById('kpiChiPhi');
                const elRec = document.getElementById('kpiPhaiThu');
                const elPay = document.getElementById('kpiPhaiTra');

                if (elRev) elRev.innerText = formatVND(data.doanhThu);
                if (elExp) elExp.innerText = formatVND(data.chiPhi);
                if (elRec) elRec.innerText = formatVND(data.phaiThu);
                if (elPay) elPay.innerText = formatVND(data.phaiTra);
            })
            .catch(err => console.error('Lỗi nạp KPI:', err));
    }

    function initCashFlowChart() {
        const el = document.getElementById('chartCashFlow');
        if (!el || !global.ApexCharts) return;

        fetch('/api/DashboardApi/cashflow?months=12')
            .then(res => res.json())
            .then(data => {
                const categories = data.map(d => d.month);
                const inflows = data.map(d => d.inflow);
                const outflows = data.map(d => d.outflow);

                const options = {
                    series: [
                        { name: 'Dòng tiền vào (Thu)', data: inflows },
                        { name: 'Dòng tiền ra (Chi)', data: outflows }
                    ],
                    chart: {
                        type: 'area',
                        height: 320,
                        toolbar: { show: false },
                        fontFamily: 'var(--nt-font-family)'
                    },
                    colors: ['#10B981', '#EF4444'],
                    dataLabels: { enabled: false },
                    stroke: { curve: 'smooth', width: 2 },
                    xaxis: { categories: categories },
                    yaxis: {
                        labels: {
                            formatter: val => (val / 1000000).toLocaleString('vi-VN') + ' tr'
                        }
                    },
                    tooltip: {
                        y: { formatter: val => formatVND(val) }
                    }
                };

                const chart = new global.ApexCharts(el, options);
                chart.render();
            })
            .catch(err => console.error('Lỗi nạp CashFlow chart:', err));
    }

    function initTopDebtorsChart() {
        const el = document.getElementById('chartTopDebtors');
        if (!el || !global.ApexCharts) return;

        fetch('/api/DashboardApi/topdebtors?count=7')
            .then(res => res.json())
            .then(data => {
                const categories = data.map(d => d.tenDoiTuong || d.maDoiTuong);
                const amounts = data.map(d => d.duNo);
                const ids = data.map(d => d.doiTuongId);

                const options = {
                    series: [{ name: 'Công nợ phải thu', data: amounts }],
                    chart: {
                        type: 'bar',
                        height: 320,
                        toolbar: { show: false },
                        fontFamily: 'var(--nt-font-family)',
                        events: {
                            dataPointSelection: function (event, chartContext, config) {
                                const selectedId = ids[config.dataPointIndex];
                                if (selectedId) {
                                    window.location.href = '/CongNo/Index?doituongId=' + selectedId;
                                }
                            }
                        }
                    },
                    colors: ['#2563EB'],
                    plotOptions: {
                        bar: { borderRadius: 4, horizontal: true }
                    },
                    dataLabels: { enabled: false },
                    xaxis: {
                        categories: categories,
                        labels: {
                            formatter: val => (val / 1000000).toLocaleString('vi-VN') + ' tr'
                        }
                    },
                    tooltip: {
                        y: { formatter: val => formatVND(val) }
                    }
                };

                const chart = new global.ApexCharts(el, options);
                chart.render();
            })
            .catch(err => console.error('Lỗi nạp TopDebtors chart:', err));
    }

    function initDashboard() {
        initKpis();
        initCashFlowChart();
        initTopDebtorsChart();
    }

    global.NtCharts = {
        initDashboard
    };

})(window);
