(function () {
    const types = 'paypal_payment,stripe_payment,mercadopago_payment,bkash,nagad,sslcommerz_payment,aamarpay_payment,iyzico,instamojo_payment,paystack,payhere,ngenius,voguepay,razorpay';
    let formElPaypal, formElStripe, formElMercadopago, formElBkash, formElNagad, formElSslcommerz, formElAamarpay, formElIyzico,
        formElInstamojo, formElPayStack, formElPayhere, formElNgenius, formElVoguePay, formElRazorPay;
    const CommonInitializer = function () {
        formElPaypal = $("#formPaypal");
        formElStripe = $("#formStripe");
        formElMercadopago = $("#formMercadopago");
        formElBkash = $("#formBkash");
        formElNagad = $("#formNagad");
        formElSslcommerz = $("#formSslcommerz");
        formElAamarpay = $("#formAamarpay");
        formElIyzico = $("#formIyzico");
        formElInstamojo = $("#formInstamojo");
        formElPayStack = $("#formPayStack");
        formElPayhere = $("#formPayhere");
        formElNgenius = $("#formNgenius");
        formElVoguePay = $("#formVoguePay");
        formElRazorPay = $("#formRazorPay");
    }

    const Save = async (type, formEl) => {
        try {
            let model = {
                Type: type,
                Value: JSON.stringify(formDataToJson(formEl.serializeArray()))
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
            
            var iyzicoData = response.find(function (element) {
                return element.type == 'iyzico';
            });
            SetData(iyzicoData, formElIyzico, 'iyzico');

            var paypalData = response.find(function (element) {
                return element.type == 'paypal_payment';
            });
            SetData(paypalData, formElPaypal, 'paypal_payment');

            var stripeData = response.find(function (element) {
                return element.type == 'stripe_payment';
            });
            SetData(stripeData, formElStripe, 'stripe_payment');

            var mercadopagoData = response.find(function (element) {
                return element.type == 'mercadopago_payment';
            });
            SetData(mercadopagoData, formElMercadopago, 'mercadopago_payment');
            
            var bkashData = response.find(function (element) {
                return element.type == 'bkash';
            });
            SetData(bkashData, formElBkash, 'bkash');
            
            var nagadData = response.find(function (element) {
                return element.type == 'nagad';
            });
            SetData(nagadData, formElNagad, 'nagad');
            
            var sslcommerzData = response.find(function (element) {
                return element.type == 'sslcommerz_payment';
            });
            SetData(sslcommerzData, formElSslcommerz, 'sslcommerz_payment');
            
            var aamarpayData = response.find(function (element) {
                return element.type == 'aamarpay_payment';
            });
            SetData(aamarpayData, formElAamarpay, 'aamarpay_payment');
            
            var instamojoData = response.find(function (element) {
                return element.type == 'instamojo_payment';
            });
            SetData(instamojoData, formElInstamojo, 'instamojo_payment');
            
            var paystackData = response.find(function (element) {
                return element.type == 'paystack';
            });
            SetData(paystackData, formElPayStack, 'paystack');
            
            var payhereData = response.find(function (element) {
                return element.type == 'payhere';
            });
            SetData(payhereData, formElPayhere, 'payhere');
            
            var ngeniusData = response.find(function (element) {
                return element.type == 'ngenius';
            });
            SetData(ngeniusData, formElNgenius, 'ngenius');
            
            var voguepayData = response.find(function (element) {
                return element.type == 'voguepay';
            });
            SetData(voguepayData, formElVoguePay, 'voguepay');
            
            var razorpayData = response.find(function (element) {
                return element.type == 'razorpay';
            });
            SetData(razorpayData, formElRazorPay, 'razorpay');

        } catch (e) {
            Failed(e);
        }
    }

    const GetOnePaymentMethod = async(type, formEl) => {
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
    });

    formElPaypal.find("#btnSave").click(function () {
        if (formElPaypal.find("#paypalSandboxMode:checkbox:checked").length) {
            Save("paypal_sandbox", formElPaypal);
        } else {
            Save(formElPaypal.find("input[name=payment_method]").val(), formElPaypal);
        }
    });

    formElStripe.find("#btnSave").click(function () {
        Save(formElStripe.find("input[name=payment_method]").val(), formElStripe);
    });
    
    formElMercadopago.find("#btnSave").click(function () {
        Save(formElMercadopago.find("input[name=payment_method]").val(), formElMercadopago);
    });
    
    formElBkash.find("#btnSave").click(function () {
        if (formElBkash.find("#bkashSandboxMode:checkbox:checked").length) {
            Save("bkash_sandbox", formElBkash);
        } else {
            Save(formElBkash.find("input[name=payment_method]").val(), formElBkash);
        }
    });
    
    formElNagad.find("#btnSave").click(function () {
        Save(formElNagad.find("input[name=payment_method]").val(), formElNagad);
    });
    
    formElSslcommerz.find("#btnSave").click(function () {
        if (formElSslcommerz.find("#sslcommerzSandboxMode:checkbox:checked").length) {
            Save("sslcommerz_sandbox", formElSslcommerz);
        } else {
            Save(formElSslcommerz.find("input[name=payment_method]").val(), formElSslcommerz);
        }
    });
    
    formElAamarpay.find("#btnSave").click(function () {
        if (formElAamarpay.find("#aamarpaySandboxMode:checkbox:checked").length) {
            Save("aamarpay_sandbox", formElAamarpay);
        } else {
            Save(formElAamarpay.find("input[name=payment_method]").val(), formElAamarpay);
        }
    });
    
    formElIyzico.find("#btnSave").click(function () {
        if (formElIyzico.find("#iyzicoSandboxMode:checkbox:checked").length) {
            Save("iyzico_sandbox", formElIyzico);
        } else {
            Save(formElIyzico.find("input[name=payment_method]").val(), formElIyzico);
        }
    });
    
    formElInstamojo.find("#btnSave").click(function () {
        if (formElInstamojo.find("#instamojoSandboxMode:checkbox:checked").length) {
            Save("instamojo_sandbox", formElInstamojo);
        } else {
            Save(formElInstamojo.find("input[name=payment_method]").val(), formElInstamojo);
        }
    });
    
    formElPayStack.find("#btnSave").click(function () {
        Save(formElPayStack.find("input[name=payment_method]").val(), formElPayStack);
    });
    
    formElPayhere.find("#btnSave").click(function () {
        if (formElPayhere.find("#payhereSandboxMode:checkbox:checked").length) {
            Save("payhere_sandbox", formElPayhere);
        } else {
            Save(formElPayhere.find("input[name=payment_method]").val(), formElPayhere);
        }
    });
    
    formElNgenius.find("#btnSave").click(function () {
        Save(formElNgenius.find("input[name=payment_method]").val(), formElNgenius);
    });
    
    formElVoguePay.find("#btnSave").click(function () {
        if (formElVoguePay.find("#voguePaySandboxMode:checkbox:checked").length) {
            Save("voguepay_sandbox", formElVoguePay);
        } else {
            Save(formElVoguePay.find("input[name=payment_method]").val(), formElVoguePay);
        }
    });
    
    formElRazorPay.find("#btnSave").click(function () {
        Save(formElRazorPay.find("input[name=payment_method]").val(), formElRazorPay);
    });


    formElPaypal.find("#paypalSandboxMode").click(function () {
        if (formElPaypal.find("#paypalSandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("paypal_sandbox", formElPaypal)
        }
        else {
            GetOnePaymentMethod("paypal_payment", formElPaypal)
        }
    });

    formElBkash.find("#bkashSandboxMode").click(function () {
        if (formElBkash.find("#bkashSandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("bkash_sandbox", formElBkash)
        }
        else {
            GetOnePaymentMethod("bkash", formElBkash)
        }
    });

    formElSslcommerz.find("#sslcommerzSandboxMode").click(function () {
        if (formElSslcommerz.find("#sslcommerzSandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("sslcommerz_sandbox", formElSslcommerz)
        }
        else {
            GetOnePaymentMethod("sslcommerz_payment", formElSslcommerz)
        }
    });

    formElAamarpay.find("#aamarpaySandboxMode").click(function () {
        if (formElAamarpay.find("#aamarpaySandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("aamarpay_sandbox", formElAamarpay)
        }
        else {
            GetOnePaymentMethod("aamarpay_payment", formElAamarpay)
        }
    });

    formElIyzico.find("#iyzicoSandboxMode").click(function () {
        if (formElIyzico.find("#iyzicoSandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("iyzico_sandbox", formElIyzico)
        }
        else {
            GetOnePaymentMethod("iyzico", formElIyzico)
        }
    });

    formElInstamojo.find("#instamojoSandboxMode").click(function () {
        if (formElInstamojo.find("#instamojoSandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("instamojo_sandbox", formElInstamojo)
        }
        else {
            GetOnePaymentMethod("instamojo_payment", formElInstamojo)
        }
    });

    formElPayhere.find("#payhereSandboxMode").click(function () {
        if (formElPayhere.find("#payhereSandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("payhere_sandbox", formElPayhere)
        }
        else {
            GetOnePaymentMethod("payhere", formElPayhere)
        }
    });

    formElVoguePay.find("#voguePaySandboxMode").click(function () {
        if (formElVoguePay.find("#voguePaySandboxMode:checkbox:checked").length) {
            GetOnePaymentMethod("voguepay_sandbox", formElVoguePay)
        }
        else {
            GetOnePaymentMethod("voguepay", formElVoguePay)
        }
    });


})();