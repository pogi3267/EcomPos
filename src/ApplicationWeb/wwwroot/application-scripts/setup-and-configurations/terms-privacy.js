(function () {
    const types = 'terms_condition,privacy_policy';
    let formTermsCondition, formPrivacyPolicy;
    const CommonInitializer = function () {
        formTermsCondition = $("#formTermsCondition");
        formPrivacyPolicy = $("#formPrivacyPolicy");
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

    const GetPaymentMethods = async types => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/BusinessSetting/Get-SmtpSettings/${types}`);
            
            var data = response.find(function (element) {
                return element.type == 'terms_condition';
            });
            $("#terms_conditionDescription").summernote('code', data.value);

            data = response.find(function (element) {
                return element.type == 'privacy_policy';
            });
            $("#privacy_policyDescription").summernote('code', data.value);

        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GetPaymentMethods(types);

        $('#terms_conditionDescription,#privacy_policyDescription').summernote(
            {
                height: 230,
            }
        );
    });

    formTermsCondition.find("#btnSave").click(function () {
        Save(formTermsCondition.find("input[name=policy_setup]").val(), formTermsCondition.find("#terms_conditionDescription").val(), formTermsCondition);
    });

    formPrivacyPolicy.find("#btnSave").click(function () {
        Save(formPrivacyPolicy.find("input[name=policy_setup]").val(), formPrivacyPolicy.find("#privacy_policyDescription").val(), formPrivacyPolicy);
    });

})();