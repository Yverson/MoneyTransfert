using System;
using System.Collections.Generic;
using MoneyTransfert.Models;
using System.Security.Claims;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System.Linq;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace MoneyTransfert.API
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        public string currentUserId { get; set; }
        public string Name { get; set; }

        public TransactionsController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            //currentUserId = _dbContext.Users.Where(c => c.UserName == this.User.Identity.Name).FirstOrDefault().Id;
            //Name = this.User.Identity.Name;

        }

        [HttpGet]
        public IEnumerable<Transactions> Get()
        {
            if (HttpContext.User.IsInRole("ADMIN"))
            {

                currentUserId = HttpContext.User.GetUserId();
                return _dbContext.Transactions.Where(c => c.TypeTransaction == "TRANSFERT").OrderByDescending(c => c.DateTransaction).Take(100);

            }
            else
            {
                currentUserId = HttpContext.User.GetUserId();
                return _dbContext.Transactions.Where(c => c.Utilisateur == currentUserId && c.TypeTransaction == "TRANSFERT").OrderByDescending(c => c.DateTransaction).Take(20);

            }
            //return _dbContext.Transactions;
        }
    }
}
