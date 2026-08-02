TRACE  POST /login
       src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:90

▸ ENTRY  POST /login  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:90)
   └─ call <lambda> POST /login  (src/Identity/Core/src/IdentityApiEndpointRouteBuilderExtensions.cs:90)
      └─ data User<TKey> [approx]
         └─ data Microsoft.AspNetCore.Identity.EntityFrameworkCore.Test.CustomDbContext  (src/Identity/EntityFrameworkCore/test/EF.Test/CustomPocoTest.cs:18)

TOUCHES  User<TKey>
RESULT   200 OK / 201 Created · failure → 400 Bad Request
