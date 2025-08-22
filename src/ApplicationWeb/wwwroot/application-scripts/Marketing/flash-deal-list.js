(function () {
    ecomTable = null;
    Elements = ["Title", "StartDate", "EndDate"]; // form validation id only

    const divProductEl = $("#divProduct");
    const tblFlashProducts = "#tblFlashProducts";
    const tblFlashProductsBody = "#tblFlashProductsBody";
    let flashDealId;

    // urls
    const saveUrl = "/api/FlashDeal/Save";
    const intialUrl = "/api/FlashDeal/GetInitial";
    const editUrl = "/api/FlashDeal/Edit";
    const deleteUrl = "/api/FlashDeal/Delete";
    const statusUpdateUrl = `/api/FlashDeal/FlashDealStatus`;

    // success message
    const deleteMessage = "Flash Deal Deleted Successfully";
    const statusMessage = "Flash Deal Status Updated Succcessfully!";
    const saveMessage = "Flash Deal Saved Succcessfully!";
    const updateMessage = "Flash Deal Updated Succcessfully!";

    // image laoder 
    const imageLoadingArray = [{ id: "#bannerImage", name: "banner" }]; // set image selector id and object name where the image link exist

    const selector = {
        entityStatus: ".status"
    };

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "title", "name": "title", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { flashDealId, status } = full;
                    return `<input type = "checkbox" class = "status" ${status == 1 ? "checked" : ""} entityId= "${flashDealId}" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.startDate);
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return setDateFormat(full.endDate);
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<button style ="width:100%;height:15px;background-color:${full.backgroundColor};"></button>`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<button style ="width:60%;height:15px;background-color:${full.textColor};"></button>`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    return `<img src = "${full.banner}" height = "40" width = "40" alt = "No Image" />`;
                }
            },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const prodButton =
                        `
                         <button type="button" class="btn btn-success btn-sm btnAddProduct" id="${full.flashDealId}" style="white-space: nowrap; width: 100px;">Add Product </button>
                        `;
                    const result = ButtonPartial('FlashDeal', full.flashDealId);
                    return `<div class="btn-group">${result + prodButton}</div>`;

                }
            },
        ];
        let dtLoader = DataTableLoader("/api/FlashDeal/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const InitialLoader = async () => {
        divDetailsEl.show();
        divProductEl.hide();
        divPrimaryEl.hide();
        ResetForm(formEl); 
    }

    const Save = async () => {
        try {
            let formData = new FormData(formEl[0]);
            formData.append("IsStatus", formEl.find('#IsStatus').val());
            formData.append("BannerImage", $("#BannerImage").get(0));
            if (IsFrmValid(formEl, Elements)) {
                let response = await ajaxOperation.SaveAjax(saveUrl, formData);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    RemoveLoadImages(imageLoadingArray);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
                    Back();
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
                divDetailsEl.show();
                divPrimaryEl.hide();
                divProductEl.hide();
                ResetForm(formEl); 
                setFormData(formEl, response);
                LoadImages(imageLoadingArray, response);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Delete = async id => {
        let decisionResult = await Decision();
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
    }

    const StatusUpdate = async (entityId, isChecked) => {
        try {
            const url = statusUpdateUrl + `/${entityId}/${isChecked}`;
            let response = await ajaxOperation.UpdateAjaxAPI(url);
            if (typeof (response) == "object") {
                Success(statusMessage);
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Back = async () => {
        divDetailsEl.hide();
        divProductEl.hide();
        ResetForm(formEl);
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAllList"), divToolbarEl);
        divPrimaryEl.show();
        RemoveLoadImages(imageLoadingArray);
        ecomTable.fnFilter();
    }

    const AddProduct = async flashDealId => {
        try {
            let response = await ajaxOperation.GetAjaxAPI(`/api/Product/GetFlashDealProduct/${flashDealId}`);
            if (typeof (response) == "object") {
                divDetailsEl.hide();
                divPrimaryEl.hide();
                divProductEl.show();
                initSelect2($("#productList-flashDeal"), response.productList, true, "Select item", true);

                $('#tblFlashProductsBody').html('');
                let slNo = 1;
                response.flashDealProductList.forEach(model => {
                    BuildProductsHtml(model, slNo++, flashDealId)
                });
            }
        } catch (e) {
            Failed(e);
        }
    }
    

    const requiredKey = ["name", "categoryName", "brandName", "currentStock", "discount", "discountType", "flashDiscount", "flashDiscountType"]
    const BuildProductsHtml = (model, slNo, flashDealId) => {
        let serialNumber = model.productId;
        let html = `<td>${slNo}</td>`;
        requiredKey.forEach(item => html += `<td>${model[item]}</td>`);
        html += `<td>
                    <button type="button" class = "btn btn-sm btn-danger btnFlashProductDelete" uniqueIdentity = "${serialNumber}" flashDealId = "${flashDealId}" title = "Delete"><i class="fas fa-trash-alt"></i></button>
                 </td>`;
        $('#tblFlashProductsBody').append(`<tr id = "${serialNumber}">${html}</tr>`);
    }

    $("#tblFlashProductsBody").on("click", ".btnFlashProductDelete", async function () {
        let decisionResult = await Decision();
        let productId = $(this).attr("uniqueIdentity");
        let flashDealId = $(this).attr("flashDealId");
        if (decisionResult) {
            let response = await ajaxOperation.DeleteAjaxAPI(`/api/Product/DeleteFlashDealProduct/${flashDealId}/${productId}`);
            if (typeof (response) == "number" && parseInt(response) > 0) {
                AddProduct(parseInt(response));
                Success("Deleted Successfully!");
            }
            
        }
    });

    const SaveFlashDealProduct = async (flashDealId, prodId, flashDealDiscount, flashDealDiscountType) => {
        try {
            let response = await ajaxOperation.SaveAjax(`/api/Product/SaveFlashDealProduct/${flashDealId}/${prodId}/${flashDealDiscount}/${flashDealDiscountType}`);
            if (typeof (response) == "object") {
                AddProduct(flashDealId);
            }
        } catch (e) {
            Failed(e.responseText);
        }
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
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
        Back();
    });

    $("#btnBackProdFlashDeal").click(function () {
        Back();
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnAddProduct", function () {
        AddProduct($(this).attr('id'));
        flashDealId = $(this).attr('id');
    });

    $(tblMasterId).on("change", selector.entityStatus, async function () {
        let entityId = $(this).attr("entityId");
        let isChecked = $(this).is(":checked");
        StatusUpdate(entityId, isChecked);
    });

    $("#BannerImage").change(function () {
        const [file] = $("#BannerImage")[0].files
        if (file) {
            $("#bannerImage").attr("src", URL.createObjectURL(file))
        }
    });

    divToolbarEl.find("#btnNew").click(function (e) {
        e.preventDefault();
        ToggleActiveToolbarBtn(this, divToolbarEl);
        InitialLoader();
    });

    $('#btnAddProdFlashDeal').click(function () {
        let prodId = $('#productList-flashDeal').val();
        let flashDealDiscount = $('#flashDealDiscount').val();
        let flashDealDiscountType = $('#flashDealDiscountType').val();
        SaveFlashDealProduct(flashDealId, prodId, flashDealDiscount, flashDealDiscountType);
    });

})();