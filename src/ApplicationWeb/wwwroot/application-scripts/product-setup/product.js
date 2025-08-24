(function () {
    const validators = [
        initializeValidation('#Name', '#productNameError', 'Please enter the product name.'),
        initializeValidation('#CategoryId', '#categoryIdError', 'Please select a category.'),
        initializeValidation('#UnitId', '#unitError', 'Please select a unit.'),
        initializeValidation('#UnitPrice', '#unitpriceError', 'Please enter unit price.'),
        initializeValidation('#SalePrice', '#salePriceError', 'Please enter sale price.'),
        initializeValidation('#CurrentStock', '#currentStock', 'Please enter current stock.')
    ];
    let needAttribute = true;
    // urls
    const initialLoaderUrl = "/api/Product/GetInitial"; // if dropdow exist
    const saveUrl = "/api/Product/Save";
    const editUrl = "/api/Product/EditProduct";

    // success message
    const saveMessage = "Product Saved Succcessfully!";

    // image laoder
    const imageLoadingArray = [{ id: "#bannerImage", name: "banner" }, { id: "#iconImage", name: "icon" }]; // set image selector id and object name where the image link exist

    let willPreviewDiv = '';
    let counter = '';
    let selector = {
        galleryImage: $("#galleryImage"),
        galleryImagePreview: "#galleryImagePreview",
        photosSetter: "#photosSetter",

        thumbnailImage: $("#thumbnailImage"),
        thumbnailImagePreview: "#thumbnailImagePreview",
        thumbnailImageSetter: "#thumbnailImageSetter",

        metaImage: $("#metaImage"),
        metaImageSetter: "#metaImageSetter",
        metaImagePreview: "#metaImagePreview",
        parentChildRelation: $("#parentChildRelation"),
        colorIds: $("#ColorIds"),
        variationsPhoto: ".variationsPhoto",
        attributesIds: "#AttributeIds",

        pdfUpload: $("#pdfUpload"),
        pdfAmount: $("#pdfAmount"),
        pdfSetter: $("#pdfSetter"),
    };
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

            let response = await ajaxOperation.GetAjax("/api/Product/GetAttribute/" + attributes.join(","));
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
       
        const { Variant: variant, Price: price, SKU: sku, Quantity: quantity, Image: image, ImageUrl: imageUrl } = variations;
        let html = '';
        if (imageUrl != '' && imageUrl != undefined) {
            html = `<div class="row">
                        <div class="col-sm-12">
                            <img src="${imageUrl}" height="90" width="90">
                        </div>
                    </div>`;
        }
        return `<tr>
                        <td> <span class="variationsName" > ${variant} </span>  </td>
                        <td><input type="number" class="form-control variationsPrice" value = "${price}" placeholder = "Price" name = "variationsPrice[${indexing}]" min = "0"/></td>
                        <td><input type="text" class="form-control variationsSKU"  value = "${sku}" placeholder = "SKU" name = "variationsSKU[${indexing}]"/></td>
                        <td>
                            <div class="input-group pointer variationsPhoto" setter = "#variationsPhotoSetter${indexing}" preview = "#variationsPhotoPreview${indexing}">
                                <div class="input-group-prepend">
                                    <div class="input-group-text bg-soft-secondary font-weight-medium">Browse </div>
                                </div>
                                <div class="form-control file-amount">Choose</div>
                                <input type="hidden" class = "variationsPhotoSetter" value = "${image}"  name = "variationsPhotoSetter${indexing}" id="variationsPhotoSetter${indexing}">
                            </div>
                            <div class="file-preview box sm" id="variationsPhotoPreview${indexing}">
                                ${html}
                            </div>
                        </td>
                    </tr>`;
    }

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

        output.forEach(item => {
            const variation = { Variant: item, Price: unitPrice, SKU: sku, Quantity: qty, Image: '', ImageUrl: '' };
            data += MakingVariationHtml(indexing, variation);
            indexing++;
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
    const InitialLoader = async () => {
        let response = await ajaxOperation.GetAjaxAPI(initialLoaderUrl);
        
        response.stockVisibilityState = 1;
        response.shippingType = 1;
        setFormData(formEl, response);
        PopulateColorCSS(response.colorList);
        PopulateSelect2Formatter(selector.attributesIds);

        formEl.find('#LowStaockQuantity').val(1);
        formEl.find('#CashOnDelivery').prop('checked', true).trigger('change');

        if (response.isShippingEnable) formEl.find('#shipping_type').show();
        else formEl.find('#shipping_type').hide();

        if (response.isFlashDealEnable) formEl.find('#flash_deal').show();
        else formEl.find('#flash_deal').hide();

        if (response.isStockVisibilityEnable) formEl.find('#stock_visibility_state').show();
        else formEl.find('#stock_visibility_state').hide();

        if (response.isVatTaxEnable) formEl.find('#vat_tax').show();
        else formEl.find('#vat_tax').hide();

    }
    const GetSKUData = () => {
        let variation = new Array();
        $("#productVariationTBody tr").each(function () {
            let Variant = $(this).find('td .variationsName').text().trim();
            let Price = Number($(this).find('td .variationsPrice').val());
            let SKU = $(this).find('td .variationsSKU').val();
            let Quantity = Number($(this).find('td .variationsQuantity').val());
            let Image = $(this).find('td .variationsPhotoSetter').val();
            let ImageUrl = $(this).find('td img').attr("src");
            variation.push({ Variant, Price, SKU, Quantity, Image, ImageUrl });
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

    const Save = async (published = false) => {
        try {
            if (IsFormsValid(validators)) {

                if (formEl.find('#Discount').val() > 0) {
                    if (!formEl.find('#DiscountType').val()) {
                        Failed("Please enter discount type!");
                        return;
                    }
                }

                if (formEl.find('#FlashDealId').val()) {
                    if (!formEl.find('#FlashDiscount').val()) {
                        Failed("Please enter flash deal discount!");
                        return;
                    }
                    if (!formEl.find('#FlashDiscountType').val()) {
                        Failed("Please enter flash deal discount type!");
                        return;
                    }
                }

                if (formEl.find('#TaxId').val()) {
                    if (!formEl.find('#Tax').val()) {
                        Failed("Please enter tax amount!");
                        return;
                    }
                    if (!formEl.find('#TaxType').val()) {
                        Failed("Please enter tax type!");
                        return;
                    }
                }

                let formData = new FormData(formEl[0]);
                formData.append("PdfUpload", selector.pdfSetter.get(0));
                formData.append("Variation", JSON.stringify(GetSKUData()));
                const [colors, attribute, choiceAttribute] = AttributeGetter();
                formData.append("Colors", JSON.stringify(colors));
                formData.append("Attributes", JSON.stringify(attribute));
                formData.append("ChoiceOptions", JSON.stringify(choiceAttribute));
                formData.append("Published", published ? 1 : 0);

                let response = await ajaxOperation.SaveAjax(saveUrl, formData);
                
                if (typeof (response) == "object") {
                    Success(saveMessage);
                    ResetForm(formEl);
                    InitialLoader();

                    $("#galleryImagePreview").empty();
                    $("#thumbnailImagePreview").empty();
                    $('#Description').summernote('reset');
                    $('#MetaDescription').summernote('reset');
                }
                else {
                    Failed(response);
                }
                 
            } else {
                Failed("Please Select Required Fields.");
            }
        } catch (e) {
            handleValidationErrors(e.responseText);
        }
    }

    const handleValidationErrors = (response) => {
        let errors;
        try {
            errors = JSON.parse(response);
        } catch (e) {
            Failed(response);
            return;
        }

        if (Array.isArray(errors)) {
            errors.forEach(error => {
                const message = error.errorMessage || "Unknown error occurred.";
                Failed(message);
            });
        } else {
            Failed("An error occurred while saving.");
        }
    };

    CommonInitializer();

    $(document).ready(function () {
        $('#MetaDescription').summernote();
        $('#Description').summernote();

        InitialLoader();
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    formEl.find("#btnPublishSave").click(function () {
        Save(true);
    });

    formEl.find("#btnCancel").click(function () {
        InitialLoader();
        RemoveLoadImages(imageLoadingArray);
        $("#galleryImagePreview").empty();
        $("#thumbnailImagePreview").empty();
        $('#Description').summernote('reset'); 
        $('#MetaDescription').summernote('reset');
    });

    $("#BannerImage").change(function () {
        const [file] = $("#BannerImage")[0].files
        if (file) {
            $("#bannerImage").attr("src", URL.createObjectURL(file))
        }
    });
    $("#IconImage").change(function () {
        const [file] = $("#IconImage")[0].files
        if (file) {
            $("#iconImage").attr("src", URL.createObjectURL(file))
        }
    });

    // show image browser
    $(document).on("click", modalPreviewSelector.addFilesToBrowse, function () {
        modalPreview.ImageSetter(counter);
        modalPreview.PopulateImageLoader(willPreviewDiv, counter, imageSource);
        modalOperation.ModalHide(modalPreviewSelector.uploaderModal);
    });

    // galleryImage selector
    selector.galleryImage.click(function () {
        willPreviewDiv = selector.galleryImagePreview;
        counter = imageCounter.ALL;
        imageCounter.SETTER = selector.photosSetter;
        imageCounter.COLUMN_SIZE = "col-sm-2",
        modalOperation.ModalStatic(modalPreviewSelector.uploaderModal);
        modalOperation.ModalShow(modalPreviewSelector.uploaderModal);
        modalPreview.GetAllSavedImage();
    });

    // thumbnail selector
    selector.thumbnailImage.click(function () {
        willPreviewDiv = selector.thumbnailImagePreview;
        counter = imageCounter.PARTIAL;
        imageCounter.COUNTER = 1;
        imageCounter.SETTER = selector.thumbnailImageSetter;
        imageCounter.COLUMN_SIZE = "col-sm-2",
        modalOperation.ModalStatic(modalPreviewSelector.uploaderModal);
        modalOperation.ModalShow(modalPreviewSelector.uploaderModal);
        modalPreview.GetAllSavedImage();
    });
    // meta image selector
    selector.metaImage.click(function () {
        willPreviewDiv = selector.metaImagePreview;
        counter = imageCounter.PARTIAL;
        imageCounter.COUNTER = 1;
        imageCounter.SETTER = selector.metaImageSetter;
        imageCounter.COLUMN_SIZE = "col-sm-2",
            modalOperation.ModalStatic(modalPreviewSelector.uploaderModal);
        modalOperation.ModalShow(modalPreviewSelector.uploaderModal);
        modalPreview.GetAllSavedImage();
    });
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

    selector.pdfUpload.click(function () {
        selector.pdfSetter.trigger("click");
    });

    selector.pdfSetter.change(function (event) {
        const files = event.target.files[0];
        if (event.target.files.length > 0) {
            selector.pdfAmount.text(files.name);
        }
        else {
            selector.pdfAmount.text("Choose File");
        }
    });

    combination.colorId.change(function () {
        PopulateCombination();
    });

    combination.attributeIds.change(function () {
        let attributes = $(this).val();
        if (attributes.length)
        {
           
            GetAttribute(attributes);
        }
        else
        {
           
            combination.choiceInfoTBody.empty();
        }
           
        PopulateCombination();
    });

    $(document).on("change", combination.attributeValue, function () {
        PopulateCombination();
    });
})();