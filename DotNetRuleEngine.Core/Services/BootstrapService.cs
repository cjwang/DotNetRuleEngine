﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Exceptions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Services
{
    internal sealed class BootstrapService<T> where T : class, new()
    {
        private readonly T _model;
        private readonly Guid _ruleEngineId;
        private readonly IDependencyResolver _dependencyResolver;

        public BootstrapService(T model, Guid ruleEngineId, IDependencyResolver dependencyResolver)
        {
            _model = model;
            _ruleEngineId = ruleEngineId;
            _dependencyResolver = dependencyResolver;
        }

        public IList<IRule<T>> Bootstrap(IList<object> rules)
        {
            Initializer(rules);
            return rules.OfType<IRule<T>>().ToList();
        }

        public async Task<IList<IRuleAsync<T>>> BootstrapAsync(IList<object> rules)
        {
            var initBag = new ConcurrentBag<Task>();
            InitializerAsync(rules, initBag);

            await Task.WhenAll(initBag);

            return rules.OfType<IRuleAsync<T>>().ToList();
        }

        private void Initializer(IList<object> rules,
            IRule<T> nestingRule = null)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = ResolveRule<IRule<T>>(rules[i]);

                rules[i] = rule;

                InitializeRule(rule, nestingRule);

                rule.Initialize();

                if (rule.IsNested) Initializer(rule.GetRules(), rule);
            }
        }

        private void InitializerAsync(IList<object> rules,
            ConcurrentBag<Task> initBag, IRuleAsync<T> nestingRule = null)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = ResolveRule<IRuleAsync<T>>(rules[i]);

                rules[i] = rule;

                InitializeRule(rule, nestingRule);

                initBag.Add(rule.InitializeAsync());

                if (rule.IsNested) InitializerAsync(rule.GetRules(), initBag, rule);
            }
        }

        private void InitializeRule(IGeneralRule<T> rule, IGeneralRule<T> nestingRule = null)
        {
            rule.Model = _model;
            rule.Configuration = new RuleEngineConfiguration<T>(rule.Configuration) { RuleEngineId = _ruleEngineId };

            if (nestingRule != null && nestingRule.Configuration.NestedRulesInheritConstraint)
            {
                rule.Configuration.Constraint = nestingRule.Configuration.Constraint;
                rule.Configuration.NestedRulesInheritConstraint = true;
            }

            var parallelRule = rule as RuleAsync<T>;
            var nestingParallelRule = nestingRule as RuleAsync<T>;

            if (parallelRule != null && parallelRule.IsParallel &&
                nestingParallelRule!= null)
            {
                if (nestingParallelRule.ParellelConfiguration != null &&
                    nestingParallelRule.ParellelConfiguration.NestedParallelRulesInherit)
                {
                    var cancellationTokenSource = parallelRule.ParellelConfiguration.CancellationTokenSource;
                    parallelRule.ParellelConfiguration = new ParallelConfiguration<T>
                    {
                        NestedParallelRulesInherit = true,
                        CancellationTokenSource = cancellationTokenSource,
                        TaskCreationOptions = nestingParallelRule.ParellelConfiguration.TaskCreationOptions,
                        TaskScheduler = nestingParallelRule.ParellelConfiguration.TaskScheduler
                    };
                }
            }

            rule.Resolve = _dependencyResolver;
        }

        private TK ResolveRule<TK>(object ruleObject) where TK : class
        {
            var resolvedRule = default(TK);

            var type = ruleObject as Type;

            if (type != null)
            {
                resolvedRule = _dependencyResolver.GetService(type) as TK;

                if (resolvedRule == null) throw new UnsupportedRuleException(ruleObject.ToString());                
            }

            return (TK)(resolvedRule ?? ruleObject);
        }
    }
}