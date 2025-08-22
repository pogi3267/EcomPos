(function () {
    ecomTable = null;
    Elements = ["Code", "StartDate", "EndDate"]; // form validation id only

    // urls
    const saveUrl = "/api/Coupon/Save";
    const intialUrl = "/api/Coupon/GetInitial";
    const editUrl = "/api/Coupon/Edit";
    const deleteUrl = "/api/Coupon/Delete";
    const statusUpdateUrl = `/api/Coupon/CouponStatus`;

    // success message
    const deleteMessage = "Coupon Deleted Successfully";
    const statusMessage = "Status Updated Succcessfully!";
    const saveMessage = "Coupon Saved Succcessfully!";
    const updateMessage = "Coupon Updated Succcessfully!";

    const selector = {
        entityStatus: ".status"
    };

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "code", "name": "code", "autowidth": true, "orderable": true },
            { "data": "discount", "name": "discount", "autowidth": true, "orderable": true },
            { "data": "discountType", "name": "discountType", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.startDate);
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.endDate);
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { couponId, isActive } = full;
                    return `<input type = "checkbox" class = "status" ${isActive == true ? "checked" : ""} entityId= "${couponId}" />`;
                }
            },
            { "data": "details", "name": "details", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Coupons', full.couponId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Coupon/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const InitialLoader = async () => {
        divDetailsEl.show();
        divPrimaryEl.hide();
        ResetForm(formEl);
    }

    const Save = async () => {
        try {
            let formData = new FormData(formEl[0]);
            formData.append("IsActive", formEl.find('#IsActive').val());
            if (IsFrmValid(formEl, Elements)) {
                let response = await ajaxOperation.SaveAjax(saveUrl, formData);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    Back();
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
                divDetailsEl.show();
                divPrimaryEl.hide();
                ResetForm(formEl);
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Delete = async id => {
        let decisionResult = await Decision();
        if (decisionResult) {
            const url = deleteUrl + "/" + id;
            let response = await ajaxOperation.DeleteAjaxAPI(url);
            if (typeof (response) == "number" && parseInt(response) > 0) {
                Success(deleteMessage);
                ResetForm(formEl);
                ecomTable.fnFilter();
            }
        }
    }

    const StatusUpdate = async (entityId, isChecked) => {
        try {
            const url = statusUpdateUrl + `/${entityId}/${isChecked}`;
            let response = await ajaxOperation.UpdateAjaxAPI(url);
            if (typeof (response) == "object") {
                Success(statusMessage);
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
        ecomTable.fnFilter();
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

    formEl.find("#btnCancel").click(function () {
        Back();
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    $(tblMasterId).on("change", selector.entityStatus, async function () {
        let entityId = $(this).attr("entityId");
        let isChecked = $(this).is(":checked");
        StatusUpdate(entityId, isChecked);
    });

    divToolbarEl.find("#btnNew").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        InitialLoader();
    });

})();