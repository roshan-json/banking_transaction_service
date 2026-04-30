using banking_transaction_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace banking_transaction_service.Controllers
{
    [ApiController]
    [Route("transactions")]
    public class TransactionController : ControllerBase
    {
        private readonly Back4AppService myService;

        public TransactionController(Back4AppService service)
        {
            myService = service;
        }

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetTransactionsByAccountId(int transactionId)
        {
            var result = await myService.GetTransaction(transactionId);

            if (result is null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(result);
        }
    }
}