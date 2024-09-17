using ContosoUni.Data;
using ContosoUni.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Identity.Client;

namespace ContosoUni.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext _context;

        public CoursesController(SchoolContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var result = _context.Courses
                .Include(c => c.Department)
                .AsNoTracking();

            return View(await result.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            return await ViewFromId(id, true);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            var course = await FindCourse(id, false);
            if (course == null)
            {
                return NotFound();
            }

            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Edit")]
        public async Task<IActionResult> EditPost(int? id)
        {
            var course = await FindCourse(id, false);
            if (course == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(course, "", c => c.Credits, c => c.DepartmentId, c => c.Title))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }

            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            return await ViewFromId(id, true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var course = await FindCourse(id, false);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult UpdateCourseCredits()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourseCredits(int? multiplier)
        {
            if (multiplier == null)
            {
                return View();
            }

            ViewData["RowsAffected"] = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Course SET Credits = Credits * {0}", parameters: multiplier);

            return View();
        }

        public async Task<IActionResult> Create()
        {
            PopulateDepartmentsDropDownList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Create")]
        public async Task<IActionResult> CreateCourse([Bind("CourseID,Credits,DepartmentId,Title")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateDepartmentsDropDownList(course.DepartmentId);
            return View(course);
        }

        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in _context.Departments
                                   orderby d.Name
                                   select d;

            ViewBag.DepartmentID = new SelectList(
                departmentsQuery.AsNoTracking(), 
                "DepartmentID", 
                "Name", 
                selectedDepartment
            );
        }

        private async Task<IActionResult> ViewFromId(int? id, bool inc)
        {
            var course = await FindCourse(id, inc);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        private async Task<Course?> FindCourse(int? id, bool include)
        {
            if (id == null) 
            {
                return null;
            }

            IQueryable<Course> set = _context.Courses;

            if (include)
            {
                set = set.Include(c => c.Department);
            }

            return await set
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.CourseID == id);
        }
    }
}
