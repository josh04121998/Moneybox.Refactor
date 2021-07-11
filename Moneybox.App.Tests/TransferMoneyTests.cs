using System;
using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moq;
using NUnit.Framework;

namespace Moneybox.App.Tests
{
    public class TransferMoneyTests
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

            BUser = new User()
            {
                Name = "Samantha Bloggs",
                Email = "sam@bloggs.com",
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

            testToAcc = new Account()
            {
                Id = toAccId,
                Balance = 10,
                PaidIn = 0,
                User = BUser,
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
            var testTransferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            var ex = Assert.Throws<InvalidOperationException>(() => testTransferMoney.Execute(fromAccId, toAccId, 11));

            Assert.AreEqual("Insufficient funds to make transfer", ex.Message);
        }

        [Test]
        public void NotifiesFundsLow()
        {
            var testTransferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            testTransferMoney.Execute(fromAccId, toAccId, 5);

            mockNotificationService.Verify(x => x.NotifyFundsLow(AUser.Email));
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        }

        [Test]
        public void ThrowsInvalidOpExceptionIfPayInLimitExceeded()
        {
            testFromAcc.Balance = 1000;
            testToAcc.PaidIn = 3800;
            var testTransferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);

            var ex = Assert.Throws<InvalidOperationException>(() => testTransferMoney.Execute(fromAccId, toAccId, 501));

            Assert.AreEqual("Account pay in limit reached", ex.Message);
        }

        [Test]
        public void NotifiesAproachingLimit()
        {
            testToAcc.PaidIn = 3500;
            testFromAcc.Balance = 550;
            var testTransferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            testTransferMoney.Execute(fromAccId, toAccId, 250);

            mockNotificationService.Verify(x => x.NotifyApproachingPayInLimit(BUser.Email));
            mockAccountRepository.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        }

        [Test]
        public void TestTransferTransactionCalculatesCorrectNumbers()
        {
            var testTransferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            testTransferMoney.Execute(fromAccId, toAccId, 5);

            Assert.AreEqual(15, testToAcc.Balance);
            Assert.AreEqual(5, testFromAcc.Balance);

            Assert.AreEqual(0, testFromAcc.PaidIn);
            Assert.AreEqual(5, testToAcc.PaidIn);

            Assert.AreEqual(-5m, testFromAcc.Withdrawn);
            Assert.AreEqual(0, testToAcc.Withdrawn);
        }

        [Test]
        public void ThrowsNoAccountError()
        {
            var testTransferMoney = new TransferMoney(mockAccountRepository.Object, mockNotificationService.Object);
            var invalidGuid = Guid.NewGuid();

            var ex = Assert.Throws<InvalidOperationException>(() => testTransferMoney.Execute(invalidGuid, toAccId, 11));

            Assert.AreEqual($"Failed to withdraw from account. Could not find the account with the provided account id: {invalidGuid}", ex.Message);
        }
    }
}