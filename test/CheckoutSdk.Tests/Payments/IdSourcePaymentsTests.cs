using Shouldly;
using System.Threading.Tasks;
using Checkout.Common;
using Checkout.Payments;
using Xunit;
using Xunit.Abstractions;

namespace Checkout.Tests.Payments
{
    public class IdSourcePaymentsTests : IClassFixture<ApiTestFixture>
    {
        private readonly ICheckoutApi _api;

        public IdSourcePaymentsTests(ApiTestFixture fixture, ITestOutputHelper outputHelper)
        {
            fixture.CaptureLogsInTestOutput(outputHelper);
            _api = fixture.Api;
        }

        [Fact]
        public async Task CanRequestCardPayment()
        {
            var firstCardPayment = TestHelper.CreateCardPaymentRequest();
            var firstCardPaymentResponse = await _api.Payments.RequestAsync(firstCardPayment);
            var idSource = new IdSource(firstCardPaymentResponse.Payment.Source.AsCardSource().Id);
            var cardIdPaymentRequest = new PaymentRequest<IdSource>(
                idSource,
                Currency.GBP,
                100
            )
            {
                Capture = false,
                Customer = firstCardPaymentResponse.Payment.Customer
            };

            PaymentResponse apiResponseForCustomerSourcePayment = await _api.Payments.RequestAsync(cardIdPaymentRequest);

            apiResponseForCustomerSourcePayment.Payment.ShouldNotBeNull();
            apiResponseForCustomerSourcePayment.Payment.Approved.ShouldBeTrue();
            apiResponseForCustomerSourcePayment.Payment.Id.ShouldNotBeNullOrEmpty();
            apiResponseForCustomerSourcePayment.Payment.Id.ShouldNotBe(firstCardPaymentResponse.Payment.Id);
            apiResponseForCustomerSourcePayment.Payment.ActionId.ShouldNotBeNullOrEmpty();
            apiResponseForCustomerSourcePayment.Payment.Amount.ShouldBe(cardIdPaymentRequest.Amount.Value);
            apiResponseForCustomerSourcePayment.Payment.Currency.ShouldBe(cardIdPaymentRequest.Currency);
            apiResponseForCustomerSourcePayment.Payment.Reference.ShouldBe(cardIdPaymentRequest.Reference);
            apiResponseForCustomerSourcePayment.Payment.Customer.ShouldNotBeNull();
            apiResponseForCustomerSourcePayment.Payment.Customer.Id.ShouldNotBeNullOrEmpty();
            apiResponseForCustomerSourcePayment.Payment.Customer.Email.ShouldNotBeNullOrEmpty();
            apiResponseForCustomerSourcePayment.Payment.Customer.Id.ShouldBe(firstCardPaymentResponse.Payment?.Customer?.Id);
            apiResponseForCustomerSourcePayment.Payment.Source.AsCardSource().Id.ShouldBe(firstCardPaymentResponse.Payment?.Source?.AsCardSource().Id);
            apiResponseForCustomerSourcePayment.Payment.CanCapture().ShouldBeTrue();
            apiResponseForCustomerSourcePayment.Payment.CanVoid().ShouldBeTrue();
        }
    }
}