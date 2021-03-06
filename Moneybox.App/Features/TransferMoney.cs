using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class TransferMoney
    {
        private IAccountRepository _accountRepository;
        private INotificationService _notificationService;

        public TransferMoney(IAccountRepository accountRepository, INotificationService notificationService)
        {
            _accountRepository = accountRepository;
            _notificationService = notificationService;
        }

        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            
            var sourceAccount = _accountRepository.GetAccountById(fromAccountId);
            if (sourceAccount == null)
            {
                throw new InvalidOperationException($"Failed to withdraw from account. " +
                    $"Could not find the account with the provided account id: {fromAccountId}");
            };
            var destinationAccount = _accountRepository.GetAccountById(toAccountId);
            if (destinationAccount == null)
            {
                throw new InvalidOperationException($"Failed to withdraw from account. " +
                    $"Could not find the account with the provided account id: {toAccountId}");
            };

            sourceAccount.Withdraw(amount);
            destinationAccount.Deposit(amount);

            _accountRepository.Update(sourceAccount);
            _accountRepository.Update(destinationAccount);


          if (sourceAccount.AreAccountFundsLow())
          {
            _notificationService.NotifyFundsLow(sourceAccount.User.Email);
          }

          if (destinationAccount.IsAccountReachingPayInLimit())
          {
            _notificationService.NotifyApproachingPayInLimit(destinationAccount.User.Email);
          }
        }
    }
}
