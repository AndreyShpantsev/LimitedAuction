namespace DataAccessLogic.HelperServices
{
    public static class LotStatusProvider
    {
        public static string GetOnModerationStatus()
        {
            return "На модерации";
        }

        public static string GetRejectedStatus()
        {
            return "Отклонен";
        }

        public static string GetAcceptedStatus()
        {
            return "Размещён";
        }

        public static string GetApplicationsStatus()
        {
            return "Подача заявок";
        }

        public static string GetApplicationsViewStatus()
        {
            return "Рассмотрение заявок";
        }

        public static string GetActiveStatus()
        {
            return "Идут торги";
        }

        public static string GetNotHeldStatus()
        {
            return "Аукцион не состоялся";
        }

        public static string GetContractStatus()
        {
            return "Заключение контракта";
        }

        public static string GetSoldStatus()
        {
            return "Продан";
        }
    }
}
