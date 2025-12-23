using Quartz;
using Dapper;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MyWorkerService
{
    public class MyJob : IJob
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MyJob> _logger;

        public MyJob(IConfiguration configuration, ILogger<MyJob> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            string connectionString = _configuration.GetConnectionString("OracleDb") ??
                throw new InvalidOperationException("Connection string 'OracleDb' not found.");

            try
            {
                await using var con = new OracleConnection(connectionString);
                await con.OpenAsync();

                var query = new StoredProcedureQuery
                {
                    LangCode = "EN",
                    SenderId = "123",
                    ProviderCode = "456",
                    SenderTransactionId = "TX123",
                    AcquirerRrn = "RRN123",
                    CustomerIdno = "CID123",
                    ServiceId = "10",
                    PaymentAmount = "100.50",
                    PaymentType = "1",
                };

                var param = QueryParameter(query);

                await con.ExecuteAsync(
                    "API_MICRO_EPAYMENT_PKG.DO_PAYEMENT",
                    param,
                    commandType: CommandType.StoredProcedure
                );

                var paymentDate = param.Get<DateTime?>("OUTP_PAYMENT_DATE");
                var refNo = param.Get<decimal?>("OUTP_REFNO");
                var errCode = param.Get<decimal?>("OUTP_error_code");
                var errMsg = param.Get<string>("OUTP_error_message");

                if (paymentDate.HasValue) Console.WriteLine($"\nPaymentDate = \"{paymentDate}\"");
                else Console.WriteLine("\nPaymentDate = \"NULL\"");

                if (refNo.HasValue) Console.WriteLine($"RefNo = \"{refNo}\"");
                else Console.WriteLine("RefNo = \"NULL\"");

                if (errCode.HasValue) Console.WriteLine($"ErrorCode = \"{errCode}\"");
                else Console.WriteLine("ErrorCode = \"NULL\"");

                if (string.IsNullOrWhiteSpace(errMsg)) Console.WriteLine("Error Message = \"NULL\"\n");
                else Console.WriteLine($"Error Message = \"{errMsg}\"\n");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing DoPaymentJob");
            }
        }

        private static DynamicParameters QueryParameter(StoredProcedureQuery query)
        {
            var param = new DynamicParameters();

            param.Add("INP_LANG_CODE", query.LangCode, DbType.String, ParameterDirection.Input);
            param.Add("INP_SENDER_ID", Convert.ToDecimal(query.SenderId), DbType.Decimal, ParameterDirection.Input);
            param.Add("INP_PROVIDER_CODE", Convert.ToDecimal(query.ProviderCode), DbType.Decimal, ParameterDirection.Input);
            param.Add("INP_SENDER_TRANSACTION_ID", query.SenderTransactionId, DbType.String, ParameterDirection.Input);
            param.Add("INP_ACQUIRER_RRN", query.AcquirerRrn, DbType.String, ParameterDirection.Input);
            param.Add("INP_Customer_Idno", query.CustomerIdno, DbType.String, ParameterDirection.Input);
            param.Add("INP_SERVICE_ID", Convert.ToDecimal(query.ServiceId), DbType.Decimal, ParameterDirection.Input);
            param.Add("INP_PAYMENT_AMOUNT", Convert.ToDecimal(query.PaymentAmount), DbType.Decimal, ParameterDirection.Input);
            param.Add("INP_PAYMENT_TYPE", Convert.ToDecimal(query.PaymentType), DbType.Decimal, ParameterDirection.Input);

            param.Add("OUTP_PAYMENT_DATE", dbType: DbType.Date, direction: ParameterDirection.Output);
            param.Add("OUTP_REFNO", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            param.Add("OUTP_error_code", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            param.Add("OUTP_error_message", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            return param;
        }
    }
}