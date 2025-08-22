
(function () {
    ecomTable = null;
    Elements = ["UserId,RoleId"]; // form validation id onlyz

    // urls
    const saveUrl = "/api/Staff/Save";
    const deleteUrl = "/api/Staff/Delete";
    const editUrl = "/api/Staff/Edit";
    const newUrl = "/api/Staff/New";

    // success message
    const saveMessage = "Staff Saved Succcessfully!";
    const updateMessage = "Staff Updated Succcessfully!";
    const deleteMessage = "Staff Deleted Successfully";

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "autowidth": true, "orderable": true },
            { "data": "role", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Staff', full.staffId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Staff/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                model.Role = formEl.find('#RoleId').select2('data')[0].text; //formEl.find('#RoleId').find(":selected").text();
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    ecomTable.fnFilter();
                }
            } else {
                Failed("Please Select Required Fields.");
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
                ResetForm(formEl);
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const New = async () => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(newUrl);
            if (typeof (response) == "object") {
                ResetForm(formEl);
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