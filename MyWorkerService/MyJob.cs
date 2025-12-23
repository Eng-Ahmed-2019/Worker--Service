using Dapper;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace MyWorkerService
{
    public class MyJob : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MyJob> _logger;

        public MyJob(
            IConfiguration configuration,
            ILogger<MyJob> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background Service running now.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await BackgroundServiceTask(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Background service task was canceled.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while executing the background service task.");
                }
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task BackgroundServiceTask(CancellationToken stoppingToken)
        {
            string connectionString =
                _configuration.GetConnectionString("OracleDb")
                ?? throw new InvalidOperationException("Connection string 'OracleDb' not found.");

            await using var con = new OracleConnection(connectionString);
            await con.OpenAsync(stoppingToken);

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

            Console.WriteLine($"\n---------------- Execution #{GetCounter.GetNextCounter()} ----------------\n");

            Console.WriteLine($"PaymentDate = \"{paymentDate?.ToString()}\"");
            Console.WriteLine($"RefNo = \"{refNo?.ToString()}\"");
            Console.WriteLine($"ErrorCode = \"{errCode?.ToString()}\"");
            if (errMsg != null) Console.WriteLine($"Error Message = \"{errMsg}\"\n");
            else Console.WriteLine($"Error Message = \"Null\"\n");
        }
        private static DynamicParameters QueryParameter(StoredProcedureQuery query)
        {
            var param = new DynamicParameters();

            param.Add("INP_LANG_CODE", query.LangCode, DbType.String);
            param.Add("INP_SENDER_ID", Convert.ToDecimal(query.SenderId), DbType.Decimal);
            param.Add("INP_PROVIDER_CODE", Convert.ToDecimal(query.ProviderCode), DbType.Decimal);
            param.Add("INP_SENDER_TRANSACTION_ID", query.SenderTransactionId, DbType.String);
            param.Add("INP_ACQUIRER_RRN", query.AcquirerRrn, DbType.String);
            param.Add("INP_Customer_Idno", query.CustomerIdno, DbType.String);
            param.Add("INP_SERVICE_ID", Convert.ToDecimal(query.ServiceId), DbType.Decimal);
            param.Add("INP_PAYMENT_AMOUNT", Convert.ToDecimal(query.PaymentAmount), DbType.Decimal);
            param.Add("INP_PAYMENT_TYPE", Convert.ToDecimal(query.PaymentType), DbType.Decimal);

            param.Add("OUTP_PAYMENT_DATE", dbType: DbType.Date, direction: ParameterDirection.Output);
            param.Add("OUTP_REFNO", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            param.Add("OUTP_error_code", dbType: DbType.Decimal, direction: ParameterDirection.Output);
            param.Add("OUTP_error_message", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            return param;
        }
    }
    public class GetCounter()
    {
        static int counter = 0;
        public static int GetNextCounter()
        {
            counter++;
            return counter;
        }
    }
}