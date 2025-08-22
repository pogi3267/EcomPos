(function () {
    ecomTable = null;

    const { paramName: dnparamName } = GetPageInformation();
    paramName = dnparamName;

    let status = {
        all: "all",
        active: "active",
        inactive: "inactive",
        currentStatus: ""
    };

    const GenerateList = (status) => {
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
                    const { avatar } = full;
                    return `<img src = "${avatar}" height = "40" width = "40" alt = "No Image" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { firstName, lastName } = full;
                    return `<span> ${firstName} ${lastName} </span>`;
                }
            },
            { "data": "email", "name": "email", "autowidth": true, "orderable": true },
            { "data": "phoneNumber", "name": "phoneNumber", "autowidth": true, "orderable": true },
            { "data": "totalOrder", "name": "totalOrder", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { id, banned } = full;
                    return `<input type = "checkbox" class = "status" ${banned == 0 ? "checked" : ""} entityId= "${id}" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Customerlist', full.id);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Customer/list", columns, status);

        // Destroy existing DataTable if it exists
        if ($.fn.DataTable.isDataTable(divPrimaryEl.find(tblMasterId))) {
            divPrimaryEl.find(tblMasterId).DataTable().destroy();
        }


        // Initialize the new DataTable
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;

    }

    const Edit = async id => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/Customer/get/${id}`);
            if (typeof (response) == "object") {
                divDetailsEl.show();
                divPrimaryEl.hide();
                ResetForm(formEl);
                setFormData(formEl, response);

                var img = $('<img />', {
                    src: response.avatar,
                    css: {
                        width: '100%', 
                        height: '150px'
                    }
                });
                formEl.find('#idImg').append(img);

                formEl.find('#customerName').text(response.firstName + " " + response.lastName);
                formEl.find('#customerEmail').text(response.email);
                formEl.find('#customerPhoneNumber').text(response.phoneNumber);
                formEl.find('#customerJoinedDate').text(setDateFormat(response.createdDate));

                formEl.find('#customerAddress').text(response.address);
                formEl.find('#customerCity').text(response.city);
                formEl.find('#customerState').text(response.state);
                formEl.find('#customerCountry').text(response.country);
                formEl.find('#customerPostalCode').text(response.postalCode);

                formEl.find('#totalOrder').text(response.totalOrder);
                formEl.find('#ongoingOrder').text(response.ongoingOrder);
                formEl.find('#completeOrder').text(response.completeOrder);
                formEl.find('#canceledOrder').text(response.canceledOrder);
                formEl.find('#returnedOrder').text(response.returnedOrder);
                formEl.find('#failedToDeliverOrder').text(response.failedToDeliverOrder);

                let slNo = 1;
                response.orders.forEach(model => {
                    model.slNo = slNo++;
                    BuildDetailsHtml(model)
                });
            }
        } catch (e) {
            Failed(e);
        }
    }

    const BuildDetailsHtml = model => {
        let serialNumber = model.slNo;
        let html = `<td>${serialNumber}</td>`;

        html += `<td><p>${model.code}</p></td>`
        html += `<td><p>${model.grandTotal}</p></td>`
        html += `<td><p>${model.paymentStatus}</p></td>`
        html += `<td><p>${model.deliveryStatus}</p></td>`

        formEl.find('#itemTbody').append(`<tr id = "${serialNumber}">${html}</tr>`);
    }

    const InitialLoader = async () => {
        divDetailsEl.show();
        divPrimaryEl.hide();
        ResetForm(formEl);
    }

    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        formEl.find('#itemTbody').html('');
        formEl.find('#idImg').empty();

        if (status.currentStatus == status.all) ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        else if (status.currentStatus == status.active) ToggleActiveToolbarBtn(divToolbarEl.find("#btnActiveList"), divToolbarEl);
        else if (status.currentStatus == status.inactive) ToggleActiveToolbarBtn(divToolbarEl.find("#btnInactiveList"), divToolbarEl);

        divPrimaryEl.show();
        ecomTable.fnFilter();
    }


    const StatusUpdate = async (entityId, isChecked) => {
        try {
            const url = `/api/Customer/update-status/${entityId}/${isChecked}`;
            let response = await ajaxOperation.UpdateAjaxAPI(url);
            Success("Updated Successfully!");
            ecomTable.fnFilter();
        } catch (e) {
            Failed(e);
        }
    }


    CommonInitializer();

    $(document).ready(function () {
        if (paramName == "Active") {
            status.currentStatus = status.active;
            divToolbarEl.find("#btnAllList,#btnInactiveList").hide();
            divToolbarEl.find("#btnActiveList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnActiveList"), divToolbarEl);
        } else if (paramName == "Inactive") {
            status.currentStatus = status.inactive;
            divToolbarEl.find("#btnAllList,#btnActiveList").hide();
            divToolbarEl.find("#btnInactiveList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnInactiveList"), divToolbarEl);
        } else {
            status.currentStatus = status.all;
            divToolbarEl.find("#btnAllList,#btnActiveList,#btnInactiveList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        }

        GenerateList(status.currentStatus);
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    formEl.find("#btnCancel").click(function () {
        Back();
    });

    divToolbarEl.find("#btnAllList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.all);
        status.currentStatus = status.all;
    });

    divToolbarEl.find("#btnActiveList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.active);
        status.currentStatus = status.active;
    });

    divToolbarEl.find("#btnInactiveList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.inactive);
        status.currentStatus = status.inactive;
    });

    $(tblMasterId).on("change", ".status", async function () {
        let entityId = $(this).attr("entityId");
        let isChecked = $(this).is(":checked");
        StatusUpdate(entityId, isChecked);
    });

})();