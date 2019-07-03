using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Core;
using Core.Models.Db;
using Microsoft.AspNetCore.Authorization;

namespace Web.Controllers
{
	[Authorize(Roles ="Admin")]
    public class CatalystsController : Controller
    {
        private readonly FailsafeContext _context;

        public CatalystsController(FailsafeContext context)
        {
            _context = context;
        }

        // GET: Catalysts
		[AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Catalysts.ToListAsync());
        }

        // GET: Catalysts/Details/5
		[AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catalyst = await _context.Catalysts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (catalyst == null)
            {
                return NotFound();
            }

            return View(catalyst);
        }

        // GET: Catalysts/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Catalysts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,WeaponName,Icon,Description,DropLocation,Quest,Masterwork")] Catalyst catalyst)
        {
            if (ModelState.IsValid)
            {
                _context.Add(catalyst);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(catalyst);
        }

        // GET: Catalysts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catalyst = await _context.Catalysts.FindAsync(id);
            if (catalyst == null)
            {
                return NotFound();
            }
            return View(catalyst);
        }

        // POST: Catalysts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,WeaponName,Icon,Description,DropLocation,Quest,Masterwork")] Catalyst catalyst)
        {
            if (id != catalyst.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(catalyst);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatalystExists(catalyst.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(catalyst);
        }

        // GET: Catalysts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var catalyst = await _context.Catalysts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (catalyst == null)
            {
                return NotFound();
            }

            return View(catalyst);
        }

        // POST: Catalysts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var catalyst = await _context.Catalysts.FindAsync(id);
            _context.Catalysts.Remove(catalyst);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CatalystExists(int id)
        {
            return _context.Catalysts.Any(e => e.Id == id);
        }
    }
}
