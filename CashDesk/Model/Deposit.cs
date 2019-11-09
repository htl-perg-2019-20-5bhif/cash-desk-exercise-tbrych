using System;
using System.ComponentModel.DataAnnotations;

namespace CashDesk.Model
{
    public class Deposit : IDeposit
    {
        public int DepositID { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        [Range(0, Double.MaxValue)]
        public decimal Amount { get; set; }

        IMembership IDeposit.Membership => Membership;
    }
}
