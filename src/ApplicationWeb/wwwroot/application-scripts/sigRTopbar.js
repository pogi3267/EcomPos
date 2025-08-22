"use strict";

var connection1 = new signalR.HubConnectionBuilder().withUrl("/dashboardHub").build();

$(function () {
    connection1.start().then(function () {
        InvokeNotification();
    }).catch(function (err) {
        return console.error(err.toString());
    });
});

function InvokeNotification() {
    connection1.invoke("ProductNotification").catch(function (err) {
        return console.error(err.toString());
    });
}
connection1.on("ProductNotification", function (products) {
    BindProductNotification(products);
});
function BindProductNotification(response) {
    $("#Notibadgewarning").text(response.length);
   
    if (response.length > 0) {
        var audio = $('#notificationAudio')[0];
        audio.currentTime = 0;
        audio.play();
    }
    var dropdownMenu = document.getElementById('OrderNotification');

    dropdownMenu.innerHTML = '';

    response.forEach(item => {
        const headerSpan = document.createElement('span');
        headerSpan.className = 'dropdown-item dropdown-header';
        dropdownMenu.appendChild(headerSpan);


        const divider = document.createElement('div');
        divider.className = 'dropdown-divider';
        dropdownMenu.appendChild(divider);


        const dropdownItem = document.createElement('a');
        dropdownItem.href = '/Admin/Orders/OrderList?id=32&orderId=' + item.ordersId;
        dropdownItem.className = 'dropdown-item';


        const icon = document.createElement('i');
        icon.className = 'fab fa-first-order mr-2';
    

        const messageText = document.createTextNode(item.notificationMessage);


        const timeSpan = document.createElement('span');
        timeSpan.className = 'float-right text-muted text-sm';
        timeSpan.textContent = item.timeSinceNotification;


        dropdownItem.appendChild(icon);
        dropdownItem.appendChild(messageText);
        dropdownItem.appendChild(timeSpan);


        dropdownMenu.appendChild(dropdownItem);


    });
}












