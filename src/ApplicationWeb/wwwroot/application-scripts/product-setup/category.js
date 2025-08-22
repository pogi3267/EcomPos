(function () {
    ecomTable = null;
    const validators = [
        initializeValidation('#Name', '#categoryNameError', 'Please enter the category name.'),
    ];

    // urls
    const saveUrl = "/api/Category/Save";
    const deleteUrl = "/api/Category/Delete";
    const editUrl = "/api/Category/Edit";
    const intialUrl = "/api/Category/GetInitial";

    // success message
    const saveMessage = "Category Saved Succcessfully!";
    const updateMessage = "Category Updated Succcessfully!";
    const deleteMessage = "Deleted Successfully";

    // image laoder 
    const imageLoadingArray = [{ id: "#bannerImage", name: "banner" }, { id: "#iconImage", name: "icon" }]; // set image selector id and object name where the image link exist


    const GenerateList =  () =>{
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "name": "name", "autowidth": true, "orderable": true },
            { "data": "parentName", "name": "parentName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<img src = "${full.banner}" height = "40" width = "40" alt = "No Image" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { categoryId, featured } = full;
                    return `<input type = "checkbox" class = "featured" ${featured == 1 ? "checked" : ""} entityId= "${categoryId}" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Category', full.categoryId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Category/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const InitialLoader = async () => {
        let response = await ajaxOperation.GetAjaxAPI(intialUrl);
        if (typeof (response) == "object") {
            setFormData(formEl, response);
        }
    }

    const Save = async () => {
        try
        {
                let formData = new FormData(formEl[0]);
                formData.append("BannerImage", $("#BannerImage").get(0));
                formData.append("IconImage", $("#IconImage").get(0));
                if (IsFormsValid(validators)) {
                    let response = await ajaxOperation.SaveAjax(saveUrl, formData);
                    if (typeof (response) == "object") {
                        ResetForm(formEl);
                        RemoveLoadImages(imageLoadingArray);
                        Success(response.entityState == 4 ? saveMessage : updateMessage);
                        ecomTable.fnFilter();
                        InitialLoader();
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
                    InitialLoader();
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

    const FeatureUpdate = async (entityId, isChecked) => {
        try {
            const url = `/api/Category/FeatureUpdate` + `/${entityId}/${isChecked}`;
            let response = await ajaxOperation.UpdateAjaxAPI(url);
            if (typeof (response) == "object") {
                Success("Updted Successfully!");
            }
        } catch (e) {
            Failed(e);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
        InitialLoader();
    });

   

    formEl.find("#btnSave").click(function () {
        Save();
    });

    formEl.find("#btnCancel").click(function () {
        InitialLoader();
        RemoveLoadImages(imageLoadingArray);
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    $(tblMasterId).on("change", ".featured", async function () {
        let entityId = $(this).attr("entityId");
        let isChecked = $(this).is(":checked");
        FeatureUpdate(entityId, isChecked);
    });

    $("#BannerImage").change(function () {
        const [file] = $("#BannerImage")[0].files
        if (file) {
            $("#bannerImage").attr("src", URL.createObjectURL(file))
        }
    });

    $("#IconImage").change(function () {
        const [file] = $("#IconImage")[0].files
        if (file) {
            $("#iconImage").attr("src", URL.createObjectURL(file))
        }
    });

})();