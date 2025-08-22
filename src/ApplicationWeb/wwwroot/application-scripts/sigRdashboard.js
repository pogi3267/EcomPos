"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/dashboardHub").build();

$(function () {
    connection.start().then(function () {
        InvokeProducts();
        InvokeAdminWallet();
        InvokeProductsAndCustomer();
    }).catch(function (err) {
        return console.error(err.toString());
    });
});

// Product
function InvokeProducts() {
    connection.invoke("SendProducts").catch(function (err) {
        return err;
    });
}

connection.on("ReceivedProducts", function (products) {
    BindProductsToGrid(products);
});

function BindProductsToGrid(response) {

    $('#TotalOrders').text(response.totalOrders);
    $('#TotalStores').text(response.totalStores);
    $('#TotalProducts').text(response.totalProducts);
    $('#TotalCustomers').text(response.totalCustomers);
    $('#Pending').text(response.pending);
    $('#Packaging').text(response.packaging);
    $('#Delivered').text(response.delivered);
    $('#Returned').text(response.returned);
    $('#Confirmed').text(response.confirmed);
    $('#Outfordelivery').text(response.outForDelivery);
    $('#Canceled').text(response.canceled);
    $('#Failedtodelivery').text(response.failedToDeliver);
}
connection.on("ReceivedSalesForGraph", function (salesForGraph) {
    BindSalesToGraph(salesForGraph);
});
connection.on("PaymentGraph", function (PaymentForGraph) {
    BindPaymentGraph(PaymentForGraph);
});
function BindSalesToGraph(salesForGraph) {
    const labels = ['JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN', 'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC'];
    const currentYearData = Array(12).fill(0); // Default values for each month
    const lastYearData = Array(12).fill(0);    // Default values for each month

    $.each(salesForGraph, function (index, item) {
        const monthIndex = item.orderMonth - 1; // Convert month to zero-based index

        if (item.yearIndicator === 1) {
            currentYearData[monthIndex] = item.totalAmounts;
        } else {
            lastYearData[monthIndex] = item.totalAmounts;
        }
    });

    renderChart(currentYearData, lastYearData);
}

function renderChart(currentYearData, lastYearData) {
    var ticksStyle = {
        fontColor: '#495057',
        fontStyle: 'bold'
    };

    var mode = 'index';
    var intersect = true;

    var $salesChart = $('#sales-chart');
    var salesChart = new Chart($salesChart, {
        type: 'bar',
        data: {
            labels: ['JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN', 'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC'],
            datasets: [
                {
                    backgroundColor: '#007bff',
                    borderColor: '#007bff',
                    data: currentYearData
                },
                {
                    backgroundColor: '#ced4da',
                    borderColor: '#ced4da',
                    data: lastYearData
                }
            ]
        },
        options: {
            maintainAspectRatio: false,
            tooltips: {
                mode: mode,
                intersect: intersect
            },
            hover: {
                mode: mode,
                intersect: intersect
            },
            legend: {
                display: false
            },
            scales: {
                yAxes: [{
                    
                }],
                xAxes: [{
                    display: true,
                    gridLines: {
                        display: false
                    },
                    ticks: ticksStyle
                }]
            }
        }
    });
}

function BindPaymentGraph(PaymentForGraph) {

    var paymentData = [];
    var labels = [];

    $.each(PaymentForGraph, function (index, item) {
        labels.push(item.paymentType);
        paymentData.push(item.totalAmount);
    });

    paymentChart(paymentData, labels);
}

function paymentChart(paymentData, labels) {
    var pieChartCanvas = $('#pieChart').get(0).getContext('2d')
    var pieData = {
        labels: labels,
        datasets: [
            {
                data: paymentData,
                backgroundColor: ['#f56954', '#00a65a', '#f39c12']
            }
        ]
    }
    var pieOptions = {
        legend: {
            display: false
        }
    }
    var pieChart = new Chart(pieChartCanvas, {
        type: 'doughnut',
        data: pieData,
        options: pieOptions
    })
}


// Helper function to convert month number to month name
function getMonthName(month) {
    const monthNames = ["JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"];
    return monthNames[month - 1];
}

// Admin Wallet
function InvokeAdminWallet() {
    connection.invoke("SendProducts").catch(function (err) {
        return err;
    });
}

connection.on("AdminWallet", function (products) {
    BindAdminWallet(products);
});

function BindAdminWallet(response) {
    $('#TotalCollection').text(response.totalCollection);
    $('#TotalDeliveryCharge').text(response.totalDeliveryCharge);
    $('#TotalTaxCollection').text(response.totalTaxCollection);
    $('#PendingAmount').text(response.pendingAmount);
    $('#TotalRevenue').text(response.totalRevenue);
}

