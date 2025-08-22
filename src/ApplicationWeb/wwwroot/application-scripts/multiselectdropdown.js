function createDropdown(id, data) {
    const selectElement = document.getElementById(id);

    data.forEach((item) => {
        const option = document.createElement('option');
        option.value = item.value;
        option.text = item.label;
        selectElement.appendChild(option);
    });

    $(`#${id}`).select2({
        width: '100%',
        placeholder: 'Select an option',
        allowClear: true,
        data: data
    });
}

