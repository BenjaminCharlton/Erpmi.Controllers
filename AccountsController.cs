using Microsoft.AspNetCore.Mvc;
using Erpmi.Core;
using Erpmi.Core.ViewModels.Accounts;
using Erpmi.Core.Models;
using Microsoft.AspNetCore.Identity;
using Basics.Mvc;
using System.Threading.Tasks;

namespace DotNetGigs.Controllers
{
    [Route("api/[controller]")]
    public class AccountsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountsController(UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
        }

        // POST api/accounts/register
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = ApplicationUser.Create(
                model.Email,
                model.GivenName, 
                model.FamilyName,
                model.PreferredName,
                model.DateOfBirth); 

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

            _unitOfWork.Users.Add(
                ApplicationUser.Create(
                    model.Email,
                    model.GivenName,
                    model.FamilyName,
                    model.PreferredName,
                    model.DateOfBirth));

            await _unitOfWork.CompleteAsync();

            return new OkObjectResult("Account created");
        }
    }
}