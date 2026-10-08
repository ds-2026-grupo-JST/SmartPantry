using SmartPantry.Products;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Applications;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreProductAppService_Tests : ProductAppService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}