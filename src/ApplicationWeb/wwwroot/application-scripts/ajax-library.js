const BEARER_TOKEN_KEY = "token";
const GetToken = () => localStorage.getItem(BEARER_TOKEN_KEY);
class AjaxOperation {
    constructor() { }
    SaveAjax(destination, jsonData) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "post",
                data: jsonData,
                processData: false,
                contentType: false,
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }
                ,error: function (response) {
                    reject(response);
                }
            });
        });
    } 
    
    SavePostAjax(destination, jsonData) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "POST",
                data: jsonData,
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }
                , error: function (response) {
                    reject(response);
                }
            });
        });
    }

    SaveModel(destination, data) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "POST",
                data: JSON.stringify(data),
                contentType: "application/json",
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                },
                error: function (response) {
                    reject(response);
                }
            });

        });
    }

    SavePostAjaxListAPI(destination, jsonData) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "POST",
                data: jsonData,
                contentType: "application/json",
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }
                , error: function (response) {
                    reject(response);
                }
            });
        });
    }
    GetAjax(destination) {
       return new Promise((resolve, reject) => {
           $.ajax({
               url: destination,
               method: "GET",
               dataType: "JSON",
               headers: {
                   Authorization: 'Bearer ' + GetToken()
               },
               success: function (response) {
                   resolve(response);
               }
               , error: function (response) {
                   reject(response);
               }
           });
       });
    }

    GetAjaxHtml = destination =>{
       return new Promise((resolve, reject) => {
           $.ajax({
               url: destination,
               method: "GET",
               dataType: "html",
               headers: {
                   Authorization: 'Bearer ' + GetToken()
               },
               success: function (response) {
                   resolve(response);
               },
               error: function (response) {
                   reject(response);
               }
           });
       })
    }
    DeleteAjaxAPI = destination => {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "DELETE",
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }, error: function (response) {
                    reject(response);
                }
            });
        });
    }

    GetAjaxAPI(destination) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "GET",
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }, error: function (response) {
                    reject(response);
                }
            });
        });
    }

    PostAjaxAPI(destination) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "POST",
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }, error: function (response) {
                    reject(response);
                }
            });
        });
    }

    UpdateAjaxAPI(destination) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: destination,
                type: "PUT",
                headers: {
                    Authorization: 'Bearer ' + GetToken()
                },
                success: function (response) {
                    resolve(response);
                }, error: function (response) {
                    reject(response);
                }
            });
        });
    }
}

const ajaxOperation = new AjaxOperation();

class ModalOperation {
    constructor() { }
    ModalHide(modalId) {
        $(modalId).modal("hide");
    }
    ModalShow(modalId) {
        $(modalId).modal("show");
    }
    ModalOpenWithHtml(modalId, modalDiv, htmlData) {
        $(modalId).modal("show");
        $(modalDiv).html(htmlData);
    }
    ModalClose(modalId, modalDiv) {
        $(modalId).modal("hide");
        $(modalDiv).html("");
    }
    ModalStatic(modalId) {
        $(modalId).modal({
            backdrop: 'static',
            keyboard: false
        });
    }
}

const modalOperation = new ModalOperation();

const modalSelector = {
    id: "#modal-lg",
    tittle: "#tittle",
    body: "#modalDiv",
    saveBtn: "#modalSave",
    setTittle: tittleName => {
        $(tittle).text(tittleName);
    },
};