(function () {
    autoRiceTable = null;
    Elements = []; // form validation id only
    let accountVoucher = new Array();
    let accountLedgerList = new Array();

    // urls
    const saveUrl = "/api/AccountJournalVoucher/Save";
    const deleteUrl = "/api/AccountJournalVoucher/Delete";
    const editUrl = "/api/AccountJournalVoucher/Edit";
    const getInitialUrl = "/api/AccountJournalVoucher/GetInitial";

    // success message
    const saveMessage = "Voucher Saved Successfully";
    const updateMessage = "Voucher Updated Successfully"
    const deleteMessage = "Voucher Deleted Successfully";

    const selector = {
        accountHeadId: $("#accountHeadId"),
        debitAmount: $("#debitAmount"),
        creditAmount: $("#creditAmount"),
        memo: $("#memo"),

        debitAmountClass: "debitAmount",
        creditAmountClass: "creditAmount",

        addAccountVoucherBtn: $("#addAccountVoucherBtn"),
        accountVoucherTbody: $("#accountVoucherTbody"),

        totalDebitAmount: $("#totalDebitAmount"),
        totalCreditAmount: $("#totalCreditAmount"),
        
        btnSave: $("#btnSave"),
    };

    const BuildDropdown = (accountLedger = accountLedgerList) => {
        let accountLedgerHeadHtml = '';
        accountLedger.forEach(item => {
            accountLedgerHeadHtml += `<option value = "${item.id}" ${item?.isDisable ? "disabled" : ""}> ${item.text} </option>`;
        });
        return accountLedgerHeadHtml;
    }

    const DisablingDropdown = accountLedgerId => {
        const index = accountLedgerList.findIndex(item => item.id == accountLedgerId);
        accountLedgerList[index].isDisable = true;
    }

    const ToggolingDebitAndCreditInputField = () => {
        const debitAmount = selector.debitAmount.val();
        const creditAmount = selector.creditAmount.val();
        if (debitAmount == '' && creditAmount == '') {
            selector.debitAmount.prop('readonly', false);
            selector.creditAmount.prop('readonly', false);
        }
        else if (debitAmount != '' && debitAmount != null) {
            selector.debitAmount.prop('readonly', false);
            selector.creditAmount.prop('readonly', true);
        }
        else if (creditAmount != '' && creditAmount != null) {
            selector.creditAmount.prop('readonly', false);
            selector.debitAmount.prop('readonly', true);
        }
    }

    const New = async () => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(getInitialUrl);
            if (typeof (response) === "object") {
                divDetailsEl.show();
                divPrimaryEl.hide();

                ResetForm(formEl);
                accountLedgerList = response.accountLedgerList;
                accountLedgerList = accountLedgerList.map(item => {
                    item.isDisable = false;
                    return item;
                });
                selector.accountHeadId.html(BuildDropdown(accountLedgerList));
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const ClearAccountVoucherEntry = () =>{
        selector.accountHeadId.val('').trigger("change");
        selector.debitAmount.val('');
        selector.creditAmount.val('');
        selector.memo.val('');
    }

    const CheckForValidation = () => {
        let message = '';
        if(selector.accountHeadId.val() === ''){
            message = 'Please select account Head';
        } else if (selector.debitAmount.val() === '' && selector.creditAmount.val() === '') {
            message = 'Please enter amount';
        }
        if(message !== ''){
            Failed(message);
            return false;
        }
        return true;
    }

    const CalculateFooterSubTotal = () => {
        const totalDebitAmount = accountVoucher.reduce((total, current) => {
            return total + current.debitAmount;
        }, 0) ?? 0.000;

        const totalCreditAmount = accountVoucher.reduce((total, current) => {
            return total + current.creditAmount;
        }, 0) ?? 0.000;
        
        selector.totalDebitAmount.text(totalDebitAmount.toFixed(3));
        selector.totalCreditAmount.text(totalCreditAmount.toFixed(3));

        if (totalDebitAmount !== totalCreditAmount) {
            selector.btnSave.attr("disabled", true);
        }
        else {
            selector.btnSave.prop("disabled", false);
        }
        
    }

    const BuildVoucherHtml = ({ uniqueIdentifier, accountHeadId, accountHeadName, debitAmount, creditAmount, memo, voucherDetailsId }) => {
        accountVoucher = [...accountVoucher, { uniqueIdentifier, accountHeadId, accountHeadName, debitAmount, creditAmount, memo, voucherDetailsId }];
        const html = `<tr>
                           <td>${accountHeadName}</td>
                           <td style="display:none;">${voucherDetailsId}</td>
                           <td>
                                <input type="number" class="form-control text-right debitAmount" id="dr${uniqueIdentifier}"
                                    value = "${debitAmount}"
                                    ${debitAmount == 0 ? "readonly" : ""}
                                >
                           <td>
                                <input type="number" class="form-control text-right creditAmount" id="cr${uniqueIdentifier}"
                                    value = "${creditAmount}"
                                    ${creditAmount == 0 ? "readonly" : "false"}
                                >
                           <td>${memo}</td>
                           <td>
                                <button type="button" class="btn btn-danger btnVoucherDelete" uniqueIdentifier="${uniqueIdentifier}">
                                    <i class="fa fa-trash-alt"></i>
                                </button>
                           </td>
                      </tr>`;
        selector.accountVoucherTbody.append(html);
        CalculateFooterSubTotal();
    }

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.voucherDate);
                }
            },
            { "data": "voucherNumber", "name": "voucherNumber", "autowidth": true, "orderable": true },
            { "data": "branchName", "name": "branchName", "autowidth": true, "orderable": true },
            { "data": "created_By", "name": "created_By", "autowidth": true, "orderable": true },
            { "data": "narration", "name": "narration", "autowidth": true, "orderable": true },
            { "data": "subTotal", "name": "subTotal", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return ButtonPartial('JournalVoucher', full.accountVoucherId);
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/AccountJournalVoucher/list", columns);
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const Save = async () => {
        alert();
        try {
            let model = formElToJson(formEl);

            /* model.SubTotal = selector.subTotalAmount.text();*/

            if (accountVoucher.length === 0) {
                Failed("Voucher Details can't be empty");
                return;
            }
            model.AccountVoucherDetails = accountVoucher.map(item => {
                return {
                    childId: item.accountHeadId,
                    creditAmount: item.creditAmount,
                    debitAmount: item.debitAmount,
                    reference: item.memo,
                    accountVoucherDetailId: item.voucherDetailsId
                };
            });
            let response = await ajaxOperation.SavePostAjax(saveUrl, model);
            if (typeof (response) === "object") {
                ResetForm(formEl);
                Success(response.entityState === 4 ? saveMessage : updateMessage);
                Back();
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
                divDetailsEl.show();
                divPrimaryEl.hide();
                ResetForm(formEl);
                accountLedgerList = response.accountLedgerList;
                accountLedgerList = accountLedgerList.map(item => {
                    item.isDisable = false;
                    return item;
                });
                selector.accountHeadId.html(BuildDropdown(accountLedgerList));
                setFormData(formEl, response);
                accountVoucher = new Array();
                selector.accountVoucherTbody.html('');
                response.accountVoucherDetails.forEach(item => {
                    const model = {
                        accountHeadId: item.accountVoucherId,
                        accountHeadName: "account head name", // to do...
                        creditAmount: item.creditAmount,
                        debitAmount: item.debitAmount,
                        memo: item.reference,
                        voucherDetailsId: item.voucherDetailsId,
                        uniqueIdentifier: uuidv4()
                    };
                    BuildVoucherHtml(model);
                });
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
            console.log(e);
            Failed(e);
        }
    }

    const Back = async () => {
        accountVoucher = new Array();
        selector.accountVoucherTbody.html('');
        divDetailsEl.hide();
        ResetForm(formEl);
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        divPrimaryEl.show();
        autoRiceTable.fnFilter();
    }

    const CalculatingByDebitCreditChange = unknownPlaceId => {
        const uniqueId = unknownPlaceId.substring(2);
        let debitAmountId = $('#dr' + uniqueId);
        let creditAmountId = $('#cr' + uniqueId);

        const debitAmount = debitAmountId.val();
        const creditAmount = creditAmountId.val();

        if ((debitAmount == '' && creditAmount == 0) || (creditAmount == '' && debitAmount == 0)) {
            debitAmountId.prop('readonly', false);
            creditAmountId.prop('readonly', false);
        }
        else if (debitAmount != 0) {
            debitAmountId.prop('readonly', false);
            creditAmountId.prop('readonly', true);
        }
        else if (creditAmount != 0) {
            creditAmountId.prop('readonly', false);
            debitAmountId.prop('readonly', true);
        }
        const index = accountVoucher.findIndex(item => item.uniqueIdentifier == uniqueId);
        accountVoucher[index].debitAmount = Number(debitAmount);
        accountVoucher[index].creditAmount = Number(creditAmount);
        CalculateFooterSubTotal();
    }

    CommonInitializer();
    $(document).ready(function () {
        divDetailsEl.hide();
        GenerateList();
    });

    selector.addAccountVoucherBtn.click(function () {
        const valid = CheckForValidation();
        if (!valid) return;
        const model = {
            accountHeadId : Number(selector.accountHeadId.val()),
            accountHeadName: selector.accountHeadId.children("option").filter(":selected").text(),
            debitAmount: Number(selector.debitAmount.val()),
            creditAmount: Number(selector.creditAmount.val()),
            memo : selector.memo.val(),
            uniqueIdentifier : uuidv4()
        };
        ClearAccountVoucherEntry();
        BuildVoucherHtml(model);

        //performnig extra operation

        DisablingDropdown(model.accountHeadId);
        selector.accountHeadId.html(BuildDropdown(accountLedgerList));
        ToggolingDebitAndCreditInputField();
    });
    
    $(document).on("click", ".btnVoucherDelete", async function(){
        const decision = await Decision();
        if(decision){
            const uniqueIdentifier = $(this).attr('uniqueIdentifier');
            const filteredAccountVoucher = accountVoucher.filter(item => item.uniqueIdentifier !== uniqueIdentifier);
            accountVoucher = [];
            selector.accountVoucherTbody.html('');
            filteredAccountVoucher.forEach(item => BuildVoucherHtml(item));
        }
    });

    selector.btnSave.click(async function () {
        try {
            let decisionResult = await Decision('Are you sure to save?', '', 'Yes, Save it');
            if (decisionResult) {
                Save();
            }
        } catch (e) {

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
    divToolbarEl.find("#btnNew").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        New();
    });

    selector.debitAmount.keyup(function () {
        ToggolingDebitAndCreditInputField();
    });

    selector.creditAmount.keyup(function () {
        ToggolingDebitAndCreditInputField();
    });

    selector.debitAmount.change(function () {
        ToggolingDebitAndCreditInputField();
    });
    selector.creditAmount.change(function () {
        ToggolingDebitAndCreditInputField();
    });

    $(document).on("keyup", "." + selector.debitAmountClass, function () {
        let unknownPlaceId = $(this).attr('id');
        CalculatingByDebitCreditChange(unknownPlaceId);
    });

    $(document).on("keyup", "." + selector.creditAmountClass, function () {
        let unknownPlaceId = $(this).attr('id');
        CalculatingByDebitCreditChange(unknownPlaceId);
    });
    $(document).on("change", "." + selector.debitAmountClass, function () {
        let unknownPlaceId = $(this).attr('id');
        CalculatingByDebitCreditChange(unknownPlaceId);
    });
    $(document).on("change", "." + selector.creditAmountClass, function () {
        let unknownPlaceId = $(this).attr('id');
        CalculatingByDebitCreditChange(unknownPlaceId);
    });

})();