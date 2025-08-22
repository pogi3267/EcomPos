(function () {
    ecomTable = null;
    Elements = ["Name", "Photo"]; // form validation id only

    // urls
    const saveUrl = "/api/GeneralSettings/Save";
    const getData = "/api/GeneralSettings/GetData";

    // success message
    const saveMessage = "General Settings Saved Succcessfully!";
    const updateMessage = "General Settings Updated Succcessfully!";

    // image laoder 
    const imageLoadingArray = [{ id: "#systemLogoWhiteImage", name: "systemLogoWhite" }, { id: "#systemLogoBlackImage", name: "systemLogoBlack" }, { id: "#loginPageBackgroundImage", name: "loginPageBackground" }]; // set image selector id and object name where the image link exist

    const Save = async () => {
        try
        {
            let formData = new FormData(formEl[0]);
            formData.append("SystemLogoWhiteImage", $("#SystemLogoWhiteImage").get(0));
            formData.append("SystemLogoBlackImage", $("#SystemLogoBlackImage").get(0));
            formData.append("LoginPageBackgroundImage", $("#LoginPageBackgroundImage").get(0));
            if (IsFrmValid(formEl, Elements)) {
                let response = await ajaxOperation.SaveAjax(saveUrl, formData);
                if (typeof (response) == "object")
                {
                    ResetForm(formEl);
                    RemoveLoadImages(imageLoadingArray);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    LoadData();
                }
            } 
        } catch (e) {
            Failed(e);
        }
    }

   
    const LoadData = async () => {
        try {
            const url = getData;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) == "object") {
                setFormData(formEl, response);
                LoadImages(imageLoadingArray, response);
            }
        } catch (e) {
            
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        LoadData();
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

    formEl.find("#btnCancel").click(function () {
        RemoveLoadImages(imageLoadingArray);
    });

   
    $("#SystemLogoWhiteImage").change(function () {
        const [file] = $("#SystemLogoWhiteImage")[0].files
        if (file) {
            $("#systemLogoWhiteImage").attr("src", URL.createObjectURL(file))
        }
    });

    $("#SystemLogoBlackImage").change(function () {
        const [file] = $("#SystemLogoBlackImage")[0].files
        if (file) {
            $("#systemLogoBlackImage").attr("src", URL.createObjectURL(file))
        }
    });
    $("#LoginPageBackgroundImage").change(function () {
        const [file] = $("#LoginPageBackgroundImage")[0].files
        if (file) {
            $("#loginPageBackgroundImage").attr("src", URL.createObjectURL(file))
        }
    });

})();