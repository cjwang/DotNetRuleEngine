﻿using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductRuleAsync : RuleAsync<Product>
    {
        public override Task BeforeInvokeAsync()
        {
            TryAddAsync("Description", Task.FromResult<object>("Description"));

            return Task.FromResult<object>(null);
        }

        public override async Task<IRuleResult> InvokeAsync(Product product)
        {
            var description = TryGetValueAsync("Description").Result.To<string>();
            product.Description = $"Product {description}";

            return await Task.FromResult(new RuleResult { Name = "ProductRule", Result = product.Description, Data = { { "Description", description } } });
        }
    }
}