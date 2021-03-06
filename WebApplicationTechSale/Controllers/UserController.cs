using DataAccessLogic.DatabaseModels;
using DataAccessLogic.Enums;
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
using System.Security.Claims;
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
        private readonly ICrudLogic<Application> appLogic;
        private readonly ICrudLogic<Bid> bidLogic;
        private readonly ICrudLogic<Contract> contractLogic;
        private readonly IWebHostEnvironment environment;
        private readonly ISavedLogic savedListLogic;
        private readonly UserManager<User> userManager;
        private readonly IBot telegramBot;
        private readonly ICrudLogic<Order> orderLogic;

        public UserController(
            ICrudLogic<Application> appLogic,
            ICrudLogic<AuctionLot> lotLogic, 
            IWebHostEnvironment environment,
            UserManager<User> userManager, 
            ICrudLogic<Bid> bidLogic, 
            ISavedLogic savedListLogic,
            IBot telegramBot, 
            ICrudLogic<Order> orderLogic,
            ICrudLogic<Contract> contractLogic
        )
        {
            this.lotLogic = lotLogic;
            this.environment = environment;
            this.userManager = userManager;
            this.bidLogic = bidLogic;
            this.savedListLogic = savedListLogic;
            this.telegramBot = telegramBot;
            this.orderLogic = orderLogic;
            this.appLogic = appLogic;
            this.contractLogic = contractLogic;
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
                    TypeOfAuction = model.TypeOfAuction,
                    AppStartDate = model.AppStartDate,
                    AppEndDate = model.AppEndDate,
                    PriceInfo = new PriceInfo
                    {
                        StartPrice = model.StartPrice.Value,
                        CurrentPrice = model.StartPrice.Value,
                        BidStep = model.BidStep.Value,
                        FinalPrice = model.FinalPrice.Value,
                        PercentBid = model.PercentBid
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

                lot.Applications = await appLogic.Read(new Application
                {
                    AuctionLotId = lot.Id
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

        private async Task SendBuyNotifications(string lotId)
        {
            AuctionLot auctionLot = (await lotLogic.Read(new AuctionLot
            {
                Id = lotId
            })).First();

            if (auctionLot.User != null
            && !string.IsNullOrEmpty(auctionLot.User.TelegramChatId))
            {
                await telegramBot.SendMessage(
                    $"Ваш антиквариат '{auctionLot.Name}', " + $"продан " +
                    $"по цене {auctionLot.PriceInfo.CurrentPrice}",
                    auctionLot.User.TelegramChatId);
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
                if (lotToEdit.Status == LotStatus.Rejected
                    || lotToEdit.Status == LotStatus.Published
                    && DateTime.Now < lotToEdit.StartDate)
                {
                    if (lotToEdit.Status == LotStatus.Rejected)
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
                        FinalPrice = lotToEdit.PriceInfo.FinalPrice,
                        PercentBid = lotToEdit.PriceInfo.PercentBid,
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
                    Status = LotStatus.OnModeration,
                    PriceInfo = new PriceInfo
                    {
                        StartPrice = model.StartPrice.Value,
                        BidStep = model.BidStep.Value,
                        FinalPrice = model.FinalPrice.Value,
                        PercentBid = model.PercentBid.Value
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrder(string lotId)
        {
            await orderLogic.Create(new Order
            {
                AuctionLotId = lotId,
                UserName = User.Identity.Name
            });

            await SendBuyNotifications(lotId);

            return View("Redirect", new RedirectModel
            {
                InfoMessages = RedirectionMessageProvider.OrderCreateMessage(),
                RedirectUrl = "/Account/MyOrders",
                SecondsToRedirect = ApplicationConstantsProvider.GetShortRedirectionTime()
            });
        }

        [HttpGet]
        public async Task<IActionResult> SendApp(string lotId)
        {
            AuctionLot auctionLot = (await lotLogic.Read(new AuctionLot
            {
                Id = lotId
            })).FirstOrDefault();

            if (
                auctionLot == null || 
                auctionLot.TypeOfAuction != TypeOfAuction.Closed
            )
            {
                return NotFound();
            }

            return View(new SendAppViewModel
            {
                AuctionLotId = auctionLot.Id,
                Seller = auctionLot.User.UserName,
                AppEndDate = auctionLot.AppEndDate.Value,
                AppStartDate = auctionLot.AppStartDate.Value,
                AuctionDescription = auctionLot.Description,
                AuctionEndDate = auctionLot.EndDate,
                AuctionName = auctionLot.Name,
                AuctionStartDate = auctionLot.StartDate,
                RulesIsAccepted = false,
                StartPrice = auctionLot.PriceInfo.StartPrice
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendApp(SendAppViewModel model)
        {
            if (!model.RulesIsAccepted)
            {
                ModelState.AddModelError(string.Empty, "Необходимо принять условия");
                return View(model);
            }
            try
            {
                string userId = User.Claims
                    .FirstOrDefault(uc => uc.Type == ClaimTypes.NameIdentifier)?.Value;

                await appLogic.Create(new Application
                {
                    UserId = userId,
                    AuctionLotId = model.AuctionLotId
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            return View("Redirect", new RedirectModel
            {
                InfoMessages = RedirectionMessageProvider.ApplicationSended(),
                RedirectUrl = $"/User/OpenLot/{model.AuctionLotId}",
                SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
            });
        }

        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            User user = await userManager.FindByNameAsync(User.Identity.Name);

            List<Order> userOrders = await orderLogic.Read(new Order
            {
                User = new User
                {
                    UserName = user.UserName
                }
            });

            return View(userOrders);
        }

        [HttpGet]
        public async Task<IActionResult> MyAppList()
        {
            string userId = User.Claims
                .FirstOrDefault(uc => uc.Type == ClaimTypes.NameIdentifier)?.Value;

            List<Application> userApps = await appLogic.Read(new Application
            {
                UserId = userId
            });

            return View(userApps);
        }

        [HttpGet]
        public async Task<IActionResult> AuctionAppList(string lotId)
        {
            List<Application> appList = await appLogic.Read(new Application
            {
                AuctionLotId = lotId
            });

            List<ApplicationAcceptViewModel> appsForView =
                appList.Select(app => new ApplicationAcceptViewModel
                {
                    ApplicationId = app.Id,
                    AppUserName = app.User.UserName,
                    IsAccepted = false
                }).ToList();

            return View(new ViewAppsViewModel
            {
                AuctionLotId = lotId,
                AppsForView = appsForView
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AuctionAppList(ViewAppsViewModel model)
        {
            try
            {
                foreach (ApplicationAcceptViewModel appView in model.AppsForView)
                {
                    await appLogic.Update(new Application
                    {
                        Id = appView.ApplicationId,
                        Status = appView.IsAccepted
                            ? ApplicationStatus.Accepted
                            : ApplicationStatus.Rejected
                    });
                }

                int acceptedAppsCount = model.AppsForView.Count(app => app.IsAccepted);

                if (acceptedAppsCount < 2)
                {
                    await lotLogic.Update(new AuctionLot
                    {
                        Id = model.AuctionLotId,
                        Status = LotStatus.NotHeld
                    });
                } 
                else
                {
                    await lotLogic.Update(new AuctionLot
                    {
                        Id = model.AuctionLotId,
                        Status = LotStatus.Published
                    });
                }

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.AppViewResultsSaved(),
                    RedirectUrl = $"/User/OpenLot/{model.AuctionLotId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MyContracts()
        {
            string userId = User.Claims
                .FirstOrDefault(uc => uc.Type == ClaimTypes.NameIdentifier)?.Value;

            List<Contract> userContracts = await contractLogic.Read(new Contract
            {
                BuyerId = userId,
                SellerId = userId
            });

            return View(userContracts);
        }

        [HttpGet]
        public async Task<IActionResult> OpenContract(string id)
        {
            Contract contractToShow = (await contractLogic.Read(new Contract
            {
                Id = id
            })).FirstOrDefault();

            if (contractToShow == null)
            {
                return NotFound();
            }

            return View(new OpenContractViewModel
            {
                AuctionName = contractToShow.AuctionLot.Name,
                BuyerName = contractToShow.Buyer.UserName,
                ContractAmount = contractToShow.Amount,
                ContractId = contractToShow.Id,
                ContractStatus = contractToShow.Status,
                SellerName = contractToShow.Seller.UserName,
                DeliveryInfo = contractToShow.DeliveryInfo
            });
        }

        [HttpPost]
        public async Task<IActionResult> ContractParticipantSign(string cntrId)
        {
            try
            {
                await contractLogic.Update(new Contract
                {
                    Id = cntrId,
                    Status = ContractStatus.SellerSigning
                });

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.TestMessage(),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            catch (Exception ex)
            {
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ErrorMessage(ex.Message),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ContractSellerSign(string cntrId)
        {
            try
            {
                await contractLogic.Update(new Contract
                {
                    Id = cntrId,
                    Status = ContractStatus.SellerInfo
                });

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.TestMessage(),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            catch (Exception ex)
            {
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ErrorMessage(ex.Message),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ContractSellerInfo(string cntrId, string deliveryInfo)
        {
            try
            {
                await contractLogic.Update(new Contract
                {
                    Id = cntrId,
                    Status = ContractStatus.ParticipantConfirmation,
                    DeliveryInfo = deliveryInfo
                });

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.TestMessage(),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            catch (Exception ex)
            {
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ErrorMessage(ex.Message),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ContractParticipantConfirmation(string cntrId)
        {
            try
            {
                await contractLogic.Update(new Contract
                {
                    Id = cntrId,
                    Status = ContractStatus.ConcludeContract
                });

                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.TestMessage(),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
            catch (Exception ex)
            {
                return View("Redirect", new RedirectModel
                {
                    InfoMessages = RedirectionMessageProvider.ErrorMessage(ex.Message),
                    RedirectUrl = $"/User/OpenContract/{cntrId}",
                    SecondsToRedirect = ApplicationConstantsProvider.GetMaxRedirectionTime()
                });
            }
        }
    }
}
