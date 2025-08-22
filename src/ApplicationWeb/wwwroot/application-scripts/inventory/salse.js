(function () {
    ecomTable = null;

    const validators = [
        initializeValidation('#customerId', '#customerError', 'select a customer.'),
        initializeValidation('#SalseDate', '#salseDateError', 'enter salse date.'),
    ];
    const initialUrl = "/api/Salse/GetInitial";
    const saveUrl = "/api/Salse/Save";
    const editUrl = "/api/Salse/EditSalse";
    const productVariantUrl = "/api/Salse/GetProductVariantList";
    const GetStockValueUrl = "/api/Salse/GetStockValue";
    let slNo = 1;

    // Success Message
    const saveMessage = "Salse Saved Successfully";
    let customerList = new Array();

    // image laoder 
    const imageLoadingArray = [{ id: "#bandImage", name: "productImage" }];

    const selector = {
        customerId: $("#customerId"),
        customerAddress: $("#customerAddress"),
        SalseDate: $("#SalseDate"),
        Expanse: $("#Expanse"),
        SalePrice: $("#SalePrice"),
        RegularPrice: $("#RegularPrice"),
        PurchasePrice: $("#PurchasePrice"),
        productId: $("#productId"),
        unitId: $("#unitId"),
        salseId: $("#salseId"),
        colorIds: $("#ColorIds"),
        variationsPhoto: ".variationsPhoto",
        attributesIds: "#AttributeIds",

        btnAdd: $("#btnAdd"),
        btnReturn: "btnReturn",
        btnDelete: "btnDelete",
        salseTbody: $("#salseTbody"),
        saveSalse: $("#saveSalse"),
        saveSalseForm: $("#saveSalseForm"),
        btnAllList: $("#btnAllList"),
        btnNew: $("#btnNew"),
        salseDetailClass: ".salse-details",
        salseExpenseClass: ".salse-expense",
        btnCancel: $("#btnCancel"),
        btnCancelExpense: $("#btnCancelExpense"),
        btnBack: $("#btnBack"),
        salseFormValidation: ".salse-form-validation",
    }

    const DivisionShowHide = (isPrimary, isDetail) => {
        isPrimary ? divPrimaryEl.show() : divPrimaryEl.hide();
        isDetail ? divDetailsEl.show() : divDetailsEl.hide();
    }

    const Save = async () => {
        try {
            if (IsFormsValid(validators)) {
                const tableData = [];
                $('#salseTbody tr').each(function () {
                    const row = $(this);
                    const rowData = {
                        ProductId: parseInt(row.find('td').eq(2).text(), 10),
                        ProductName: row.find('td').eq(1).text(),
                        VariantId: parseInt(row.find('td').eq(4).text(), 10),
                        VariantName: row.find('td').eq(3).text(),
                        UnitId: parseInt(row.find('td').eq(6).text(), 10),
                        UnitName: row.find('td').eq(5).text(),
                        Quantity: parseInt(row.find('td').eq(7).text(), 10),
                        SalePrice: parseFloat(row.find('td').eq(8).text().trim())
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
                    ResetForm(formEl);
                    GetInitial();
                    $("#salseTbody").empty();
                    /*RemoveLoadImages(imageLoadingArray);*/
                } else {
                    Failed(response);
                }
            } else {
                Failed("Please Select Required Fields.");
            }
        } catch (e) {
            console.error("Error during save operation:", e);
            Failed("An error occurred while saving the data.");
        }
    };



    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        divPrimaryEl.show();
        ecomTable.fnFilter();
    }


    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            setFormData(formEl, response);
            customerList = response.customerList.map(item => {
                const { id: customerId, description: address } = item;
                return { customerId, address };
            });

            // Process product list (assuming response contains 'productList')
            productList = response.productList.map(item => {
                const { id: productId, text: productName, description: imageUrl } = item;
                return { productId, productName, imageUrl };
            });

            // Populate the product dropdown
            PopulateProductDropdown(productList);


        }
        catch (e) {
            console.log(e);
            Failed(e);
        }
    }

    const PopulateProductDropdown = (productList) => {
        $("#productId").empty().append(new Option("Select one", "0", true, true)).select2({
            data: productList.map(item => ({
                id: item.productId,
                text: item.productName,
                image: item.imageUrl
            })),
            templateResult: formatProduct,  // Function to format dropdown list items
            templateSelection: formatProductSelection  // Function for selected item display
        });
    };

    // Format dropdown list
    function formatProduct(product) {
        if (!product.id) return product.text;
        return $(`<div style="display: flex; align-items: center;">
                <img src="${product.image}" style="width: 60px; height: 60px; border-radius: 5px; margin-right: 10px;" />
                <span>${product.text}</span>
            </div>`);
    }

    // Format selected item
    function formatProductSelection(product) {
        if (!product.id) return product.text;
        return $(`<div style="display: flex; align-items: center;">
                <img src="${product.image}" style="width: 30px; height: 30px; border-radius: 5px; margin-right: 5px;" />
                <span>${product.text}</span>
            </div>`);
    }

    async function GetStockValue(productId, variantName) {
        try {
            debugger;
            const response = await ajaxOperation.GetAjaxAPI(`${GetStockValueUrl}/${productId}/${variantName}`);

            if (response) {
                const mainStock = parseFloat(response.quantity) || 0;
                $("#stockQty").val(mainStock);
                $("#quantity").prop("disabled", mainStock <= 0);
                $("#quantity").attr("placeholder", mainStock > 0 ? mainStock : "0");
            } else {
                console.warn("No stock data returned.");
            }
        } catch (error) {
            console.error("Error fetching stock value:", error);
        }
    }

    function checkStockOverQty(givenStock) {
        const orgiStock = parseFloat($("#stockQty").val()) || 0;
        givenStock = parseFloat(givenStock) || 0;

        if (givenStock > orgiStock) {
            $("#quantity").val(orgiStock);
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
                    return setDateFormat(full.salseDate);
                }
            },
            { "data": "customerName", "name": "customerName", "autowidth": true, "orderable": true },
            { "data": "totalQty", "name": "totalQty", "autowidth": true, "orderable": true },
            { "data": "salsePrice", "name": "salsePrice", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Salse', full.salseId);
                    return result;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<div class="btn-group" style ="text-align:center;">
                                <button type = "button" class="btn btn-info btn-sm btnReturn" id="${full.salseId}">Salse Return</button>&nbsp
                            </div>`;
                }
            }


        ];
        let dtLoader = DataTableLoader("/api/Salse/list", columns);
        ecomTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }


    const Edit = async id => {

        try {
            const response = await ajaxOperation.GetAjaxAPI(editUrl + `/${id}`);

            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            setFormData(formEl, response);

            customerList = response.customerList.map(item => {
                const { id: customerId, description: address } = item;
                return { customerId, address };
            });

            // Process product list (assuming response contains 'productList')
            productList = response.productList.map(item => {
                const { id: productId, text: productName, description: imageUrl } = item;
                return { productId, productName, imageUrl };
            });

            // Populate the product dropdown
            PopulateProductDropdown(productList);


            $("#salseTbody").empty();
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
            <td>${item.quantity}</td>
            <td>${item.salePrice}</td>
            <td>
                <button class="btn btn-danger btn-sm btnDelete">Delete</button>
            </td>
        </tr>`;

                // Append the new row to the table
                $('#salseTbody').append(newRow);

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




    CommonInitializer();

    $(document).ready(function () {
        DivisionShowHide(true, false);
        GenerateList();
        /*GetInitial();*/
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    function emptyEntryBox() {
        $('#productId').val(0).trigger('change');
        $('#variantStock').val(0).trigger('change');
        $('#unitId').val(0).trigger('change');
        $('#quantity').val("");
        $('#salePrice').val("");
    }



    $('#btnAdd').click(function () {
        const productId = $('#productId').val();
        const productName = $('#productId option:selected').text();
        const variantId = $('#variantStock').val();
        const variantName = $('#variantStock option:selected').text();
        const unit = $('#unitId').val();
        const unitName = $('#unitId option:selected').text();
        const quantity = $('#quantity').val();
        const salePrice = $('#salePrice').val();

        // Check if Product, Variant, and Unit are valid
        if (productId <= 0 || variantId <= 0 || unit <= 0) {
            alert('Please select a valid product, variant, and unit.');
            return;
        }

        // Check if Quantity and Sale Price are valid numbers and greater than zero
        if (!quantity || quantity <= 0) {
            alert('Please enter a valid quantity.');
            return;
        }

        if (!salePrice || salePrice <= 0) {
            alert('Please enter a valid sale price.');
            return;
        }

        // Check if the same productId and variantId exist in the table
        let exists = false;
        $('#salseTbody tr').each(function () {
            const existingProductId = $(this).find('td:eq(2)').text().trim();
            const existingVariantId = $(this).find('td:eq(4)').text().trim();

            if (existingProductId === productId && existingVariantId === variantId) {
                exists = true;
                return false; // Break out of the loop
            }
        });

        if (exists) {
            alert('This product with the selected variant already exists in the table.');
            return;
        }

        // Add new row
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

        $('#salseTbody').append(newRow);

        slNo++;
        emptyEntryBox();
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
        $('#salseTbody tr').each(function (index, row) {
            $(row).find('td:first').text(index + 1);
        });
        slNo = $('#salseTbody tr').length + 1; // Reset serial number counter
    }

    selector.customerId.change(function () {
        const customerId = $(this).val();

        if (customerId !== null) {
            const { address } = customerList.find(item => item.customerId === customerId);
            selector.customerAddress.val(address);
        }
    });

    selector.productId.change(function () {
        $("#stockQty").val("");
        $("#quantity").val("");
        $("#quantity").attr("placeholder", "0");
        const productId = $(this).val();
        if (productId != null || productId > 0) {
            ProductVariantList(productId);
        }


    });

    $("#variantStock").change(function () {
        $("#stockQty").val("");
        $("#quantity").val("");
        const productId = $("#productId").val(); // Assuming there's a hidden input or select for ProductId
        const variantName = $("#variantStock option:selected").text();

        if (productId && variantName) {
            GetStockValue(productId, variantName);
        }
    });

    $("#quantity").on("input", function () {
        checkStockOverQty($(this).val());
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });


    $(tblMasterId).on("click", "." + selector.btnReturn, async function () {
        productReturnStatus = true;
        try {
            const salseId = $(this).attr("id");
            window.location.href = window.location.origin + '/Admin/Inventory/SalseReturnLoad?salseId=' + encodeURIComponent(salseId);
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
        $("#salseTbody").empty();

        /* $("#salseTbody").empty();*/
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
