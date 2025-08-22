(function () {
    autoRiceTable = null;
    Elements = ["ParentName"]; // form validation id only
    let hasChanged = false;
    let accountLedgerList = new Array();
    // urls
    const saveUrl = "/api/AccountLedger/Save";
    const deleteUrl = "/api/AccountLedger/Delete";
    const editUrl = "/api/AccountLedger/Edit";
    const codeGeneratorUrl = "/api/AccountLedger/GetAccountLedgerCode";
    const getParentLedger = "/api/AccountLedger/GetParentLedger";
    const getAccountLedgerByParentId = "/api/AccountLedger/GetAccountLedgerByParentId";

    // success message
    const saveMessage = "AccountLedger Saved Successfully!";
    const updateMessage = "AccountLedger Updated Successfully!";
    const deleteMessage = "AccountLedger Deleted Successfully";

    const selector = {
        rootAccount: $("#RootAccount"),
        code: $("#Code"),
        parentId: $("#ParentId"),
        chartOfAccountDetailTable: $("#chartOfAccountDetailTable"),
        chartOfAccountDetailTbody: $("#chartOfAccountDetailTbody"),
        btnCancel: $("#btnCancel"),
    }

    const formClear = () => {
        ResetForm(formEl);
        $("#ParentId").prop('disabled', false);
        $("#RootAccount").prop('disabled', false);
        hasChanged = false;
        if ($.fn.DataTable.isDataTable('#chartOfAccountDetailTable')) {
            $('#chartOfAccountDetailTable').DataTable().destroy();
            selector.chartOfAccountDetailTbody.html('');
        }
    }

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "code", "name": "code", "autowidth": true, "orderable": true },
            { "data": "parentName", "name": "parentName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const active = `<span class="badge badge-success">Active</span>`;
                    const inactive = `<span class="badge badge-danger">Inactive</span>`;
                    const { posted } = full;
                    return posted ? active : inactive;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return ButtonPartial('ChartofAccount', full.id);
                }
            },
            
        ];
        let dtLoader = DataTableLoader("/api/AccountLedger/list", columns);
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const LoadParentLedger = async () => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(getParentLedger);
            ResetForm(formEl);
            initSelect2(formEl.find("#ParentId"), response.parentList, true, "Select One", true);
            accountLedgerList = response.parentList;
        } catch (e) {
            Failed(e);
        }
    }

    const buildHtml = accountLedgerParentList => {
        let html = '';
        const requiredKeys = ["serialNumber", "rootParentName", "parentName"];
        let serialNumber = 1;
        accountLedgerParentList.forEach(accountLedger => {
            accountLedger.serialNumber = serialNumber++;
            html += `<tr>`;
            requiredKeys.forEach(item => {
                html += `<td>${accountLedger[item]}</td>`;
            });
            html += `</tr>`;
        });
        return html;
    }

    const populateAccountLedgerTable = async parentId => {
        try {
            if ($.fn.DataTable.isDataTable('#chartOfAccountDetailTable')) {
                $('#chartOfAccountDetailTable').DataTable().destroy();
                selector.chartOfAccountDetailTbody.html('');
            }
            const url = `${getAccountLedgerByParentId}/${parentId}`;
            const response = await ajaxOperation.GetAjaxAPI(url);
            selector.chartOfAccountDetailTbody.html(buildHtml(response));
            if (!$.fn.DataTable.isDataTable('#chartOfAccountDetailTable')) {
                $('#chartOfAccountDetailTable').dataTable();
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                model.Posted = formEl.find("#Posted").is(':checked');
                model.ParentId = (!model.ParentId) ? 0 : model.ParentId;
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) === "object") {
                    ResetForm(formEl);
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
                    $("#ParentId").prop('disabled', false);
                    $("#RootAccount").prop('disabled', false);
                    LoadParentLedger();
                    autoRiceTable.fnFilter();
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
                    autoRiceTable.fnFilter();
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
                hasChanged = true;
                response.parentList = accountLedgerList;
                setFormData(formEl, response);
                $("#ParentId").prop('disabled', true);
                $("#RootAccount").prop('disabled', true);
                //selector.code.val(response.code);
                hasChanged = false;
            }
        } catch (e) {
            Failed(e);
        }
    }

    const RootAccountChange = async code => {
        try {
            if (hasChanged) return;
            const url = codeGeneratorUrl + "/" + code;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) === "string") {
                selector.code.val(response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const getCodeFromAccountLedger = id => {
        const accountLedger = accountLedgerList.find(item => item.id == id);
        return accountLedger.description;
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
        LoadParentLedger();
    });

    $(SetValidatorElement(Elements)).keyup(function () {
        Elements.forEach(item => {
            if ($(item).val() !== '') {
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

    selector.rootAccount.change(function () {
        debugger;
        const code = $(this).val();
        if (!code) return;
        RootAccountChange(code);
    });
    selector.parentId.change(function () {
        debugger;
        const id = $(this).val();
        if (!id) return;
        const ledgerCode = getCodeFromAccountLedger(id);
        RootAccountChange(ledgerCode);
        populateAccountLedgerTable(id);
    });

    selector.btnCancel.click(function () {
        formClear();
    });
})();