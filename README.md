# NBasis.OneTable [![NuGet Prerelease Version](https://img.shields.io/nuget/vpre/NBasis.OneTable.svg?style=flat)](https://www.nuget.org/packages/NBasis.OneTable/)

This is an Amazon DynamoDb "One Table" library for dotnet. The goal here is to simplify the "One Table" design approach by using attributed POCO's as the basis for the Key/Attribute schema and providing a native expression basis for Key, Query and Condition expressions.

Requirements:
- You use _Microsoft.Extensions.DependencyInjection.IServiceProvider_ to resolve your services.
- _IAmazonDynamoDb_ must be registered in within _IServiceProvider_
- .Net Core 6 (at the moment)

### Setup

**First**, install the _NBasis.OneTable_ [NuGet package](https://www.nuget.org/packages/NBasis.OneTable) into your app.

```shell
dotnet add package NBasis.OneTable
```

**Next**, add a table context to your project by inheriting from _NBasis.OneTable.TableContext_

```csharp
using NBasis.OneTable;

public class DemoTableContext : TableContext
{     
}

```

**Then**, within your application's setup add the Amazon DynamoDb Client and NBasis.OneTable to your _IServiceCollection_ using the provided extension method and DynamoDb table name.

```csharp

 services.AddSingleton<IAmazonDynamoDB>(new AmazonDynamoDBClient())
         .AddOneTable<DemoTableContext>("<table-name>");

```

The NBasis.OneTable interfaces are now available via _IServiceProvider_. 
