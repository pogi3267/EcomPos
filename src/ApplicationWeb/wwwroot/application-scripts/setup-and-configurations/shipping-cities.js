(function () {
    ecomTable = null;
    Elements = ["Name"]; // form validation id only

    // urls
    const saveUrl = "/api/ShippingCity/Save";
    const deleteUrl = "/api/ShippingCity/Delete";
    const newUrl = "/api/ShippingCity/New";
    const editUrl = "/api/ShippingCity/Edit";

    // success message
    const saveMessage = "City Saved Succcessfully!";
    const updateMessage = "City Updated Succcessfully!";
    const deleteMessage = "City Deleted Successfully";

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "name": "name", "autowidth": true, "orderable": true },
            { "data": "cost", "name": "cost", "autowidth": true, "orderable": true },
            { "data": "countryName", "name": "countryName", "autowidth": true, "orderable": true },
            { "data": "stateName", "name": "stateName", "autowidth": true, "orderable": true },

            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('ShippingCities', full.citiesId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/ShippingCity/list", columns);
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
            Failed(e);
        }
    }

    const New = async () => {
        try {
            const url = newUrl;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) == "object") {
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
        New();
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

})();