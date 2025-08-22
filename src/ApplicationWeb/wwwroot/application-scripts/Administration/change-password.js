
(function () {
    Elements = ["Password,ConfirmPassword"]; // form validation id only

    // urls
    const changeUrl = "/api/Authenticate/change-password";

    // success message
    const saveMessage = "Saved Succcessfully!";

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                await ajaxOperation.SavePostAjax(changeUrl, model);
                Success(saveMessage);
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {

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