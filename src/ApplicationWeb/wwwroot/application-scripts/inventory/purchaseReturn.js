(function () {
    ecomTable = null;
    Elements = ["supplierId"]; // form validation id only
    const initialUrl = "/api/PurchaseReturn/GetInitial";
    const saveUrl = "/api/PurchaseReturn/Save";
    const editUrl = "/api/PurchaseReturn/EditPurchase";
    const purchaseReturnLoadUrl = "/api/PurchaseReturn/PurchaseReturnLoad";

    // Success Message
    const saveMessage = "Purchase Return Saved Successfully";
    let supplierList = new Array();

    // image laoder 
    const imageLoadingArray = [{ id: "#bandImage", name: "productImage" }];

    const selector = {
        supplierId: $("#supplierId"),
        supplierAddress: $("#supplierAddress"),
        PurchaseDate: $("#PurchaseDate"),
        Expanse: $("#Expanse"),
        SalePrice: $("#SalePrice"),
        RegularPrice: $("#RegularPrice"),
        PurchasePrice: $("#PurchasePrice"),
        productId: $("#productId"),
        unitId: $("#unitId"),
        purchaseId: $("#purchaseId"),
        variationsPhoto: ".variationsPhoto",
        btnReturn: "btnReturn",
        btnDelete: "btnDelete",
        purchaseTbody: $("#purchaseTbody"),
        savePurchase: $("#savePurchase"),
        savePurchaseForm: $("#savePurchaseForm"),
        purchaseDetailClass: ".purchase-details",
        purchaseExpenseClass: ".purchase-expense",
        btnCancel: $("#btnCancel"),
        btnCancelExpense: $("#btnCancelExpense"),
        btnBack: $("#btnBack"),
        purchaseFormValidation: ".purchase-form-validation",
    }

    const DivisionShowHide = (isPrimary, isDetail) => {
        isPrimary ? divPrimaryEl.show() : divPrimaryEl.hide();
        isDetail ? divDetailsEl.show() : divDetailsEl.hide();
    }

    

    const Save = async () => {

        try {
            let formData = new FormData(formEl[0]);
            formData.append("Photo", $("#Photo").get(0));
            const tableData = [];

            // Loop through each row in the table
            $('#productVariationTBody tr').each(function () {
                const row = $(this);
                const rowData = {
                    Id: parseInt(row.find('.return-quantity').data('id'), 10), // Item ID
                    Variant: row.find('td').eq(1).text().trim(), // Variant Name
                    VariantPrice: parseFloat(row.find('td').eq(2).text().trim()), // Variant Price
                    PurchaseQuantity: parseInt(row.find('td').eq(3).text().trim(), 10), // Purchase Quantity
                    AlreadyReturnedQuantity: parseInt(row.find('td').eq(4).text().trim(), 10), // Already Returned Quantity
                    ReturnQuantity: parseInt(row.find('.return-quantity').val().trim(), 10) || 0 // Return Quantity
                };

                tableData.push(rowData);
            });

            // Validate table data
            if (tableData.length === 0) {
                alert('No data to save. Please add items to the table.');
                return;
            }

            formData.append("Variation", JSON.stringify(tableData));
            var imageSrc = $("#bandImage").attr("src");
            formData.append("ProductImage", imageSrc);

            let response = await ajaxOperation.SaveAjax(saveUrl, formData);

            if (typeof (response) === "object") {
                Success(saveMessage);
            }
            else {
                 Failed(response);
            }

        } catch (e) {
        }
    }

    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        divPrimaryEl.show();
        GenerateList();
      
    }

    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            setFormData(formEl, response);
            initialResponse = response;
            supplierList = response.supplierList.map(item => {
                const { id: supplierId, description: address } = item;
                return { supplierId, address };
            });
        }
        catch (e) {
            console.log(e);
            Failed(e);
        }
    }

    const GenerateList = () => {

        if ($.fn.DataTable.isDataTable(tblMasterId)) {
            $(tblMasterId).DataTable().clear().destroy(); // Clear data and destroy
        }

        // Ensure the table has the correct structure before reinitialization
        if ($(tblMasterId).find("thead").length === 0) {
            $(tblMasterId).append(`
            <thead>
                <tr>
                    <th>#</th>
                    <th>Image</th>
                    <th>Purchase Date</th>
                    <th>Item Name</th>
                    <th>Total Qty</th>
                    <th>Purchase Price</th>
                    <th>Action</th>
                </tr>
            </thead>
            <tbody></tbody>
        `);
        }

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
                    return `<img src = "${full.productImage}" height = "40" width = "40" alt = "No Image" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.purchaseDate);
                }
            },
            { "data": "itemName", "name": "itemName", "autowidth": true, "orderable": true },
            { "data": "totalQty", "name": "totalQty", "autowidth": true, "orderable": true },
            { "data": "purchasePrice", "name": "purchasePrice", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('PurchaseReturn', full.purchaseRetuenId);
                    return result;
                }
            }
            
           
        ];

        let dtLoader = DataTableLoader("/api/PurchaseReturn/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }
   
  
    const Edit = async id => {

        try {
            needAttribute = false;
            const response = await ajaxOperation.GetAjaxAPI(editUrl + `/${id}`);

            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            setFormData(formEl, response);
            LoadImages(imageLoadingArray, response);
            let tbody = $("#productVariationTBody");
            tbody.empty(); // Clear existing rows
            $.each(response.purchaseItemDTO, function (index, item) {
                let row = `<tr>
                    <td>${index + 1}</td>
                    <td>${item.variant}</td>
                    <td>${item.variantPrice}</td>
                    <td>${item.purchaseQuantity}</td>
                    <td>${item.alreadyReturnedQuantity}</td>
                    <td><input type="number" style="width: 200px;" value="${item.quantity}"  class="return-quantity" data-id="${item.id}" min="0" max="${item.purchaseQuantity - item.alreadyReturnedQuantity}" /></td>
                </tr>`;
                tbody.append(row);
            });
          
        } catch (e) {
            Failed(e);
        }
    }

    const PurchaseReturnDataLoad = async id => {

        try {
            needAttribute = false;
            const response = await ajaxOperation.GetAjaxAPI(purchaseReturnLoadUrl + `/${id}`);

            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            setFormData(formEl, response);
            LoadImages(imageLoadingArray, response);

            let tbody = $("#productVariationTBody");
            tbody.empty(); // Clear existing rows
            $.each(response.purchaseItemDTO, function (index, item) {
                let row = `<tr>
                    <td>${index + 1}</td>
                    <td>${item.variant}</td>
                    <td>${item.variantPrice}</td>
                    <td>${item.purchaseQuantity}</td>
                    <td>${item.alreadyReturnedQuantity}</td>
                    <td><input type="number" style="width: 200px;"  class="return-quantity" data-id="${item.id}" min="0" max="${item.purchaseQuantity - item.alreadyReturnedQuantity}" /></td>
                </tr>`;
                tbody.append(row);
            });

        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        $("#productId, #unitId, #supplierId").on("mousedown keydown", function (e) {
            e.preventDefault(); // Prevents opening dropdown (Mouse & Keyboard)
        });

        var purchaseId = $("#purchaseId").val();
        if (purchaseId === null || purchaseId <= 0) {
            divPrimaryEl.show();
            divDetailsEl.hide();
            GenerateList();
        } else {
            PurchaseReturnDataLoad(purchaseId);
        }

       
       
        //GetInitial();
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });
   

    selector.supplierId.change(function () {
        const supplierId = $(this).val();
       
        if (supplierId !== null) {
            const { address } = supplierList.find(item => item.supplierId === supplierId);
            selector.supplierAddress.val(address);
        }
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
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
