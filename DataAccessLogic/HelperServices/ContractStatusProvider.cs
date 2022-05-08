using DataAccessLogic.Enums;

namespace DataAccessLogic.HelperServices
{
    public static class ContractStatusProvider
    {
        public static string GetStatusOnRussian(ContractStatus? status)
        {
            return status switch
            {
                ContractStatus.PatricipantSigning => GetPatricipantSigning(),
                ContractStatus.SellerSigning => GetSellerSigning(),
                ContractStatus.SellerInfo => GetSellerInfo(),
                ContractStatus.ParticipantConfirmation => GetParticipantConfirmation(),
                ContractStatus.ConcludeContract => GetConcludeContract(),
                _ => string.Empty,
            };
        }

        private static string GetPatricipantSigning()
        {
            return "Ожидает подписания участником";
        }

        private static string GetSellerSigning()
        {
            return "Ожидает подписания продавцом";
        }

        private static string GetSellerInfo()
        {
            return "Ожидается информация от продавца";
        }

        private static string GetParticipantConfirmation()
        {
            return "Ожидает подтверждения получения товара";
        }

        private static string GetConcludeContract()
        {
            return "Контракт заключен";
        }
    }
}
