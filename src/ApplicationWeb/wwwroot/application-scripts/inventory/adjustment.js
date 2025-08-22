(function () {
    autoRiceTable = null;
    Elements = []; // form validation id only
    const saveUrl = "/api/Adjustment/Save";
    const editUrl = "/api/Adjustment/Edit";
    const deleteUrl = "/api/Adjustment/Delete";
    const operationalUserByRole = "/api/Adjustment/GetOperationalUserByRole";

    // Success Message
    const saveMessage = "Adjustment Saved Successfully";
    const updateMessage = "Adjustment Updated Successfully";
    const deleteMessage = "Adjustment Deleted Successfully";

    var masterData = {};

    const selector = {
        btnAllList: $("#btnAllList"),
        btnNew: $("#btnNew"),
        adjustmentFor: $("#AdjustmentFor"),
        operationalUserId: $("#OperationalUserId"),
    }

    const GetOperationalUserByRole = async roleName => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(`${operationalUserByRole}/${roleName}`);
            if (typeof (response) === "object") {
                const mapper = response.map(item => {
                    return { id: item.operationalUserId, text: item.name };
                });
                initSelect2(selector.operationalUserId, mapper, true, "Select Item", false);
                if (masterData) selector.operationalUserId.val(masterData.operationalUserId).trigger('change');
            }
        } catch (e) {

        }

    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) === "object") {
                    ResetForm(formEl);
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
                    autoRiceTable.fnFilter();
                    Back();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        divPrimaryEl.show();
        autoRiceTable.fnFilter();
    }

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "invoiceNumber", "name": "invoiceNumber", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.adjustmentDate);
                }
            },
            { "data": "adjustmentFor", "name": "adjustmentFor", "autowidth": true, "orderable": true },
            { "data": "operationalUserName", "name": "operationalUserName", "autowidth": true, "orderable": true },
            { "data": "amount", "name": "amount", "autowidth": true, "orderable": true },
            { "data": "reason", "name": "reason", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('AdjustmentSetup', full.adjustmentId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Adjustment/list", columns);
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const Edit = async id => {
        try {
            divDetailsEl.show();
            divPrimaryEl.hide();
            const url = editUrl + "/" + id;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) == "object") {
                masterData = response;
                setFormData(formEl, response);
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
                    autoRiceTable.fnFilter();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        selector.adjustmentFor.select2({ width: "100%", height: "100%" });
        GenerateList();
    });

    selector.adjustmentFor.change(function () {
        GetOperationalUserByRole($(this).val());
    })
    formEl.find("#btnSave").click(async function () {
        if (!formEl.find("#AdjustmentFor").val()) {
            Failed("Please select party type!");
            formEl.find("#AdjustmentFor").focus();
            return;
        }
        if (!formEl.find("#OperationalUserId").val()) {
            Failed("Please select party name!");
            formEl.find("#OperationalUserId").focus();
            return;
        }
        if (!formEl.find("#Amount").val() || formEl.find("#Amount").val() == 0) {
            Failed("Please enter amount!");
            formEl.find("#Amount").focus();
            return;
        }
        let decisionResult = await Decision('Are you sure to save?', '', 'Yes, Save it');
        if (decisionResult) {
            Save();
        }
    });

    formEl.find("#btnCancel").click(function () {
        Back();
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });


    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    selector.btnNew.click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        divDetailsEl.show();
        divPrimaryEl.hide();
        masterData = {};
        GetOperationalUserByRole(selector.adjustmentFor.val());
    });

})();
