using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney
    {
        private IAccountRepository _accountRepository;
        private INotificationService _notificationService;

        public WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            _accountRepository = accountRepository;
            _notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, decimal amount)
        {
            var account = _accountRepository.GetAccountById(fromAccountId);

            if (account == null)
            {
                throw new InvalidOperationException($"Failed to withdraw from account. " +
                    $"Could not find the account with the provided account id: {fromAccountId}");
            }

            account.Withdraw(amount);

            _accountRepository.Update(account);


            if (account.AreAccountFundsLow())
            {
                _notificationService.NotifyFundsLow(account.User.Email);
            }

        }
    }
}
