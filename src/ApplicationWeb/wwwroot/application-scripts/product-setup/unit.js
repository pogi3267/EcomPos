
(function () {
    ecomTable = null;
    Elements = ["Name", "Abbreviation"]; // form validation id only

    // urls
    const saveUrl = "/api/Unit/Save";
    const deleteUrl = "/api/Unit/Delete";
    const editUrl = "/api/Unit/Edit";

    // success message
    const saveMessage = "Unit Saved Succcessfully!";
    const updateMessage = "Unit Updated Succcessfully!";
    const deleteMessage = "Unit Deleted Successfully";

    const GenerateList = () =>{
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "name": "name", "autowidth": true, "orderable": true },
            { "data": "abbreviation", "name": "abbreviation", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Unit', full.unitId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Unit/list", columns);
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
})();