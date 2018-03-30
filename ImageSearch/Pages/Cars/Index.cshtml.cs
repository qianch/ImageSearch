﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ImageSearchApp.Models;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using ImageSearchApp.BaiduClient;
using ImageSearchApp.Utilities;

namespace ImageSearchApp.Pages.Cars
{
    public class IndexModel : PageModel
    {
        private readonly WorldContext _context;

        public IndexModel(WorldContext context)
        {
            _context = context;
        }

        [BindProperty]
        public ImageUpload ImageUpload { get; set; }

        public IList<Car> Car { get; set; }

        public async Task OnGetAsync()
        {
            Car = await _context.Car.ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Car = await _context.Car.AsNoTracking().ToListAsync();
                return Page();
            }

            var ImgContent = await FileHelpers.ProcessFormFile(ImageUpload.UploadImg, ModelState);

            if (!ModelState.IsValid)
            {
                Car = await _context.Car.AsNoTracking().ToListAsync();
                return Page();
            }

            var ImageClassify = new ImageClassifyClient().imageClassify;
            var options = new Dictionary<string, object> { };
            var result = ImageClassify.CarDetect(Convert.FromBase64String(ImgContent), options);
            var title = string.IsNullOrEmpty(ImageUpload.Title) ?
                Path.GetFileNameWithoutExtension(ImageUpload.UploadImg.FileName) :
                ImageUpload.Title;

            var car = new Car
            {
                Title = title,
                ImgSize = ImageUpload.UploadImg.Length,
                ImgType = ImageUpload.UploadImg.ContentType,
                ImgContent = ImgContent,
                Data = result.ToString(),
                UploadDT = DateTime.UtcNow
            };

            _context.Car.Add(car);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
