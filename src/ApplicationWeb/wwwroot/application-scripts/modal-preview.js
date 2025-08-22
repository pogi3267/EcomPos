
let imageCounter = {
    ALL: "ALL",
    PARTIAL: "PARTIAL",
    COUNTER: 0,
    SETTER: '',
    COLUMN_SIZE : 'col-sm-2',
}
let initialHTML = null;
let complexImage = new Array();
let imageSource = new Array();

let modalPreviewSelector = {
    uploaderModal: "#uploaderModal",
    allDirectoryImage: $("#allDirectoryImage"),
    numberOfFileSelected: $("#numberOfFileSelected"),
    imageSourceClear: "#imageSourceClear",
    addFilesToBrowse: "#addFilesToBrowse",
    deleteFiles: "#deleteFiles",
    imageUploader: '.imageUploader',
}
class ModalPreview {
    GetAllSavedImage() {
        let response = ajaxOperation.GetAjaxHtml("/Admin/Common/GetAllDirectoryImages");
        response.then(response => {
            imageSource = new Array();
            initialHTML = response;
            modalPreviewSelector.numberOfFileSelected.text("0 File Selected.");
            modalPreviewSelector.allDirectoryImage.html("");
            modalPreviewSelector.allDirectoryImage.html(response);
        }).catch(error => { console.log(error); });
    }
    UpdateCheckedRelated(isChecked, source) {
        if (isChecked) {
            imageSource.push(source);
        }
        else {
            imageSource = imageSource.filter(item => item.uploadId !== source.uploadId);
        }
        modalPreviewSelector.numberOfFileSelected.text(imageSource.length + " File Selected.");
    }
    PopulateImageLoader(divisionId, counter, sourceList) {
        if (sourceList.length == 0 || sourceList == undefined) return;
        let data = '';
        if (counter === imageCounter.ALL)
            sourceList.forEach(item => data += `<div class="${imageCounter.COLUMN_SIZE}"><img src="${item.imageSource}" height="90" width="90" /></div>`)
        else {
            for (var i = 0; i < imageCounter.COUNTER; i++) {
                const item = sourceList[i];
                data += `<div class="${imageCounter.COLUMN_SIZE}"><img src="${item.imageSource}" height="90" width="90" /></div>`
            }
        }
        let finalDiv = `<div class="row">${data}</div>`;
        $(divisionId).html(finalDiv);
    }
    ImageSetter (counter) {
        if (counter == imageCounter.ALL && imageSource.length > 0) {
            const photoSetter = imageSource.map(item => { return item.uploadId; });
            $(imageCounter.SETTER).val(photoSetter.join(","));
        }
        else if (counter == imageCounter.PARTIAL && imageSource.length > 0) {
            $(imageCounter.SETTER).val(imageSource[0].uploadId);
        }
    }
}
let modalPreview = new ModalPreview();

$(document).on("click", modalPreviewSelector.imageUploader, function () {
    let imageSource = $(this).attr("imageSource");
    let uploadId = $(this).attr("uploadId");

    modalPreview.UpdateCheckedRelated($(this).is(':checked'), { imageSource, uploadId });
});

$(document).on("click", modalPreviewSelector.imageSourceClear, function(){
    imageSource = new Array();
    modalPreviewSelector.numberOfFileSelected.text("0 File Selected.");
    modalPreviewSelector.allDirectoryImage.html(initialHTML);
});

(function () {
    const uploadPhoto = document.getElementById("fileInputButton");
    const photoUploader = document.getElementById("photoUpload");
    const filePreviewer = document.getElementById("uploadFilePreviewer");
    const fileUpload = document.getElementById("fileUpload");
    const deleteFiles = document.getElementById("deleteFiles");

    const UploadImage = async formData => {
        try {
            let response = await ajaxOperation.SaveAjax("/api/Product/SaveImage", formData);
            modalPreview.GetAllSavedImage();
            filePreviewer.innerHTML = '';
            Success("Image Uploaded Successfully");
        } catch (e) {
            Failed(e.responseText);
        }
    }

    const DeleteImage = async (ids) => {
        let decisionResult = await Decision();
        try {
            if (decisionResult) {
                let response = await ajaxOperation.DeleteAjaxAPI("/api/Product/DeleteUploadImg/" + ids);
                Success("Deleted Successfully!");
                imageSource = new Array();
                modalPreviewSelector.numberOfFileSelected.text("0 File Selected.");
                modalPreviewSelector.allDirectoryImage.html(initialHTML);
                modalPreview.GetAllSavedImage();
                filePreviewer.innerHTML = '';
            }
        } catch (e) {
            Failed(e);
        }
    }

    uploadPhoto.addEventListener("click", function () {
        $("#photoUpload").trigger("click");
    });


    photoUploader.addEventListener('change', (event) => {
        const fileList = event.target.files;
        const fileLength = event.target.files.length;
        let data = '';
        let unitOfProgress = Math.ceil((100 / fileLength));
        let currentProgress = unitOfProgress;
        $("#animatedProgressbar").show();
        for (let i = 0; i < fileLength; i++) {
            const objectUrl = URL.createObjectURL(fileList[i]);
            data += `<div class="col-sm-2" style = "margin: 8px -69px 4px 0;">
                <img src = "${objectUrl}" height = "100" width = "100" />
            </div>`;
        }
        filePreviewer.innerHTML = data;
    });

    fileUpload.addEventListener("click", function () {
        let formData = new FormData();
        for (let i = 0; i < photoUploader.files.length; i++) {
            formData.append("UploadImage[]", photoUploader.files[i]);
        }
        UploadImage(formData);
    });

    deleteFiles.addEventListener("click", function () {
        if (imageSource.length == 0) {
            Failed("Please select image(s)!");
            return;
        }
        else {
            var ids = "";
            for (var i = 0; i < imageSource.length; i++) {
                if (i == 0) ids += imageSource[i].uploadId;
                else ids += "," + imageSource[i].uploadId;
            }
            DeleteImage(ids);
        }
    });

})();