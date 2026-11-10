using SmartPantry.Warnings;
using Xunit;

namespace SmartPantry.EntityFrameworkCore.Domains;

[Collection(SmartPantryTestConsts.CollectionDefinitionName)]
public class EfCoreExpirationWarningDomainService_Tests
    : ExpirationWarningDomainService_Tests<SmartPantryEntityFrameworkCoreTestModule>
{
}