// Products And Customer
function InvokeProductsAndCustomer() {
    connection.invoke("SendProductAndCustomer").catch(function (err) {
        return console.error(err.toString());
    });
}

connection.on("ProductsAndCustomer", function (products) {
    BindProductsAndCustomer(products);
});

function BindProductsAndCustomer(response) {
    // Assuming response is an array of objects with properties id, name, description, imageUrl, and price
    const topSellingproductList = document.getElementById('topSellinglistItem');
    const recentlyItemList = document.getElementById('recentlylistItem');
    const topCustomerList = document.getElementById('topCustomers');

    topSellingproductList.innerHTML = '';
    recentlyItemList.innerHTML = '';
    topCustomerList.innerHTML = '';

    response.topSellingItems.forEach(item => {
        const li = document.createElement('li');
        li.classList.add('item');

        const productImgDiv = document.createElement('div');
        productImgDiv.classList.add('product-img');
        const img = document.createElement('img');
        img.src = item.thumbnailFileName;
        img.classList.add('img-size-50');
        productImgDiv.appendChild(img);

        const productInfoDiv = document.createElement('div');
        productInfoDiv.classList.add('product-info');

        const productTitleLink = document.createElement('a');
        productTitleLink.href = 'javascript:void(0)';
        productTitleLink.classList.add('product-title');
        productTitleLink.innerHTML = `${item.name} <span class="badge badge-warning float-right">${item.totalQuantitySold}</span>`;

        productInfoDiv.appendChild(productTitleLink);

        li.appendChild(productImgDiv);
        li.appendChild(productInfoDiv);

        topSellingproductList.appendChild(li);
    });

    response.recentlyCreatedProducts.forEach(item => {
        const li = document.createElement('li');
        li.classList.add('item');

        const productImgDiv = document.createElement('div');
        productImgDiv.classList.add('product-img');
        const img = document.createElement('img');
        img.src = item.thumbnailFileName;
        img.classList.add('img-size-50');
        productImgDiv.appendChild(img);

        const productInfoDiv = document.createElement('div');
        productInfoDiv.classList.add('product-info');

        const productTitleLink = document.createElement('a');
        productTitleLink.href = 'javascript:void(0)';
        productTitleLink.classList.add('product-title');
        productTitleLink.innerHTML = `${item.name} <span class="badge badge-warning float-right">$${item.unitPrice}</span>`;

        productInfoDiv.appendChild(productTitleLink);

        li.appendChild(productImgDiv);
        li.appendChild(productInfoDiv);

        recentlyItemList.appendChild(li);
    });

    response.topCustomer.forEach(item => {
        const li = document.createElement('li');
        li.classList.add('item');

        const customerImgDiv = document.createElement('div');
        customerImgDiv.classList.add('product-img');
        const img = document.createElement('img');
        img.src = item.customerImg;
        img.classList.add('img-size-50');
        customerImgDiv.appendChild(img);

        const customerInfoDiv = document.createElement('div');
        customerInfoDiv.classList.add('product-info');

        const customerTitleLink = document.createElement('a');
        customerTitleLink.href = 'javascript:void(0)';
        customerTitleLink.classList.add('product-title');
        customerTitleLink.innerHTML = `${item.customerName} <span class="badge badge-warning float-right">$${item.totalSpent}</span>`;


        customerInfoDiv.appendChild(customerTitleLink);

        li.appendChild(customerImgDiv);
        li.appendChild(customerInfoDiv);

        topCustomerList.appendChild(li);
    });
}

// supporting functions for Graphs
function DestroyCanvasIfExists(canvasId) {
    let chartStatus = Chart.getChart(canvasId);
    if (chartStatus != undefined) {
        chartStatus.destroy();
    }
}

var backgroundColors = [
    'rgba(255, 99, 132, 0.2)',
    'rgba(54, 162, 235, 0.2)',
    'rgba(255, 206, 86, 0.2)',
    'rgba(75, 192, 192, 0.2)',
    'rgba(153, 102, 255, 0.2)',
    'rgba(255, 159, 64, 0.2)'
];
var borderColors = [
    'rgba(255, 99, 132, 1)',
    'rgba(54, 162, 235, 1)',
    'rgba(255, 206, 86, 1)',
    'rgba(75, 192, 192, 1)',
    'rgba(153, 102, 255, 1)',
    'rgba(255, 159, 64, 1)'
];







