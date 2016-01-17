﻿using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core
{
    public abstract class NestedRuleAsync<T> : RuleEngine<T>,
        IRuleAsync<T> where T : class, new()
    {
        public ConcurrentDictionary<string, Task<object>> Data { get; set; } = new ConcurrentDictionary<string, Task<object>>();

        public Expression<Predicate<T>> Constraint { get; set; }

        public bool Terminate { get; set; }

        public bool Skip { get; set; }

        public int? ExecutionOrder { get; set; }

        public virtual void SetExecutionOrder()
        {
        }

        public async Task<object> TryGetValueAsync(string key)
        {
            Task<object> value;
            return Data.TryGetValue(key, out value) ? await value : null;
        }

        public bool TryAddAsync(string key, Task<object> value)
        {
            return Data.TryAdd(key, value);
        }

        public virtual async Task BeforeInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public virtual async Task AfterInvokeAsync()
        {
            await Task.FromResult<object>(null);
        }

        public abstract Task<IRuleResult> InvokeAsync(T product);
        

        public bool Parallel { get; set; }
    }
}