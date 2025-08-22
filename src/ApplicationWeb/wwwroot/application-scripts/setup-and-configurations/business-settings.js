(function () {
    const prodTypes = 'shipping_configuration_enabled,flash_deal_enabled,stock_visibility_enabled,vat_tax_enabled';
    const types = 'site_copywrite_text,payment_mode';
    const currencyTypes = 'system_default_currency_name,system_default_currency,currency_position,amount_format';
    const mailingTypes = 'admin_mailing_address,send_mail_to_admin_after_order,send_mail_to_customer_after_order,send_mail_after_change_order_status,maintenance_mode,maintenance_time';
    const addressTypes = 'address_config';
    const timezoneTypes = 'timezone';
    const homepageTypes = 'home_page_config';
    const images = 'site_carousel_images,footer_online_payment_icon,home_banner1_image,home_banner2_image,home_banner3_image';
    let formCarouselImage, formpaymentImage, formCopywrite, formCurrency, formMailing, formAddress, formTimezone, formPaymentMode, formMaintenanceMode, formHomePage, formHomeBannerSetup, formProductConfiguration;

    let willPreviewDiv = '';
    let counter = '', rowCount = 1;

    const CommonInitializer = function () {
        formCarouselImage = $("#formCarouselImage");
        formpaymentImage = $("#formpaymentImage");
        formCopywrite = $("#formCopywrite");
        formCurrency = $("#formCurrency");
        formMailing = $("#formMailing");
        formAddress = $("#formAddress");
        formTimezone = $("#formTimezone");
        formPaymentMode = $("#formPaymentMode");
        formMaintenanceMode = $("#formMaintenanceMode");
        formHomePage = $("#formHomePage");
        formHomeBannerSetup = $("#formHomeBannerSetup");
        formProductConfiguration = $("#formProductConfiguration");
    }

    const Save = async (type, val, formEl) => {
        try {
            let model = {
                Type: type,
                Value: val
            }
            let response = await ajaxOperation.SavePostAjax("/api/BusinessSetting/Save", model);
            if (typeof (response) == "object") {
                Success("Saved Successfully!");
            }
        } catch (e) {
            Failed(e);
        }
    }

    const SaveImages = async (type, imgs, formEl) => {
        try {
            let formData = new FormData();
            formData.append("Type", type);
            formData.append("Value", "");

            for (let i = 0; i < imgs.length; i++) {
                formData.append("Images", imgs[i]);
            }

            let response = await ajaxOperation.SaveAjax("/api/BusinessSetting/SaveImages", formData);

            if (typeof (response) == "object") {
                Success("Saved Successfully!");
            }
        } catch (e) {
            Failed(e);
        }
    }

    const SaveBannerImages = async (files1, files2, files3) => {
        try {
            let formData = new FormData();
            formData.append("Type", formHomeBannerSetup.find("input[name=home_banner1_image]").val());
            for (let i = 0; i < files1.length; i++) {
                formData.append("Images", files1[i]);
            }

            formData.append("Type2", formHomeBannerSetup.find("input[name=home_banner2_image]").val());
            for (let i = 0; i < files2.length; i++) {
                formData.append("Images2", files2[i]);
            }

            formData.append("Type3", formHomeBannerSetup.find("input[name=home_banner3_image]").val());
            for (let i = 0; i < files3.length; i++) {
                formData.append("Images3", files3[i]);
            }

            let response = await ajaxOperation.SaveAjax("/api/BusinessSetting/SaveBannerImages", formData);

            Success("Saved Successfully!");
        } catch (e) {
            Failed(e);
        }
    }

    const SaveMultiple = async (models, formEl) => {
        try {
            let response = await ajaxOperation.SavePostAjaxListAPI('/api/BusinessSetting/SaveMultiple', JSON.stringify(models));
            Success("Saved Successfully!");
        } catch (e) {
            Failed(e);
        }
    }

    const SaveModel = async (model) => {
        try {
            let response = await ajaxOperation.SavePostAjax("/api/BusinessSetting/Save", model);
            if (typeof (response) == "object") {
                Success("Saved Successfully!");
            }
        } catch (e) {
            Failed(e);
        }
    }

    const GetImageSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-Image-Settings/${types}`);
            loadImages(response.carouselImageList, 'carouselImagePreview', 200);
            loadImages(response.paymentImageList, 'paymentImagePreview', 100);

            loadImages(response.banner1ImageList, 'banner1ImagePreview', 200);
            loadImages(response.banner2ImageList, 'banner2ImagePreview', 200);
            loadImages(response.banner3ImageList, 'banner3ImagePreview', 200);

        } catch (e) {
            Failed(e);
        }
    }

    const GetSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-Settings/${types}`);
            var data = response.settings.find(function (element) {
                return element.type == 'site_copywrite_text';
            });
            $("#site_copywrite_text_value").val(data.value);

            data = response.settings.find(function (element) {
                return element.type == 'payment_mode';
            });
            $("#payment_mode_value").val(data.value);
        } catch (e) {
            Failed(e);
        }
    }

    const GetCurrencySettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-Settings/${types}`);

            data = response.settings.find(function (element) {
                return element.type == 'system_default_currency_name';
            });
            $("#system_default_currency_name_value").val(data.value);

            data = response.settings.find(function (element) {
                return element.type == 'system_default_currency';
            });
            $("#system_default_currency_value").val(data.value);

            data = response.settings.find(function (element) {
                return element.type == 'currency_position';
            });
            $("#currency_position_value").val(data.value);

            data = response.settings.find(function (element) {
                return element.type == 'amount_format';
            });
            $("#amount_format_value").val(data.value);


        } catch (e) {
            Failed(e);
        }
    }

    const GetProductSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-Settings/${types}`);

            data = response.settings.find(function (element) {
                return element.type == 'shipping_configuration_enabled';
            });
            $('#shipping_configuration_enabled_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'flash_deal_enabled';
            });
            $('#flash_deal_enabled_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'stock_visibility_enabled';
            });
            $('#stock_visibility_enabled_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'vat_tax_enabled';
            });
            $('#vat_tax_enabled_value').prop('checked', data.value == 'true').trigger('change');

        } catch (e) {
            Failed(e);
        }
    }

    const GetMailingSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-Settings/${types}`);

            data = response.settings.find(function (element) {
                return element.type == 'admin_mailing_address';
            });
            $("#admin_mailing_address_value").val(data.value);

            data = response.settings.find(function (element) {
                return element.type == 'send_mail_to_admin_after_order';
            });
            $('#send_mail_to_admin_after_order_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'send_mail_to_customer_after_order';
            });
            $('#send_mail_to_customer_after_order_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'send_mail_after_change_order_status';
            });
            $('#send_mail_after_change_order_status_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'maintenance_mode';
            });
            $('#maintenance_mode_value').prop('checked', data.value == 'true').trigger('change');

            data = response.settings.find(function (element) {
                return element.type == 'maintenance_time';
            });
            $('#maintenance_time_value').val(moment(DateTimeFormatter(data.value)).format(dateFormats.DEFAULT_TIME));

        } catch (e) {
            Failed(e);
        }
    }

    const GetAddressSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-Settings/${types}`);
            data = response.settings.find(function (element) {
                return element.type == 'address_config';
            });
            let settingsData = data.value;

            // Parse JSON if not already an object
            let jsonData = typeof settingsData === "string" ? JSON.parse(settingsData) : settingsData;

            var headerRow = `
                <div class="row mt-2 font-weight-bold">
                    <div class="col-sm-3"><input type="text" class="form-control font-weight-bold text-center" value="Name" disabled/></div>
                    <div class="col-sm-3"><input type="text" class="form-control font-weight-bold text-center" value="Label" disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Type" disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Required" disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Actions" disabled/></div>
                </div>
            `;

            if ($('#dynamicRowsContainer').find('.font-weight-bold').length === 0) {
                $('#dynamicRowsContainer').append(headerRow);
            }


            if (jsonData) {
                jsonData.forEach(item => {
                    let newRow = `
                    <div class="row mt-2 dynamic-row">
                        <div class="col-sm-3">
                            <input type="text" class="form-control" name="fieldText" value="${item.fieldName}" disabled/>
                            <input type="hidden" name="field" value="${item.field}">
                        </div>
                        <div class="col-sm-3">
                            <input type="text" class="form-control" value="${item.fieldLabel}" disabled/>
                            <input type="hidden" name="fieldLabel" value="${item.fieldLabel}">
                        </div>
                        <div class="col-sm-2">
                            <input type="text" class="form-control" name="fieldTypeText" value="${item.fieldTypeText}" disabled/>
                            <input type="hidden" name="fieldType" value="${item.fieldType}">
                        </div>
                        <div class="col-sm-2">
                            <input type="text" class="form-control" value="${item.isRequired}" disabled/>
                            <input type="hidden" name="isRequired" value="${item.isRequired}">
                        </div>
                        <div class="col-sm-2 d-flex justify-content-around align-items-center">
                            <button class="btn btn-danger btnRemoveField">Remove</button>
                            <button class="btn btn-secondary btnMoveUp">↑</button>
                            <button class="btn btn-secondary btnMoveDown">↓</button>
                        </div>
                    </div>
                    `;

                    // Append the new row to the container
                    $('#dynamicRowsContainer').append(newRow);
                });
            }
            
        } catch (e) {
            Failed(e);
        }
    }

    const GetTimeZoneSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-timezone-Settings/${types}`);
            
            const timezoneSelect = document.getElementById('timezone_value');

            response.timezones.forEach(timezone => {
                const option = document.createElement('option');
                option.value = timezone.Id; 
                option.text = timezone.DisplayName; 
                timezoneSelect.appendChild(option);
            });

            data = response.settings.find(function (element) {
                return element.type == 'timezone';
            });
            $('#timezone_value').val(data.value);

        } catch (e) {
            Failed(e);
        }
    }

    const GetHomePageSettings = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-home-Settings/${types}`);

            const fieldFlashDealSelect = document.getElementById('fieldFlashDeal');
            const option1 = document.createElement('option');
            option1.value = 0;
            option1.text = 'Select One';
            fieldFlashDealSelect.appendChild(option1);

            const fieldCategorySelect = document.getElementById('fieldCategory');
            option2 = document.createElement('option');
            option2.value = 0;
            option2.text = 'Select One';
            fieldCategorySelect.appendChild(option2);

            response.flashDeals.forEach(fd => {
                const option = document.createElement('option');
                option.value = fd.flashDealId;
                option.text = fd.title;
                fieldFlashDealSelect.appendChild(option);
            });
            response.categories.forEach(cat => {
                const option = document.createElement('option');
                option.value = cat.categoryId;
                option.text = cat.name;
                fieldCategorySelect.appendChild(option);
            });

            ['.sectionNoOfProd', '.sectionFD', '.sectionCat'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = 'none';
                });
            });

            data = response.settings.find(function (element) {
                return element.type == 'home_page_config';
            });
            let settingsData = data.value;
            let jsonData = typeof settingsData === "string" ? JSON.parse(settingsData) : settingsData;

            var headerRow = `
                <div class="row mt-2 font-weight-bold">
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Section" disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Title" disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Subtitle" disabled/></div>
                    <div class="col-sm-1"><input type="text" class="form-control font-weight-bold text-center" value="No." disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Flash Deal" disabled/></div>
                    <div class="col-sm-1"><input type="text" class="form-control font-weight-bold text-center" value="Category" disabled/></div>
                    <div class="col-sm-2"><input type="text" class="form-control font-weight-bold text-center" value="Actions" disabled/></div>
                </div>
            `;

            if ($('#dynamicRowsContainerHomePage .font-weight-bold').length === 0) {
                $('#dynamicRowsContainerHomePage').append(headerRow);
            }

            if (jsonData) {
                jsonData.forEach(item => {
                    var newRow = `
                    <div class="row mt-2 dynamic-row-home">
                        <div class="col-sm-2">
                            <input type="text" class="form-control" name="fieldSectionText" value="${item.sectionText}" disabled/>
                            <input type="hidden" name="fieldSection" value="${item.section}">
                        </div>
                        <div class="col-sm-2">
                            <input type="text" class="form-control" value="${item.title}" disabled/>
                            <input type="hidden" name="fieldTitle" value="${item.title}">
                        </div>
                        <div class="col-sm-2">
                            <input type="text" class="form-control" value="${item.subtitle}" disabled/>
                            <input type="hidden" name="fieldSubtitle" value="${item.subtitle}">
                        </div>
                        <div class="col-sm-1">
                            <input type="text" class="form-control" value="${item.noOfProduct}" disabled/>
                            <input type="hidden" name="fieldNoOfProduct" value="${item.noOfProduct}">
                        </div>
                        <div class="col-sm-2">
                            <input type="text" class="form-control" name="fieldFlashDealText" value="${item.flashDealText}" disabled/>
                            <input type="hidden" name="fieldFlashDeal" value="${item.flashDeal}">
                        </div>
                        <div class="col-sm-1">
                            <input type="text" class="form-control" name="fieldCategoryText" value="${item.categoryText}" disabled/>
                            <input type="hidden" name="fieldCategory" value="${item.category}">
                        </div>
                        <div class="col-sm-2 d-flex justify-content-around align-items-center">
                            <button class="btn btn-danger btnRemoveFieldHomePage"><i class="fa fa-window-close"></i></button>
                            <button class="btn btn-secondary btnMoveUpHomePage">↑</button>
                            <button class="btn btn-secondary btnMoveDownHomePage">↓</button>
                        </div>
                    </div>
                    `;

                    $('#dynamicRowsContainerHomePage').append(newRow);
                });
            }
            

        } catch (e) {
            Failed(e);
        }
    }

    function loadImages(imageList, imagePreview, width) {
        const imageContainer = $("#" + imagePreview);
        imageContainer.empty(); 

        const baseUrl = window.location.origin;
        imageList.forEach(image => {
            const imgElement = `<img src="${baseUrl}${image.text}" alt="Image" style="max-width: ${width}px; margin: 5px;" />`;
            imageContainer.append(imgElement);
        });
    }


    CommonInitializer();

    $(document).ready(function () {
        GetProductSettings(prodTypes);
        GetImageSettings(images);
        GetSettings(types);
        GetCurrencySettings(currencyTypes);
        GetMailingSettings(mailingTypes);
        GetAddressSettings(addressTypes);
        GetTimeZoneSettings(timezoneTypes);
        GetHomePageSettings(homepageTypes);
    });

    $(document).on("click", modalPreviewSelector.addFilesToBrowse, function () {
        modalPreview.ImageSetter(counter);
        modalPreview.PopulateImageLoader(willPreviewDiv, counter, imageSource);
        modalOperation.ModalHide(modalPreviewSelector.uploaderModal);
    });

    $("#carouselImageInp").change(function () {
        const files = $("#carouselImageInp")[0].files;
        const previewContainer = $("#carouselImagePreview");

        previewContainer.empty();

        Array.from(files).forEach(file => {
            if (file) {
                const img = $('<img>', {
                    src: URL.createObjectURL(file),
                    alt: file.name,
                    class: 'preview-image', 
                    style: 'max-width: 200px; margin: 5px;'
                });
                previewContainer.append(img);
            }
        });
    });

    $("#paymentImageInp").change(function () {
        const files = $("#paymentImageInp")[0].files;
        const previewContainer = $("#paymentImagePreview");

        previewContainer.empty();

        Array.from(files).forEach(file => {
            if (file) {
                const img = $('<img>', {
                    src: URL.createObjectURL(file),
                    alt: file.name,
                    class: 'preview-image',
                    style: 'max-width: 100px; margin: 5px;'
                });
                previewContainer.append(img);
            }
        });
    });

    $("#banner1ImageInp").change(function () {
        const files = $("#banner1ImageInp")[0].files;
        const previewContainer = $("#banner1ImagePreview");

        previewContainer.empty();

        Array.from(files).forEach(file => {
            if (file) {
                const img = $('<img>', {
                    src: URL.createObjectURL(file),
                    alt: file.name,
                    class: 'preview-image',
                    style: 'max-width: 200px; margin: 5px;'
                });
                previewContainer.append(img);
            }
        });
    });

    $("#banner2ImageInp").change(function () {
        const files = $("#banner2ImageInp")[0].files;
        const previewContainer = $("#banner2ImagePreview");

        previewContainer.empty();

        Array.from(files).forEach(file => {
            if (file) {
                const img = $('<img>', {
                    src: URL.createObjectURL(file),
                    alt: file.name,
                    class: 'preview-image',
                    style: 'max-width: 200px; margin: 5px;'
                });
                previewContainer.append(img);
            }
        });
    });

    $("#banner3ImageInp").change(function () {
        const files = $("#banner3ImageInp")[0].files;
        const previewContainer = $("#banner3ImagePreview");

        previewContainer.empty();

        Array.from(files).forEach(file => {
            if (file) {
                const img = $('<img>', {
                    src: URL.createObjectURL(file),
                    alt: file.name,
                    class: 'preview-image',
                    style: 'max-width: 200px; margin: 5px;'
                });
                previewContainer.append(img);
            }
        });
    });


    formCarouselImage.find("#btnSave").click(function () {
        const files = $("#carouselImageInp")[0].files;
        if (files.length === 0) {
            Failed("Please select at least one image.");
            return;
        }
        SaveImages(formCarouselImage.find("input[name=setting]").val(), files, formCarouselImage);
    });

    formpaymentImage.find("#btnSave").click(function () {
        const files = $("#paymentImageInp")[0].files;
        if (files.length === 0) {
            Failed("Please select at least one image.");
            return;
        }
        SaveImages(formpaymentImage.find("input[name=setting]").val(), files, formpaymentImage);
    });

    formHomeBannerSetup.find("#btnSave").click(function () {
        const files1 = $("#banner1ImageInp")[0].files;
        const files2 = $("#banner2ImageInp")[0].files;
        const files3 = $("#banner3ImageInp")[0].files;

        SaveBannerImages(files1, files2, files3);

    });

    formCopywrite.find("#btnSave").click(function () {
        Save(formCopywrite.find("input[name=setting]").val(), formCopywrite.find("#site_copywrite_text_value").val(), formCopywrite);
    });

    formCurrency.find("#btnSave").click(function () {
        let models = [];
        models.push({
            Type: formCurrency.find("input[name=system_default_currency_name]").val(),
            Value: formCurrency.find("#system_default_currency_name_value").val()
        });
        models.push({
            Type: formCurrency.find("input[name=system_default_currency]").val(),
            Value: formCurrency.find("#system_default_currency_value").val()
        });
        models.push({
            Type: formCurrency.find("input[name=currency_position]").val(),
            Value: formCurrency.find("#currency_position_value").val()
        });
        models.push({
            Type: formCurrency.find("input[name=amount_format]").val(),
            Value: formCurrency.find("#amount_format_value").val()
        });
        SaveMultiple(models, formCurrency);
    });

    formMailing.find("#btnSave").click(function () {
        let models = [];
        models.push({
            Type: formMailing.find("input[name=admin_mailing_address]").val(),
            Value: formMailing.find("#admin_mailing_address_value").val()
        });
        models.push({
            Type: formMailing.find("input[name=send_mail_to_admin_after_order]").val(),
            Value: formMailing.find('#send_mail_to_admin_after_order_value').prop('checked')
        });
        models.push({
            Type: formMailing.find("input[name=send_mail_to_customer_after_order]").val(),
            Value: formMailing.find('#send_mail_to_customer_after_order_value').prop('checked')
        });
        models.push({
            Type: formMailing.find("input[name=send_mail_after_change_order_status]").val(),
            Value: formMailing.find('#send_mail_after_change_order_status_value').prop('checked')
        });
        SaveMultiple(models, formMailing);
    });

    formProductConfiguration.find("#btnSave").click(function () {
        let models = [];
        models.push({
            Type: formProductConfiguration.find("input[name=shipping_configuration_enabled]").val(),
            Value: formProductConfiguration.find('#shipping_configuration_enabled_value').prop('checked')
        });
        models.push({
            Type: formProductConfiguration.find("input[name=flash_deal_enabled]").val(),
            Value: formProductConfiguration.find('#flash_deal_enabled_value').prop('checked')
        });
        models.push({
            Type: formProductConfiguration.find("input[name=stock_visibility_enabled]").val(),
            Value: formProductConfiguration.find('#stock_visibility_enabled_value').prop('checked')
        });
        models.push({
            Type: formProductConfiguration.find("input[name=vat_tax_enabled]").val(),
            Value: formProductConfiguration.find('#vat_tax_enabled_value').prop('checked')
        });
        SaveMultiple(models, formProductConfiguration);
    });

    formAddress.find('#btnAddField').click(function () {
        var field = $('#fieldAddress').val();
        var fieldText = $('#fieldAddress option:selected').text();
        if (!field) {
            Failed("Please select a field.");
            return;
        }
        var fieldLabel = $('#fieldLabel').val();
        if (!fieldLabel) {
            Failed("Please enter field label.");
            return;
        }
        var fieldType = $('#fieldType').val();
        var fieldTypeText = $('#fieldType option:selected').text();
        if (!fieldType) {
            Failed("Please enter field type.");
            return;
        }
        var isRequired = $('#IsFieldRequired').is(':checked') ? 'Yes' : 'No';

        var exists = false;
        $('#dynamicRowsContainer .dynamic-row').each(function () {
            if ($(this).find('input[name="field"]').val() === field) {
                exists = true;
                return false;
            }
        });

        if (exists) {
            Failed("This field has already been added.");
            return;
        }

        var newRow = `
            <div class="row mt-2 dynamic-row">
                <div class="col-sm-3">
                    <input type="text" class="form-control" name="fieldText" value="${fieldText}" disabled/>
                    <input type="hidden" name="field" value="${field}">
                </div>
                <div class="col-sm-3">
                    <input type="text" class="form-control" value="${fieldLabel}" disabled/>
                    <input type="hidden" name="fieldLabel" value="${fieldLabel}">
                </div>
                <div class="col-sm-2">
                    <input type="text" class="form-control" name="fieldTypeText" value="${fieldTypeText}" disabled/>
                    <input type="hidden" name="fieldType" value="${fieldType}">
                </div>
                <div class="col-sm-2">
                    <input type="text" class="form-control" value="${isRequired}" disabled/>
                    <input type="hidden" name="isRequired" value="${isRequired}">
                </div>
                <div class="col-sm-2 d-flex justify-content-around align-items-center">
                    <button class="btn btn-danger btnRemoveField">Remove</button>
                    <button class="btn btn-secondary btnMoveUp">↑</button>
                    <button class="btn btn-secondary btnMoveDown">↓</button>
                </div>
            </div>
        `;

        $('#dynamicRowsContainer').append(newRow);

        // Reset the form fields
        $('#fieldAddress').val('Name');
        $('#fieldLabel').val('');
        $('#fieldType').val('Text');
        $('#IsFieldRequired').prop('checked', false).trigger('change');
    });

    $(document).on('click', '.btnRemoveField', function () {
        $(this).closest('.row').remove();
    });

    $(document).on('click', '.btnMoveUp', function (event) {
        event.preventDefault();
        var row = $(this).closest('.dynamic-row');
        var prevRow = row.prev('.dynamic-row');

        if (prevRow.length) {
            row.insertBefore(prevRow).hide().slideDown();
        }
    });

    $(document).on('click', '.btnMoveDown', function (event) {
        event.preventDefault();
        var row = $(this).closest('.dynamic-row');
        var nextRow = row.next('.dynamic-row');

        if (nextRow.length) {
            row.insertAfter(nextRow).hide().slideDown();
        }
    });

    formAddress.find('#btnSave').click(function () {
        var fieldsArray = [];
        $('#dynamicRowsContainer .dynamic-row').each(function () {
            var field = $(this).find('input[name="field"]').val();
            var fieldText = $(this).find('input[name="fieldText"]').val();
            var fieldLabel = $(this).find('input[name="fieldLabel"]').val();
            var fieldType = $(this).find('input[name="fieldType"]').val();
            var fieldTypeText = $(this).find('input[name="fieldTypeText"]').val();
            var isRequired = $(this).find('input[name="isRequired"]').val();

            fieldsArray.push({
                "field": field,
                "fieldName": fieldText,
                "fieldLabel": fieldLabel,
                "fieldType": fieldType,
                "fieldTypeText": fieldTypeText,
                "isRequired": isRequired
            });
        });

        var jsonResult = JSON.stringify(fieldsArray, null, 4);

        let model = {
            Type: 'address_config',
            Value: jsonResult
        }
        SaveModel(model);
    });

    formTimezone.find('#btnSave').click(function () {
        let model = {
            Type: 'timezone',
            Value: $('#timezone_value').val()
        }
        SaveModel(model);
    });

    formPaymentMode.find("#btnSave").click(function () {
        Save(formPaymentMode.find("input[name=setting]").val(), formPaymentMode.find("#payment_mode_value").val(), formPaymentMode);
    });

    formMaintenanceMode.find("#btnSave").click(function () {
        let models = [];
        models.push({
            Type: formMaintenanceMode.find("input[name=maintenance_mode]").val(),
            Value: formMaintenanceMode.find('#maintenance_mode_value').prop('checked')
        });
        models.push({
            Type: formMaintenanceMode.find("input[name=maintenance_time]").val(),
            Value: new Date(formMaintenanceMode.find('#maintenance_time_value').val())
        });
        SaveMultiple(models, formMaintenanceMode);
    });

    formHomePage.find('#fieldSection').on('change', function () {
        // Reset the form fields
        $('#fieldTitle').val('');
        $('#fieldSubtitle').val('');
        $('#fieldNoOfProduct').val(0);
        $('#fieldFlashDeal').val('0');
        $('#fieldCategory').val('0');

        const selectedValue = $(this).val();
        if (selectedValue == 'FlashDealProducts') {
            ['.sectionCat'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = 'none';
                });
            });

            ['.sectionNoOfProd', '.sectionFD'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = '';
                });
            });
        }
        else if (selectedValue == 'TodaysDealProducts' || selectedValue == 'TrendingProducts' || selectedValue == 'DiscountProducts' || selectedValue == 'FeaturedProducts') {
            ['.sectionCat', '.sectionFD'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = 'none';
                });
            });

            ['.sectionNoOfProd'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = '';
                });
            });
        }
        else if (selectedValue == 'AllCategory' || selectedValue == 'CustomerReview' || selectedValue == 'Subscription') {
            ['.sectionCat', '.sectionFD', '.sectionNoOfProd'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = 'none';
                });
            });
        }
        else if (selectedValue == 'ProductsByCategory' || selectedValue == 'Banner1' || selectedValue == 'Banner2' || selectedValue == 'Banner3') {
            ['.sectionFD', '.sectionNoOfProd'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = 'none';
                });
            });
            ['.sectionCat'].forEach(className => {
                document.querySelectorAll(className).forEach(element => {
                    element.style.display = '';
                });
            });
        }
    });

    formHomePage.find('#btnAddSection').click(function () {
        var fieldSection = $('#fieldSection').val();
        var fieldSectionText = $('#fieldSection option:selected').text();
        if (!fieldSection) {
            Failed("Please select a section.");
            return;
        }
        var fieldTitle = $('#fieldTitle').val();
        if (!fieldTitle) {
            Failed("Please enter title.");
            return;
        }
        var fieldSubtitle = $('#fieldSubtitle').val();
        var fieldNoOfProduct = $('#fieldNoOfProduct').val();
        const selectedValue = formHomePage.find('#fieldSection').val();
        if (selectedValue == 'FlashDealProducts') {
            if (formHomePage.find('#fieldFlashDeal').val() == 0) {
                Failed("Please select a flash deal.");
                return;
            }
        }
        else if (selectedValue == 'ProductsByCategory' || selectedValue == 'Banner1' || selectedValue == 'Banner2' || selectedValue == 'Banner3') {
            if (formHomePage.find('#fieldCategory').val() == 0) {
                Failed("Please select a category.");
                return;
            }
        }

        var fieldFlashDeal = $('#fieldFlashDeal').val();
        var fieldFlashDealText = $('#fieldFlashDeal option:selected').text();
        if (fieldFlashDealText == "Select One") fieldFlashDealText = "";

        var fieldCategory = $('#fieldCategory').val();
        var fieldCategoryText = $('#fieldCategory option:selected').text();
        if (fieldCategoryText == "Select One") fieldCategoryText = "";
        
        var newRow = `
            <div class="row mt-2 dynamic-row-home">
                <div class="col-sm-2">
                    <input type="text" class="form-control" name="fieldSectionText" value="${fieldSectionText}" disabled/>
                    <input type="hidden" name="fieldSection" value="${fieldSection}">
                </div>
                <div class="col-sm-2">
                    <input type="text" class="form-control" value="${fieldTitle}" disabled/>
                    <input type="hidden" name="fieldTitle" value="${fieldTitle}">
                </div>
                <div class="col-sm-2">
                    <input type="text" class="form-control" value="${fieldSubtitle}" disabled/>
                    <input type="hidden" name="fieldSubtitle" value="${fieldSubtitle}">
                </div>
                <div class="col-sm-1">
                    <input type="text" class="form-control" value="${fieldNoOfProduct}" disabled/>
                    <input type="hidden" name="fieldNoOfProduct" value="${fieldNoOfProduct}">
                </div>
                <div class="col-sm-2">
                    <input type="text" class="form-control" name="fieldFlashDealText" value="${fieldFlashDealText}" disabled/>
                    <input type="hidden" name="fieldFlashDeal" value="${fieldFlashDeal}">
                </div>
                <div class="col-sm-1">
                    <input type="text" class="form-control" name="fieldCategoryText" value="${fieldCategoryText}" disabled/>
                    <input type="hidden" name="fieldCategory" value="${fieldCategory}">
                </div>
                <div class="col-sm-2 d-flex justify-content-around align-items-center">
                    <button class="btn btn-danger btnRemoveFieldHomePage"><i class="fa fa-window-close"></i></button>
                    <button class="btn btn-secondary btnMoveUpHomePage">↑</button>
                    <button class="btn btn-secondary btnMoveDownHomePage">↓</button>
                </div>
            </div>
        `;

        $('#dynamicRowsContainerHomePage').append(newRow);

        // Reset the form fields
        $('#fieldSection').val('');
        $('#fieldTitle').val('');
        $('#fieldSubtitle').val('');
        $('#fieldNoOfProduct').val(0);
        $('#fieldFlashDeal').val('0');
        $('#fieldCategory').val('0');
    });

    $(document).on('click', '.btnRemoveFieldHomePage', function () {
        $(this).closest('.row').remove();
    });

    $(document).on('click', '.btnMoveUpHomePage', function (event) {
        event.preventDefault();
        var row = $(this).closest('.dynamic-row-home');
        var prevRow = row.prev('.dynamic-row-home');

        if (prevRow.length) {
            row.insertBefore(prevRow).hide().slideDown();
        }
    });

    $(document).on('click', '.btnMoveDownHomePage', function (event) {
        event.preventDefault();
        var row = $(this).closest('.dynamic-row-home');
        var nextRow = row.next('.dynamic-row-home');

        if (nextRow.length) {
            row.insertAfter(nextRow).hide().slideDown();
        }
    });

    formHomePage.find('#btnSave').click(function () {
        var fieldsArray = [];
        $('#dynamicRowsContainerHomePage .dynamic-row-home').each(function () {
            var fieldSection = $(this).find('input[name="fieldSection"]').val();
            var fieldSectionText = $(this).find('input[name="fieldSectionText"]').val();
            var fieldTitle = $(this).find('input[name="fieldTitle"]').val();
            var fieldSubtitle = $(this).find('input[name="fieldSubtitle"]').val();
            var fieldNoOfProduct = $(this).find('input[name="fieldNoOfProduct"]').val();
            var fieldFlashDeal = $(this).find('input[name="fieldFlashDeal"]').val();
            var fieldFlashDealText = $(this).find('input[name="fieldFlashDealText"]').val();
            var fieldCategory = $(this).find('input[name="fieldCategory"]').val();
            var fieldCategoryText = $(this).find('input[name="fieldCategoryText"]').val();
            

            fieldsArray.push({
                "section": fieldSection,
                "sectionText": fieldSectionText,
                "title": fieldTitle,
                "subtitle": fieldSubtitle,
                "noOfProduct": fieldNoOfProduct,
                "flashDeal": fieldFlashDeal,
                "flashDealText": fieldFlashDealText,
                "category": fieldCategory,
                "categoryText": fieldCategoryText
            });
        });

        var jsonResult = JSON.stringify(fieldsArray, null, 4);

        let model = {
            Type: 'home_page_config',
            Value: jsonResult
        }
        SaveModel(model);
    });


})();