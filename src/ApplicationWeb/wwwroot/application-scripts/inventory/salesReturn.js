(function () {
    ecomTable = null;
    Elements = ["customerId"]; // form validation id only
    const initialUrl = "/api/SalesReturn/GetInitial";
    const saveUrl = "/api/SalesReturn/Save";
    const editUrl = "/api/SalesReturn/EditSalesReturn";
    const productVariantUrl = "/api/SalesReturn/GetProductVariantList";
    const salesReturnLoadUrl = "/api/SalesReturn/SalesReturnLoad";
    let slNo = 1;

    // Success Message
    const saveMessage = "SalesReturn Saved Successfully";
    let customerList = new Array();

    // image laoder 
    const imageLoadingArray = [{ id: "#bandImage", name: "productImage" }];

    const selector = {
        customerId: $("#customerId"),
        customerAddress: $("#customerAddress"),
        SalesReturnDate: $("#SalesReturnDate"),
        Expanse: $("#Expanse"),
        SalePrice: $("#SalePrice"),
        RegularPrice: $("#RegularPrice"),
        PurchasePrice: $("#PurchasePrice"),
        productId: $("#productId"),
        unitId: $("#unitId"),
        salesReturnId: $("#salesReturnId"),
        colorIds: $("#ColorIds"),
        variationsPhoto: ".variationsPhoto",
        attributesIds: "#AttributeIds",

        btnAdd: $("#btnAdd"),
        btnReturn: "btnReturn",
        btnDelete: "btnDelete",
        salesReturnTbody: $("#salesReturnTbody"),
        saveSalesReturn: $("#saveSalesReturn"),
        saveSalesReturnForm: $("#saveSalesReturnForm"),
        btnAllList: $("#btnAllList"),
        btnNew: $("#btnNew"),
        salesReturnDetailClass: ".salesReturn-details",
        salesReturnExpenseClass: ".salesReturn-expense",
        btnCancel: $("#btnCancel"),
        btnCancelExpense: $("#btnCancelExpense"),
        btnBack: $("#btnBack"),
        salesReturnFormValidation: ".salesReturn-form-validation",
    }

    const DivisionShowHide = (isPrimary, isDetail) => {
        isPrimary ? divPrimaryEl.show() : divPrimaryEl.hide();
        isDetail ? divDetailsEl.show() : divDetailsEl.hide();
    }

    const Save = async () => {
        try {
            debugger
            const tableData = [];
            $('#salesReturnTbody tr').each(function () {
                const row = $(this);
                const rowData = {
                    ProductId: parseInt(row.find('td').eq(2).text(), 10),
                    ProductName: row.find('td').eq(1).text(),
                    VariantId: parseInt(row.find('td').eq(4).text(), 10),
                    VariantName: row.find('td').eq(3).text(),
                    UnitId: parseInt(row.find('td').eq(6).text(), 10),
                    UnitName: row.find('td').eq(5).text(),
                    Quantity: parseInt(row.find('td').eq(7).text(), 10),
                    ReturnQuantity: parseInt(row.find('.return-quantity').val().trim(), 10) || 0, // Return Quantity
                    SalePrice: parseFloat(row.find('td').eq(10).text().trim())
                };
                tableData.push(rowData);
            });

            if (tableData.length === 0) {
                alert('No data to save. Please add items to the table.');
                return;
            }
            let formData = new FormData(formEl[0]);
            formData.append("SalseItem", JSON.stringify(tableData));
            const response = await ajaxOperation.SaveAjax(saveUrl, formData);

            if (typeof response === "object") {
                Success(saveMessage);
                divDetailsEl.hide();
                divPrimaryEl.show();
                if ($("#salesReturnId").val() <= 0 || $("#salesReturnId").val() == null || $("#salesReturnId").val() == "undefined") {
                    window.location.href = "/Admin/Inventory/Salse?id=114";
                  
                } else {
                    ResetForm(formEl);
                    ecomTable.fnFilter();
                }

             
            } else {
                Failed(response);
            }
        } catch (e) {
            Failed("An error occurred while saving the data.");
        }
    };



    const Back = async () => {
        divDetailsEl.hide();
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        divPrimaryEl.show();
        if ($("#salesReturnId").val() <= 0 || $("#salesReturnId").val() == null || $("#salesReturnId").val() == "undefined") {
            window.location.href = "/Admin/Inventory/Salse?id=114";

        } else {
            ResetForm(formEl);
            ecomTable.fnFilter();
        }
    }


    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            setFormData(formEl, response);
            initialResponse = response;
            customerList = response.customerList.map(item => {
                const { id: customerId, description: address } = item;
                return { customerId, address };
            });
        }
        catch (e) {
            console.log(e);
            Failed(e);
        }
    }

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                "render": function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.saleReturnDate);
                }
            },
            { "data": "customerName", "name": "customerName", "autowidth": true, "orderable": true },
            { "data": "totalQty", "name": "totalQty", "autowidth": true, "orderable": true },
            { "data": "salseReturnPrice", "name": "salseReturnPrice", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('SalesReturn', full.saleReturnId);
                    return result;
                }
            }
           

        ];
        let dtLoader = DataTableLoader("/api/SalesReturn/list", columns);
        ecomTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }


    const Edit = async id => {

        try {
            const response = await ajaxOperation.GetAjaxAPI(editUrl + `/${id}`);

            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            setFormData(formEl, response);
            LoadImages(imageLoadingArray, response);

            $("#salesReturnTbody").empty();
            let slNo = 1;
            response.items.forEach(item => {
                const newRow = `
        <tr>
            <td>${slNo}</td>
            <td>${item.productName}</td>
            <td style="display:none;">${item.productId}</td>
            <td>${item.variantName}</td>
            <td style="display:none;">${item.variantId}</td>
            <td>${item.unitName}</td>
            <td style="display:none;">${item.unitId}</td>
            <td class="quantity">${item.saleQuantity}</td>
            <td class="already-returned">${item.alreadyReturnedQuantity}</td>
            <td><input type="number" style="width: 200px;" value="${item.quantity}"  class="return-quantity" data-id="${item.id}" min="0" max="${item.quantity - item.alreadyReturnedQuantity}" /></td>
            <td>${item.salePrice}</td>
            <td>
                <button class="btn btn-danger btn-sm btnDelete">Delete</button>
            </td>
        </tr>`;

                // Append the new row to the table
                $('#salesReturnTbody').append(newRow);

                // Increment the serial number
                slNo++;

            });

        } catch (e) {
            Failed(e);
        }
    }
    const ProductVariantList = async id => {

        try {
            const response = await ajaxOperation.GetAjaxAPI(productVariantUrl + `/${id}`);
            $("#variantStock").empty();
            $("#variantStock").append('<option value="' + 0 + '">--Select One--</option>');
            $.each(response.variantList, function (i, value) {
                $("#variantStock").append('<option value="' + value.id + '">' + value.text + '</option>');
            });

        } catch (e) {
            Failed(e);
        }
    }



    const SalesReturnDataLoad = async id => {

        try {
            const response = await ajaxOperation.GetAjaxAPI(salesReturnLoadUrl + `/${id}`);

            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            setFormData(formEl, response);
            LoadImages(imageLoadingArray, response);


            $("#salesReturnTbody").empty();
            let slNo = 1;
            response.items.forEach(item => {
                const newRow = `
        <tr>
            <td>${slNo}</td>
            <td>${item.productName}</td>
            <td style="display:none;">${item.productId}</td>
            <td>${item.variantName}</td>
            <td style="display:none;">${item.variantId}</td>
            <td>${item.unitName}</td>
            <td style="display:none;">${item.unitId}</td>
            <td class="quantity">${item.quantity}</td>
            <td class="already-returned">${item.alreadyReturnedQuantity}</td>
            <td><input type="number" style="width: 200px;"  class="return-quantity" data-id="${item.id}" min="0" max="${item.quantity - item.alreadyReturnedQuantity}" /></td>
            <td>${item.salePrice}</td>
            <td>
                <button class="btn btn-danger btn-sm btnDelete">Delete</button>
            </td>
        </tr>`;

                // Append the new row to the table
                $('#salesReturnTbody').append(newRow);

                // Increment the serial number
                slNo++;

            });

        } catch (e) {
            Failed(e);
        }
    }



    CommonInitializer();

    $(document).ready(function () {

        var saleId = $("#saleId").val();
        if (saleId === null || saleId <= 0) {
            divPrimaryEl.show();
            divDetailsEl.hide();
            GenerateList();
        } else {
            SalesReturnDataLoad(saleId);
        }


        //GetInitial();
    });


    formEl.find("#btnSave").click(function () {
        Save();
    });

    $(document).on("input", ".return-quantity", function () {
        let row = $(this).closest("tr");
        let quantity = parseInt(row.find(".quantity").text(), 10);
        let alreadyReturned = parseInt(row.find(".already-returned").text(), 10);
        let maxReturnable = quantity - alreadyReturned;

        if (parseInt($(this).val(), 10) > maxReturnable) {
            $(this).val(maxReturnable);
            alert(`Returned quantity cannot exceed ${maxReturnable}`);
        }
    });



    $('#btnAdd').click(function () {
        const productId = $('#productId').val();
        const productName = $('#productId option:selected').text(); // Corrected selector
        const variantId = $('#variantStock').val();
        const variantName = $('#variantStock option:selected').text(); // Corrected selector
        const unit = $('#unitId').val();
        const unitName = $('#unitId option:selected').text(); // Corrected selector
        const quantity = $('#quantity').val();
        const salePrice = $('#salePrice').val();

        if (!productId || !variantId || !unit || !quantity || !salePrice) {
            alert('Please fill all fields.');
            return;
        }
        $("#salesReturnTbody").empty();
        const newRow = `
        <tr>
            <td>${slNo}</td>
            <td>${productName}</td>
            <td style="display:none;">${productId}</td>
            <td>${variantName}</td>
            <td style="display:none;">${variantId}</td>
            <td>${unitName}</td>
            <td style="display:none;">${unit}</td>
            <td>${quantity}</td>
            <td>${salePrice}</td>
            <td>
                <button class="btn btn-danger btn-sm btnDelete">Delete</button>
            </td>
        </tr>
    `;

        // Append the new row to the table
        $('#salesReturnTbody').append(newRow);

        // Increment the serial number
        slNo++;

        // Clear the form fields
        $('#productId').val('');
        $('#variantStock').val('');
        $('#unitId').val('');
        $('#quantity').val('');
        $('#salePrice').val('');
    });

    // Delete button click event (using event delegation)
    $(document).on('click', '.btnDelete', function () {
        $(this).closest('tr').remove(); // Remove the row
        updateSerialNumbers(); // Update serial numbers after deletion
    });

    // Cancel button click event
    $('#btnCancel').click(function () {
        $('#productId').val('');
        $('#variantStock').val('');
        $('#unitId').val('');
        $('quantity').val('');
        $('salePrice').val('');
    });

    // Function to update serial numbers
    function updateSerialNumbers() {
        $('#salesReturnTbody tr').each(function (index, row) {
            $(row).find('td:first').text(index + 1);
        });
        slNo = $('#salesReturnTbody tr').length + 1; // Reset serial number counter
    }

    selector.customerId.change(function () {
        const customerId = $(this).val();

        if (customerId !== null) {
            const { address } = customerList.find(item => item.customerId === customerId);
            selector.customerAddress.val(address);
        }
    });

    selector.productId.change(function () {
        const productId = $(this).val();
        ProductVariantList(productId);

    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });


    $(tblMasterId).on("click", "." + selector.btnReturn, async function () {
        productReturnStatus = true;
        try {
            const salesReturnId = $(this).attr("id");
            window.location.href = window.location.origin + '/Admin/Inventory/SalesReturnReturnLoad?salesReturnId=' + encodeURIComponent(salesReturnId);
        } catch (e) {
            Failed(e);
        }
    });



    selector.btnAllList.click(function () {
        DivisionShowHide(true, false);
    });

    selector.btnNew.click(function () {
        DivisionShowHide(false, true);
        GetInitial();
        $("#salesReturnTbody").empty();
    });

    selector.btnBack.click(function () {
        Back();
    })


    $("#Photo").change(function () {
        const [file] = $("#Photo")[0].files
        if (file) {
            $("#bandImage").attr("src", URL.createObjectURL(file))
        }
    });
})();
