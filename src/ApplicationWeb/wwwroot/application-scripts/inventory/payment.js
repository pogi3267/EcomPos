
(function () {
    autoRiceTable = null;
    Elements = []; // form validation id only
    var masterData = {};
    // urls
    const initialUrl = "/api/UserPayment/GetInitial";
    const getSupplierDueAmount = "/api/UserPayment/GetSupplierDueAmount";
    const saveUrl = "/api/UserPayment/Save";
    const deleteUrl = "/api/UserPayment/Delete";
    const editUrl = "/api/UserPayment/Edit";

    // success message
    const saveMessage = "Payment Saved Successfully!";
    const updateMessage = "Payment Updated Successfully!";
    const deleteMessage = "Payment Deleted Successfully";

    const selector = {
        dueMoneyAmount: $("#dueMoneyAmount"),
        supplierId: $("#SupplierId"),
        payType: $("#PayType"),
        bankId: $("#payBankId"),
        payAccountNo: $("#payAccountNo"),
        chequeNo: $("#ChequeNo"),
        chequeDate: $("#ChequeDate")
    };

    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            setFormData(formEl, response);
            masterData = response;
        }
        catch (e) {
            console.log(e);
            Failed(e);
        }
    }

    const GetSupplierDueAmount = async supplierId => {
        try {
            if (!supplierId) return; // if empty

            const url = `${getSupplierDueAmount}/${supplierId}`;
            const response = await ajaxOperation.GetAjaxAPI(url);
            if (response) {
                selector.dueMoneyAmount.val(response.dueBalance.toFixed(2));
            }
        }
        catch (e) {
            Failed(e);
        }
    }

    const EnableDisbaleBankInfo = payType => {
        let isDisable = payType === "cash" || !payType;

        selector.bankId.prop("disabled", isDisable);
        selector.payAccountNo.prop("disabled", isDisable);
        selector.chequeNo.prop("disabled", isDisable);
        selector.chequeDate.prop("disabled", isDisable);
    }


    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "payInvoiceNo", "name": "payInvoiceNo", "autowidth": true, "orderable": true },
            { "data": "supplierName", "name": "supplierName", "autowidth": true, "orderable": true },
            { "data": "chequeNo", "name": "chequeNo", "autowidth": true, "orderable": true },
            { "data": "payType", "name": "payType", "autowidth": true, "orderable": true },
            { "data": "bankName", "name": "bankName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.chequeDate);
                }
            },
            { "data": "payAmount", "name": "payAmount", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return ButtonPartial('UserPayment', full.paymentId);
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/UserPayment/list", columns);
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const CheckValidity = () => {
        if (!formEl.find("#SupplierId").val()) {
            Failed("Please select party!");
            formEl.find("#SupplierId").focus()
            return false;
        }

        if (!formEl.find("#PayDate").val()) {
            Failed("Please select date!");
            formEl.find("#PayDate").focus()
            return false;
        }

        if (!formEl.find("#chalanType").val()) {
            Failed("Please select challan type!");
            formEl.find("#chalanType").focus()
            return false;
        }

        if (!formEl.find("#NextPayDate").val()) {
            Failed("Please select next pay date!");
            formEl.find("#NextPayDate").focus()
            return false;
        }

        if (!formEl.find("#PayType").val()) {
            Failed("Please select payment type!");
            formEl.find("#PayType").focus()
            return false;
        }

        const payType = selector.payType.val();
        if (payType !== "cash") {
            if (!selector.bankId.val() || !selector.chequeNo.val() || !selector.chequeDate.val()) {
                Failed("Please fill up bank info");
                return false;
            }
        }

        if (!formEl.find("#PayAmount").val()) {
            Failed("Please enter payment amount!");
            formEl.find("#PayAmount").focus()
            return false;
        }

        if (!formEl.find("#BranchId").val()) {
            Failed("Please select branch!");
            formEl.find("#BranchId").focus()
            return false;
        }

        return true;
    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements) && CheckValidity()) {
                let model = formElToJson(formEl);
                model.supplierName = selector.supplierId.children("option").filter(":selected").text();
                model.bankName = selector.bankId.children("option").filter(":selected").text();
                model.accountNo = selector.payAccountNo.children("option").filter(":selected").text();
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);

                if (typeof (response) === "object") {
                    ResetForm(formEl);
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
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
                response.supplierList = masterData.supplierList;
                response.branchList = masterData.branchList;
                response.bankList = masterData.bankList;
                response.bankAccountList = masterData.bankAccountList;
                setFormData(formEl, response);
                formEl.find('#payAccountNo').val(response.bankId).trigger('change');
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GetInitial();
        GenerateList();
    });

    selector.supplierId.change(function () {
        GetSupplierDueAmount($(this).val());
    });
    selector.payType.change(function () {
        EnableDisbaleBankInfo($(this).val());
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

    formEl.find("#btnCancel").click(function () {
        ResetForm(formEl);
        setFormData(formEl, masterData);
    });

    formEl.find('#payBankId,#payAccountNo').on('select2:select', function (e) {
        formEl.find('#payBankId,#payAccountNo').val($(this).val()).trigger('change');
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });
})();