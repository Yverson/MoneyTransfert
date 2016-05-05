using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyTransfert.Models
{
    public class AuthData
    {
        public string NoTel { get; set; }
        public string CodeSecret { get; set; }
        public string NvoCodeSecret { get; set; }
        public string Email { get; set; }
        public int Montant { get; set; }
    }
}