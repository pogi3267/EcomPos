(function () {
    const types = 'google_login,facebook_login';
    let formElGoogle, formElFacebook;
    const CommonInitializer = function () {
        formElGoogle = $("#formGoogle");
        formElFacebook = $("#formFacebook");
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
            model["social_login"] = type;
        }
        ResetForm(formEl);
        setFormData(formEl, model);
    }

    const GetMethods = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-SmtpSettings/${types}`);
            
            var googleData = response.find(function (element) {
                return element.type == 'google_login';
            });
            SetData(googleData, formElGoogle, 'google_login');

            var fbData = response.find(function (element) {
                return element.type == 'facebook_login';
            });
            SetData(fbData, formElFacebook, 'facebook_login');

        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GetMethods(types);
    });

    formElGoogle.find("#btnSave").click(function () {
        Save(formElGoogle.find("input[name=social_login]").val(), formElGoogle);
    });

    formElFacebook.find("#btnSave").click(function () {
        Save(formElFacebook.find("input[name=social_login]").val(), formElFacebook);
    });


})();