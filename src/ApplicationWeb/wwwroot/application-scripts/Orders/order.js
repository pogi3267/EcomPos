(function () {
    ecomTable = null;
    let master = {};
    let productStocks = [];
    let editedId = 0, vaiant = '';
    let editedDetail = {};

    const { paramName: dnparamName, pickupOrder: dnpickupOrder, detailsForOrderId: dndetailsForOrderId } = GetPageInformation();
    paramName = dnparamName;
    pickupOrder = dnpickupOrder;
    detailsForOrderId = dndetailsForOrderId;

    let status = {
        all: "all",
        pending: "pending",
        confirmed: "confirmed",
        packaging: "packaging",
        outForDelivery: "outForDelivery",
        delivered: "delivered",
        returned: "returned",
        failedToDeliver: "failedToDeliver",
        canceled: "canceled",
        currentStatus: ""
    };

    const GenerateList = (status, isPickupOrder) => {
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
                    const { code, date } = full;
                    return `<p>${code}</br>${setDateTimeFormat(date)}</p>`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { customerName, email, phoneNumber } = full;
                    return `<p>${customerName}</br>${email}</br>${phoneNumber}</p>`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { grandTotal, paymentType, paymentStatus } = full;
                    return `<p>${grandTotal}</br>${paymentType}</br>${paymentStatus}</p>`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { deliveryStatus, shippingType } = full;
                    return `<p>${deliveryStatus}</br>${shippingType || 'Not Selected'}</p>`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const viewButton =
                        `<div class="btn-group"><button type = "button" class="btn btn-success btn-sm btnViewOrder" id = "${full.ordersId}">View</button>&nbsp`;
                    const invoiceButton = ` <a href="/Admin/Reports/GetInvoice/${full.ordersId}" target="_blank" class="btn btn-warning btn-sm ml-2">Print</a> </div>`;
                    const result = ButtonPartial('AllOrders', full.ordersId);
                    return viewButton + result + invoiceButton;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/OrderList/list", columns, status, isPickupOrder);

        if ($.fn.DataTable.isDataTable(divPrimaryEl.find(tblMasterId))) {
            divPrimaryEl.find(tblMasterId).DataTable().destroy();
        }

        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;

    }

    const Edit = async (id, isView = false) => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/OrderList/get/${id}`);
            if (typeof (response) == "object") {
                divDetailsEl.show();
                divPrimaryEl.hide();
                ResetForm(formEl);
                setFormData(formEl, response);
                master = response;

                formEl.find('#customerName').text(response.customerName);
                formEl.find('#email').text(response.email);
                formEl.find('#phoneNumber').text(response.phoneNumber);

                if (pickupOrder?.toLowerCase?.() === 'true') {
                    formEl.find('#divNormalOrder').hide();
                    formEl.find('#divPickupOrder').show();
                    formEl.find('#pickupPointName').text(response.pickupPointName);
                    formEl.find('#pickupPointPhone').text(response.pickupPointPhone);
                    formEl.find('#pickupPointAddress').text(response.pickupPointAddress);
                }
                else {
                    formEl.find('#divNormalOrder').show();
                    formEl.find('#divPickupOrder').hide();
                    formEl.find('#receiverName').text(response.address.receiverName);
                    formEl.find('#receiverPhone').text(response.address.phone);
                    formEl.find('#receiverAddress').text(response.address.address1);
                    formEl.find('#receiverCity').text(response.address.city);
                    formEl.find('#receiverState').text(response.address.state);
                    formEl.find('#receiverCountry').text(response.address.country);
                }

                $.each(response.productList, function (index, item) {
                    item.id = item.productId;
                    item.text = item.name;
                });
                initSelect2(formEl.find("#detailProduct"), response.productList, true, "Select Item", true);

                BuildOrderDetails();

                if (isView) {
                    formEl.find('#DeliveryStatus,#ShippingType').prop('disabled', true);
                    $('#isPaymentStatusPaid').bootstrapSwitch('disabled', true);
                    formEl.find('#CourierName,#CourierTrackingNo').prop('disabled', true);
                    formEl.find('#btnSave,#btnEditOrder,#btnResetOrder').prop('disabled', true);
                }
                else {
                    formEl.find('#DeliveryStatus,#ShippingType').prop('disabled', false);
                    $('#isPaymentStatusPaid').bootstrapSwitch('disabled', false);
                    formEl.find('#CourierName,#CourierTrackingNo').prop('disabled', false);
                    formEl.find('#btnSave,#btnEditOrder,#btnResetOrder').prop('disabled', false);
                }

                formEl.find("#detailQty").val(1);
                formEl.find("#detailRebate").val(0);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const BuildOrderDetails = async() => {
        let summaryPrice = 0, summaryDiscount = 0, summaryTax = 0, summaryShippingCost = 0, summarySubTotal = 0, summaryCouponDiscount = 0, summaryTotal = 0, summaryRebate = 0;
        let slNo = 1;
        master.orderDetailsList.forEach(model => {
            model.slNo = slNo++;
            BuildDetailsHtml(model)

            summaryPrice += parseFloat(model.price);
            summaryTax += parseFloat(model.tax);
            summaryShippingCost += parseFloat(model.shippingCost);
            summaryRebate += parseFloat(model.adminDiscount);
            summarySubTotal += (parseFloat(model.price) + parseFloat(model.tax) + parseFloat(model.shippingCost));
        });

        summaryCouponDiscount += parseFloat(master.couponDiscount);
        summaryTotal += parseFloat(summarySubTotal - (master.couponDiscount + summaryRebate));

        formEl.find('#summaryPrice').text(summaryPrice.toFixed(2));
        formEl.find('#summaryTax').text(summaryTax.toFixed(2));
        formEl.find('#summaryShippingCost').text(summaryShippingCost.toFixed(2));
        formEl.find('#summarySubTotal').text(summarySubTotal.toFixed(2));
        formEl.find('#summaryRebate').text(summaryRebate.toFixed(2));
        formEl.find('#summaryCouponDiscount').text(summaryCouponDiscount.toFixed(2));
        formEl.find('#summaryTotal').text(summaryTotal.toFixed(2));

        master.grandTotal = summaryTotal;
    }

    const BuildDetailsHtml = (model) => {
        let serialNumber = model.slNo;
        let html = `
        <tr id="${serialNumber}">
            <td>${serialNumber}</td>
            <td><img src="${model.thumbnailImage}" height="40" width="40" alt="No Image"></td>
            <td>
                <p>
                    Name: ${model.productName}<br>
                    Price: ${model.productPrice.toFixed(2)}
                </p>
            </td>
            <td>
                <p>
                    Variant: ${model.variation}<br>
                    Price: ${model.variantPrice.toFixed(2)}
                </p>
            </td>
            <td>${model.quantity}</td>
            <td>${model.price.toFixed(2)}</td>
            <td>${model.tax.toFixed(2)}</td>
            <td>${model.shippingCost.toFixed(2)}</td>
            <td>${(model.price + model.tax + model.shippingCost).toFixed(2)}</td>
            <td>${model.adminDiscount}</td>
            <td>
                <button type="button" class="btn btn-sm btnDetailsEdit" uniqueIdentity="${serialNumber}" title="Edit">
                    <i class="fas fa-edit"></i>
                </button>
            </td>
        </tr>
    `;
        $('#itemTbody').append(html);
    };

    const GetVariants = async (id) => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/Product/GetVariant/${id}`);
            $.each(response, function (index, item) {
                item.id = item.productStockId;
                item.text = item.variant;
            });
            initSelect2(formEl.find("#detailVariant"), response, true, "Select Item", true);
            productStocks = response;
            
            if (vaiant) {
                let stck = productStocks.find(item => item.text == vaiant);
                formEl.find("#detailVariant").val(stck.id).trigger('change').trigger('select2:select'); 
            }
            
        } catch (e) {
            Failed(e);
        }
    }

    const GetOrderDetail = async () => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/OrderList/order-product-details/${formEl.find('#detailProduct').val()}/${formEl.find('#detailVariant').val()}/${formEl.find('#detailQty').val()}`);
            editedDetail = response;
            formEl.find("#detailQty").val(editedDetail.quantity);
            formEl.find("#detailPrice").val(editedDetail.price);
            formEl.find("#detailTax").val(editedDetail.tax);
            //formEl.find("#detailDiscount").val(editedDetail.discount);
            formEl.find("#detailShipCost").val(editedDetail.shippingCost);

            //let total = (parseFloat(editedDetail.price) + parseFloat(editedDetail.tax) + parseFloat(editedDetail.shippingCost)) - parseFloat(editedDetail.discount);
            let total = (parseFloat(editedDetail.price) + parseFloat(editedDetail.tax) + parseFloat(editedDetail.shippingCost));

            formEl.find("#detailTotalPrice").val(total);
        } catch (e) {
            Failed(e);
        }
    }

    const Save = async () => {
        try {
            let model = formElToJson(formEl);
            let url = `/api/OrderList/update/${model.OrdersId}/${model.PaymentStatus}/${model.DeliveryStatus}/${model.ShippingType}/${model.CourierName || 'undefined'}/${model.CourierTrackingNo ? encodeURIComponent(model.CourierTrackingNo) : 'undefined'}`;
            let response = await ajaxOperation.SavePostAjax(url);

            ResetForm(formEl);
            Success("Saved Successfully!");
            Back();
            ecomTable.fnFilter();
        } catch (e) {
            Failed(e);
        }
    }

    const SaveOrderDetail = async (detail) => {
        try {
            let response = await ajaxOperation.SavePostAjax(`/api/OrderList/save-detail`, detail);
            Success("Added Successfully!");
        } catch (e) {
            Failed(e);
        }
    }

    const InitialLoader = async () => {
        divDetailsEl.show();
        divPrimaryEl.hide();
        ResetForm(formEl);
    }

    const Back = async () => {
        master = {};
        divDetailsEl.hide();
        ResetForm(formEl);
        formEl.find('#itemTbody').html('');

        if (status.currentStatus == status.all) ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        else if (status.currentStatus == status.pending) ToggleActiveToolbarBtn(divToolbarEl.find("#btnPendingList"), divToolbarEl);
        else if (status.currentStatus == status.confirmed) ToggleActiveToolbarBtn(divToolbarEl.find("#btnConfirmedList"), divToolbarEl);
        else if (status.currentStatus == status.packaging) ToggleActiveToolbarBtn(divToolbarEl.find("#btnPackagingList"), divToolbarEl);
        else if (status.currentStatus == status.outForDelivery) ToggleActiveToolbarBtn(divToolbarEl.find("#btnOutForDeliveryList"), divToolbarEl);
        else if (status.currentStatus == status.delivered) ToggleActiveToolbarBtn(divToolbarEl.find("#btnDeliveredList"), divToolbarEl);
        else if (status.currentStatus == status.returned) ToggleActiveToolbarBtn(divToolbarEl.find("#btnReturnedList"), divToolbarEl);
        else if (status.currentStatus == status.failedToDeliver) ToggleActiveToolbarBtn(divToolbarEl.find("#btnFailedToDeliverList"), divToolbarEl);
        else if (status.currentStatus == status.canceled) ToggleActiveToolbarBtn(divToolbarEl.find("#btnCanceledList"), divToolbarEl);

        divPrimaryEl.show();
        ecomTable.fnFilter();
    }


    CommonInitializer();

    $(document).ready(function () {
        if (paramName == "Pending") {
            status.currentStatus = status.pending;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnOutForDeliveryList,#btnDeliveredList,#btnReturnedList,#btnFailedToDeliverList,#btnCanceledList").hide();
            divToolbarEl.find("#btnPendingList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnPendingList"), divToolbarEl);
        } else if (paramName == "Confirmed") {
            status.currentStatus = status.confirmed;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnPackagingList,#btnOutForDeliveryList,#btnDeliveredList,#btnReturnedList,#btnFailedToDeliverList,#btnCanceledList").hide();
            divToolbarEl.find("#btnConfirmedList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnConfirmedList"), divToolbarEl);
        } else if (paramName == "Packaging") {
            status.currentStatus = status.packaging;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnOutForDeliveryList,#btnDeliveredList,#btnReturnedList,#btnFailedToDeliverList,#btnCanceledList").hide();
            divToolbarEl.find("#btnPackagingList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnPackagingList"), divToolbarEl);
        } else if (paramName == "Out of delivery") {
            status.currentStatus = status.outForDelivery;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnDeliveredList,#btnReturnedList,#btnFailedToDeliverList,#btnCanceledList").hide();
            divToolbarEl.find("#btnOutForDeliveryList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnOutForDeliveryList"), divToolbarEl);
        } else if (paramName == "Delivered") {
            status.currentStatus = status.delivered;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnOutForDeliveryList,#btnReturnedList,#btnFailedToDeliverList,#btnCanceledList").hide();
            divToolbarEl.find("#btnDeliveredList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnDeliveredList"), divToolbarEl);
        } else if (paramName == "Return") {
            status.currentStatus = status.returned;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnOutForDeliveryList,#btnDeliveredList,#btnFailedToDeliverList,#btnCanceledList").hide();
            divToolbarEl.find("#btnReturnedList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnReturnedList"), divToolbarEl);
        } else if (paramName == "Failed to deliver") {
            status.currentStatus = status.failedToDeliver;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnOutForDeliveryList,#btnDeliveredList,#btnReturnedList,#btnCanceledList").hide();
            divToolbarEl.find("#btnFailedToDeliverList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnFailedToDeliverList"), divToolbarEl);
        } else if (paramName == "Cencelled") {
            status.currentStatus = status.canceled;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnOutForDeliveryList,#btnDeliveredList,#btnReturnedList,#btnFailedToDeliverList").hide();
            divToolbarEl.find("#btnCanceledList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnCanceledList"), divToolbarEl);
        }
        else {
            status.currentStatus = status.all;
            divToolbarEl.find("#btnAllList,#btnPendingList,#btnConfirmedList,#btnPackagingList,#btnOutForDeliveryList,#btnDeliveredList,#btnReturnedList,#btnFailedToDeliverList,#btnCanceledList").show();
            ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        }

       
        if (detailsForOrderId > 0) {
            Edit(detailsForOrderId);
            formEl.find('#btnCancel').hide();

        } else {
            GenerateList(status.currentStatus, pickupOrder);
            formEl.find('#btnCancel').show();
        }
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnViewOrder", function () {
        Edit($(this).attr('id'), true);
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    formEl.find("#btnCancel").click(function () {
        Back();
    });

    formEl.find("[data-bootstrap-switch]").bootstrapSwitch();
    formEl.find('#isPaymentStatusPaid').on('switchChange.bootstrapSwitch', function (event, state) {
        if (state) {
            formEl.find('#PaymentStatus').val("Paid");
        } else {
            formEl.find('#PaymentStatus').val("Unpaid");
        }
    });

    divToolbarEl.find("#btnAllList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.all, pickupOrder);
        status.currentStatus = status.all;
    });

    divToolbarEl.find("#btnPendingList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.pending, pickupOrder);
        status.currentStatus = status.pending;
    });

    divToolbarEl.find("#btnConfirmedList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.confirmed, pickupOrder);
        status.currentStatus = status.confirmed;
    });

    divToolbarEl.find("#btnPackagingList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.packaging, pickupOrder);
        status.currentStatus = status.packaging;
    });

    divToolbarEl.find("#btnOutForDeliveryList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.outForDelivery, pickupOrder);
        status.currentStatus = status.outForDelivery;
    });

    divToolbarEl.find("#btnDeliveredList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.delivered, pickupOrder);
        status.currentStatus = status.delivered;
    });

    divToolbarEl.find("#btnReturnedList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.returned, pickupOrder);
        status.currentStatus = status.returned;
    });

    divToolbarEl.find("#btnFailedToDeliverList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.failedToDeliver, pickupOrder);
        status.currentStatus = status.failedToDeliver;
    });

    divToolbarEl.find("#btnCanceledList").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GenerateList(status.canceled, pickupOrder);
        status.currentStatus = status.canceled;
    });

    formEl.find("#itemTbody").on("click", ".btnDetailsDelete", async function () {
        let decisionResult = await Decision();
        try {
            let uniqueIdentity = $(this).attr("uniqueIdentity");
            if (decisionResult) {
                master.orderDetailsList = master.orderDetailsList.filter(item => item.slNo != uniqueIdentity);
                formEl.find('#itemTbody').html('');
                OrderDetails();
                Success("Deleted Successfully!");
            }
        } catch (e) {
            Failed(e);
        }
    });

    formEl.find("#itemTbody").on("click", ".btnDetailsEdit", async function () {
        try {
            let uniqueIdentity = $(this).attr("uniqueIdentity");
            editedId = uniqueIdentity;
            let row = master.orderDetailsList.find(item => item.slNo == uniqueIdentity);
            if (row) {
                formEl.find("#detailProduct").val(row.productId).trigger('change').trigger('select2:select'); 
                vaiant = row.variation;
                //formEl.find("#detailVariant").val(row.variation); 
                formEl.find("#detailQty").val(row.quantity);         
                formEl.find("#detailRebate").val(row.adminDiscount); 

                formEl.find("#detailPrice").val(row.price.toFixed(2));  
                formEl.find("#detailTax").val(row.tax.toFixed(2));  
                //formEl.find("#detailDiscount").val(row.discount.toFixed(2));  
                formEl.find("#detailShipCost").val(row.shippingCost.toFixed(2));  
                //formEl.find("#detailTotalPrice").val(((row.price + row.tax + row.shippingCost) - row.discount).toFixed(2)); 
                formEl.find("#detailTotalPrice").val((row.price + row.tax + row.shippingCost).toFixed(2)); 
            }
        } catch (e) {
            Failed(e);
        }
    });

    formEl.find('#detailProduct').on('select2:select', function (e) {
        GetVariants(formEl.find('#detailProduct').val());
    });

    formEl.find('#detailVariant').on('select2:select', function (e) {
        GetOrderDetail();
    });

    formEl.find('#detailQty').on('select2:select', function (e) {
        GetOrderDetail();
    });
    formEl.find('#detailQty').keyup(function () {
        if ($(this).val()) {
            GetOrderDetail();
        }
    });

    formEl.find("#btnEditOrder").on("click", function () {
        if (!formEl.find('#detailProduct').val()) {
            Failed("Please select product!");
            return;
        }

        if (!formEl.find('#detailVariant').val()) {
            Failed("Please select variant!");
            return;
        }

        if (!editedDetail.productId) {
            Failed("Please select a valid item!");
            return;
        }

        if (editedId) {
            const index = master.orderDetailsList.findIndex(item => item.slNo == editedId);
            if (index !== -1) {
                master.orderDetailsList[index].productId = editedDetail.productId;
                master.orderDetailsList[index].productName = editedDetail.productName;
                master.orderDetailsList[index].thumbnailImage = editedDetail.thumbnailImage;
                master.orderDetailsList[index].productPrice = editedDetail.productPrice;
                master.orderDetailsList[index].variation = editedDetail.variation;
                master.orderDetailsList[index].variantPrice = editedDetail.variantPrice;
                master.orderDetailsList[index].price = editedDetail.price;
                master.orderDetailsList[index].tax = editedDetail.tax;
                master.orderDetailsList[index].quantity = editedDetail.quantity;
                master.orderDetailsList[index].shippingType = editedDetail.shippingType;
                master.orderDetailsList[index].shippingCost = editedDetail.shippingCost;

                editedDetail = master.orderDetailsList[index];
            } else {
                editedDetail.orderId = master.ordersId;
                master.orderDetailsList.push(editedDetail);
            }
        } else {
            editedDetail.orderId = master.ordersId;
            master.orderDetailsList.push(editedDetail);
        }
        editedDetail.adminDiscount = formEl.find('#detailRebate').val();

        SaveOrderDetail(editedDetail);

        formEl.find('#itemTbody').html('');
        BuildOrderDetails();

        editedId = 0;
        formEl.find("#detailProduct").val(null).trigger('change'); 
        formEl.find("#detailVariant").val(null).trigger('change');
        formEl.find("#detailQty").val(1);
        formEl.find("#detailRebate").val(0);

        formEl.find("#detailPrice").val(0);
        formEl.find("#detailTax").val(0);
        formEl.find("#detailShipCost").val(0);
        formEl.find("#detailTotalPrice").val(0);

        formEl.find("#GrandTotal").val(master.grandTotal);
        
    });

    formEl.find("#btnResetOrder").on("click", function () {
        editedId = 0;
        formEl.find("#detailProduct").val(null).trigger('change');
        formEl.find("#detailVariant").val(null).trigger('change');
        formEl.find("#detailQty").val(1);
        formEl.find("#detailRebate").val(0);

        formEl.find("#detailPrice").val(0);
        formEl.find("#detailTax").val(0);
        formEl.find("#detailShipCost").val(0);
        formEl.find("#detailTotalPrice").val(0);
    });


})();