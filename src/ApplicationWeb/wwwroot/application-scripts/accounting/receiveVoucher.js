(function () {
    autoRiceTable = null;
    Elements = []; // form validation id only
    let accountVoucher = new Array();
    // urls
    const saveUrl = "/api/AccountReceiveVoucher/Save";
    const deleteUrl = "/api/AccountReceiveVoucher/Delete";
    const editUrl = "/api/AccountReceiveVoucher/Edit";
    const getInitialUrl = "/api/AccountReceiveVoucher/GetInitial";
    const getLedgerBalanceById = "/api/AccountReceiveVoucher/GetLedgerBalanceById";
    const getAccountHeadBySupplierId = "/api/AccountReceiveVoucher/GetAccountHeadBySupplierId";

    // success message
    const saveMessage = "Voucher Saved Successfully";
    const updateMessage = "Voucher Updated Successfully"
    const deleteMessage = "Voucher Deleted Successfully";

    const selector = {
        payeeToTextbox: $("#payeeToTextbox"),
        supplierIdDropdown: $("#supplierIdDropdown"),
        accountType: $("#AccountType"),
        accountLedgerId: $("#AccountLedgerId"),
        ledgerBalance: $("#LedgerBalance"),
        supplierId: $("#SupplierId"),

        accountHeadId: $("#accountHeadId"),
        amount: $("#amount"),
        memo: $("#memo"),
        addAccountVoucherBtn: $("#addAccountVoucherBtn"),
        accountVoucherTbody: $("#accountVoucherTbody"),
        subTotalAmount: $("#subTotalAmount"),

        btnSave: $("#btnSave"),
    };
    
    const PayToChange = () => {
        selector.payeeToTextbox.hide();
        selector.supplierIdDropdown.hide();
        const accountType = selector.accountType.val();
        accountType == 3 && accountType !== '' ? selector.supplierIdDropdown.show()  : selector.payeeToTextbox.show() ;
    }

    const New = async () => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(getInitialUrl);
            if (typeof (response) == "object") {
                divDetailsEl.show();
                divPrimaryEl.hide();

                ResetForm(formEl);
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const GetLedgerBalance = async id => {
        try {
            if (id === '' || id === null) return;

            const url = `${getLedgerBalanceById}/${id}`;
            const response = await ajaxOperation.GetAjaxAPI(url);
            selector.ledgerBalance.val(response);
        } catch (e) {
            Failed(e);
        }
    }

    const GetAccountHead = async id => {
        try {
            if (id === '' || id === null) return;

            const url = `${getAccountHeadBySupplierId}/${id}`;
            const response = await ajaxOperation.GetAjaxAPI(url);
            let options = `<option value = ""> Select an Item </option>`;
            response.forEach(item => options += `<option value = "${item.id}"> ${item.text} </option>`);
            selector.accountHeadId.html(options);
        }
        catch (e) {
            Failed(e);
        }
    }

    const ClearAccountVoucherEntry = () =>{
        selector.accountHeadId.val('').trigger("change");
        selector.amount.val('');
        selector.memo.val('');
    }
    const CheckForValidation = () => {
        let message = '';
        if(selector.accountHeadId.val() === ''){
            message = 'Please select account Head';
        }else if(selector.amount.val() === ''){
            message = 'Please enter amount';
        }
        if(message !== ''){
            Failed(message);
            return false;
        }
        return true;
    }

    const BuildVoucherHtml = ({ uniqueIdentifier, accountHeadId, accountHeadName, amount, memo, voucherDetailsId }) => {
        accountVoucher = [...accountVoucher, { uniqueIdentifier, accountHeadId, accountHeadName, amount, memo, voucherDetailsId }];
        const html = `<tr>
                           <td>${accountHeadName}</td>
                           <td style="display:none;">${voucherDetailsId}</td>
                           <td>${amount}</td>
                           <td>${memo}</td>
                           <td>
                                <button type="button" class="btn btn-danger btnVoucherDelete" uniqueIdentifier="${uniqueIdentifier}">
                                    <i class="fa fa-trash-alt"></i>
                                </button>
                           </td>
                      </tr>`;
        selector.accountVoucherTbody.append(html);
        const subTotalAmount = accountVoucher.reduce((total, current) => {
            return total + current.amount;
        }, 0) ?? 0.000;
        selector.subTotalAmount.text(subTotalAmount.toFixed(3));
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
                    return ButtonPartial('ReceivedVoucher', full.accountVoucherId);
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/AccountReceiveVoucher/list", columns);
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const Save = async () => {
        try {
            let model = formElToJson(formEl);
            model.SubTotal = selector.subTotalAmount.text();
            if (accountVoucher.length === 0) {
                Failed("Voucher Details can't be empty");
                return;
            }
            model.AccountVoucherDetails = accountVoucher.map(item => {
                return {
                    childId: item.accountHeadId,
                    creditAmount: item.amount,
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
                setFormData(formEl, response);
                accountVoucher = new Array();
                selector.accountVoucherTbody.html('');
                console.log(response);
                response.accountVoucherDetails.forEach(item => {
                    const model = {
                        accountHeadId: item.accountVoucherId,
                        accountHeadName: "account head name", // to do...
                        amount: item.creditAmount,
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
    
    CommonInitializer();
    $(document).ready(function () {
        divDetailsEl.hide();
        GenerateList();
    });

    selector.accountType.change(function () {
        PayToChange();
    });
    selector.accountLedgerId.change(function () {
        const id = $(this).val();
        GetLedgerBalance(id);
    });

    selector.supplierId.change(function () {
        const id = $(this).val();
        GetAccountHead(id);
    });

    selector.addAccountVoucherBtn.click(function () {
        const valid = CheckForValidation();
        if (!valid) return;
        const model = {
            accountHeadId : Number(selector.accountHeadId.val()),
            accountHeadName : selector.accountHeadId.children("option").filter(":selected").text(),
            amount : Number(selector.amount.val()),
            memo : selector.memo.val(),
            uniqueIdentifier : uuidv4()
        };
        ClearAccountVoucherEntry();
        BuildVoucherHtml(model);
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
})();