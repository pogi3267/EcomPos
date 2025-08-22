(function () {
    autoRiceTable = null;
    Elements = ["OperationalUserName", "OperationalUserCode", "OperationalUserType", "Unit"]; // form validation id only

    // urls
    const initialUrl = "/api/OperationalUser/GetInitial";
    const saveUrl = "/api/OperationalUser/Save";
    const deleteUrl = "/api/OperationalUser/Delete";
    const editUrl = "/api/OperationalUser/Edit";

    // success message
    const saveMessage = "Saved Successfully!";
    const updateMessage = "Updated Successfully!";
    const deleteMessage = "Deleted Successfully";

    CommonInitializer();

    const selector = {
        btnAllList: divToolbarEl.find("#btnAllList"),
        btnNew: divToolbarEl.find("#btnNew"),
        photo: formEl.find("#Photo"),
        roleName: formEl.find("#Role"),
    }

    const GenerateList = () => {
        let columns = [
            {
                "data":null,
                "render": function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "organizationName", "name": "organizationName", "autowidth": true, "orderable": true },
            { "data": "department", "name": "department", "autowidth": true, "orderable": true },
            { "data": "code", "name": "code", "autowidth": true, "orderable": true },
            //{ "data": "imageUrl", "name": "imageUrl", "autowidth": true, "orderable": true },
            { "data": "openingBalance", "name": "openingBalance", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return ButtonPartial('OperationalUserSetup', full.operationalUserId);
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/OperationalUser/list", columns);
        dtLoader.ajax.data = function (data) {
            data.roleName = selector.roleName.val();
        };
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const GetInitial = async () => {
        try {
            const response = await ajaxOperation.GetAjaxAPI(initialUrl);
            divDetailsEl.show();
            divPrimaryEl.hide();
            ResetForm(formEl);
            response.role = selector.roleName.val();
            setFormData(formEl, response);
        } catch (e) {
            Failed(e);
        }
    }

    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        ToggleActiveToolbarBtn(selector.btnAllList, divToolbarEl);
        divPrimaryEl.show();
        autoRiceTable.fnFilter();
    }

    //const DivisionShowHide = (isPrimary, isDetail) => {
    //    isPrimary ? $(divPrimaryEl).show() : $(divPrimaryEl).hide();
    //    isDetail ? $(divDetailsEl).show() : $(divDetailsEl).hide();
    //}
    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = new FormData(formEl[0]);
                model.append("Role", selector.roleName.val());
                model.append("Photo", selector.photo.get(0));
                let response = await ajaxOperation.SaveAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
                    Back();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Delete = async id => {
        let decisionResult = await Decision();
        try {
            if (decisionResult) {
                const url = deleteUrl + "/" + id;
                let response = await ajaxOperation.DeleteAjaxAPI(url);
                if (typeof (response) === "number" && parseInt(response) > 0) {
                    Success(deleteMessage);
                    ResetForm(formEl);
                    autoRiceTable.fnFilter();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }
    const Edit = async id => {
        try {
            console.log(id);
            const url = editUrl + "/" + id;
            let response = await ajaxOperation.GetAjaxAPI(url);
            if (typeof (response) == "object") {
                divDetailsEl.show();
                divPrimaryEl.hide();
                ResetForm(formEl);
                setFormData(formEl, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    

    $(document).ready(function () {
        divDetailsEl.hide();
        GenerateList();
    });

    $(SetValidatorElement(Elements)).keyup(function () {
        Elements.forEach(OperationalUser => {
            if ($(OperationalUser).val() !== '') {
                $("#" + OperationalUser).removeClass("is-invalid");
                $("." + OperationalUser).hide();
            }
        });
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    formEl.find("#btnCancel").click(function () {
        Back();
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    selector.btnNew.click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        GetInitial();
    });
})();