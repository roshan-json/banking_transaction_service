using banking_transaction_service.Models.Requests;
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

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] TransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
            {
                return BadRequest("IdempotencyKey is required");
            }

            var result = await myService.CreateTransaction(request);
            return Ok(result);
        }
    }
}