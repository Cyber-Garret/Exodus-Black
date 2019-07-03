using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Web.Models;
using Web.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
	[Authorize(Roles = "Admin")]
	public class RolesController : Controller
	{
		RoleManager<NeiraRole> _roleManager;
		UserManager<NeiraUser> _userManager;
		public RolesController(RoleManager<NeiraRole> roleManager, UserManager<NeiraUser> userManager)
		{
			_roleManager = roleManager;
			_userManager = userManager;
		}
		public IActionResult Index() => View(_roleManager.Roles.ToList());

		public IActionResult Create() => View();
		[HttpPost]
		public async Task<IActionResult> Create(NeiraRole role)
		{
			if (!string.IsNullOrEmpty(role.Name))
			{
				IdentityResult result = await _roleManager.CreateAsync(new NeiraRole { Name = role.Name, Icon = role.Icon, DisplayName = role.DisplayName });
				if (result.Succeeded)
				{
					return RedirectToAction("Index");
				}
				else
				{
					foreach (var error in result.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
				}
			}
			return View(role);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(string id)
		{
			NeiraRole role = await _roleManager.FindByIdAsync(id);
			if (role != null)
			{
				IdentityResult result = await _roleManager.DeleteAsync(role);
			}
			return RedirectToAction("Index");
		}

		public IActionResult UserList() => View(_userManager.Users.ToList());

		public async Task<IActionResult> Edit(string userId)
		{
			// получаем пользователя
			NeiraUser user = await _userManager.FindByIdAsync(userId);
			if (user != null)
			{
				// получем список ролей пользователя
				var userRoles = await _userManager.GetRolesAsync(user);
				var allRoles = _roleManager.Roles.ToList();
				ChangeRoleViewModel model = new ChangeRoleViewModel
				{
					UserId = user.Id,
					UserName = user.UserName,
					UserEmail = user.Email,
					UserRoles = userRoles,
					AllRoles = allRoles
				};
				return View(model);
			}

			return NotFound();
		}
		[HttpPost]
		public async Task<IActionResult> Edit(string userId, List<string> roles)
		{
			// получаем пользователя
			NeiraUser user = await _userManager.FindByIdAsync(userId);
			if (user != null)
			{
				// получем список ролей пользователя
				var userRoles = await _userManager.GetRolesAsync(user);
				// получаем все роли
				var allRoles = _roleManager.Roles.ToList();
				// получаем список ролей, которые были добавлены
				var addedRoles = roles.Except(userRoles);
				// получаем роли, которые были удалены
				var removedRoles = userRoles.Except(roles);

				await _userManager.AddToRolesAsync(user, addedRoles);

				await _userManager.RemoveFromRolesAsync(user, removedRoles);

				return RedirectToAction("UserList");
			}

			return NotFound();
		}
	}
}
