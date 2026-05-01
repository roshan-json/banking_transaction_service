using System;
using banking_transaction_service.Models;
using banking_transaction_service.Models.Dtos;
using banking_transaction_service.Models.Requests;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace banking_transaction_service.Swagger
{
    public class SwaggerExamplesSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema == null || context == null || context.Type == null)
            {
                return;
            }

            switch (context.Type)
            {
                case Type t when t == typeof(CreateTransactionRequest):
                    schema.Example = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("WITHDRAWAL"),
                        ["accountId"] = new OpenApiInteger(123456),
                        ["amount"] = new OpenApiDouble(250.75),
                        ["counterParty"] = new OpenApiString("Acme Billing"),
                        ["reference"] = new OpenApiString("Invoice #A-1234"),
                        ["idempotencyKey"] = new OpenApiString("3f5f0f42-1d07-4a2e-9d9a-8d31f0c76b3c")
                    };
                    break;

                case Type t when t == typeof(UpdateTransactionRequest):
                    schema.Example = new OpenApiObject
                    {
                        ["amount"] = new OpenApiDouble(300.00),
                        ["reference"] = new OpenApiString("Updated payment reference"),
                        ["counterParty"] = new OpenApiString("Acme Billing"),
                        ["type"] = new OpenApiString("DEPOSIT")
                    };
                    break;

                case Type t when t == typeof(TransactionResponse) || t == typeof(TransactionDto):
                    schema.Example = new OpenApiObject
                    {
                        ["id"] = new OpenApiInteger(1001),
                        ["type"] = new OpenApiString("WITHDRAWAL"),
                        ["accountId"] = new OpenApiInteger(123456),
                        ["amount"] = new OpenApiDouble(250.75),
                        ["counterParty"] = new OpenApiString("Acme Billing"),
                        ["reference"] = new OpenApiString("Invoice #A-1234"),
                        ["createdAt"] = new OpenApiString(DateTime.UtcNow.ToString("o"))
                    };
                    break;

                default:
                    break;
            }
        }
    }
}
