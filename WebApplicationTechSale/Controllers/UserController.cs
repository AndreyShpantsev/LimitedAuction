using DataAccessLogic.DatabaseModels;
using DataAccessLogic.HelperServices;
using DataAccessLogic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TechSaleTelegramBot;
using WebApplicationTechSale.HelperServices;
using WebApplicationTechSale.Models;

namespace WebApplicationTechSale.Controllers
{
    [Authorize(Roles = "regular user")]
    public class UserController : Controller
    {
        private readonly ICrudLogic<AuctionLot> lotLogic;
        private readonly ICrudLogic<Bid> bidLogic;
        private readonly IWebHostEnvironment environment;
        private readonly ISavedLogic savedListLogic;
        private readonly UserManager<User> userManager;
        private readonly IBot telegramBot;

        public UserController(ICrudLogic<AuctionLot> lotLogic, IWebHostEnvironment environment,
            UserManager<User> userManager, ICrudLogic<Bid> bidLogic, ISavedLogic savedListLogic,
            IBot telegramBot)
        {
            this.lotLogic = lotLogic;
            this.environment = environment;
            this.userManager = userManager;
            this.bidLogic = bidLogic;
            this.savedListLogic = savedListLogic;
            this.telegramBot = telegramBot;
        }

        [HttpGet]
        public IActionResult CreateLot()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLot(CreateLotViewModel model)
        {
            if (ModelState.IsValid)
            {
                AuctionLot toAdd = new AuctionLot
                {
                    Name = model.Name,
                    User = new User
                    {
                        UserName = User.Identity.Name
                    },
                    Description = model.Description,
                    StartDate = model.StartDate.Value,
                    EndDate = model.EndDate.Value,
                    PriceInfo = new PriceInfo
                    {
                        StartPrice = model.StartPrice.Value,
                        CurrentPrice = model.StartPrice.Value,
                        BidStep = model.BidStep.Value
                    }
                };

                string dbPhotoPath = $"/images/{User.Identity.Name}/{model.Name}/photo{Path.GetExtension(model.Photo.FileName)}";
                toAdd.PhotoSrc = dbPhotoPath;

                try
                {
                   await lotLogic.Create(toAdd);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                string physicalDirectory = Path.GetDirectoryName($"{environment.WebRootPath + dbPhotoPath}");
                if (!Directory.Exists(physicalDirectory))
                {
                    Directory.CreateDirectory(physicalDirectory);
                }

                using (FileStream fs = new FileStream($"{environment.WebRootPath + dbPhotoPath}", FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fs);
                }

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.LotCreatedMessages(),
                    RedirectUrl = "/Home/Lots",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OpenLot(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                AuctionLot lot = (await lotLogic.Read(new AuctionLot
                {
                    Id = id
                })).First();

                lot.Bids = await bidLogic.Read(new Bid
                {
                    AuctionLotId = id
                });

                User user = await userManager.FindByNameAsync(User.Identity.Name);

                SavedList userList = await savedListLogic.Read(user);

                if (userList.AuctionLots.Any(lot => lot.Id == id))
                {
                    ViewBag.IsSaved = true;
                }
                else
                {
                    ViewBag.IsSaved = false;
                }

                if (lot == null)
                {
                    return NotFound();
                }
                return View(lot);
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceBid(string lotId)
        {
            if (!string.IsNullOrWhiteSpace(lotId))
            {
                User user = await userManager.FindByNameAsync(User.Identity.Name);
                try
                {
                    await bidLogic.Create(new Bid
                    {
                        AuctionLot = (await lotLogic.Read(new AuctionLot
                        {
                            Id = lotId
                        }))?.First(),
                        User = user
                    });
                } catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View("Redirect", new RedirectModel
                    {
                        InfoMessages = RedirectionMessageProvider.AuctionTimeUpMessages(),
                        RedirectUrl = $"/User/OpenLot/?id={lotId}",
                        SecondsToRedirect = ApplicationConstantsProvider.GetShortRedirectionTime()
                    });
                }
                AuctionLot lotToAdd = new AuctionLot { Id = lotId };
                await savedListLogic.Add(user, lotToAdd);
                await SendNotifications(lotId, User.Identity.Name);
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.BidPlacedMessages(),
                    RedirectUrl = $"/User/OpenLot/?id={lotId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetShortRedirectionTime()
                });
            }
            return NotFound();
        }

        private async Task SendNotifications(string lotId, string userName)
        {
            AuctionLot auctionLot = (await lotLogic.Read(new AuctionLot 
            { 
                Id = lotId 
            })).First();

            List<Bid> bids = await bidLogic.Read(new Bid
            {
                AuctionLotId = lotId
            });


            List<User> users = new List<User>();

            foreach (Bid bid in bids)
            {
                if (!string.IsNullOrWhiteSpace(bid.User.TelegramChatId)
                    &&!users.Contains(bid.User) 
                    && userName != bid.User.UserName)
                {
                    users.Add(bid.User);
                }
            }

            foreach (User user in users)
            {
                await telegramBot.SendMessage(
                    $"Новая ставка в лоте '{auctionLot.Name}', " +
                    $"текущая цена {auctionLot.PriceInfo.CurrentPrice}", 
                    user.TelegramChatId);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveLot(string lotId)
        {
            if (!string.IsNullOrWhiteSpace(lotId))
            {
                User user = await userManager.FindByNameAsync(User.Identity.Name);
                AuctionLot lotToAdd = new AuctionLot { Id = lotId };
                await savedListLogic.Add(user, lotToAdd);
                return RedirectToAction("OpenLot", "User", new { id = lotId });
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> MySavedList()
        {
            User user = await userManager.FindByNameAsync(User.Identity.Name);

            SavedList userSavedList = await savedListLogic.Read(user);

            return View(userSavedList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLot(string lotId)
        {
            if (!string.IsNullOrWhiteSpace(lotId))
            {
                User user = await userManager.FindByNameAsync(User.Identity.Name);
                AuctionLot lotToAdd = new AuctionLot { Id = lotId };
                await savedListLogic.Remove(user, lotToAdd);
                return RedirectToAction("OpenLot", "User", new { id = lotId });
            }
            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> EditLot(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                AuctionLot lotToEdit = (await lotLogic.Read(new AuctionLot { Id = id })).First();
                if (lotToEdit.Status == LotStatusProvider.GetRejectedStatus()
                    || lotToEdit.Status == LotStatusProvider.GetAcceptedStatus()
                    && DateTime.Now < lotToEdit.StartDate)
                {
                    if (lotToEdit.Status == LotStatusProvider.GetRejectedStatus())
                    {
                        ViewBag.RejectNote = "Причина, по которой ваш лот не был опубликован: " 
                            + lotToEdit.Note.Text;
                    }
                    else
                    {
                        ViewBag.RejectNote = string.Empty;
                    }
                    return View(new EditLotViewModel
                    {
                        Id = lotToEdit.Id,
                        BidStep = lotToEdit.PriceInfo.BidStep,
                        Description = lotToEdit.Description,
                        Name = lotToEdit.Name,
                        OldName = lotToEdit.Name,
                        StartDate = lotToEdit.StartDate,
                        EndDate = lotToEdit.EndDate,
                        StartPrice = lotToEdit.PriceInfo.StartPrice,
                        OldPhotoSrc = lotToEdit.PhotoSrc
                    });
                }
                else
                {
                    return NotFound();
                }
            }
            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLot(EditLotViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(model.Id))
                {
                    return NotFound();
                }

                AuctionLot lotToEdit = new AuctionLot
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    StartDate = model.StartDate.Value,
                    EndDate = model.EndDate.Value,
                    Status = LotStatusProvider.GetOnModerationStatus(),
                    PriceInfo = new PriceInfo
                    {
                        StartPrice = model.StartPrice.Value,
                        BidStep = model.BidStep.Value
                    }
                };

                string newDbPath = $"/images/{User.Identity.Name}/{model.Name}/photo{Path.GetExtension(model.Photo.FileName)}";
                lotToEdit.PhotoSrc = newDbPath;

                try
                {
                    await lotLogic.Update(lotToEdit);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                string oldPath = $"{environment.WebRootPath + Path.GetDirectoryName(model.OldPhotoSrc)}";
                if (Directory.Exists(oldPath))
                {
                    Directory.Delete(oldPath, true);
                }

                string newPhysicalDirectory = Path.GetDirectoryName($"{environment.WebRootPath + newDbPath}");

                if (!Directory.Exists(newPhysicalDirectory))
                {
                    Directory.CreateDirectory(newPhysicalDirectory);
                }

                using (FileStream fs = new FileStream($"{environment.WebRootPath + newDbPath}", FileMode.Create))
                {
                    await model.Photo.CopyToAsync(fs);
                }

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.LotUpdatedMessages(),
                    RedirectUrl = "/Home/Lots",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            return View(model);
        }
    }
}
