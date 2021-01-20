using System.Collections.Generic;
using System.Linq;

namespace Server.Account
{
    public class AccountHandler
    {
        public static void InitAccountSystem()
        {
            using Context context = new Context();

            List<Models.Account> userAccounts = context.Account.ToList();

            foreach (var userAccount in userAccounts.Where(userAccount => userAccount.IsOnline))
            {
                userAccount.IsOnline = false;
            }

            context.SaveChanges();
        }
    }
}