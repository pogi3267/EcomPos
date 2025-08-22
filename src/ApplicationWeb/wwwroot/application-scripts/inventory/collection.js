
(function () {
   ecomTable = null;
    Elements = []; // form validation id only
    var masterData = {};
    // urls
    const initialUrl = "/api/UserCollection/GetInitial";
    const getCustomerDueAmount = "/api/UserCollection/GetCustomerDueAmount";
    const saveUrl = "/api/UserCollection/Save";
    const deleteUrl = "/api/UserCollection/Delete";
    const editUrl = "/api/UserCollection/Edit";

    // success message
    const saveMessage = "Collection Saved Successfully!";
    const updateMessage = "Collection Updated Successfully!";
    const deleteMessage = "Collection Deleted Successfully";

    const selector = {
        dueMoneyAmount: $("#dueMoneyAmount"),
        customerId: $("#CustomerId"),
        collectionType: $("#Collectiontype"),
        bankId: $("#BankId"),
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

    const GetCustomerDueAmount = async customerId => {
        try {
            if (!customerId) return; // if empty

            const url = `${getCustomerDueAmount}/${customerId}`;
            const response = await ajaxOperation.GetAjaxAPI(url);
            if (response) {
                if (customerId == 5001) { // retail customer
                    selector.dueMoneyAmount.val(response.salseAmount.toFixed(2));
                    formEl.find("#salesNo").val(response.invoice);
                    formEl.find("#divChallanType").hide();
                    formEl.find("#divSalesNo").show();
                }
                else {
                    selector.dueMoneyAmount.val(response.oldbalance.toFixed(2));
                    formEl.find("#divChallanType").show();
                    formEl.find("#divSalesNo").hide();
                    formEl.find("#salesNo").val("");
                }

            } else {
                if (customerId == 5001) { // retail customer
                    formEl.find("#divChallanType").hide();
                    formEl.find("#divSalesNo").show();
                }
                else {
                    formEl.find("#divChallanType").show();
                    formEl.find("#divSalesNo").hide();
                    formEl.find("#salesNo").val("");
                }
                Failed("কোনো ডাটা পাওয়া যায়নি!");
            }
        }
        catch (e) {
            console.log(e);
            Failed(e);
        }
    }

    const EnableDisbaleBankInfo = collectionType => {
        let isDisable = collectionType === "cash" || !collectionType;

        selector.bankId.prop("disabled", isDisable);
        selector.chequeNo.prop("disabled", isDisable);
        selector.chequeDate.prop("disabled", isDisable);
        if (isDisable) selector.chequeDate.val("");
        else selector.chequeDate.val(DateFormatter(new Date()));
    }


    const GenerateList = () => {
        let columns = [
            {
               "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "invoiceNoCollection", "name": "invoiceNoCollection", "autowidth": true, "orderable": true },
            { "data": "customerName", "name": "customerName", "autowidth": true, "orderable": true },
            { "data": "chequeNo", "name": "chequeNo", "autowidth": true, "orderable": true },
            { "data": "collectiontype", "name": "collectiontype", "autowidth": true, "orderable": true },
            { "data": "bankName", "name": "bankName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.chequeDate);
                }
            },
            { "data": "collectionAmount", "name": "collectionAmount", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Collection', full.collectionId);
                    return result;
                }
            }, 
        ];
        let dtLoader = DataTableLoader("/api/UserCollection/list", columns);
       ecomTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const CheckValidity = () => {
        if (!formEl.find("#CustomerId").val()) {
            Failed("Please select customer!");
            formEl.find("#CustomerId").focus()
            return false;
        }

        if (formEl.find("#CustomerId").val() != 5001 && !formEl.find("#chalanType").val()) {
            Failed("Please select Challan type!");
            formEl.find("#chalanType").focus()
            return false;
        }

        if (!formEl.find("#Collectiontype").val()) {
            Failed("Please select collection type!");
            formEl.find("#Collectiontype").focus()
            return false;
        }

        if (!formEl.find("#CollectionDate").val()) {
            Failed("Please select collection date!");
            formEl.find("#CollectionDate").focus()
            return false;
        }

        if (!formEl.find("#CollectionAmount").val()) {
            Failed("Please select collection amount!");
            formEl.find("#CollectionAmount").focus()
            return false;
        }

        const collectionType = selector.collectionType.val();
        if (collectionType !== "cash") {
            if (!selector.bankId.val() || !selector.chequeNo.val() || !selector.chequeDate.val()) {
                Failed("Please fill up bank info");
                return false;
            }
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
            if (CheckValidity()) { //&& IsFrmValid(formEl, Elements)
                let model = formElToJson(formEl);
                model.customerName = selector.customerId.children("option").filter(":selected").text();
                model.bankName = selector.bankId.children("option").filter(":selected").text();
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);

                if (typeof (response) === "object") {
                    ResetForm(formEl);
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
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
                response.customerList = masterData.customerList;
                response.branchList = masterData.branchList;
                response.bankList = masterData.bankList;
                setFormData(formEl, response);
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

    selector.customerId.change(function () {
        GetCustomerDueAmount($(this).val());
    });
    selector.collectionType.change(function () {
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

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });
})();