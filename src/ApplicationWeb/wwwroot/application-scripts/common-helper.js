'use strict'
$(document).ready(function () {
    $('input[type="radio"]').click(function () {
        var $radio = $(this);
        if ($radio.data('waschecked') === true) {
            $radio.prop('checked', false);
            $radio.data('waschecked', false);
        } else {
            $radio.prop('checked', true);
            $radio.data('waschecked', true);
        }
        $radio.siblings('input[type="radio"]').data('waschecked', false);
    });
});
let dateFormats = Object.freeze({
    DEFAULT: "DD MMM YYYY",
    DEFAULT_TIME: "DD MMM YYYY hh:mm A"
});
function setDateFormat(date) {
    if (date == null || date == undefined || !date) return "";
    const dt = moment(date).format(dateFormats.DEFAULT);
    return dt;
}

function setDateTimeFormat(date) {
    if (date == null || date == undefined || !date) return "";
    const dt = moment(date).format(dateFormats.DEFAULT_TIME);
    return dt;
}

function DateFormatter(dt) {
    let now = new Date();
    if (dt)
        now = new Date(dt);

    let day = ("0" + now.getDate()).slice(-2);
    let month = ("0" + (now.getMonth() + 1)).slice(-2);

    let date = now.getFullYear() + "-" + (month) + "-" + (day);
    return date;
}

function DateTimeFormatter(dt) {
    let now = new Date();
    if (dt) {
        now = new Date(dt);
    }

    let day = ("0" + now.getDate()).slice(-2);
    let month = ("0" + (now.getMonth() + 1)).slice(-2);
    let year = now.getFullYear();

    let hours = ("0" + now.getHours()).slice(-2);
    let minutes = ("0" + now.getMinutes()).slice(-2);
    let seconds = ("0" + now.getSeconds()).slice(-2);

    let dateTime = `${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;
    return dateTime;
}


$('.devensedate').datetimepicker({
    //format: 'L'
    format: dateFormats.DEFAULT
});

$('.devensedatetime').datetimepicker({
    //format: 'L'
    format: dateFormats.DEFAULT_TIME,
    icons: { time: 'far fa-clock' }
});

function capitalizeFirstLetter(string) {
    return string.charAt(0).toLowerCase() + string.slice(1);
}
function setFormData($formEl, data) {
    if (!data && ! typeof data === 'object') {
        console.error("Your data is not valid.");
        return;
    }
    try {
        $formEl.find("input, select, textarea").each(function () {
            try {
                let $input = $(this);
                let value = data[capitalizeFirstLetter(this.name)];
                if (this.tagName.toLowerCase() === "textarea") {
                    $input.val(value);
                }
                else if (this.tagName.toLowerCase() === "input") {

                    if (this.className.includes('devensedate') && this.type == 'text') //check if field is date
                    {
                        $input.val(moment(DateFormatter(value)).format(dateFormats.DEFAULT));
                        return;
                    }
                    else if (this.className.includes('devensedatetime') && this.type == 'text') //check if field is datetime
                    {
                        $input.val(moment(DateTimeFormatter(value)).format(dateFormats.DEFAULT_TIME));
                        return;
                    }

                    switch (this.type) {
                        case "checkbox":
                            $input.attr("checked", value).trigger("change");
                            break;
                        case "radio":
                            $input.each(function (i) {
                                if ($(this).val() == value) $(this).attr({ checked: true })
                                else $(this).attr({ checked: false })
                            });
                            break;
                        case "date":
                            $input.val(DateFormatter(value));
                            break;
                        case "file":
                            break;
                        default:
                            $input.val(value);
                            break;
                    }
                }
                else if (this.tagName.toLowerCase() === "select") {
                    let optionListName = "";
                    const myButton = document.getElementById(this.id);
                    let answer = myButton.hasAttribute("single") || false;
                    if (!answer) {
                        if (this.multiple) {
                            optionListName = this.name.replace("Ids", '');
                            optionListName = optionListName.replace("IDs", '');
                        } else {
                            if (this.name) {
                                optionListName = this.name.replace("Id", '');
                                optionListName = optionListName.replace("ID", '');
                            }
                            else {
                                optionListName = this.id.replace("Id", '');
                                optionListName = optionListName.replace("ID", '');
                            }
                        }
                        optionListName += "List";
                        initSelect2($input, data[capitalizeFirstLetter(optionListName)], true, "Select Item", false);

                    }
                    else {
                        $(`#${this.id}`).select2();
                    }
                    $input.val(value).attr("selected", true).trigger("change");
                }
            } catch (e) {
                console.error(e);
            }
        });
    } catch (e) {
        console.error(e);
    }
}
function getFormData($formEl) {
    setAllFormCheckBoxValue($formEl);
    let data = $formEl.serializeArray();

    let formData = new FormData();
    $.each(data, function (i, v) {
        formData.append(v.name, v.value);
    });

    return formData;
}
function formDataToJson(data) {
    let jsonObj = {};
    $.each(data, function (i, v) {
        jsonObj[v.name] = v.value;
    });

    return jsonObj;
}

