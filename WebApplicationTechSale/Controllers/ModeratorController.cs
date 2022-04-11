using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TechSaleTelegramBot;
using WebApplicationTechSale.HelperServices;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    [Authorize(Roles = "moderator")]
    public class ModeratorController : Controller
    {
        private readonly IPagination<AuctionLot> paginationLotLogic;
        private readonly ICrudLogic<AuctionLot> crudLotLogic;
        private readonly ICrudLogic<Note> crudNoteLogic;
        private readonly IBot telegramBot;

        public ModeratorController(IPagination<AuctionLot> paginationLotLogic, 
            ICrudLogic<AuctionLot> crudLotLogic, ICrudLogic<Note> crudNoteLogic,
            IBot telegramBot)
        {
            this.paginationLotLogic = paginationLotLogic;
            this.crudLotLogic = crudLotLogic;
            this.crudNoteLogic = crudNoteLogic;
            this.telegramBot = telegramBot;
        }

        [HttpGet]
        public async Task<IActionResult> Lots(int page = 1)
        {
            List<AuctionLot> lotsOnModeration = await paginationLotLogic.GetPage(page, new AuctionLot
            {
                Status = LotStatusProvider.GetOnModerationStatus()
            });

            int lotsCount = await paginationLotLogic.GetCount(new AuctionLot
            {
                Status = LotStatusProvider.GetOnModerationStatus()
            });

            return View(new AuctionLotsViewModel
            {
                AuctionLots = lotsOnModeration,
                PageViewModel = new PageViewModel(lotsCount, page, ApplicationConstantsProvider.GetPageSize())
            });
        }

        [HttpGet]
        public async Task<IActionResult> CheckLot(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                AuctionLot lotToCheck = (await crudLotLogic.Read(new AuctionLot
                {
                    Id = id
                }))?.First();
                return View(new LotModerationModel
                { 
                    AuctionLot = lotToCheck
                });
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptLot(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                await crudLotLogic.Update(new AuctionLot
                {
                    Id = id,
                    Status = LotStatusProvider.GetAcceptedStatus()
                });
                await SendAcceptMessage(id);
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.LotAcceptedMessages(),
                    SecondsToRedirect = ApplicationConstantsProvider.GetLongRedirectionTime(),
                    RedirectUrl = "/Moderator/Lots"
                });
            }
            return NotFound();
        }

        private async Task SendAcceptMessage(string lotId)
        {
            AuctionLot lot = (await crudLotLogic.Read(new AuctionLot
            {
                Id = lotId
            })).First();

            if (!string.IsNullOrEmpty(lot.User.TelegramChatId))
            {
                await telegramBot.SendMessage(
                    $"Ваш лот '{lot.Name}' успешно прошел модерацию!" +
                    $" Теперь он опубликован на сайте и виден всем пользователям",
                    lot.User.TelegramChatId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLot(LotModerationModel model)
        {
            if (ModelState.IsValid)
            {
                await crudLotLogic.Update(new AuctionLot
                {
                    Id = model.AuctionLot.Id,
                    Status = LotStatusProvider.GetRejectedStatus()
                });
                await crudNoteLogic.Delete(new Note
                {
                    AuctionLotId = model.AuctionLot.Id
                });
                await crudNoteLogic.Create(new Note
                {
                    AuctionLotId = model.AuctionLot.Id,
                    Text = model.RejectNote
                });
                await SendRejectMessage(model.AuctionLot.Id, model.RejectNote);
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.LotRejectedMessages(),
                    SecondsToRedirect = ApplicationConstantsProvider.GetLongRedirectionTime(),
                    RedirectUrl = "/Moderator/Lots"
                });
            }
            model.Expanded = true;
            return View("CheckLot", model);
        }

        private async Task SendRejectMessage(string lotId, string note)
        {
            AuctionLot lot = (await crudLotLogic.Read(new AuctionLot
            {
                Id = lotId
            })).First();

            if (!string.IsNullOrEmpty(lot.User.TelegramChatId))
            {
                await telegramBot.SendMessage(
                    $"Публикация вашего лота '{lot.Name}' отклонена модератором." +
                    $" Причина, по которой ваш лот не прошел модерацию: {note}",
                    lot.User.TelegramChatId);
            }
        }
    }
}
