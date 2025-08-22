// Handles dynamic row addition, deletion, and calculation for Add Purchase page
$(document).ready(function () {
    let orderTable = $('#orderTable tbody');
    let productList = [
        { name: "30-EverCraft RuggedFlex Men's Classic Fit Denim Long Pants – All-Day Comfort Jeans 4", code: "30-43489007", unitCost: 500 },
        { name: "QuietHalo Max – Silence the World, Hear Everything 3", code: "04982894", unitCost: 500 }
    ];

    // Add product to table on enter or selection
    $('#productSearch').on('keypress', function (e) {
        if (e.which === 13) {
            let val = $(this).val();
            let product = productList.find(p => p.code === val || p.name.includes(val));
            if (product) {
                addRow(product);
                $(this).val('');
            }
            e.preventDefault();
        }
    });

    function addRow(product) {
        let row = `<tr>
            <td>${product.name}</td>
            <td>${product.code}</td>
            <td><input type="number" class="form-control qty" value="1" min="1" /></td>
            <td><input type="text" class="form-control batchNo" /></td>
            <td><input type="date" class="form-control expiredDate" /></td>
            <td><input type="number" class="form-control netUnitCost" value="${product.unitCost}" min="0" step="0.01" /></td>
            <td><input type="number" class="form-control discount" value="0" min="0" step="0.01" /></td>
            <td><input type="number" class="form-control vat" value="0" min="0" step="0.01" /></td>
            <td class="subTotal">${product.unitCost.toFixed(2)}</td>
            <td><button type="button" class="btn btn-danger btn-sm btnDelete">Delete</button></td>
        </tr>`;
        orderTable.append(row);
        recalculateTotals();
    }

    // Delete row
    orderTable.on('click', '.btnDelete', function () {
        $(this).closest('tr').remove();
        recalculateTotals();
    });

    // Recalculate on input change
    orderTable.on('input', '.qty, .netUnitCost, .discount, .vat', function () {
        let row = $(this).closest('tr');
        let qty = parseFloat(row.find('.qty').val()) || 0;
        let cost = parseFloat(row.find('.netUnitCost').val()) || 0;
        let discount = parseFloat(row.find('.discount').val()) || 0;
        let vat = parseFloat(row.find('.vat').val()) || 0;
        let subTotal = (qty * cost) - discount + vat;
        row.find('.subTotal').text(subTotal.toFixed(2));
        recalculateTotals();
    });

    function recalculateTotals() {
        let totalQty = 0, totalNet = 0, totalDiscount = 0, totalVAT = 0, totalSub = 0;
        orderTable.find('tr').each(function () {
            let qty = parseFloat($(this).find('.qty').val()) || 0;
            let cost = parseFloat($(this).find('.netUnitCost').val()) || 0;
            let discount = parseFloat($(this).find('.discount').val()) || 0;
            let vat = parseFloat($(this).find('.vat').val()) || 0;
            let subTotal = parseFloat($(this).find('.subTotal').text()) || 0;
            totalQty += qty;
            totalNet += cost;
            totalDiscount += discount;
            totalVAT += vat;
            totalSub += subTotal;
        });
        $('#totalQuantity').text(totalQty);
        $('#totalNetUnitCost').text(totalNet.toFixed(2));
        $('#totalDiscount').text(totalDiscount.toFixed(2));
        $('#totalVAT').text(totalVAT.toFixed(2));
        $('#totalSubTotal').text(totalSub.toFixed(2));
    }

    // Form submit
    $('#addPurchaseForm').on('submit', function (e) {
        e.preventDefault();
        // Collect form data and send via AJAX or handle as needed
        alert('Submitted! (Implement AJAX as needed)');
    });
});
