(function () {
    const types = 'shipping_type,flat_rate_shipping_cost,shipping_cost_admin';
    let formIdShippingMethod = $("#formIdShippingMethod"), formIdFlatRate = $("#formIdFlatRate"), formIdAdminShipping = $("#formIdAdminShipping");
    Elements = [""]; // form validation id only

    // urls
    const saveUrl = "/api/BusinessSetting/Save";


    // success message
    const saveMessage = "Saved Succcessfully!";
    const updateMessage = "Updated Succcessfully!";

   

    const Save = async (formElSh) => {
        try {
            
                let model = formElToJson(formElSh);
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    GetShippingValue(types);
                }
            
        } catch (e) {
            Failed(e);
        }
    }


    const GetShippingValue = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-SmtpSettings/${types}`);
            if (typeof (response) == "object") {
                if ('product_wise_shipping' == response[0].value) {
                    $("#product_wise_shipping").attr("checked", true);
                }
                if ('flat_rate' == response[0].value) {
                    $("#flat_rate").attr("checked", true);
                }
                if ('area_wise_shipping' == response[0].value) {
                    $("#area_wise_shipping").attr("checked", true);
                }
                if ('flat_rate_shipping_cost' == response[1].type) {
                    $("#flat_rate_shipping_cost").val(response[1].value);
                }
                if ('shipping_cost_admin' == response[2].type) {
                    $("#shipping_cost_admin").val(response[2].value);
                }
                  
            }
        } catch (e) {
            Failed(e);
        }
    }


    CommonInitializer();

    $(document).ready(function () {
        GetShippingValue(types);
    });

    formIdShippingMethod.find("#btnSave").click(function () {
        Save(formIdShippingMethod);
    });
    formIdFlatRate.find("#btnSave").click(function () {
        Save(formIdFlatRate);
    });
    formIdAdminShipping.find("#btnSave").click(function () {
        Save(formIdAdminShipping);
    });

})();