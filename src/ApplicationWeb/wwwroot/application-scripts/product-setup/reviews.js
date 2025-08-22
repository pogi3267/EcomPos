(function () {
    ecomTable = null;
    // urls
    const saveUrl = "/api/Reviews/Save";
    const deleteUrl = "/api/Reviews/Delete";
    const newUrl = "/api/Reviews/New";
    const editUrl = "/api/Reviews/Edit";

    // success message
    const saveMessage = "Reviews Saved Succcessfully!";
    const updateMessage = "Reviews Updated Succcessfully!";
    const deleteMessage = "Reviews Deleted Successfully";

    const ratingList = [{ id: "1", text: "1" }, { id: "2", text: "2" }, { id: "3", text: "3" }, { id: "4", text: "4" }, { id: "5", text: "5" }];
    const statusList = [{ id: "0", text: "Inactive" }, { id: "1", text: "Active" }];

    const GenerateList = () => {
        let columns = [
            {
                "data": null,
                render: function (data, type, row, meta) {
                    return meta.row + meta.settings._iDisplayStart + 1;
                }
            },
            { "data": "productName", "name": "productName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const { comment } = full;
                    let Words = comment.split(' ')
                    let res = Words.slice(0, 5);
                    res = res.join(' ');
                    if (Words.length > 5) res = res + '...';

                    return `<p>${res}</p>`;
                }
            },
            { "data": "rating", "name": "rating", "autowidth": true, "orderable": true },
            { "data": "statusInString", "name": "statusInString", "autowidth": true, "orderable": true },
            { "data": "userName", "name": "userName", "autowidth": true, "orderable": true },
            {
                "data": null,
                "render": function (data, type, full, meta) {
                    const result = ButtonPartial('Reviews', full.reviewId);
                    return result;
                }
            },
        ];
        let dtLoader = DataTableLoader("/api/Reviews/list", columns);

        if ($.fn.DataTable.isDataTable(divPrimaryEl.find(tblMasterId))) {
            divPrimaryEl.find(tblMasterId).DataTable().destroy();
        }

        let tableData = divPrimaryEl.find(tblMasterId).dataTable(dtLoader);
        ecomTable = tableData;

    }

    const New = async () => {
        try {
            const url = newUrl;
            let response = await ajaxOperation.GetAjaxAPI(url);
            response.ratingList = ratingList;
            response.statusList = statusList;
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


    const Edit = async id => {
        try {
            const url = editUrl + "/" + id;
            let response = await ajaxOperation.GetAjaxAPI(url);
            response.ratingList = ratingList;
            response.statusList = statusList;
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

    const Save = async () => {
        try {
            if (IsFrmValid(formEl, Elements)) {
                let model = formElToJson(formEl);
                let response = await ajaxOperation.SavePostAjax(saveUrl, model);
                if (typeof (response) == "object") {
                    ResetForm(formEl);
                    Success(response.entityState == 4 ? saveMessage : updateMessage);
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
                if (typeof (response) == "number" && parseInt(response) > 0) {
                    Success(deleteMessage);
                    ResetForm(formEl);
                    ecomTable.fnFilter();
                }
            }
        } catch (e) {
            Failed(e);
        }
    }

    const Back = async () => {
        divDetailsEl.hide();
        ResetForm(formEl);
        formEl.find('#itemTbody').html('');
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAdd"), divToolbarEl);
        divPrimaryEl.show();
        ecomTable.fnFilter();
    }

    CommonInitializer();

    $(document).ready(function () {
        ToggleActiveToolbarBtn(divToolbarEl.find("#btnAdd"), divToolbarEl);
        GenerateList();
    });

    $(tblMasterId).on("click", ".btnEdit", function () {
        Edit($(this).attr('id'));
    });

    $(tblMasterId).on("click", ".btnDelete", function () {
        Delete($(this).attr('id'));
    });

    formEl.find("#btnSave").click(function () {
        Save();
    });

    formEl.find("#btnCancel").click(function () {
        Back();
    });

    divToolbarEl.find("#btnAdd").click(function (e) {
        e.preventDefault();
        New();
    });

})();