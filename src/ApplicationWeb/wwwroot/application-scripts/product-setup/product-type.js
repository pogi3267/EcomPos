
(function () {
    let menuId, pageName;
    let divToolbarEl, toolbarId, tblMasterId, tblBodyId, tblChildId, formEl, divPrimaryEl, divDetailsEl;
    let ecomTable = null;
    let Elements = ["Name"]; // form validation id only

    const CommonInitializer = function () {
        const { pageName: dnPageName, menuId: dnMenuId } = GetPageInformation();
        menuId = dnMenuId, pageName = dnPageName;
        let pageId = pageName + "-" + menuId;
        divToolbarEl = $(pageConstants.DIV_TOOLBAR_ID + pageId);
        toolbarId = pageConstants.TOOLBAR_ID + pageId;
        tblMasterId = pageConstants.MASTER_TABLE_ID + pageId;
        tblBodyId = pageConstants.MASTER_TABLE_BODY_ID + pageId;
        tblChildId = pageConstants.CHILD_TABLE_ID + pageId;
        formEl = $(pageConstants.FORM_ID + pageId);
        divPrimaryEl = $(pageConstants.DIV_PRIMARY_ID + pageId);
        divDetailsEl = $(pageConstants.DIV_DETAILS_ID + pageId);
    }

    const GenerateList = function () {
        let columns = [
            {
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "name", "name": "name", "autowidth": true, "orderable": true },
            {
                "render": function (data, type, full, meta) {
                    return `<div class="btn-group" style ="text-align:center;">
                                <button type = "button" class="btn btn-info btn-sm btnEdit" id = "${full.productTypeId}">Edit</button>
                                <button type = "button" class="btn btn-danger btn-sm btnDelete" id = "${full.productTypeId}">Delete</button>
                            </div>`;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/ProductType/list", columns);
        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;
    }

    const Save = function () {
        const message = "ProductType Saved Succcessfully!";
        const url = "/api/ProductType/Save";
        crudOperation.Save(formEl, Elements, url, ecomTable, message);
    }

    const Delete = function (id) {
        const message = "Deleted Successfully";
        const url = "/api/ProductType/Delete/" + id;
        crudOperation.Delete(formEl, url, ecomTable, message);
    }

    const Edit = function (id) {
        const url = "/api/ProductType/Edit/" + id;
        crudOperation.Edit(url, formEl);
    }

    CommonInitializer();

    $(document).ready(function () {
        GenerateList();
    });

    $(crudOperation.SetValidatorElement(Elements)).keyup(function () {
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

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });
})();