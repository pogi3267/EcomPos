(function () {
    autoRiceTable = null;
    Elements = []; // form validation id only
    var initialData = {};
    // urls
    const initialUrl = "/api/CollectionDeposit/GetInitial";
    const getCustomerDueAmount = "/api/CollectionDeposit/GetCustomerDueAmount";
    const saveUrl = "/api/CollectionDeposit/Save";
    const deleteUrl = "/api/CollectionDeposit/Delete";
    const editUrl = "/api/CollectionDeposit/Edit";

    // success message
    const saveMessage = "Collection Saved Successfully!";
    const updateMessage = "Collection Updated Successfully!";
    const deleteMessage = "Collection Deleted Successfully";

    const selector = {
        depositInfo: $(".depositInfo"),
    }

    const setDepositInfo = response => {
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
    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
           /* response.depositBankList = response.bankList*/
            setDepositInfo(response);
            setFormData(formEl, response);
            initialData = response;
            formEl.find("#btnSave").prop("disabled", true);
        }
        catch (e) {
            console.log(e);
            Failed(e);
        }
    }

    const GenerateList = () => {
        let columns = [
            { "data": "chequeNo", "name": "chequeNo", "autowidth": true, "orderable": true },
            { "data": "clType", "name": "clType", "autowidth": true, "orderable": true },
            { "data": "invoiceNoCollection", "name": "invoiceNoCollection", "autowidth": true, "orderable": true },
            { "data": "collectionAmount", "name": "collectionAmount", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<input type = "button" value = "Deposit" class = "btn btn-success btn-sm btnEdit" id = "${full.collectionId}" /> `;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/CollectionDeposit/list", columns);
        
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                if (!formEl.find("#depositBankId").val()) {
                    Failed("Please select bank!");
                    return;
                }
                if (!formEl.find("#depositAccountNumberId").val()) {
                    Failed("Please select account!");
                    return;
                }
                if (!formEl.find("#DepositDate").val()) {
                    Failed("Please select date!");
                    return;
                }
                let model = formElToJson(formEl);
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) === "object") {
                    ResetForm(formEl);
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
                    autoRiceTable.fnFilter();
                    setDepositInfo(initialData);
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
           
            /*response.depositBankList = response.bankList*/
            /*console.log(response);*/
            if (typeof (response) === "object") {
                $("#CollectionId").val(response.collectionId)
                //setFormData(formEl, response);
                setDepositInfo(response);
                formEl.find("#btnSave").prop("disabled", false);
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
        setDepositInfo(initialData);
        formEl.find("#btnSave").prop("disabled", true);
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    formEl.find('#depositBankId,#depositAccountNumberId').on('select2:select', function (e) {
        debugger;
        formEl.find('#depositBankId,#depositAccountNumberId').val($(this).val()).trigger('change');
    });
})();