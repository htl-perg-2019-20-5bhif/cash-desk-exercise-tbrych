using CashDesk.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashDesk
{
    /// <inheritdoc />
    public class DataAccess : IDataAccess
    {
        CashDeskDataContext context = null;

        /// <inheritdoc />
        public Task InitializeDatabaseAsync()
        {
            if (context == null)
            {
                context = new CashDeskDataContext();
            }
            else
            {
                throw new InvalidOperationException("Error: Already initialized!");
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Error: Database not Initialized!");
            }

            if (String.IsNullOrEmpty(firstName) || String.IsNullOrEmpty(lastName))
            {
                throw new ArgumentException("Error: Wrong arguments!");
            }

            if (await context.Members.AnyAsync(m => m.LastName == lastName))
            {
                throw new DuplicateNameException("Error: Duplicate name!");
            }

            Member member = new Member();
            member.FirstName = firstName;
            member.LastName = lastName;
            member.Birthday = birthday;

            context.Members.Add(member);
            context.SaveChanges();

            return member.MemberNumber;
        }

        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Error: Database not Initialized!");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Error: MemberNumber unknown!");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);
            context.Members.Remove(member);
            context.SaveChanges();
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Error: Database not Initialized!");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Error: MemberNumber unknown!");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);

            if (member.Memberships == null)
            {
                member.Memberships = new List<Membership>();
            }

            if (member.Memberships.Count == 0 || member.Memberships[member.Memberships.Count - 1].End != DateTime.MaxValue)
            {
                Membership membership = new Membership();
                membership.Member = member;
                membership.Begin = DateTime.Now;
                context.Memberships.Add(membership);
                context.SaveChanges();

                return membership;
            }
            else
            {
                throw new AlreadyMemberException("Error: Member is already in a membership!");
            }

        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Error: Database not Initialized!");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Error: MemberNumber unknown!");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);

            if (member.Memberships != null && member.Memberships.Count > 0 && member.Memberships[member.Memberships.Count - 1].End == DateTime.MaxValue)
            {
                int membershipID = member.Memberships[member.Memberships.Count - 1].MemberShipID;

                Membership membership = await context.Memberships.FirstAsync(ms => ms.MemberShipID == membershipID);
                membership.End = DateTime.Now;

                context.Memberships.Update(membership);
                context.SaveChanges();

                return membership;
            }
            else
            {
                throw new NoMemberException("Error: Member is not in a membership!");
            }
        }

        /// <inheritdoc />
        public async Task DepositAsync(int memberNumber, decimal amount)
        {
            if (context == null)
            {
                throw new InvalidOperationException("Error: Database not Initialized!");
            }

            if (!await context.Members.AnyAsync(m => m.MemberNumber == memberNumber))
            {
                throw new ArgumentException("Error: MemberNumber unknown!");
            }

            if (amount < 0)
            {
                throw new ArgumentException("Error: Amount not valid");
            }

            Member member = await context.Members.FirstAsync(m => m.MemberNumber == memberNumber);

            if (member.Memberships != null && member.Memberships.Count > 0 && member.Memberships[member.Memberships.Count - 1].End == DateTime.MaxValue)
            {
                Membership membership = member.Memberships[member.Memberships.Count - 1];

                Deposit deposit = new Deposit();
                deposit.Membership = membership;
                deposit.Amount = amount;

                context.Deposits.Add(deposit);
                context.SaveChanges();
            }
            else
            {
                throw new NoMemberException("Error: Member is not in a membership!");
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDepositStatistics>> GetDepositStatisticsAsync()
        {
            if (context == null)
            {
                throw new InvalidOperationException("Error: Database not Initialized!");
            }

            List<Member> members = await context.Members.ToListAsync();
            List<DepositStatistics> statistics = new List<DepositStatistics>();

            foreach (var curMember in members)
            {
                DepositStatistics curSatistic = new DepositStatistics();
                curSatistic.Member = curMember;

                //I only used the deposits of the current membership (if the user was in another membership before, these Deposits are ignored)
                if (curMember.Memberships != null && curMember.Memberships.Count > 0 && curMember.Memberships[curMember.Memberships.Count - 1].End == DateTime.MaxValue)
                {
                    Membership membership = curMember.Memberships[curMember.Memberships.Count - 1];
                    decimal curTotalAmount = 0;
                    foreach (var curDeposit in membership.Deposits)
                    {
                        curTotalAmount += curDeposit.Amount;
                    }
                    curSatistic.TotalAmount = curTotalAmount;
                }
                else
                {
                    curSatistic.TotalAmount = 0;
                }

                statistics.Add(curSatistic);
            }

            return statistics;
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}
