(function () {
    ecomTable = null;

    const validators = [
        initializeValidation('#purchaseDate', '#purchaseDateError', 'Please select purchase date.'),
        initializeValidation('#supplierId', '#supplierError', 'Please select a supplier.'),
    ];

    const initialUrl = "/api/PurchaseNew/GetInitial";
    const saveUrl = "/api/PurchaseNew/Save";
    const editUrl = "/api/PurchaseNew/Edit";
    const deleteUrl = "/api/PurchaseNew/Delete";
    const variantsUrl = "/api/PurchaseNew/variants";

    // Success Message
    const saveMessage = "Purchase Saved Successfully";
    const deleteMessage = "Purchase Deleted Successfully";
    
    let supplierList = new Array();
    let productList = new Array();
    let purchaseItems = new Array();
    let expenses = new Array();
    let editingProductIndex = -1;
    let editingExpenseIndex = -1;

    const selector = {
        // Form elements
        purchaseId: $("#purchaseId"),
        purchaseDate: $("#purchaseDate"),
        purchaseNo: $("#purchaseNo"),
        supplierId: $("#supplierId"),
        supplierAddress: $("#supplierAddress"),
        
        // Product form elements
        productId: $("#productId"),
        variantId: $("#variantId"),
        quantity: $("#quantity"),
        unitId: $("#unitId"),
        purchasePrice: $("#purchasePrice"),
        branchId: $("#branchId"),
        
        // Expense form elements
        costId: $("#costId"),
        firstNumber: $("#firstNumber"),
        secondNumber: $("#secondNumber"),
        expenseTotal: $("#expenseTotal"),
        
        // Summary elements
        discount: $("#discount"),
        discountType: $("#discountType"),
        subTotalAmount: $("#subTotalAmount"),
        otherAmount: $("#otherAmount"),
        totalWithExpenses: $("#totalWithExpenses"),
        netPrice: $("#netPrice"),
        remarks: $("#remarks"),
        
        // Buttons
        btnAddProduct: $("#btnAddProduct"),
        btnCancelProduct: $("#btnCancelProduct"),
        btnAddExpense: $("#btnAddExpense"),
        btnCancelExpense: $("#btnCancelExpense"),
        btnSave: $("#btnSave"),
        btnBack: $("#btnBack"),
        btnNew: $("#btnNew"),
        btnAllList: $("#btnAllList"),
        
        // Tables
        productTableBody: $("#productTableBody"),
        expenseTableBody: $("#expenseTableBody"),
        
        // Form validation
        purchaseFormValidation: ".purchase-form-validation",
    }

    const DivisionShowHide = (isPrimary, isDetail) => {
        isPrimary ? divPrimaryEl.show() : divPrimaryEl.hide();
        isDetail ? divDetailsEl.show() : divDetailsEl.hide();
    }

    const ResetForm = () => {
        formEl[0].reset();
        selector.purchaseId.val(0);
        selector.purchaseNo.val("");
        selector.supplierAddress.val("");
        purchaseItems = [];
        expenses = [];
        editingProductIndex = -1;
        editingExpenseIndex = -1;
        updateProductTable();
        updateExpenseTable();
        
        // Reset summary display
        selector.subTotalAmount.text("0.00");
        selector.totalWithExpenses.text("0.00");
        selector.netPrice.text("0.00");
        selector.otherAmount.text("0.00");
        
        calculateTotals();
    }

    const Save = async () => {
        try {
            if (IsFormsValid(validators)) {
                if (purchaseItems.length === 0) {
                    Failed("Please add at least one product to the purchase.");
                    return;
                }

                const subTotalAmount = parseFloat(selector.subTotalAmount.text() || 0);
                const model = {
                    PurchaseId: parseInt(selector.purchaseId.val()) || 0,
                    PurchaseDate: selector.purchaseDate.val(),
                    //PurchaseDate: selector.purchaseDate.val() ? new Date(selector.purchaseDate.val()).toISOString() : null,
                    PurchaseNo: selector.purchaseNo.val(),
                    SupplierId: parseInt(selector.supplierId.val()),
                    SubTotalAmount: subTotalAmount,
                    Discount: parseFloat(selector.discount.val()) || 0,
                    DiscountType: selector.discountType.val(),
                    OtherAmount: parseFloat(selector.otherAmount.text()) || 0,
                    GrandTotalAmount: parseFloat(selector.netPrice.text() || 0),
                    Remarks: selector.remarks.val(),
                    PurchaseItems: purchaseItems,
                    Expenses: expenses
                };

                let response = await ajaxOperation.SaveModel(saveUrl, model);

                if (typeof (response) == "object") {
                    Success(saveMessage);
                    Back();
                } else {
                    Failed(response);
                }
            } else {
                Failed("Please fill in all required fields.");
            }
        } catch (e) {
            Failed(e.message || "An error occurred while saving.");
        }
    }

    const Back = () => {
        DivisionShowHide(true, false);
        ResetForm();
        ecomTable.fnFilter();
    }

    const AddProduct = () => {
        const productId = parseInt(selector.productId.val());
        const variantId = selector.variantId.val() ? parseInt(selector.variantId.val()) : null;
        const variantName = selector.variantId.find('option:selected').text();
        const quantity = parseInt(selector.quantity.val());
        const unitId = parseInt(selector.unitId.val());
        const unitText = selector.unitId.find('option:selected').text();
        const purchasePrice = parseFloat(selector.purchasePrice.val());
        const branchId = parseInt(selector.branchId.val());

        if (!productId) {
            Failed("Please select a product.");
            return;
        }
        if (!quantity || quantity <= 0) {
            Failed("Please enter a valid quantity.");
            return;
        }
        if (!purchasePrice || purchasePrice < 0) {
            Failed("Please enter a valid purchase price.");
            return;
        }

        const selectedProduct = productList.find(p => p.id == productId);
        const productName = selectedProduct ? selectedProduct.text : "";

        const productItem = {
            purchaseItemId: editingProductIndex >= 0 ? (purchaseItems[editingProductIndex].purchaseItemId || 0) : 0,
            productId: productId,
            productName: productName,
            variantId: variantId,
            variantName: variantName || "N/A",
            quantity: quantity,
            unitId: unitId,
            unit: unitText,
            price: purchasePrice,
            branchId: branchId,
            branchName: selector.branchId.find('option:selected').text(),
            totalPrice: quantity * purchasePrice
        };

        if (editingProductIndex >= 0) {
            purchaseItems[editingProductIndex] = productItem;
            editingProductIndex = -1;
        } else {
            purchaseItems.push(productItem);
        }

        updateProductTable();
        calculateTotals();
        clearProductForm();
    }

    const EditProduct = (index) => {
        const item = purchaseItems[index];
        selector.productId.val(item.productId);
        selector.variantId.val(item.variantId);
        selector.quantity.val(item.quantity);
        selector.unitId.val(item.unitId);
        selector.purchasePrice.val(item.price);
        selector.branchId.val(item.branchId);
        editingProductIndex = index;
        selector.btnAddProduct.text("Update");
    }

    const DeleteProduct = (index) => {
        purchaseItems.splice(index, 1);
        updateProductTable();
        calculateTotals();
    }

    const updateProductTable = () => {
        let html = "";
        purchaseItems.forEach((item, index) => {
            html += `
                <tr>
                    <td>${index + 1}</td>
                    <td>${item.productName}</td>
                    <td>${ item.variantName ?? "N/A" }</td>
                    <td>${item.quantity}</td>
                    <td>${item.unit}</td>
                    <td>${item.price.toFixed(2)}</td>
                    <td>${item.branchName}</td>
                    <td>${item.totalPrice.toFixed(2)}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn.warning btnEditProduct" data-index="${index}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-danger btnDeleteProduct" data-index="${index}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
        selector.productTableBody.html(html);
    }

    const clearProductForm = () => {
        selector.productId.val("");
        selector.variantId.val("");
        selector.quantity.val("");
        selector.unitId.val("");
        selector.purchasePrice.val("");
        selector.branchId.val(1);
        editingProductIndex = -1;
        selector.btnAddProduct.text("Add");
    }

    const AddExpense = () => {
        const costId = parseInt(selector.costId.val());
        const costName = selector.costId.find('option:selected').text();
        const firstNumber = parseFloat(selector.firstNumber.val()) || 0;
        const secondNumber = parseFloat(selector.secondNumber.val()) || 0;
        const total = firstNumber * secondNumber;

        if (!costId) {
            Failed("Please select a cost item.");
            return;
        }

        const expenseItem = {
            purchaseExpenseId: editingExpenseIndex >= 0 ? (expenses[editingExpenseIndex].purchaseExpenseId || 0) : 0,
            costId: costId,
            description: costName,
            firstAmount: firstNumber,
            secondAmount: secondNumber,
            totalAmount: total
        };

        if (editingExpenseIndex >= 0) {
            expenses[editingExpenseIndex] = expenseItem;
            editingExpenseIndex = -1;
        } else {
            expenses.push(expenseItem);
        }

        updateExpenseTable();
        calculateTotals();
        clearExpenseForm();
    }

    const EditExpense = (index) => {
        const item = expenses[index];
        selector.costId.val(item.costId);
        selector.firstNumber.val(item.firstAmount);
        selector.secondNumber.val(item.secondAmount);
        selector.expenseTotal.val(item.totalAmount);
        editingExpenseIndex = index;
        selector.btnAddExpense.text("Update");
    }

    const DeleteExpense = (index) => {
        expenses.splice(index, 1);
        updateExpenseTable();
        calculateTotals();
    }

    const updateExpenseTable = () => {
        let html = "";
        expenses.forEach((item, index) => {
            html += `
                <tr>
                    <td>${index + 1}</td>
                    <td>${item.description}</td>
                    <td>${Number(item.firstAmount).toFixed(2)}</td>
                    <td>${Number(item.secondAmount).toFixed(2)}</td>
                    <td>${Number(item.totalAmount).toFixed(2)}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-warning btnEditExpense" data-index="${index}">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button type="button" class="btn btn-sm btn-danger btnDeleteExpense" data-index="${index}">
                            <i class="fas fa-trash"></i>
                        </button>
                    </td>
                </tr>
            `;
        });
        selector.expenseTableBody.html(html);
    }

    const clearExpenseForm = () => {
        selector.costId.val("");
        selector.firstNumber.val(0);
        selector.secondNumber.val(0);
        selector.expenseTotal.val(0);
        editingExpenseIndex = -1;
        selector.btnAddExpense.text("Add");
    }

    const calculateTotals = () => {
        // Calculate subtotal from products
        const productSubtotal = purchaseItems.reduce((sum, item) => sum + item.totalPrice, 0);
        
        // Calculate expense total
        const expenseTotal = expenses.reduce((sum, item) => sum + item.totalAmount, 0);
        
        // Apply discount
        const discount = parseFloat(selector.discount.val()) || 0;
        const discountType = selector.discountType.val();
        let discountedTotal = productSubtotal;
        
        if (discountType === 'amount') {
            discountedTotal = productSubtotal - discount;
        } else if (discountType === 'percentage') {
            discountedTotal = productSubtotal - (productSubtotal * discount / 100);
        }
        
        // Auto-populate OtherExpense from cost total
        selector.otherAmount.text(expenseTotal.toFixed(2));
        
        // Calculate net price
        const netPrice = discountedTotal + expenseTotal;
        
        // Update display
        selector.subTotalAmount.text(discountedTotal.toFixed(2));
        selector.totalWithExpenses.text((discountedTotal + expenseTotal).toFixed(2));
        selector.netPrice.text(netPrice.toFixed(2));
    }

    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            //setFormData(formEl, response);
            // Populate supplier dropdown
            let supplierHtml = '<option value="">Select Supplier</option>';
            response.supplierList.forEach(item => {
                supplierHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.supplierId.html(supplierHtml);
            
            // Populate product dropdown
            let productHtml = '<option value="">Select Product</option>';
            response.productList.forEach(item => {
                productHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.productId.html(productHtml);

            let unitHtml = '<option value="">Select unit</option>';
            response.unitList.forEach(item => {
                unitHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.unitId.html(unitHtml);

            let branchHtml = '<option value="">Select branch</option>';
            response.branchList.forEach(item => {
                branchHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.branchId.html(branchHtml);
            
            let costHtml = '<option value="">Select cost</option>';
            response.costList.forEach(item => {
                costHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.costId.html(costHtml);
            
            // Store lists for reference
            supplierList = response.supplierList;
            productList = response.productList;
            UnitList = response.unitList;
            branchList = response.branchList;
            costList = response.costList;
            
            // Set current date
            const today = new Date().toISOString().split('T')[0];
            selector.purchaseDate.val(today);

            selector.purchaseNo.val(response.purchaseNo)
            
        } catch (e) {
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
                    return setDateFormat(full.purchaseDate);
                }
            },
            { "data": "purchaseNo", "name": "purchaseNo", "autowidth": true, "orderable": true },
            { "data": "supplierName", "name": "supplierName", "autowidth": true, "orderable": true },
            { "data": "totalItems", "name": "totalItems", "autowidth": true, "orderable": true },
            { "data": "grandTotal", "name": "grandTotal", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<div class="btn-group">
                                <button type="button" class="btn btn-sm btn-warning btnEdit" id="${full.purchaseId}">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-danger btnDelete" id="${full.purchaseId}">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </div>`;
                }
            }
        ];
        
        let dtLoader = DataTableLoader("/api/PurchaseNew/list", columns);
        ecomTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const Edit = async (id) => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(editUrl + `/${id}`);
            
            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm();

            let supplierHtml = '<option value="">Select Supplier</option>';
            response.supplierList.forEach(item => {
                supplierHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.supplierId.html(supplierHtml);

            // Populate product dropdown
            let productHtml = '<option value="">Select Product</option>';
            response.productList.forEach(item => {
                productHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.productId.html(productHtml);

            let unitHtml = '<option value="">Select unit</option>';
            response.unitList.forEach(item => {
                unitHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.unitId.html(unitHtml);

            let branchHtml = '<option value="">Select branch</option>';
            response.branchList.forEach(item => {
                branchHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.branchId.html(branchHtml);

            let costHtml = '<option value="">Select cost</option>';
            response.costList.forEach(item => {
                costHtml += `<option value="${item.id}">${item.text}</option>`;
            });
            selector.costId.html(costHtml);

            // Store lists for reference
            supplierList = response.supplierList;
            productList = response.productList;
            UnitList = response.unitList;
            branchList = response.branchList;
            costList = response.costList;
            
            // Populate form with response data
            selector.purchaseId.val(response.purchaseId);
            selector.purchaseDate.val(response.purchaseDate ? response.purchaseDate.split('T')[0] : '');
            selector.purchaseNo.val(response.purchaseNo);
            selector.supplierId.val(response.supplierId);
            selector.supplierAddress.val(response.supplierAddress);
            selector.discount.val(response.discount);
            selector.discountType.val(response.discountType);
            selector.otherAmount.text(response.otherAmount);
            selector.netPrice.text(response.grandTotalAmount);
            selector.remarks.val(response.remarks);
            
            // Set purchase items
            purchaseItems = response.purchaseItems || [];
            updateProductTable();
            
            // Set expenses
            expenses = response.expenses || [];
            updateExpenseTable();
            
            calculateTotals();
            
        } catch (e) {
            Failed(e);
        }
    }

    const Delete = async (id) => {
        try {
            if (confirm("Are you sure you want to delete this purchase?")) {
                const response = await ajaxOperation.DeleteAjax(deleteUrl + `/${id}`);
                if (response) {
                    Success(deleteMessage);
                    GenerateList();
                } else {
                    Failed("Failed to delete purchase.");
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        DivisionShowHide(true, false);
        GenerateList();
    });

    // Event handlers
    selector.btnSave.click(function () {
        Save();
    });

    selector.btnBack.click(function () {
        Back();
    });

    selector.btnNew.click(function () {
        DivisionShowHide(false, true);
        GetInitial();
    });

    selector.btnAllList.click(function () {
        DivisionShowHide(true, false);
    });

    selector.btnAddProduct.click(function () {
        AddProduct();
    });

    selector.btnCancelProduct.click(function () {
        clearProductForm();
    });

    selector.btnAddExpense.click(function () {
        AddExpense();
    });

    selector.btnCancelExpense.click(function () {
        clearExpenseForm();
    });

    // Product table events
    $(document).on("click", ".btnEditProduct", function () {
        const index = parseInt($(this).data("index"));
        EditProduct(index);
    });

    $(document).on("click", ".btnDeleteProduct", function () {
        const index = parseInt($(this).data("index"));
        if (confirm("Are you sure you want to remove this product?")) {
            DeleteProduct(index);
        }
    });

    // Expense table events
    $(document).on("click", ".btnEditExpense", function () {
        const index = parseInt($(this).data("index"));
        EditExpense(index);
    });

    $(document).on("click", ".btnDeleteExpense", function () {
        const index = parseInt($(this).data("index"));
        if (confirm("Are you sure you want to remove this expense?")) {
            DeleteExpense(index);
        }
    });

    // DataTable events
    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    // Supplier change event
    selector.supplierId.change(function () {
        const supplierId = $(this).val();
        if (supplierId) {
            const supplier = supplierList.find(s => s.id == supplierId);
            if (supplier) {
                selector.supplierAddress.val(supplier.description || "");
            }
        } else {
            selector.supplierAddress.val("");
        }
    });

    // Load variants when product changes
    selector.productId.change(async function () {
        debugger;
        const productId = $(this).val();
        selector.variantId.empty().append('<option value="">Select variant</option>');
        if (!productId) return;
        try {
            const list = await ajaxOperation.GetAjaxAPI(`${variantsUrl}/${productId}`);
            let html = '<option value="">Select variant</option>';
            list.forEach(v => { html += `<option value="${v.id}">${v.text}</option>`; });
            selector.variantId.html(html);
        } catch (e) { console.log(e); }
    });

    // Expense calculation
    selector.firstNumber.on("input", function() {
        const first = parseFloat($(this).val()) || 0;
        const second = parseFloat(selector.secondNumber.val()) || 0;
        selector.expenseTotal.val((first * second).toFixed(2));
    });

    selector.secondNumber.on("input", function() {
        const first = parseFloat(selector.firstNumber.val()) || 0;
        const second = parseFloat($(this).val()) || 0;
        selector.expenseTotal.val((first * second).toFixed(2));
    });

    // Summary calculation events
    selector.discount.on("input", calculateTotals);
    selector.discountType.on("change", calculateTotals);
    selector.otherAmount.on("input", calculateTotals);

})();