function formElToJson($formEl) {
    setAllFormCheckBoxValue($formEl);
    let disabledInputs = $formEl.find(':disabled');
    disabledInputs.each(function () {
        $(this).removeAttr('disabled');
    });

    let data = $formEl.serializeArray();
    let jsonObj = {};
    for (let i = 0; i < data.length; i++) {
        let prop = data[i];
        jsonObj[prop.name] = prop.value;
    }

    disabledInputs.each(function () {
        $(this).attr('disabled', 'disabled');
    });

    return jsonObj;
}
function setAllFormCheckBoxValue($formEl) {
    $formEl.find(':checkbox').each(function () {
        this.value = this.checked;
    });
}

function PopulateSelect2Formatter(domId) {
    $(domId).select2({
        width: "100%",
        height: "100%",
        placeholder: "Select a Item",
        templateSelection: function (data, container) {
            let selection = $(domId).select2('data');
            let idx = selection.indexOf(data);
            data.idx = idx;
            $(container).css("background-color", "blue");
            $(container).css("color", "white");
            return data.text;
        },
        templateResult: function (data, container) {
            let selection = $(domId).select2('data');
            let idx = selection.indexOf(data);
            data.idx = idx;
            return data.text;
        }
    });
}

function initSelect2($el, data, allowClear = true, placeholder = "Select a Value", showDefaultOption = true) {
    if (showDefaultOption)
        data.unshift({ id: '', text: '' });
    $el.html('').select2({ 'data': data, 'allowClear': allowClear, 'placeholder': placeholder, width: "100%", height: "100%" });
}

function convertToBoolean(str) {
    if (!str)
        return false;

    switch (str.toLowerCase()) {
        case "on":
        case "true":
        case "yes":
        case "1":
            return true;
        default: return false;
    }
}
//#region Table Formatter
function priceFormatter(value, row, index) {
    let p = value.toFixed(2).split(".");
    let formatedPrice = p[0].split("").reduceRight(function (acc, value, i, orig) {
        let pos = orig.length - i - 1;
        return value + (pos && !(pos % 3) ? "," : "") + acc;
    }, "") + (p[1] ? "." + p[1] : "");

    if (row.CurrencyCode === "USD")
        formatedPrice = "$" + formatedPrice;
    return formatedPrice;
}

function ToggleActiveToolbarBtn(el, $parentEl) {
    $parentEl.find('button').not("#" + el.id).removeClass("btn-success text-white").addClass("btn-default text-black");

    if (el instanceof jQuery)
        el.removeClass("btn-default text-black").addClass("btn-success text-white");
    else
        $(el).removeClass("btn-default text-black").addClass("btn-success text-white");
}

// #region Constants 
let statusConstants = Object.freeze({
    ALL: 1,
    NEW: 2,
    EDIT: 3,
    PENDING: 4,
    COMPLETED: 5,
    PARTIALLY_COMPLETED: 6,
    APPROVED: 7,
    PROPOSED_FOR_APPROVAL: 8,
    UN_APPROVE: 9,
    ACKNOWLEDGE: 10,
    PROPOSED_FOR_ACKNOWLEDGE: 11,
    UN_ACKNOWLEDGE: 12,
    ACKNOWLEDGE_ACCEPTENCE: 13,
    PROPOSED_FOR_ACKNOWLEDGE_ACCEPTENCE: 14,
    REJECT: 15,
    REVISE: 16,
    CHECK: 17,
    CHECK_REJECT: 18,
    ACTIVE: 19,
    IN_ACTIVE: 20,
    ADDITIONAL: 21,
    ReTest: 22,
    EXECUTED: 23,
    REPORT: 24,
    RnD: 25
});


