
(function () {
    ecomTable = null;
    Elements = ["Name,CountriesId"]; // form validation id only

    // urls
    const saveUrl = "/api/ShippingState/Save";
    const deleteUrl = "/api/ShippingState/Delete";
    const editUrl = "/api/ShippingState/Edit";
    const intialUrl = "/api/ShippingState/GetInitial";

    // success message
    const saveMessage = "State Saved Succcessfully!";
    const updateMessage = "State Updated Succcessfully!";
    const deleteMessage = "State Deleted Successfully";

    const GenerateList = () =>{
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "name": "name", "autowidth": true, "orderable": true },
            { "data": "countryName", "name": "countryName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('ShippingStates', full.stateId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/ShippingState/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }
    const InitialLoader = async () => {
        let response = await ajaxOperation.GetAjaxAPI(intialUrl);
        if (typeof (response) == "object") {
            setFormData(formEl, response);
        }
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
                    InitialLoader();
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
                    InitialLoader();
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

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
        InitialLoader();
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
    formEl.find("#btnCancel").click(function () {
        InitialLoader();
    });
    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });
})();