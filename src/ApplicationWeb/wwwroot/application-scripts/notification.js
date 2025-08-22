const Success = message => {
    toastr.success(message, 'Success!');
}
$("#Notibadgewarning").text("ertertdfgdfg");
const Failed = message => {
    if (typeof (message) == "object") {
        const { status } = message;
        if (status == 403) toastr.error("You are not authorized");
        else if (status == 404) toastr.error("The server cannot find the requested resource");
        else toastr.error(message.responseText);

        return;
    }
    toastr.error(message);
}

const ValidationError = message => {
    Swal.fire(
        'Error!',
        message,
        'error'
    );
}

const Decision = (title = 'Are you sure want to delete this?', text = "You won't be able to revert this!", buttonText = 'Yes, delete it!') => {
    return new Promise((resolve, reject) => {
        Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: buttonText,
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed) {
                resolve(result.isConfirmed);
            }
            else
                reject(result);
        });
    });
}

const ConfirmationModal = (title = 'Are you sure want to delete this?') => {
    return new Promise((resolve, reject) => {
        // Create a div element for the modal
        const modalDiv = document.createElement('div');
        modalDiv.classList.add('modal');
        modalDiv.id = 'myModal';

        // Create a div element for the modal dialog
        const modalDialogDiv = document.createElement('div');
        modalDialogDiv.classList.add('modal-dialog', 'modal-sm');

        // Create a div element for the modal content
        const modalContentDiv = document.createElement('div');
        modalContentDiv.classList.add('modal-content');

        // Create the modal header
        const modalHeaderDiv = document.createElement('div');
        modalHeaderDiv.classList.add('modal-header');
        const modalHeaderTitle = document.createElement('h6');
        modalHeaderTitle.classList.add('modal-title');
        modalHeaderTitle.textContent = title;
        modalHeaderDiv.appendChild(modalHeaderTitle);

        // Create the modal footer
        const modalFooterDiv = document.createElement('div');
        modalFooterDiv.classList.add('modal-footer');
        const okButton = document.createElement('button');
        okButton.type = 'button';
        okButton.classList.add('btn', 'btn-success');
        okButton.textContent = 'Ok';
        okButton.id = 'myModalBtnOk';
        const closeButton = document.createElement('button');
        closeButton.type = 'button';
        closeButton.classList.add('btn', 'btn-danger');
        closeButton.setAttribute('data-bs-dismiss', 'modal');
        closeButton.textContent = 'Close';
        modalFooterDiv.appendChild(okButton);
        modalFooterDiv.appendChild(closeButton);

        modalContentDiv.appendChild(modalHeaderDiv);
        modalContentDiv.appendChild(modalFooterDiv);
        modalDialogDiv.appendChild(modalContentDiv);
        modalDiv.appendChild(modalDialogDiv);

        document.body.appendChild(modalDiv);

        $('#myModal').modal('show');

        okButton.addEventListener('click', () => {
            $('#myModal').modal('hide'); 
            modalDiv.remove();
            resolve(true); 
        });

        closeButton.addEventListener('click', () => {
            $('#myModal').modal('hide');
            modalDiv.remove(); 
            reject(false);
        });
    });
};