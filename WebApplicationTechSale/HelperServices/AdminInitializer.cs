using DataAccessLogic.CrudLogic;
using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace WebApplicationTechSale.HelperServices
{
    public class AdminInitializer
    {
        public static async Task InitializeAdmin(
            UserManager<User> userManager, 
            ICrudLogic<Account> accLogic,
            IConfiguration configuration
        )
        {
            string email = configuration["AdminEmailAzure"];
            string password = configuration["AdminPasswordAzure"];
            string username = configuration["AdminUsernameAzure"];
            if (await userManager.FindByEmailAsync(email) == null)
            {
                User admin = new User 
                { 
                    Email = email, 
                    UserName = username 
                };
                var registerResult = await userManager.CreateAsync(admin, password);
                if (registerResult.Succeeded)
                {
                    admin.Email += ApplicationConstantsProvider.AvoidValidationCode();
                    admin.UserName += ApplicationConstantsProvider.AvoidValidationCode();
                    await userManager.AddToRoleAsync(admin, "admin");

                    await accLogic.Create(new Account()
                    {
                        UserId = admin.Id
                    });
                }

            }
        }
    }
}
