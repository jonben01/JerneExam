namespace Tests.DatabaseUtil;

public class SeedUsers
{
    public static readonly Guid AdminId      = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ActiveRichId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid ActivePoorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid InactiveId   = Guid.Parse("44444444-4444-4444-4444-444444444444");

    public const string AdminEmail      = "admin@test.local";
    public const string ActiveRichEmail = "rich@test.local";
    public const string ActivePoorEmail = "poor@test.local";
    public const string InactiveEmail   = "inactive@test.local";
}