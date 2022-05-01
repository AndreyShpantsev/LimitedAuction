using DataAccessLogic.Enums;

namespace DataAccessLogic.HelperServices
{
    public static class LotStatusProvider
    {
        public static string GetStatusOnRussian(LotStatus? status)
        {
            return status switch
            {
                LotStatus.Active => GetActiveStatus(),
                LotStatus.Applications => GetApplicationsStatus(),
                LotStatus.ApplicationsView => GetApplicationsViewStatus(),
                LotStatus.Contract => GetContractStatus(),
                LotStatus.NotHeld => GetNotHeldStatus(),
                LotStatus.OnModeration => GetOnModerationStatus(),
                LotStatus.Published => GetPublishedStatus(),
                LotStatus.Rejected => GetRejectedStatus(),
                LotStatus.Sold => GetSoldStatus(),
                _ => string.Empty,
            };
        }

        private static string GetOnModerationStatus()
        {
            return "На модерации";
        }

        private static string GetRejectedStatus()
        {
            return "Отклонен";
        }

        private static string GetPublishedStatus()
        {
            return "Размещён";
        }

        private static string GetApplicationsStatus()
        {
            return "Подача заявок";
        }

        private static string GetApplicationsViewStatus()
        {
            return "Рассмотрение заявок";
        }

        private static string GetActiveStatus()
        {
            return "Идут торги";
        }

        private static string GetNotHeldStatus()
        {
            return "Аукцион не состоялся";
        }

        private static string GetContractStatus()
        {
            return "Заключение контракта";
        }

        private static string GetSoldStatus()
        {
            return "Продан";
        }
    }
}
