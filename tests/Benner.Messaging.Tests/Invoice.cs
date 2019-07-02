using System;

namespace Benner.Messaging.Tests
{
    public class Invoice
    {
        public int InvoiceNumber { get; set; }
        public int CustomerOrderNumber { get; set; }
        public decimal ForeignRate { get; set; }
        public string AccountReference { get; set; }
        public int OrderNumber { get; set; }
        public bool CurrencyUsed { get; set; }
        public DateTime InvoiceDate { get; set; }
        public int ItemsNet { get; set; }
        public decimal ItemsTax { get; set; }
        public decimal ItemsTotal { get; set; }
    }

    public class AnotherClass
    {
        public Invoice MyInvoice { get; set; }
        public System.Collections.Generic.List<string> Lista { get; set; } = new System.Collections.Generic.List<string>() { "oloco meu", "brincadeira bixo" };
    }
}
