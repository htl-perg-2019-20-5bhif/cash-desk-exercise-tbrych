using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CashDesk.Model
{
    public class Membership : IMembership
    {
        public int MemberShipID { get; set; }

        [Required]
        public Member Member { get; set; }

        [Required]
        public DateTime Begin { get; set; }

        private DateTime end = DateTime.MaxValue;

        public DateTime End
        {
            get
            {
                return end;
            }
            set
            {
                if (value >= Begin)
                {
                    end = value;
                }
            }
        }

        IMember IMembership.Member => Member;

        public List<Deposit> Deposits { get; set; }
    }
}
