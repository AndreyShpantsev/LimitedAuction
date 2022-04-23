using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplicationTechSale.ViewComponents
{
    public class BalanceInfoViewComponent : ViewComponent
    {
        private readonly ICrudLogic<Account> accLogic;

        public BalanceInfoViewComponent(
            ICrudLogic<Account> accLogic
        )
        {
            this.accLogic = accLogic;
        }

        public async Task<string> InvokeAsync()
        {
            string userId = UserClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            string balance;

            if (string.IsNullOrWhiteSpace(userId))
            {
                balance = "нет данных";
            }
            else
            {
                try
                {
                    List<Account> userAccountData = await accLogic.Read(new Account
                    {
                        UserId = userId
                    });

                    Account userAccountInfo = userAccountData.FirstOrDefault();

                    if (userAccountInfo == null)
                    {
                        throw new Exception();
                    }
                    balance = $"{userAccountInfo.Balance.ToString("C", CultureInfo.CurrentCulture)}";
                }
                catch (Exception)
                {
                    balance = "нет данных";
                }
            }

            return balance;
        }
    }
}
