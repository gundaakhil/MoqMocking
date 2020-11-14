using System;
using Loans.Domain.Applications;
using NUnit.Framework;
using Moq;
using Moq.Protected;

namespace Loans.Tests
{
    [TestFixture]
    [Category("Moq Mocking")]
    public class LoanApplicationProcessorShould
    {
        [Test]
        public void DeclineLowSalary()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  64_999);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();
            var mockCreditScorer = new Mock<ICreditScorer>();

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.False);
        }


        delegate void ValidateCallback(string applicantName, 
                                       int applicantAge, 
                                       string applicantAddress, 
                                       ref IdentityVerificationStatus status);


        [Test]
        public void Accept()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            //General
            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                       25,
                                                       "133 Pluralsight Drive, Draper, Utah"))
                                .Returns(true);

            //mockIdentityVerifier.Setup(x => x.Validate(It.IsAny<string>(),
            //                                           It.IsAny<int>(),
            //                                           It.IsAny<string>()))
            //                    .Returns(true);


            // With Out type
            //bool isValidOutValue = true;
            //mockIdentityVerifier.Setup(x => x.Validate("Sarah",
            //                                           25,
            //                                           "133 Pluralsight Drive, Draper, Utah",
            //                                           out isValidOutValue));


            // With Ref type
            //mockIdentityVerifier
            //    .Setup(x => x.Validate("Sarah", 
            //                           25, 
            //                           "133 Pluralsight Drive, Draper, Utah", 
            //                           ref It.Ref<IdentityVerificationStatus>.IsAny))
            //    .Callback(new ValidateCallback(
            //                (string applicantName, 
            //                 int applicantAge, 
            //                 string applicantAddress, 
            //                 ref IdentityVerificationStatus status) => 
            //                             status = new IdentityVerificationStatus(true)));


            var mockCreditScorer = new Mock<ICreditScorer>();

            // Track all properties changes
            // mockCreditScore.SetupAllProperties();

            // mockCreditScorer.Setup(x => x.Score).Returns(300);

            // Mock Properties to Track changes
            // mockCreditScorer.SetupProperty(x => x.Count, initial value);
            mockCreditScorer.SetupProperty(x => x.Count);

            //Manullay mocking hierarchies
            //var mockScoreValue = new Mock<ScoreValue>();
            //mockScoreValue.Setup(x => x.Score).Returns(300);
            //var mockScoreResult = new Mock<ScoreResult>();
            //mockScoreResult.Setup(x => x.ScoreValue).Returns(mockScoreValue.Object);
            //mockCreditScorer.Setup(x => x.ScoreResult).Returns(mockScoreResult.Object);

            //Auto Mocking hierarchies provided all properties are in virtual
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            //Auto Mocking hierarchies method 2
            //var mockCreditScorer = new Mock<ICreditScorer> { DefaultValue = DefaultValue.Mock};  or { DefaultValue = DefaultValue.Empty}
            //mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            // Verfying property setter and getter were called
            mockCreditScorer.VerifyGet(x => x.ScoreResult.ScoreValue.Score, Times.Once);
            mockCreditScorer.VerifySet(x => x.Count = It.IsAny<int>(), Times.Once);

            Assert.That(application.GetIsAccepted(), Is.True);
            Assert.That(mockCreditScorer.Object.Count, Is.EqualTo(1));
        }


        [Test]
        public void AcceptStrictMode()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>(MockBehavior.Strict);

            mockIdentityVerifier.Setup(x => x.Initialize());

            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                       25,
                                                       "133 Pluralsight Drive, Draper, Utah"))
                                .Returns(true);



            var mockCreditScorer = new Mock<ICreditScorer>();

            mockCreditScorer.SetupAllProperties();

            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.True);
        }

        [Test]
        public void InitializeIdentityVerifier()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                       25,
                                                       "133 Pluralsight Drive, Draper, Utah"))
                                .Returns(true);



            var mockCreditScorer = new Mock<ICreditScorer>();
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            // Verifying a method where no parameters are called
            mockIdentityVerifier.Verify(x => x.Initialize());

            mockIdentityVerifier.Verify(x => x.Validate(It.IsAny<string>(),
                                                        It.IsAny<int>(),
                                                        It.IsAny<string>()));

            mockIdentityVerifier.VerifyNoOtherCalls();
        }

        [Test]
        public void CalculateScore()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                       25,
                                                       "133 Pluralsight Drive, Draper, Utah"))
                                .Returns(true);



            var mockCreditScorer = new Mock<ICreditScorer>();
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            // Verfying a method where parameters was called
            // mockCreditScorer.Verify(x => x.CalculateScore(It.IsAny<string>(), It.IsAny<string>()));
            mockCreditScorer.Verify(x => x.CalculateScore("Sarah", "133 Pluralsight Drive, Draper, Utah"));

            // Verifying a method was called a specific number of times
            mockCreditScorer.Verify(x => x.CalculateScore("Sarah", "133 Pluralsight Drive, Draper, Utah"),Times.Once);
        }


        [Test]
        public void DeclineWhenCreditScoreError()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var mockIdentityVerifier = new Mock<IIdentityVerifier>();

            mockIdentityVerifier.Setup(x => x.Validate("Sarah",
                                                       25,
                                                       "133 Pluralsight Drive, Draper, Utah"))
                                .Returns(true);

            var mockCreditScorer = new Mock<ICreditScorer>();
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);

            //mockCreditScorer.Setup(x => x.CalculateScore(It.IsAny<string>(), It.IsAny<string>()))
            //                .Throws(new InvalidOperationException("Test Exception"));

            mockCreditScorer.Setup(x => x.CalculateScore(It.IsAny<string>(), It.IsAny<string>()))
                            .Throws<InvalidOperationException>();

            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.False);
        }


        interface IIdentityVerifierServiceGatewayProtectedMembers
        {
            bool CallService(string applicantName, int applicantAge, string applicantAddress);
        }

        [Test]
        public void AcceptUsingPartialMock()
        {
            LoanProduct product = new LoanProduct(99, "Loan", 5.25m);
            LoanAmount amount = new LoanAmount("USD", 200_000);
            var application = new LoanApplication(42,
                                                  product,
                                                  amount,
                                                  "Sarah",
                                                  25,
                                                  "133 Pluralsight Drive, Draper, Utah",
                                                  65_000);

            var expectedTime = new DateTime(2000, 1, 1);

            var mockNowProvider = new Mock<INowProvider>();
            mockNowProvider.Setup(x => x.GetNow()).Returns(expectedTime);

            var mockIdentityVerifier =
                new Mock<IdentityVerifierServiceGateway>(mockNowProvider.Object);

	    // mockIdentityVerifier.Protected().Setup<bool>("CallService",
            // 							 "Sarah",25,"address")
            //                    .Returns(true);
            mockIdentityVerifier.Protected()
                                .As<IIdentityVerifierServiceGatewayProtectedMembers>()
                                .Setup(x => x.CallService(It.IsAny<string>(),
                                                          It.IsAny<int>(),
                                                          It.IsAny<string>()))
                                .Returns(true);


            var mockCreditScorer = new Mock<ICreditScorer>();
            mockCreditScorer.Setup(x => x.ScoreResult.ScoreValue.Score).Returns(300);


            var sut = new LoanApplicationProcessor(mockIdentityVerifier.Object,
                                                   mockCreditScorer.Object);

            sut.Process(application);

            Assert.That(application.GetIsAccepted(), Is.True);
            Assert.That(mockIdentityVerifier.Object.LastCheckTime, Is.EqualTo(expectedTime));
        }


        [Test]
        public void NullReturnExample()
        {
            var mock = new Mock<INullExample>();

            mock.Setup(x => x.SomeMethod());
                //.Returns<string>(null);

            string mockReturnValue = mock.Object.SomeMethod();

            Assert.That(mockReturnValue, Is.Null);
        }
    }

    public interface INullExample
    {
        string SomeMethod();
    }
}
