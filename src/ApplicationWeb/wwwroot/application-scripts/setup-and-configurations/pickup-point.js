
(function () {
    ecomTable = null;
    Elements = ["Name"]; // form validation id only

    // urls
    const saveUrl = "/api/PickupPointsIndex/Save";
    const deleteUrl = "/api/PickupPointsIndex/Delete";
    const editUrl = "/api/PickupPointsIndex/Edit";
    const newUrl = "/api/PickupPointsIndex/New";
    const updatePickupPointStatusFeature = `/api/PickupPointsIndex/UpdatePickupPointStatusFeature`;

    // success message
    const saveMessage = "Pickup Point Saved Succcessfully!";
    const updateMessage = "Pickup Point Updated Succcessfully!";
    const deleteMessage = "Pickup Point Deleted Successfully";

    const selector = {
        pickUpStatusSelector: "pickUpStatus-selector",
        cashOnPickupStatusSelector: "cashOnPickupStatus-selector",
        flagUpdater: "flag-updater",
        btnDelete: "btnDelete",
    };

    const CheckboxGenerator = (className, pickupPointId, isChecked) => {
        let data = `<input type="checkbox" class = "${className} ${selector.flagUpdater}" ${isChecked == 1 ? "checked" : ""} pickupPointId="${pickupPointId}" data-bootstrap-switch data-off-color="danger" data-on-color="success">`;
        $("." + className).bootstrapSwitch();
        return data;
    }

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "autowidth": true, "orderable": true },
            { "data": "staffName", "autowidth": true, "orderable": true },
            { "data": "address", "autowidth": true, "orderable": true },
            { "data": "phone", "autowidth": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return CheckboxGenerator(selector.pickUpStatusSelector, full.pickupPointId, full.pickUpStatus);
                }
                , "autowidth": true
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return CheckboxGenerator(selector.cashOnPickupStatusSelector, full.pickupPointId, full.cashOnPickupStatus);
                }
                , "autowidth": true
            },

            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('PickupPoint', full.pickupPointId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/PickupPointsIndex/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                model.PickUpStatus = formEl.find('#PickUpStatus').val();
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
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

    $(tblMasterId).on("click", "." + selector.flagUpdater, async function () {
        let pickupPointId = $(this).attr("pickupPointId");
        let isChecked = $(this).is(":checked");
        let urls = '';
        let message = '';
        if ($(this).hasClass(selector.pickUpStatusSelector)) {
            urls = `/${pickupPointId}/PickUpStatus/${isChecked}`;
            message = 'PickUp Status';
        }
        if ($(this).hasClass(selector.cashOnPickupStatusSelector)) {
            urls = `/${pickupPointId}/CashOnPickupStatus/${isChecked}`;
            message = 'Cash On Pickup Status';
        }

        try {
            let result = await ajaxOperation.UpdateAjaxAPI(updatePickupPointStatusFeature + urls);
            if (result) {
                Success(`Successfully ${message} Updated`);
            }
            else {
                Failed(result);
            }
        } catch (e) {
            Failed(e.responseJSON.title);
        }
    });

})();