using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Api.Services;
using Api.Repositories;

namespace Tests.Services
{
    namespace Tests.Services.RoleServiceTests {
        
        public class RoleService_GetRolesAsync
        {
            private IRoleService _roleService;

            [SetUp]
            public void Setup()
            {
                _roleService = new RoleService(new RoleRepository());
            }

            [Test]
            public async Task GetRolesAsync_NoInput_ReturnRoles()
            {
                // Arrange

                // Act
                var result = await _roleService.GetRolesAsync();

                // Assert
                Assert.That(result, Is.Not.Null.And.Not.Empty, "List of roles should not be empty");
            }
        }
    }
}