(function () {
    ecomTable = null;
    Elements = ["Name", "Symbol", "ExchangeRate"]; // form validation id only

    // urls
    const saveUrl = "/api/Currency/Save";
    const deleteUrl = "/api/Currency/Delete";
    const editUrl = "/api/Currency/Edit";
    const statusUpdateUrl = `/api/Currency/CurrencyStatus`;

    // success message
    const saveMessage = "Currency Saved Succcessfully!";
    const updateMessage = "Currency Updated Succcessfully!";
    const deleteMessage = "Deleted Successfully";
    const statusMessage = "Currency Status Updated Succcessfully!";

    const selector = {
        currencyStatus: ".currency-status"
    };

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "autowidth": true, "orderable": true },
            { "data": "symbol", "autowidth": true, "orderable": true },
            { "data": "code", "autowidth": true, "orderable": true },
            { "data": "exchangeRate", "autowidth": true },

            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { currencyId, status } = full;
                    return `<input type = "checkbox" class = "currency-status" ${status == 1 ? "checked" : ""} currencyId= "${currencyId}" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Currency', full.currencyId);
                    return result;
                }
            },
        ];

        let dtLoader = DataTableLoader("/api/Currency/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    ecomTable.fnFilter();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Delete = async id => {
        let decisionResult = await Decision();
        try {
            if (decisionResult) {
                const url = deleteUrl + "/" + id;
                let response = await ajaxOperation.DeleteAjaxAPI(url);
                if (typeof (response) == "number" && parseInt(response) > 0) {
                    Success(deleteMessage);
                    ResetForm(formEl);
                    ecomTable.fnFilter();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Edit = async id => {
        try {
            const url = editUrl + "/" + id;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) == "object") {
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(response);
        }
    }
    const StatusUpdate = async (currencyId, isChecked) => {
        try {
            const url = statusUpdateUrl + `/${currencyId}/${isChecked}`;
            let response = await ajaxOperation.UpdateAjaxAPI(url);
            Success(statusMessage);
            ecomTable.fnFilter();
        } catch (e) {
            Failed(e);
        }
    }
    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
    });

    $(SetValidatorElement(Elements)).keyup(function () {
        Elements.forEach(item => {
            if ($(item).val() != '') {
                $("#" + item).removeClass("is-invalid");
                $("." + item).hide();
            }
        });
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    $(tblMasterId).on("change", selector.currencyStatus, async function () {
        let currencyId = $(this).attr("currencyId");
        let isChecked = $(this).is(":checked");
        StatusUpdate(currencyId, isChecked);
    });
})();