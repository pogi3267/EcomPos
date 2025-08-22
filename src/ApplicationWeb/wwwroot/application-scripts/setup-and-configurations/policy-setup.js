(function () {
    const types = 'free_shipping_note,secure_payment_note,money_return_note,support_note,delivery_note';
    let formFreeShippingDescription, formSecurePaymentDescription, formMoneyReturnDescription, formSupportDescription, formDaysDeliveryDescription;
    const CommonInitializer = function () {
        formFreeShippingDescription = $("#formFreeShippingDescription");
        formSecurePaymentDescription = $("#formSecurePaymentDescription");
        formMoneyReturnDescription = $("#formMoneyReturnDescription");
        formSupportDescription = $("#formSupportDescription");
        formDaysDeliveryDescription = $("#formDaysDeliveryDescription");
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

    const SetData = async (response, formEl, type) => {
        let model = null;
        if (response) model = JSON.parse(response.value);
        if (!model || typeof (model) != 'object') {
            model = {};
            model["payment_method"] = type;
        }
        ResetForm(formEl);
        setFormData(formEl, model);
    }

    const GetPaymentMethods = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-SmtpSettings/${types}`);
            
            var data = response.find(function (element) {
                return element.type == 'free_shipping_note';
            });
            $("#freeShippingDescription").summernote('code', data.value);

            data = response.find(function (element) {
                return element.type == 'secure_payment_note';
            });
            $("#securePaymentDescription").summernote('code', data.value);

            data = response.find(function (element) {
                return element.type == 'money_return_note';
            });
            $("#moneyReturnDescription").summernote('code', data.value);

            data = response.find(function (element) {
                return element.type == 'support_note';
            });
            $("#supportDescription").summernote('code', data.value);

            data = response.find(function (element) {
                return element.type == 'delivery_note';
            });
            $("#deliveryDescription").summernote('code', data.value);

        } catch (e) {
            Failed(e);
        }
    }

    const GetOnePaymentMethod = async (type, formEl) => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-SmtpSetting/${type}`);
            let model = null;
            if (response) model = JSON.parse(response.value);
            if (!model || typeof (model) != 'object') {
                model = {};
                model["payment_method"] = type;
            }
            setFormData(formEl, model);
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GetPaymentMethods(types);

        $('#freeShippingDescription,#securePaymentDescription,#moneyReturnDescription,#supportDescription,#deliveryDescription').summernote(
            {
                height: 230,
            }
        );
    });

    formFreeShippingDescription.find("#btnSave").click(function () {
        Save(formFreeShippingDescription.find("input[name=policy_setup]").val(), formFreeShippingDescription.find("#freeShippingDescription").val(), formFreeShippingDescription);
    });

    formSecurePaymentDescription.find("#btnSave").click(function () {
        Save(formSecurePaymentDescription.find("input[name=policy_setup]").val(), formSecurePaymentDescription.find("#securePaymentDescription").val(), formSecurePaymentDescription);
    });

    formMoneyReturnDescription.find("#btnSave").click(function () {
        Save(formMoneyReturnDescription.find("input[name=policy_setup]").val(), formMoneyReturnDescription.find("#moneyReturnDescription").val(), formMoneyReturnDescription);
    });

    formSupportDescription.find("#btnSave").click(function () {
        Save(formSupportDescription.find("input[name=policy_setup]").val(), formSupportDescription.find("#supportDescription").val(), formSupportDescription);
    });

    formDaysDeliveryDescription.find("#btnSave").click(function () {
        Save(formDaysDeliveryDescription.find("input[name=policy_setup]").val(), formDaysDeliveryDescription.find("#deliveryDescription").val(), formDaysDeliveryDescription);
    });

})();