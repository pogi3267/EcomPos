(function () {
    autoRiceTable = null;
    Elements = []; // form validation id only
    // urls
    const saveUrl = "/api/PaymentFinalized/Save";
    const deleteUrl = "/api/PaymentFinalized/Delete";
    const editUrl = "/api/PaymentFinalized/Edit";

    // success message
    const saveMessage = "Payment Saved Successfully!";
    const updateMessage = "Payment Updated Successfully!";
    const deleteMessage = "Payment Deleted Successfully";

    const selector = {
        depositInfo: $(".depositInfo"),
    }

    const setDepositInfo = response => {
        if (response) {
            selector.depositInfo.each(function () {
                let propName = $(this).attr("for");
                propName = propName[0].toLowerCase() + propName.substring(1);
                if (propName.toLowerCase().includes("date")) {
                    $(this).text(setDateFormat(response[propName]));
                    return;
                }
                $(this).text(response[propName]);
            });
        }
        else {
            selector.depositInfo.each(function () {
                let propName = $(this).attr("for");
                propName = propName[0].toLowerCase() + propName.substring(1);
                $(this).text("");
            });
        }
    }
    

    const GenerateList = () => {
        let columns = [
            { "data": "payInvoiceNo", "name": "payInvoiceNo", "autowidth": true, "orderable": true },
            { "data": "supplierName", "name": "supplierName", "autowidth": true, "orderable": true },
            { "data": "chequeNo", "name": "chequeNo", "autowidth": true, "orderable": true },
            { "data": "payType", "name": "payType", "autowidth": true, "orderable": true },
            { "data": "payAmount", "name": "payAmount", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<input type = "button" value = "Deposit" class = "btn btn-success btn-sm btnEdit" id = "${full.paymentId}" /> `;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/PaymentFinalized/list", columns);
        
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
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
                    setDepositInfo(null);
                    formEl.find("#btnSave").prop("disabled", true);
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
          
            if (typeof (response) === "object") {
                $("#PaymentId").val(response.paymentId)
                setDepositInfo(response);
                formEl.find("#btnSave").prop("disabled", false);
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
        formEl.find("#btnSave").prop("disabled", true);
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
        setDepositInfo(null);
        ResetForm(formEl);
        formEl.find("#btnSave").prop("disabled", true);
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });
    
})();