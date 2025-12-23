namespace MyWorkerService
{
    public class StoredProcedureQuery
    {
        public string? LangCode { set; get; } = "EN";
        public string SenderId { set; get; } = "123";
        public string ProviderCode { set; get; } = "456";
        public string? SenderTransactionId { set; get; } = "TX123";
        public string? AcquirerRrn { set; get; } = "RRN123";
        public string? CustomerIdno { set; get; } = "CID123";
        public string? ServiceId { set; get; } = "10";
        public string? PaymentAmount { set; get; } = "100.50";
        public string? PaymentType { set; get; } = "1";
    }
}