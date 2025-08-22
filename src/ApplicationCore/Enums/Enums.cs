using System.ComponentModel;

namespace ApplicationCore.Enums
{
    public enum AccountVoucherType
    {
        [Description("Received")]
        RECEIEVED = 1,

        [Description("Payment")]
        PAYMENT = 2,

        [Description("Journal")]
        JOURNAL = 3
    }

    public enum AmountType
    {
        [Description("Debit Amount")]
        DEBIT_AMOUNT = 1,

        [Description("Credit Amount")]
        CREDIT_AMOUNT = 2,

        [Description("Debit And Credit Amount")]
        DEBITANDCREDITAMOUNT = 3

    }

    public enum ProcessType
    {
        [Description("Processed Products")]
        Pocessed_Products = 1,

        [Description("Unprocessed Products")]
        Unprocessed_Products = 2
    }

    public enum OperationalStatus
    {
        PENDING = 1,
        APPROVED = 2,
        ACKNOWLEDGE = 3,
        REJECTED = 4
    }
}