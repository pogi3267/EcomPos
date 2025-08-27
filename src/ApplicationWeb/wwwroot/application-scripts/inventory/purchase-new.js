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
        subTotal: $("#subTotal"),
        otherExpenses: $("#otherExpenses"),
        totalWithExpenses: $("#totalWithExpenses"),
        netPrice: $("#netPrice"),
        note: $("#note"),
        
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
        calculateTotals();
    }

    const Save = async () => {
        try {
            if (IsFormsValid(validators)) {
                if (purchaseItems.length === 0) {
                    Failed("Please add at least one product to the purchase.");
                    return;
                }

                const model = {
                    Id: parseInt(selector.purchaseId.val()) || 0,
                    PurchaseDate: selector.purchaseDate.val() ? new Date(selector.purchaseDate.val()) : null,
                    PurchaseNo: selector.purchaseNo.val(),
                    SupplierId: parseInt(selector.supplierId.val()),
                    SupplierAddress: selector.supplierAddress.val(),
                    PurchaseItems: purchaseItems,
                    Expenses: expenses,
                    Discount: parseFloat(selector.discount.val()) || 0,
                    DiscountType: selector.discountType.val(),
                    OtherExpenses: parseFloat(selector.otherExpenses.val()) || 0,
                    NetPrice: parseFloat(selector.netPrice.val()) || 0,
                    Note: selector.note.val(),
                    GrandTotalAmount: parseFloat(selector.netPrice.val()) || 0
                };

                let response = await ajaxOperation.SaveAjax(saveUrl, model, 'application/json');

                if (typeof (response) == "object") {
                    Success(saveMessage);
                    ResetForm();
                    DivisionShowHide(true, false);
                    GenerateList();
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
        const variantName = selector.variantName.val().trim();
        const quantity = parseInt(selector.quantity.val());
        const unit = selector.unit.val().trim();
        const purchasePrice = parseFloat(selector.purchasePrice.val());
        const branchId = parseInt(selector.branchId.val());

        if (!productId) {
            Failed("Please select a product.");
            return;
        }
        if (!variantName) {
            Failed("Please enter variant name.");
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
            Id: editingProductIndex >= 0 ? purchaseItems[editingProductIndex].Id : 0,
            ProductId: productId,
            ProductName: productName,
            VariantName: variantName,
            Quantity: quantity,
            Unit: unit,
            PurchasePrice: purchasePrice,
            BranchId: branchId,
            BranchName: "Main Branch",
            TotalPrice: quantity * purchasePrice
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
        selector.productId.val(item.ProductId);
        selector.variantName.val(item.VariantName);
        selector.quantity.val(item.Quantity);
        selector.unit.val(item.Unit);
        selector.purchasePrice.val(item.PurchasePrice);
        selector.branchId.val(item.BranchId);
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
                    <td>${item.ProductName}</td>
                    <td>${item.VariantName}</td>
                    <td>${item.Quantity}</td>
                    <td>${item.Unit}</td>
                    <td>${item.PurchasePrice.toFixed(2)}</td>
                    <td>${item.BranchName}</td>
                    <td>${item.TotalPrice.toFixed(2)}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-warning btnEditProduct" data-index="${index}">
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
        selector.variantName.val("");
        selector.quantity.val("");
        selector.unit.val("PCS");
        selector.purchasePrice.val("");
        selector.branchId.val(1);
        editingProductIndex = -1;
        selector.btnAddProduct.text("Add");
    }

    const AddExpense = () => {
        const costName = selector.costName.val().trim();
        const firstNumber = parseFloat(selector.firstNumber.val()) || 0;
        const secondNumber = parseFloat(selector.secondNumber.val()) || 0;
        const total = firstNumber * secondNumber;

        if (!costName) {
            Failed("Please enter cost name.");
            return;
        }

        const expenseItem = {
            Id: editingExpenseIndex >= 0 ? expenses[editingExpenseIndex].Id : 0,
            CostName: costName,
            FirstNumber: firstNumber,
            SecondNumber: secondNumber,
            Total: total
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
        selector.costName.val(item.CostName);
        selector.firstNumber.val(item.FirstNumber);
        selector.secondNumber.val(item.SecondNumber);
        selector.expenseTotal.val(item.Total);
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
                    <td>${item.CostName}</td>
                    <td>${item.FirstNumber.toFixed(2)}</td>
                    <td>${item.SecondNumber.toFixed(2)}</td>
                    <td>${item.Total.toFixed(2)}</td>
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
        selector.costName.val("");
        selector.firstNumber.val(0);
        selector.secondNumber.val(0);
        selector.expenseTotal.val(0);
        editingExpenseIndex = -1;
        selector.btnAddExpense.text("Add");
    }

    const calculateTotals = () => {
        // Calculate subtotal from products
        const productSubtotal = purchaseItems.reduce((sum, item) => sum + item.TotalPrice, 0);
        
        // Calculate expense total
        const expenseTotal = expenses.reduce((sum, item) => sum + item.Total, 0);
        
        // Apply discount
        const discount = parseFloat(selector.discount.val()) || 0;
        const discountType = selector.discountType.val();
        let discountedTotal = productSubtotal;
        
        if (discountType === 'amount') {
            discountedTotal = productSubtotal - discount;
        } else if (discountType === 'percentage') {
            discountedTotal = productSubtotal - (productSubtotal * discount / 100);
        }
        
        // Add other expenses
        const otherExpenses = parseFloat(selector.otherExpenses.val()) || 0;
        const netPrice = discountedTotal + expenseTotal + otherExpenses;
        
        // Update display
        selector.subTotal.val(discountedTotal.toFixed(2));
        selector.totalWithExpenses.val((discountedTotal + expenseTotal).toFixed(2));
        selector.netPrice.val(netPrice.toFixed(2));
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
                                <button type="button" class="btn btn-sm btn-warning btnEdit" id="${full.id}">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button type="button" class="btn btn-sm btn-danger btnDelete" id="${full.id}">
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
            
            // Populate form with response data
            selector.purchaseId.val(response.id);
            selector.purchaseDate.val(response.purchaseDate ? response.purchaseDate.split('T')[0] : '');
            selector.purchaseNo.val(response.purchaseNo);
            selector.supplierId.val(response.supplierId);
            selector.supplierAddress.val(response.supplierAddress);
            selector.discount.val(response.discount);
            selector.discountType.val(response.discountType);
            selector.otherExpenses.val(response.otherExpenses);
            selector.netPrice.val(response.netPrice);
            selector.note.val(response.note);
            
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
    selector.otherExpenses.on("input", calculateTotals);

})();