let pageConstants = Object.freeze({
    PAGE_ID: "#",
    DIV_TOOLBAR_ID: "#divtoolbar",
    DIV_TBL_ID_PREFIX: "#divtbl",
    TOOLBAR_ID: "#toolbar",
    MASTER_TABLE_ID: "#tbl",
    MASTER_TABLE_BODY_ID: "#tblBody",
    CHILD_TABLE_ID: "#tblChild",
    CHILD_TABLE_ID2: "#tblChild2",
    CHILD_TABLE_ID3: "#tblChild3",
    FORM_ID: "#form",
    DIV_PRIMARY_ID: "#divPrimary",
    DIV_DETAILS_ID: "#divDetails"
});

const dataTableButtons = [
    {
        "extend": 'copy', "text": 'Copy', "className": 'btn btn-info btn-sm', exportOptions: { columns: ':visible' },
        init: function (api, node, config) {
            $(node).removeClass('dt-button');
        }
    },
    {
        "extend": 'csv', "text": 'CSV', "className": 'btn btn-info btn-sm', exportOptions: { columns: ':visible' },
        init: function (api, node, config) {
            $(node).removeClass('dt-button');
        }
    },
    {
        "extend": 'excel', "text": 'Excel', "className": 'btn btn-info btn-sm', exportOptions: { columns: ':visible' },
        init: function (api, node, config) {
            $(node).removeClass('dt-button');
        }
    },
    {
        "extend": 'pdf', "text": 'Pdf', "className": 'btn btn-info btn-sm', exportOptions: { columns: ':visible' },
        init: function (api, node, config) {
            $(node).removeClass('dt-button');
        }
    },
    {
        "extend": 'print', "text": 'Print', "className": 'btn btn-info btn-sm', exportOptions: { columns: ':visible' },
        init: function (api, node, config) {
            $(node).removeClass('dt-button');
        }
    },
    {
        "extend": 'colvis', "text": 'Visible', "className": 'btn btn-info btn-sm', exportOptions: { columns: ':visible' },
        init: function (api, node, config) {
            $(node).removeClass('dt-button');
        }
    },
];

function DataTableLoader(url, columns, statusType = null, additionalparam1 = null) {
    return {
        "processing": true,
        "serverSide": true,
        "paging": true,
        "lengthChange": true,
        "ordering": true,
        "searching": true,
        "info": true,
        "responsive": true,
        "language": {
            search: "_INPUT_",
            searchPlaceholder: "Search",
            "lengthMenu": "Display _MENU_ Records",
            "emptyTable": "No Data Available!",
            "paginate": {
                "next": ">",
                "previous": "<"
            }
        },
        "filter": true,
        "pageLength": 10,
        "autoWidth": false,
        "autoHeight": false,
        "dom": "<'row'<'col-sm-3'l><'col-sm-6 text-center'B><'col-sm-3'f>>" + "<'row'<'col-sm-12'tr>>" + "<'row'<'col-sm-5'i><'col-sm-7'p>>",
        "lengthMenu": [[10, 25, 50, 100, 200], ['10', '25', '50', '100', '200']],
        "buttons": dataTableButtons,
        "ajax": {
            "url": url,
            "type": "POST",
            "headers": { "Authorization": "Bearer " + localStorage.getItem("token") },
            "data": function (data) {
                data.status = statusType;
                data.additionalparam1 = additionalparam1;
            },
            "complete": function (json) {
            }
        },
        "columns": columns
    };
}

