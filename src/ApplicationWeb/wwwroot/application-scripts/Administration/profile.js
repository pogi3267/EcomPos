
(function () {
    Elements = ["Name"]; // form validation id only

    // urls
    const saveUrl = "/api/Authenticate/update-profile";
    const getUrl = "/api/Authenticate/get";

    // success message
    const saveMessage = "Saved Succcessfully!";

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                await ajaxOperation.SavePostAjax(saveUrl, model);
                Success(saveMessage);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Get = async id => {
        try {
            const url = getUrl + "/" + id;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) == "object") {
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        Get(formEl.find('#Id').val());
    });
    
    $(SetValidatorElement(Elements)).keyup(function () {
        Elements.forEach(item => {
            if ($(item).val() != '') {
                $("#" + item).removeClass("is-invalid");
                $("." + item).hide();
            }
        });
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

})();