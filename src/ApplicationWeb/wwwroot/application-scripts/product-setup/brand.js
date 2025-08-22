
(function () {
    ecomTable = null;
    const validators = [
        initializeValidation('#Name', '#brandNameError', 'Name is required.'),
        //initializeValidation('#Photo', '#brandPhotoError', 'Photo is required.')
    ];
    // urls
    const saveUrl = "/api/Brand/Save";
    const deleteUrl = "/api/Brand/Delete";
    const editUrl = "/api/Brand/Edit";

    // success message
    const saveMessage = "Brand Saved Succcessfully!";
    const updateMessage = "Brand Updated Succcessfully!";
    const deleteMessage = "Deleted Successfully";

    // image laoder 
    const imageLoadingArray = [{ id: "#bandImage", name: "logo" }];

    const GenerateList = function () {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "name": "name", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<img src = "${full.logo}" height = "40" width = "40" alt = "No Image" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Brand', full.brandId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Brand/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const Save = async () => {
        try {
            let formData = new FormData(formEl[0]);
            formData.append("Photo", $("#Photo").get(0));
            if (IsFormsValid(validators)) {
                let response = await ajaxOperation.SaveAjax(saveUrl, formData);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    RemoveLoadImages(imageLoadingArray);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    ecomTable.fnFilter();
                }
            } else {
                Failed("Please Select Required Fields.");
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
                    RemoveLoadImages(imageLoadingArray);
                    ecomTable.fnFilter();
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
                LoadImages(imageLoadingArray, response);
            }
        } catch (e) {
            Failed(response);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
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

    $("#Photo").change(function () {
        const [file] = $("#Photo")[0].files
        if (file) {
            $("#bandImage").attr("src", URL.createObjectURL(file))
        }
    });
})();