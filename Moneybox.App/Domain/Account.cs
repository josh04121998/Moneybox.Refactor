using System;

namespace Moneybox.App
{
    public class Account
    {
        public const decimal MinimumAccountFundsAmount = 0m;

        public const decimal AccountFundsLowAmount = 500m;

        public const decimal PayInLimit = 4000m;

        public Guid Id { get; set; }

        public User User { get; set; }

        public decimal Balance { get; set; }

        public decimal Withdrawn { get; set; }

        public decimal PaidIn { get; set; }

        public void Withdraw(decimal amount)
        {
            if (Balance - amount < 0)
            {
                throw new InvalidOperationException($"The amount ({amount}) to withdraw" +
                    $" exceeds the available balance ({Balance}) in the account.");
            }

            Balance -= amount;
            Withdrawn -= amount;
        }

        public void Deposit(decimal amount)
        {
            if (PaidIn + amount > PayInLimit)
            {
                throw new InvalidOperationException("The pay in limit for the account has reached its limit");
            }

            Balance += amount;
            PaidIn += amount;
        }

        public bool AreAccountFundsLow()
        {
            return Balance < AccountFundsLowAmount;
        }

        public bool IsAccountReachingPayInLimit()
        {
            return PayInLimit - PaidIn < AccountFundsLowAmount;
        }
    }
}
