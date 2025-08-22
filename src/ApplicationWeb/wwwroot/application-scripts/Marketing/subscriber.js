(function () {
    ecomTable = null;
    
    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "email", "name": "email", "autowidth": true, "orderable": true }
        ];
        let dtLoader = DataTableLoader("/api/Coupon/subscriber-list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
    });

})();