> **DotNetRuleEngine** allows you to write your code as series of rules to keep your code clean and structured. Supports both **synchronous** and **asynchronous** execution and it is **S.O.L.I.D** compliant.


### A few reasons use DotNetRuleEngine ###
- S.O.L.I.D
- Separation of Concern.
- Easy to maintain.
- Testable code.
- Encapsulates varying behavior. Such as business rules.


```csharp
PM> Install-Package DotNetRuleEngine
```
Nuget package available at: [DotNetRuleEngine](https://www.nuget.org/packages/DotNetRuleEngine "DotNetRuleEngine")

**.NET Core Version available now! [DotNetRuleEngine.Core](https://www.nuget.org/packages/DotNetRuleEngine.Core "DotNetRuleEngine.Core")**


Get Started at: [DotNetRuleEngine Wiki](https://github.com/ayayalar/DotNetRuleEngine/wiki)


## Example

#### Create Rule(s)

*Create a rule to update FreeShipping attribute if the amount is greater than $50.00*

```csharp
public class QualifiesForFreeShipping: Rule<Order>
{   
    public override IRuleResult Invoke()
    {
        // Order instance available through the model property.
        if (Model.Total > 50.0m)
        {
            Model.FreeShipping = true;
        }
        
        return null;
    }
}
```

#### Invoke Rule(s)

```csharp    

Order order = new Order { Id = 1, Total = 79.99 };

// Pass order instance to RuleEngine.
var ruleResults = RuleEngine<Order>.GetInstance(order)
    .ApplyRules(new QualifiesForFreeShipping())
    .Execute()
```

###### Model used as an example

```csharp
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public bool FreeShipping { get; set; }
}
```
