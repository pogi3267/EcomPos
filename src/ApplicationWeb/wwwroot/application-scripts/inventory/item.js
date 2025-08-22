
(function () {
    autoRiceTable = null;
    Elements = ["ItemName", "ItemCode", "ItemType", "Unit"]; // form validation id only

    // urls
    const saveUrl = "/api/Item/Save";
    const deleteUrl = "/api/Item/Delete";
    const editUrl = "/api/Item/Edit";

    // success message
    const saveMessage = "Item Saved Successfully!";
    const updateMessage = "Item Updated Successfully!";
    const deleteMessage = "Item Deleted Successfully";
    
    const GenerateList = () =>{
        let columns = [
            {
                "data": null,
                "render": function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "itemName", "name": "itemName", "autowidth": true, "orderable": true },
            { "data": "itemCode", "name": "itemCode", "autowidth": true, "orderable": true },
            { "data": "unit", "name": "unit", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return ButtonPartial('ItemSetup', full.itemId);
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Item/list", columns);
        autoRiceTable = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
    }

    const generateString = () => {
        const length = 5;
        const characters ='ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
        let result = '';
        const charactersLength = characters.length;
        for ( let i = 0; i < length; i++ ) {
            result += characters.charAt(Math.floor(Math.random() * charactersLength));
        }
        return result;
    }
    
    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    Success(response.entityState === 4 ? saveMessage : updateMessage);
                    autoRiceTable.fnFilter();
                    $("#ItemCode").val(generateString());
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
                if (typeof (response) == "number" && parseInt(response) > 0) {
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
            const url = editUrl + "/" + id;
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
        GenerateList();
        $("#ItemCode").val(generateString());
    });

    $(SetValidatorElement(Elements)).keyup(function () {
        Elements.forEach(item => {
            if ($(item).val() !== '') {
                $("#" + item).removeClass("is-invalid");
                $("." + item).hide();
            }
        });
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });
})();