using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Moneybox.App.Tests
{
    public class WithdrawMoneyTests
    {

        private User AUser;
        private User BUser;

        private Guid fromAccId;
        private Guid toAccId;

        private Mock<IAccountRepository> mockAccountRepository;
        private Mock<INotificationService> mockNotificationService;

        private Account testFromAcc;
        private Account testToAcc;

        [SetUp]
        public void setUp()
        {
            AUser = new User()
            {
                Name = "Joe Bloggs",
                Email = "joe@bloggs.com",
                Id = Guid.NewGuid()
            };

            fromAccId = Guid.NewGuid();
            toAccId = Guid.NewGuid();

            testFromAcc = new Account()
            {
                Id = fromAccId,
                Balance = 10,
                PaidIn = 0,
                User = AUser,
                Withdrawn = 0
            };

            mockAccountRepository = new Mock<IAccountRepository>();
            mockNotificationService = new Mock<INotificationService>();
            mockAccountRepository.Setup(x => x.GetAccountById(fromAccId)).Returns(testFromAcc);
            mockAccountRepository.Setup(x => x.GetAccountById(toAccId)).Returns(testToAcc);
        }

        [Test]
        public void ThrowsInvalidOpExceptionIfInsufficientFunds()
        {
            var testWithdrawMoney = new WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);

            var ex = Assert.Throws<InvalidOperationException>(() => testWithdrawMoney.Execute(fromAccId, 11));

            Assert.AreEqual("The amount (11) to withdraw exceeds the available balance (10) in the account.", ex.Message);
        }

        [Test]
        public void NotifiesFundsLow()
        {
            var testWithdrawMoney = new WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);
            testWithdrawMoney.Execute(fromAccId, 5);

            mockNotificationService.Verify(x => x.NotifyFundsLow(AUser.Email), Times.Once());
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Once());
        }

        [Test]
        public void ThrowsNoAccountError()
        {
            var testWithdrawMoney = new WithdrawMoney(mockAccountRepository.Object, mockNotificationService.Object);
            var invalidGuid = Guid.NewGuid();

            var ex = Assert.Throws<InvalidOperationException>(() => testWithdrawMoney.Execute(invalidGuid, 11));

            Assert.AreEqual($"Failed to withdraw from account. Could not find the account with the provided account id: {invalidGuid}", ex.Message);
        }
    }
}