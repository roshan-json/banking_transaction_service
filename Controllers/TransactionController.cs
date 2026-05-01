using banking_transaction_service.Models.Requests;
using banking_transaction_service.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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

        /// <summary>
        /// Retrieve a single transaction by its transaction identifier.
        /// </summary>
        /// <param name="transactionId">The unique transaction id.</param>
        [ProducesResponseType(typeof(Models.TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetTransaction(int transactionId)
        {
            var result = await myService.GetTransaction(transactionId);

            if (result is null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Retrieve all transactions for a specific account.
        /// </summary>
        /// <param name="accountId">The account id used to filter transactions.</param>
        [ProducesResponseType(typeof(IEnumerable<Models.TransactionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] int accountId)
        {
            var result = await myService.GetTransactions(accountId);

            if (result is null)
            {
                return NotFound("Transaction not found");
            }

            return Ok(result);
        }

        /// <summary>
        /// Create a new transaction with idempotency support.
        /// </summary>
        /// <param name="request">The transaction create request containing amount, account, and metadata.</param>
        [ProducesResponseType(typeof(Models.TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
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

        /// <summary>
        /// Update one or more fields on an existing transaction.
        /// </summary>
        /// <param name="transactionId">The id of the transaction to patch.</param>
        /// <param name="request">The partial update payload.</param>
        [ProducesResponseType(typeof(Models.TransactionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{transactionId}")]
        public async Task<IActionResult> UpdateTransaction(int transactionId, [FromBody] UpdateTransactionRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null");
            }

            var updated = await myService.UpdateTransaction(transactionId, request);

            if (updated == null)
            {
                return NotFound($"Transaction with txnId {transactionId} not found");
            }

            return Ok(updated);
        }
    }
}