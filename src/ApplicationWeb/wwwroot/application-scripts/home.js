(function () {
    // Function to get business data
    const GetAllBusinessData = async (status) => {
        try {
            let response = await $.ajax({
                url: '/Admin/DashBoard/GetAllBusinessAnalyticsData',
                method: 'GET',
                data: { status: status }
            });
            if (typeof response === "object") {
                // Update the UI with the received data
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
        } catch (e) {
            console.log(e);
        }
    }

    // Function to get admin wallet data
    const GetAdminWalletData = async (status) => {
        try {
            let response = await $.ajax({
                url: '/Admin/DashBoard/GetAdminWalletIdData',
                method: 'GET',
                data: { status: status }
            });
            if (typeof response === "object") {
                // Update the UI with the received data
                $('#TotalCollection').text(response.totalCollection);
                $('#TotalDeliveryCharge').text(response.totalDeliveryCharge);
                $('#TotalTaxCollection').text(response.totalTaxCollection);
                $('#PendingAmount').text(response.pendingAmount);
                $('#TotalRevenue').text(response.totalRevenue);
            }
        } catch (e) {
            console.log(e); 
        }
    }

    $(document).ready(function () {
        
        $("#businessId").on("change", function () {
            let businessId = $(this).val();
            if (businessId) {
                GetAllBusinessData(businessId);
                GetAdminWalletData(businessId); 
            }
        });

    });
})();
