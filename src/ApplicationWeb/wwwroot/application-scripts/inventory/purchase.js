(function () {
    ecomTable = null;

    const validators = [
        initializeValidation('#productId', '#productNameError', 'Please enter the product name.'),
        initializeValidation('#unitId', '#unitError', 'Please select a unit.'),
        initializeValidation('#supplierId', '#supplierIdError', 'Please select a supplier.'),
        initializeValidation('#PurchasePrice', '#purchasePriceError', 'Please enter unit price.'),
        initializeValidation('#PurchaseDate', '#purchaseDateError', 'Please enter purchase price.'),
    ];

    const initialUrl = "/api/Purchase/GetInitial";
    const saveUrl = "/api/Purchase/Save";
    const editUrl = "/api/Purchase/EditPurchase";

    // Success Message
    const saveMessage = "Purchase Saved Successfully";
    let supplierList = new Array();
    let variationList = new Array();
    let needAttribute = true;
    let productReturnStatus = false;

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
        colorIds: $("#ColorIds"),
        variationsPhoto: ".variationsPhoto",
        attributesIds: "#AttributeIds",

        btnAdd: $("#btnAdd"),
        btnReturn: "btnReturn",
        btnDelete: "btnDelete",
        purchaseTbody: $("#purchaseTbody"),
        savePurchase: $("#savePurchase"),
        savePurchaseForm: $("#savePurchaseForm"),
        btnAllList: $("#btnAllList"),
        btnNew: $("#btnNew"),
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
            if (IsFormsValid(validators)) {
                if ($("#productVariationInfo tbody tr").length === 0) {
                    $("#message").show();
                    return;
                }
                let isValid = true;

                $(".variationsQuantity").each(function () {
                    if ($(this).val().trim() === "") {
                        isValid = false;
                        $(this).css("border", "1px solid red");
                    } else {
                        $(this).css("border", "");
                    }
                });

                if (!isValid) {
                    alert("Please enter a quantity before saving.");
                    e.preventDefault();
                }

                let formData = new FormData(formEl[0]);
                formData.append("Photo", $("#Photo").get(0));
                formData.append("Variation", JSON.stringify(GetSKUData()));
                const [colors, attribute, choiceAttribute] = AttributeGetter();
                formData.append("Colors", JSON.stringify(colors));
                formData.append("Attributes", JSON.stringify(attribute));
                formData.append("ChoiceOptions", JSON.stringify(choiceAttribute));
                formData.append("ProductReturnStatus", productReturnStatus);
                var imageSrc = $("#bandImage").attr("src");
                formData.append("ProductImage", imageSrc);

                let response = await ajaxOperation.SaveAjax(saveUrl, formData);

                if (typeof (response) == "object") {
                    Success(saveMessage);
                    ResetForm(formEl);
                    GetInitial();
                    RemoveLoadImages(imageLoadingArray);

                }
                else {
                    Failed(response);
                }

            } else {
                Failed("Please Select Required Fields.");
            }
        } catch (e) {
            /* handleValidationErrors(e.responseText);*/
        }
    }

    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        divPrimaryEl.show();
        ecomTable.fnFilter();

    }


    let combination = {
        colorId: $("#ColorIds"),
        attributeIds: $("#AttributeIds"),
        productVariationTBody: $("#productVariationTBody"),
        choiceInfoTBody: $("#choiceInfoTBody"),
        productVariation: $("#productVariation"),
        productVariationInfo: $("#productVariationInfo"),
        attributeValue: ".attribute-value",
        variationsPhoto: ".variationsPhoto"
    }

    // region product combination

    const MakeDropdown = selectListItem => {
        let html = "";
        selectListItem.forEach(item => {
            const { id, text } = item;
            html += `<option value = "${id}"> ${text} </option>`;
        });
        return html;
    }

    const GetAttribute = async attributes => {
        try {
            if (!needAttribute) return; // while editing products it auto triggers. To avoid this we need to maintain this flag

            let response = await ajaxOperation.GetAjax("/api/Purchase/GetAttribute/" + attributes.join(","));
            if (!Array.isArray(response)) return;

            //combination.choiceInfoTBody.empty();

            let attrIds = [];
            $("#choiceAtrributeInfo tbody tr").each(function () {
                var attributeId = $(this).find('.attrValue select').attr('attributeId');
                attrIds.push(attributeId);
            });
            let indexing = 1;
            if (attrIds.length > attributes.length) { //remove attributes
                combination.choiceInfoTBody.empty();
                attributes.forEach(item => {
                    let attributeList = response.filter(atribute => atribute.attributeId == item).map(obj => {
                        return { id: obj.value, attributeName: obj.attributeName, text: obj.value, attributeId: obj.attributeId };
                    });

                    if (!Array.isArray(attributeList) || attributeList.length == 0) return;

                    const { attributeName, attributeId } = attributeList[0];
                    let data = `<tr>
                            <td><input type="text" name = "choiceOptionName[${attributeId}]" class="form-control attributeName"  value="${attributeName}" readonly /></td>
                            <td class="attrValue"><select class="form-control attribute-value" id = "attributeId${attributeId}"  name = "choiceOptionValue[${attributeId}]" attributeId = "${attributeId}" multiple="multiple">${MakeDropdown(attributeList)} </select></td>
                        </tr>`;
                    combination.choiceInfoTBody.append(data);
                    $("#attributeId" + attributeId).select2();
                    PopulateSelect2Formatter("#attributeId" + attributeId);
                    indexing++;
                });
            }
            else {
                attributes.forEach(item => {

                    const foundAttribute = attrIds.find(function (attr) {
                        return item == attr;
                    });
                    if (!foundAttribute) {
                        let attributeList = response.filter(atribute => atribute.attributeId == item).map(obj => {
                            return { id: obj.value, attributeName: obj.attributeName, text: obj.value, attributeId: obj.attributeId };
                        });

                        if (!Array.isArray(attributeList) || attributeList.length == 0) return;

                        const { attributeName, attributeId } = attributeList[0];
                        let data = `<tr>
                            <td><input type="text" name = "choiceOptionName[${attributeId}]" class="form-control attributeName"  value="${attributeName}" readonly /></td>
                            <td class="attrValue"><select class="form-control attribute-value" id = "attributeId${attributeId}"  name = "choiceOptionValue[${attributeId}]" attributeId = "${attributeId}" multiple="multiple">${MakeDropdown(attributeList)} </select></td>
                        </tr>`;
                        combination.choiceInfoTBody.append(data);
                        $("#attributeId" + attributeId).select2();
                        PopulateSelect2Formatter("#attributeId" + attributeId);
                        indexing++;
                    }


                });
            }

        }
        catch (e) {
            console.log(e);
        }
    }

    const CombinationMaker = combination => {
        if (combination.size == 1) {
            const [lastValue] = combination.values();
            return lastValue;
        } else {
            let [currentKey, currentValue] = combination.entries().next().value;
            let output = new Array();
            let ancestor = new Map();
            combination.forEach((value, key) => {
                if (currentKey != key)
                    ancestor.set(key, value);
            });
            let requiredChildren = CombinationMaker(ancestor);
            currentValue.forEach(parent => {
                requiredChildren.forEach(child => {
                    output.push(parent + "-" + child);
                });
            });
            return output;
        }
    }

    const MakingVariationHtml = (indexing, variations) => {

        const { SL: sl, Variant: variant, Price: price, Quantity: quantity } = variations;
        let html = '';
        return `<tr>
                        <td> <span class="sl" > ${sl} </span>  </td>
                        <td> <span class="variationsName" > ${variant} </span>  </td>
                        <td><input type="number" class="form-control variationsPrice" value = "${price}" placeholder = "Price" name = "variationsPrice[${indexing}]" min = "0"/></td>
                        <td><input type="number" class="form-control variationsQuantity" value = "${quantity}" placeholder = "Quantity" name = "variationsQuantity[${indexing}]" value = "0" min = "0"/></td>
                        <td> <button type = "button" class = "btn btn-sm btn-danger btnVariationDelete" uniqueIdentity = "${indexing}" title = "Delete"><i class="fas fa-trash-alt"></i></button>
                 </td>
                    </tr>`;
    }

    $(document).on("click", ".btnVariationDelete", function () {
        try {
            let uniqueIdentity = $(this).attr("uniqueIdentity");
            $(this).closest("tr").remove();
            variationList = variationList.filter(item => item.uniqueIdentity !== uniqueIdentity);
        } catch (e) {
            console.error("Error deleting variation:", e);
        }
    });

    const VariationSetter = variationAttributes => {
        let output = CombinationMaker(variationAttributes);
        let data = '';
        let indexing = 0;
        combination.productVariation.hide();
        if (output.length === 0) return;

        combination.productVariation.show();
        if ($.fn.DataTable.isDataTable('#productVariationInfo')) {
            combination.productVariationInfo.DataTable().destroy();
        }

        var unitPrice = formEl.find("#UnitPrice").val();
        var discount = formEl.find("#Discount").val();
        var discountType = formEl.find("#DiscountType").val();
        var qty = formEl.find("#CurrentStock").val();
        var sku = formEl.find("#ProductSKU").val();

        if (discountType == 'amount') {
            unitPrice -= discount;
        } else {
            unitPrice -= (discount * unitPrice) / 100;
        }

        qty = Math.floor(qty / output.length);
        var sl = 1;
        output.forEach(item => {
            const variation = { SL: sl, Variant: item, Price: unitPrice, SKU: sku, Quantity: qty, Image: '', ImageUrl: '' };
            data += MakingVariationHtml(indexing, variation);
            indexing++;
            sl++;
        });
        combination.productVariationTBody.html(data);
        combination.productVariationInfo.DataTable({
            "paging": false,
            "ordering": true,
            "searching": false,
        });
    }

    const PopulateCombination = () => {

        let variationAttributes = new Map();
        let index = 0;
        let colors = new Array();
        $("#ColorIds :selected").each(function (i, selectedElement) {
            colors.push($(selectedElement).text());
        });
        if (colors.length > 0) {
            variationAttributes.set(index++, colors);
        }


        $(combination.attributeValue).each(function () {
            let currentSelected = $(this).children("option:selected").text().split("  ").filter(item => item !== '');
            if (currentSelected.length > 0) {
                variationAttributes.set(index++, currentSelected);
            }
        });
        combination.productVariationTBody.empty();
        if (variationAttributes.size > 0)
            VariationSetter(variationAttributes);
    }

    // end region
    const PopulateColorCSS = colors => {
        $("#ColorIds").select2({
            width: "100%",
            height: "100%",
            placeholder: "Select a Item",
            templateSelection: function (data, container) {
                var selection = $('#ColorIds').select2('data');
                var idx = selection.indexOf(data);
                data.idx = idx;
                let colorArrayIndex = colors.findIndex(item => item.text == data.text.trim());
                $(container).css("background-color", colors[colorArrayIndex].code);
                return data.text;
            },
            templateResult: function (data, container) {
                var selection = $("#ColorIds").select2('data');
                var idx = selection.indexOf(data);
                data.idx = idx;
                let colorArrayIndex = colors.findIndex(item => item.text == data.text.trim());
                return $(`<span><span class='size-15px d-inline-block mr-2 rounded border' style='background:${colors[colorArrayIndex]?.code}; color: ${colors[colorArrayIndex]?.code}'>SM</span><span>${data.text}</span></span>`);
            }
        });
    }

    const GetSKUData = () => {
        let variation = new Array();
        $("#productVariationTBody tr").each(function () {
            let Variant = $(this).find('td .variationsName').text().trim().replace(/\s*-\s*/g, "-").replace(/\s+/g, " ");
            let Price = Number($(this).find('td .variationsPrice').val());
            let Quantity = Number($(this).find('td .variationsQuantity').val());
            variation.push({ Variant, Price, Quantity });
        });
        return variation;
    }

    const AttributeGetter = () => {
        let colors = $("#ColorIds").val();
        let attribute = $(selector.attributesIds).val();
        let choiceAttribute = new Array();
        $("#choiceInfoTBody tr").each(function () {
            let attributeValue = $(this).find('td .attribute-value').val();
            let attributeId = $(this).find('td .attribute-value').attr('attributeId');
            choiceAttribute.push({ attributeId, value: attributeValue });
        });
        return [colors, attribute, choiceAttribute]
    }

    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            setFormData(formEl, response);
            PopulateColorCSS(response.colorList);
            PopulateSelect2Formatter(selector.attributesIds);

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
                    const result = ButtonPartial('Purchase', full.id);
                    return result;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<div class="btn-group" style ="text-align:center;">
                                <button type = "button" class="btn btn-info btn-sm btnReturn" id = "${full.id}">Product Return</button>&nbsp
                            </div>`;
                }
            }


        ];
        let dtLoader = DataTableLoader("/api/Purchase/list", columns);
        ecomTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }


    const Edit = async id => {

        try {
            needAttribute = false;
            const response = await ajaxOperation.GetAjaxAPI(editUrl + `/${id}`);
            let { colorList, colors, attributes, choiceOptions, variations } = response;

            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            setFormData(formEl, response);
            LoadImages(imageLoadingArray, response);
            PopulateColorCSS(colorList);
            PopulateSelect2Formatter(selector.attributesIds);

            selector.colorIds.val(JSON.parse(colors)).attr("selected", true).trigger("change");
            $(selector.attributesIds).val(JSON.parse(attributes)).attr("selected", true).trigger("change");

            //generating the attribute dropdown and setting up the selected value
            needAttribute = true;
            await GetAttribute(JSON.parse(attributes)); // await is required cause first we need to create the actual DOM of choices option
            choiceOptions = JSON.parse(choiceOptions);
            choiceOptions.forEach(item => {
                const { attributeId, value: optionValue } = item;
                $(combination.attributeValue).each(function () {
                    const choicesAttributeValue = $(this).attr("attributeId");
                    if (choicesAttributeValue == attributeId) {
                        $(this).val(optionValue).attr("selected", true).trigger("change");
                    }
                });
            });

            $("#productVariationTBody tr").each(function (index) {
                var variantName = $(this).find("td:eq(1)").text().trim()
                    .replace(/\s*-\s*/g, "-") // Remove spaces around hyphens
                    .replace(/\s+/g, " "); // Normalize spaces

                var matchedVariant = response.purchaseItemDTO.find(v =>
                    v.variant.trim().replace(/\s*-\s*/g, "-").replace(/\s+/g, " ") === variantName
                );

                if (matchedVariant) {
                    $(this).find("td:eq(2) input").val(matchedVariant.price);
                    $(this).find("td:eq(3) input").val(matchedVariant.quantity);
                }
            });



        } catch (e) {
            Failed(e);
        }
    }



    CommonInitializer();

    $(document).ready(function () {
        DivisionShowHide(true, false);
        GenerateList();
        GetInitial();
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


    $(tblMasterId).on("click", "." + selector.btnReturn, async function () {
        productReturnStatus = true;
        try {
            const purchaseId = $(this).attr("id");
            window.location.href = window.location.origin + '/Admin/Inventory/PurchaseReturnLoad?purchaseId=' + encodeURIComponent(purchaseId);
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
    });

    selector.btnBack.click(function () {
        Back();

        /*  DivisionShowHide(true, false);*/
    })


    // variation image selector
    $(document).on("click", selector.variationsPhoto, function () {
        const setter = $(this).attr("setter");
        const preview = $(this).attr("preview");
        willPreviewDiv = preview;
        counter = imageCounter.PARTIAL;
        imageCounter.COUNTER = 1;
        imageCounter.SETTER = setter;
        imageCounter.COLUMN_SIZE = "col-sm-12",
            modalOperation.ModalStatic(modalPreviewSelector.uploaderModal);
        modalOperation.ModalShow(modalPreviewSelector.uploaderModal);
        modalPreview.GetAllSavedImage();
    });



    combination.colorId.change(function () {
        PopulateCombination();
        $("#message").hide();
    });

    combination.attributeIds.change(function () {
        let attributes = $(this).val();
        if (attributes.length) {
            GetAttribute(attributes);
        }
        else {
            combination.choiceInfoTBody.empty();
        }

        PopulateCombination();
    });

    $(document).on("change", combination.attributeValue, function () {
        PopulateCombination();
    });
    $("#Photo").change(function () {
        const [file] = $("#Photo")[0].files
        if (file) {
            $("#bandImage").attr("src", URL.createObjectURL(file))
        }
    });
})();
