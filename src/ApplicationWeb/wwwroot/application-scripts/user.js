

$(document).ready(function () {
    GenerateList();
});

let ecomTable = null;
let Elements = ["Name"]; // form validation id only

let crudSelector = {
    formId: $("#userId"),
    btnSave: $("#btnSave"),
    ecomTableTbody: $("#ecomTableTbody"),
    ecomTableId: $("#ecomTableId"),
};

$(crudOperation.SetValidatorElement(Elements)).keyup(function () {
    Elements.forEach(item => {
        if ($(item).val() != '') {
            $("#" + item).removeClass("is-invalid");
            $("." + item).hide();
        }
    });
});

crudSelector.btnSave.click(function () {
    Save();
});

function GenerateList() {
    let columns = [
        {
            "data": null,
            render: function (data, type, row, meta) {
                return meta.row + meta.settings._iDisplayStart + 1;
            }
        },
        { "data": "name", "name": "name", "autowidth": true, "orderable": true },
        { "data": "code", "name": "code", "autowidth": true, "orderable": true },
        {
            "data": null,
            "render": function (data, type, full, meta) {
                return `<div class="btn-group" style ="text-align:center;">
                                <button type = "button" class="btn btn-info btn-sm btnEdit" onClick = "Edit(${full.userId})">Edit</button>
                                <button type = "button" class="btn btn-danger btn-sm btnDelete" onClick = "Delete(${full.userId})">Delete</button>
                            </div>`;
            }
        },
    ];
    let dtLoader = DataTableLoader("/api/user/list", columns);
    let tableData = crudSelector.ecomTableId.dataTable(dtLoader);
    ecomTable = tableData;
}

function Save() {
    const message = "user Saved Succcessfully!";
    const url = "/api/user/Save";
    crudOperation.Save(crudSelector.formId, Elements, url, ecomTable, message );
}

function Delete(id) {
    const message = "Deleted Successfully";
    const url = "/api/user/Delete/" + id;
    crudOperation.Delete(crudSelector.formId, url, ecomTable, message);
}

function Edit(id) {
    const url = "/api/user/Edit/" + id;
    crudOperation.Edit(url, crudSelector.formId);
}
