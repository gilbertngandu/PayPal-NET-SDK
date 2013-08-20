﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using PayPal.Api.Payments;
using PayPal.Manager;
using PayPal;

namespace RestApiSDKUnitTest
{
    [TestClass()]
    public class CaptureTest
    {
        private string ClientId
        {
            get
            {
                string Id = PayPal.Manager.ConfigManager.Instance.GetProperties()["ClientID"];
                return Id;
            }
        }

        private string ClientSecret
        {
            get
            {
                string secret = ConfigManager.Instance.GetProperties()["ClientSecret"];
                return secret;
            }
        }

        private string AccessToken
        {
            get
            {
                string token = new OAuthTokenCredential(ClientId, ClientSecret).GetAccessToken();
                return token;
            }
        }       

        private List<Links> GetLinksList()
        {
            Links lnk = new Links();
            lnk.href = "http://www.paypal.com";
            lnk.method = "POST";
            lnk.rel = "authorize";
            List<Links> lnks = new List<Links>();
            lnks.Add(lnk);
            return lnks;
        }

        private Details GetDetails()
        {
            Details detail = new Details();
            detail.tax = "15";
            detail.fee = "2";
            detail.shipping = "10";
            detail.subtotal = "75";
            return detail;
        }

        private Amount GetAmount()
        {
            Amount amnt = new Amount();
            amnt.currency = "USD";
            amnt.details = GetDetails();
            amnt.total = "100";
            return amnt;
        }

        private Capture GetCapture()
        {
            Capture cap = new Capture();
            cap.amount = GetAmount();
            cap.create_time = "2013-01-15T15:10:05.123Z";
            cap.state = "Authorized";
            cap.parent_payment = "1000";
            cap.links = GetLinksList();
            cap.id = "001";
            return cap;
        }

        private Payment GetPayment()
        {
            Payment pay = new Payment();
            pay.intent = "authorize";
            CreditCard card = GetCreditCard();
            List<FundingInstrument> instruments = new List<FundingInstrument>();
            FundingInstrument instrument = new FundingInstrument();
            instrument.credit_card = card;
            instruments.Add(instrument);
            Payer payer = new Payer();
            payer.payment_method = "credit_card";
            payer.funding_instruments = instruments;
            List<Transaction> transacts = new List<Transaction>();
            Transaction trans = new Transaction();
            trans.amount = GetAmount();
            transacts.Add(trans);
            pay.transactions = transacts;
            pay.payer = payer;
            return pay.Create(AccessToken);
        }

        private Address GetAddress()
        {
            Address addrss = new Address();
            addrss.line1 = "2211";
            addrss.line2 = "N 1st St";
            addrss.city = "San Jose";
            addrss.phone = "408-456-0392";
            addrss.postal_code = "95131";
            addrss.state = "California";
            addrss.country_code = "US";
            return addrss;
        }

        private CreditCard GetCreditCard()
        {
            CreditCard card = new CreditCard();
            card.cvv2 = "962";
            card.expire_month = 01;
            card.expire_year = 2015;
            card.first_name = "John";
            card.last_name = "Doe";
            card.number = "4825854086744369";
            card.type = "visa";
            card.state = "New York";
            card.payer_id = "008";
            card.id = "002";
            card.billing_address = GetAddress();
            return card;
        }

        [TestMethod()]
        public void StateTest()
        {
            Capture target = GetCapture();
            string expected = "Authorized";
            string actual = target.state;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ParentPaymentTest()
        {
            Capture target = GetCapture();
            string expected = "1000";
            string actual = target.parent_payment;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void LinksTest()
        {
            Capture target = GetCapture();
            List<Links> expected = GetLinksList();
            List<Links> actual = target.links;
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.AreEqual(expected.Capacity, actual.Capacity);
        }

        [TestMethod()]
        public void IdTest()
        {
            Capture target = GetCapture();
            string expected = "001";
            string actual = target.id;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void CreateTimeTest()
        {
            Capture target = GetCapture();
            string expected = "2013-01-15T15:10:05.123Z";
            string actual = target.create_time;
            Assert.AreEqual(expected, actual);
        }
        
        [TestMethod()]
        public void AmountTest()
        {
            Capture target = GetCapture();
            Amount expected = GetAmount();
            Amount actual = target.amount;
            Assert.AreEqual(expected.currency, actual.currency);
            Assert.AreEqual(expected.details.fee, actual.details.fee);
            Assert.AreEqual(expected.details.shipping, actual.details.shipping);
            Assert.AreEqual(expected.details.subtotal, actual.details.subtotal);
            Assert.AreEqual(expected.details.tax, actual.details.tax);
            Assert.AreEqual(expected.total, actual.total);
        }

        [TestMethod()]
        public void ConvertToJsonTest()
        {
            Capture target = GetCapture();
            string actual = target.ConvertToJson();
            Assert.AreEqual("Authorized", target.state);
            Assert.AreEqual("001", target.id);
        }

        [TestMethod()]
        public void CaptureObjectTest()
        {
            Capture target = new Capture();
            Assert.IsNotNull(target);
        }

        [TestMethod()]
        public void GetCaptureTest()
        {
            Payment payment = GetPayment();
            string authorizationId = payment.transactions[0].related_resources[0].authorization.id;
            Authorization authorization = Authorization.Get(AccessToken, authorizationId);
            Capture capture = new Capture();
            Amount amount = new Amount();
            amount.total = "1";
            amount.currency = "USD";
            capture.amount = amount;
            Capture response = authorization.Capture(AccessToken, capture);
            Capture returnCapture = Capture.Get(AccessToken, response.id);
            Assert.AreEqual(response.id, returnCapture.id);
        }

        [TestMethod()]
        public void RefundCaptureTest()
        {
            Payment pay = GetPayment();
            string authorizationId = pay.transactions[0].related_resources[0].authorization.id;
            Authorization authorization = Authorization.Get(AccessToken, authorizationId);
            Capture cap = new Capture();
            Amount amnt = new Amount();
            amnt.total = "1";
            amnt.currency = "USD";
            cap.amount = amnt;
            Capture response = authorization.Capture(AccessToken, cap);
            Refund fund = new Refund();
            Amount refundAmount = new Amount();
            refundAmount.total = "1";
            refundAmount.currency = "USD";
            fund.amount = refundAmount;
            Refund responseRefund = response.Refund(AccessToken, fund);
            Assert.AreEqual("completed", responseRefund.state);
        }

        [TestMethod()]
        [ExpectedException(typeof(System.ArgumentNullException), "Value cannot be null. Parameter name: captureId cannot be null")]
        public void GetCaptureNullIdTest()
        {
            Capture returnCapture = Capture.Get(AccessToken, null);           
        } 
    }
}