function IsFormValid($formEl, element) {
    let isValidItem = true;
    let $input;
    $($formEl).find(':input').each(function () {
        if ($.inArray(this.id, element) > -1) {

            $input = $("#" + this.id);

            switch ($input[0].type) {
                case 'text':
                case 'textarea':
                case 'password':
                case 'hidden':
                    if ($input.val() === "" || $input.val() === null) {
                        isValidItem = false;
                        $input.closest('.form-group').find('.' + $input[0].id).show();
                        $input.addClass("is-invalid");
                    }
                    else {
                        $input.closest('.form-group').find('.' + $input[0].id).hide();
                        $input.removeClass("is-invalid");
                    }
                    break;
                case 'select-multiple':
                case 'select-one':
                    if ($input.val() === "" || $input.val() === null || $input.val() === "-1" || $input.val() === "99") {
                        isValidItem = false;
                        $input.closest('.form-group').find('.' + $input[0].id).show();
                        $input.addClass("is-invalid");
                    }
                    else {
                        $input.closest('.form-group').find('.' + $input[0].id).hide();
                        $input.removeClass("is-invalid");
                    }
                    break;
                case 'checkbox':
                case 'radio':
                    if (($input[0].id).is(':checked')) {
                        $input.closest('.form-group').find('.' + $input[0].id).hide();
                        $input.removeClass("is-invalid");
                    } else {
                        isValidItem = false;
                        $input.closest('.form-group').find('.' + $input[0].id).show();
                        $input.addClass("is-invalid");
                    }
                    break;
                default:
                    break;
            }
        }
    });
    return isValidItem;
}


let menuId, pageName;
let divToolbarEl, tblMasterId, tblBodyId, tblChildId, formEl, divPrimaryEl, divDetailsEl;
let toolbarId = null;
let gridTable = null;
let Elements;

const IsFrmValid = (formId, Elements) => {
    let isValidItem = true;
    if (!IsFormValid(formId, Elements)) {
        isValidItem = false;
        Failed("Please fill-up mandatory field.");
    }
    return isValidItem;
}
const ResetForm = formId => {
    formId.trigger("reset");
    $(formId).find(":input:hidden").each(function () {
        const name = this.name.toLowerCase().slice(-2);
        if (name == "id") this.value = '0';
    });
    $.each($(formId).find('select'), function (i, el) {
        $(el).val('').trigger('change');
    });
    $(formId).find('input:checkbox').removeAttr('checked');
}

const CommonInitializer = () => {
    const { pageName: dnPageName, menuId: dnMenuId } = GetPageInformation();
    menuId = dnMenuId, pageName = dnPageName;
    let pageId = pageName + "-" + menuId;
    toolbarId = pageConstants.TOOLBAR_ID + pageId;
    divToolbarEl = $(toolbarId);
    tblMasterId = pageConstants.MASTER_TABLE_ID + pageId;
    tblBodyId = pageConstants.MASTER_TABLE_BODY_ID + pageId;
    tblChildId = pageConstants.CHILD_TABLE_ID + pageId;
    formEl = $(pageConstants.FORM_ID + pageId);
    divPrimaryEl = $(pageConstants.DIV_PRIMARY_ID + pageId);
    divDetailsEl = $(pageConstants.DIV_DETAILS_ID + pageId);
}
const LoadImages = (imageLoadingArray, response) => {
    imageLoadingArray.forEach(item => {
        const { id: selectorId, name } = item;
        $(selectorId).attr("src", response[name]);
    });
}
const RemoveLoadImages = imageLoadingArray => {
    imageLoadingArray.forEach(item => {
        const { id: selectorId } = item;
        $(selectorId).attr("src", "#");
    });
}
const SetValidatorElement = Elements => {
    let validationId = Elements.join(",#");
    validationId = "#" + validationId[0] + validationId.substring(1, validationId.length);
    return validationId;
}

const ButtonPartialHelper = (id, buttonType, key, linkInfo, className, buttonText) => {
    const { type, url, pageName } = linkInfo;
    const isExist = type.includes(buttonType);
    return isExist ? `<a href = "${url[key]}?id=${id}&&pageName=${pageName}" class = "${className}" >${buttonText} </a>` : '';
}

