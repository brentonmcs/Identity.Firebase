using Aguacongas.Firebase;
using Aguacongas.Firebase.TokenManager;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.Identity.Firebase.IntegrationTest
{
    public class UserStoreTest : IdentitySpecificationTestBase<TestUser, TestRole>, IClassFixture<FirebaseTestFixture>
    {
        private readonly FirebaseTestFixture _fixture;

        public UserStoreTest(FirebaseTestFixture fixture)
        {
            _fixture = fixture;
        }

        protected override void AddUserStore(IServiceCollection services, object context = null)
        {
            services.Configure<EmailPasswordOptions>(options =>
            {
                _fixture.Configuration.GetSection("EmailPasswordOptions").Bind(options);
            });

            services.Configure<FirebaseOptions>(options =>
            {
                options.DatabaseUrl = _fixture.FirebaseOptions.DatabaseUrl;
            });

            var userType = typeof(TestUser);
            var userStoreType = typeof(UserStore<,>).MakeGenericType(userType, typeof(TestRole));
            services.TryAddSingleton(typeof(IUserStore<>).MakeGenericType(userType), userStoreType);

            services.AddSingleton<HttpClient>()
                .AddSingleton<IFirebaseClient, FirebaseClient>()
                .AddSingleton<IFirebaseTokenManager, EmailPasswordTokenManager>()
                .AddSingleton<ILookupNormalizer, FirebaseLookupNormalizer>();
        }

        protected override void AddRoleStore(IServiceCollection services, object context = null)
        {
            var roleType = typeof(TestRole);
            var roleStoreType = typeof(RoleStore<>).MakeGenericType(roleType);
            services.TryAddSingleton(typeof(IRoleStore<>).MakeGenericType(roleType), roleStoreType);
        }

        protected override object CreateTestContext()
        {
            return null;
        }

        protected override TestUser CreateTestUser(string namePrefix = "", string email = "", string phoneNumber = "",
            bool lockoutEnabled = false, DateTimeOffset? lockoutEnd = default(DateTimeOffset?), bool useNamePrefixAsUserName = false)
        {
            return new TestUser
            {
                UserName = useNamePrefixAsUserName ? namePrefix : string.Format("{0}{1}", namePrefix, Guid.NewGuid()),
                Email = email,
                PhoneNumber = phoneNumber,
                LockoutEnabled = lockoutEnabled,
                LockoutEnd = lockoutEnd
            };
        }

        protected override TestRole CreateTestRole(string roleNamePrefix = "", bool useRoleNamePrefixAsRoleName = false)
        {
            var roleName = useRoleNamePrefixAsRoleName ? roleNamePrefix : string.Format("{0}{1}", roleNamePrefix, Guid.NewGuid());
            return new TestRole(roleName);
        }

        protected override void SetUserPasswordHash(TestUser user, string hashedPassword)
        {
            user.PasswordHash = hashedPassword;
        }

        protected override Expression<Func<TestUser, bool>> UserNameEqualsPredicate(string userName) => u => u.UserName == userName;

        protected override Expression<Func<TestRole, bool>> RoleNameEqualsPredicate(string roleName) => r => r.Name == roleName;

        protected override Expression<Func<TestRole, bool>> RoleNameStartsWithPredicate(string roleName) => r => r.Name.StartsWith(roleName);

        protected override Expression<Func<TestUser, bool>> UserNameStartsWithPredicate(string userName) => u => u.UserName.StartsWith(userName);
    }
}
