using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Enums;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSaleTelegramBot;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPagination<AuctionLot> lotLogic;

        public HomeController(IPagination<AuctionLot> lotLogic)
        {
            this.lotLogic = lotLogic;
        }

        [HttpGet]
        public IActionResult Rules()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Lots(int page = 1)
        {
            List<AuctionLot> lotsToDisplay = await lotLogic.GetPage(page, null);

            int lotsCount = await lotLogic.GetCount(null);

            return View(new AuctionLotsViewModel()
            {
                PageViewModel = new PageViewModel(lotsCount, page, ApplicationConstantsProvider.GetPageSize()),
                AuctionLots = lotsToDisplay
            });
        }
    }
}
