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
    }
}
