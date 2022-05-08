using DataAccessLogic.Enums;

namespace DataAccessLogic.HelperServices
{
    public static class AppStatusProvider
    {
        public static string GetStatusOnRussian(ApplicationStatus? status)
        {
            return status switch
            {
                ApplicationStatus.Submitted => GetSubmittedApplication(),
                ApplicationStatus.Accepted => GetAcceptedApplication(),
                ApplicationStatus.Rejected => GetRejectedApplication(),
                _ => string.Empty,
            };
        }

        private static string GetSubmittedApplication()
        {
            return "Заявка подана";
        }

        private static string GetAcceptedApplication()
        {
            return "Заявка одобрена";
        }

        private static string GetRejectedApplication()
        {
            return "Заявка отклонена";
        }
    }
}