const ButtonPartial = (claimName, id, isLink = false, linkInfo = {}) => {
    const userClaims = JSON.parse(localStorage.getItem("CLAIM_KEY"));
    const newUserClaim = userClaims.filter(item => {
        const { type } = item;
        const [values] = type.split(".");
        if (values.toLowerCase() == claimName.toLowerCase())
            return item;
        return false;
    });
    let button = `<div class="d-flex justify-content-center gap-2">`; // Wrapper div with flex and gap

    newUserClaim.forEach(item => {
        const { type, value } = item;
        const [firstProperty, claimProperty] = type.split(".");
        if (claimProperty.toLowerCase() == "edit" && value === "true") {
            if (isLink) {
                button += ButtonPartialHelper(id, "edit", "editUrl", linkInfo, "btn btn-info btn-sm ml-2 btnEdit", "Edit");
            }
            else {
                button += `<button type="button" class="btn btn-info btn-sm ml-2 btnEdit" id="${id}">Edit</button>`;
            }
        }
        if (claimProperty.toLowerCase() == "delete" && value === "true") {
            button += `<button type="button" class="btn btn-danger btn-sm ml-2 btnDelete" id="${id}">Delete</button>`;
        }
    });
    button += `</div>`; // Closing wrapper div
    return button;
}

function IsEmailValid(inputText) {
    let regex = /^([\w-\.]+\u0040([\w-]+\.)+[\w-]{2,4})?$/;
    return regex.test(inputText);
}

function FormClear(FormID) {
    $(".form-text").hide();
    $("." + FormID).find(':input').each(function () {
        switch (this.type) {
            case 'text':
            case 'textarea':
            case 'password':
                $(this).val(''); break;
            case 'select-multiple':
            case 'select-one':
                $(this).val('').trigger('change'); break;
            case 'hidden': $(this).val('0'); break;
            case 'checkbox':
            case 'radio':
                this.checked = false; break;
            default:
                break;
        }
    });
}
function IsFormValidation($formEl, element) {
    let isValidItem = true;
    let $input;
    $($formEl).find(':input').each(function () {
        if ($.inArray(this.id, element) > -1) {

            $input = $("#" + this.id);

            switch ($input[0].type) {
                case 'text':
                case 'number':
                case 'textarea':
                case 'password':
                case 'hidden':
                    if ($input.val() === "" || $input.val() === null) {
                        isValidItem = false;
                        $input.closest('.form-group').find('.' + $input[0].id).show();
                        $input.addClass("is-invalid");
                    }
                    else {
                        $input.closest('.form-group').find('.' + $input[0].id).hide();
                        $input.removeClass("is-invalid");
                    }
                    break;
                case 'select-multiple':
                case 'select-one':
                    if ($input.val() === "" || $input.val() === null || $input.val() === "-1" || $input.val() === "99") {
                        isValidItem = false;
                        $input.closest('.form-group').find('.' + $input[0].id).show();
                        $input.addClass("is-invalid");
                    }
                    else {
                        $input.closest('.form-group').find('.' + $input[0].id).hide();
                        $input.removeClass("is-invalid");
                    }
                    break;
                case 'checkbox':
                case 'radio':
                    if (($input[0].id).is(':checked')) {
                        $input.closest('.form-group').find('.' + $input[0].id).hide();
                        $input.removeClass("is-invalid");
                    } else {
                        isValidItem = false;
                        $input.closest('.form-group').find('.' + $input[0].id).show();
                        $input.addClass("is-invalid");
                    }
                    break;
                default:
                    break;
            }
        }
    });
    return isValidItem;
}

function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        let date = new Date();
        date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + encodeURIComponent(value) + expires + "; path=/";
}

function getCookie(name) {
    let cookieName = name + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let cookieArray = decodedCookie.split(';');

    for (let i = 0; i < cookieArray.length; i++) {
        let cookie = cookieArray[i].trim();
        if (cookie.indexOf(cookieName) === 0) {
            return cookie.substring(cookieName.length, cookie.length);
        }
    }
    return null;
}

function deleteCookie(name) {
    document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
}

function uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
        .replace(/[xy]/g, function (c) {
            const r = Math.random() * 16 | 0,
                v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
}
function initializeValidation(inputSelector, errorSelector, errorMessage) {
    $(errorSelector).hide();
    $(inputSelector).on('input change blur', function () {
        $(errorSelector).hide();
    });

    return function () {
        let isValid = true;
        if ($(inputSelector).val() == null || $(inputSelector).val() == 0 || $(inputSelector).val().trim() === '') {
            $(errorSelector).text(errorMessage).show();
            isValid = false;
        } else {
            $(errorSelector).hide();
        }
        return isValid;
    }
}

function IsFormsValid(validators) {
    let isValid = true;
    validators.forEach(function (validator) {
        if (!validator()) {
            isValid = false;
        }
    });
    return isValid;
}